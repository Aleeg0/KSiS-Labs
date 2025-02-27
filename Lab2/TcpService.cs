using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Lab2;

public class TcpService
{
    private const int SynMessageOffset = 2;
    private const int TcpPortReceive = 5001;
    private const int TcpPortSend = 54001;
    private readonly TcpClient _sendClient;
    private readonly UserData _user;
    private List<TcpClient> _clients;
    private List<Message> _messages;
    private Dictionary<IPAddress, string> _userTable;
    
    public event Action<string>? MessageReceived;

    public TcpService(UserData user, List<Message> messages, Dictionary<IPAddress, string> userTable)
    {
         _user = user;
        _sendClient = new TcpClient();
        _clients = new List<TcpClient>();
        _messages = messages;
        _userTable = userTable;
    }


    public async Task ListenAsync()
    {
        // инициализируем прослушивателя
        using TcpListener tcpListener = new TcpListener(new IPEndPoint(_user.ipAddress, TcpPortReceive));
        tcpListener.Start();
        Random random = new Random();

        while (true)
        {
            try
            {
                // принимаем запросы от пользователей
                TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();
                IPEndPoint clientEndPoint = (IPEndPoint)tcpClient.Client.RemoteEndPoint!;
                
                #if DEBUG
                    Console.WriteLine($"Accept connection from client {clientEndPoint}");
                #endif
                
                // сгенерируем число
                int randomNumber = random.Next(int.MinValue + SynMessageOffset, int.MaxValue);
                // формируем тест сообщение
                byte[] message = PackageTools.FormDatagram(MessageType.TCP_SYN, PackageTools.FormTcpAcceptBody(randomNumber));
                // отправить пользователю тест сообщение
                await SendMessageAsync(tcpClient, message);
                // принять от пользователя тест сообщение
                int receiveNumber = ReceiveAcceptMessage(tcpClient, MessageType.TCP_ACK);
                // проверяем результаты
                if (receiveNumber + SynMessageOffset != randomNumber)
                    throw new Exception("The message has been corrupted or intercepted!");
                // добавляем клиента в общий список клиентов
                _clients.Add(tcpClient);
                // если все хорошо, запускаем таску с сообщениями
                _ = Task.Run(() => ProceedDialogMessages(tcpClient));
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public async Task ConnectAsync(IPAddress address)
    {
        try
        {
            // создаем удаленного клиента
            TcpClient tcpClient = new TcpClient();
            // подключаемся к этому клиенту
            await tcpClient.ConnectAsync(address, TcpPortReceive);
            // ждем тест сообщение
            int acceptMessage = ReceiveAcceptMessage(tcpClient, MessageType.TCP_SYN);
            // формируем тест-ответ сообщение
            byte[] datagram = PackageTools.FormDatagram(
                MessageType.TCP_ACK, 
                PackageTools.FormTcpAcceptBody(acceptMessage - SynMessageOffset)
            );
            // отправляем тест-ответ сообщение
            await SendMessageAsync(tcpClient, datagram);
            // добавляем клиента в общий список клиентов
            _clients.Add(tcpClient);
            // если все хорошо, запускаем таску с сообщениями
            _ = Task.Run(() => ProceedDialogMessages(tcpClient));
        }
        catch (SocketException e)
        {
            Console.WriteLine(e);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public async Task RequestHistoryAsync()
    {
        byte[] datagram = PackageTools.FormDatagram(MessageType.TCP_HISTORY_REQUEST, []);
        // отправляем каждому запрос на историю
        foreach (var client in _clients)
        {
            if (client.Connected && client.GetStream().CanWrite)
            {
                await SendMessageAsync(client, datagram);
            }
        }
    }
    
    public async Task SendDialogMessageAsync(Message message)
    {
        // формируем сообщение
        byte[] datagram = PackageTools.FormDatagram(MessageType.TCP_DIALOG, PackageTools.FormDialogMessage(message));
        
        List<TcpClient> disconnectedClients = new List<TcpClient>();
        // по каждому проходимся отправляем сообщение
        foreach (var client in _clients)
        {
            try
            {
                if (client.Connected && client.GetStream().CanWrite)
                {
                    await SendMessageAsync(client, datagram);
                }
                else
                {
                    disconnectedClients.Add(client);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to send message to {client.Client.RemoteEndPoint}: {e.Message}");
                disconnectedClients.Add(client);
            }
        }
        foreach (var client in disconnectedClients)
        {
            _clients.Remove(client);
        }
    }

    async Task<int> SendMessageAsync(TcpClient tcpClient, byte[] datagram)
    {
        // получаем поток
        NetworkStream reader = tcpClient.GetStream();
        // отправляем сообщение удаленному клиенту
        await reader.WriteAsync(datagram, 0, datagram.Length);
        return datagram.Length;
    }
    
    int ReceiveAcceptMessage(TcpClient tcpClient, MessageType type)
    {
        #if DEBUG
            Console.WriteLine($"Getting hello message from {tcpClient.Client.RemoteEndPoint}...");
        #endif
        
        NetworkStream stream = tcpClient.GetStream();
        using BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true);
    
        byte[] header = reader.ReadBytes(PackageTools.DatagramHeaderSize);
        
        // проверяем пакет на соответствие 
        if (!PackageTools.ApproveMessage(header, type))
            throw new Exception("Invalid type");
        
        return reader.ReadInt32();
    }
    
    void ProceedDialogMessages(TcpClient tcpClient)
    {
        try
        {
            // получаем IpV4
            IPAddress clientIp = NetworkTools.GetIpV4(tcpClient.Client.RemoteEndPoint!);
            
            #if DEBUG
                Console.WriteLine($"Start handling messages from {clientIp}");
            #endif
            
            try 
            {
                NetworkStream stream = tcpClient.GetStream();
                using BinaryReader reader = new BinaryReader(stream, Encoding.UTF8);

                // запускаем цикл пока подключены на прослушку сообщений
                while (tcpClient.Connected)
                {
                    byte[] header = reader.ReadBytes(PackageTools.DatagramHeaderSize);
                    // проверяем на валидность
                    if (!PackageTools.ApproveChecksum(header)) continue;

                    switch (PackageTools.GetMessageType(header))
                    {
                        case MessageType.TCP_DIALOG:
                        {
                            HandleDialogMessage(
                                reader, 
                                PackageTools.GetMessageLength(header),
                                clientIp
                            );
                        }
                            break;
                        case MessageType.TCP_HISTORY_REQUEST:
                        {
                            _ = Task.Run(() => HandleHistoryRequest(tcpClient));
                        }
                            break;
                        case MessageType.TCP_HISTORY_RESPONSE:
                        {
                            HandleHistoryResponse(
                                reader, 
                                PackageTools.GetMessageLength(header),
                                clientIp
                            );
                            
                        }
                            break;
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            // как только отключились, корректно закрываем поток
            finally
            {
                tcpClient.Close();
                // удаляем из активных пользователей
                _clients.Remove(tcpClient);
                // удаляем из таблицы информацию
                _userTable.Remove(clientIp);
                
                #if DEBUG
                    Console.WriteLine($"user with ip {clientIp} closed.");
                #endif
            }
        }
        catch (Exception)
        {
            Console.WriteLine("Connection error");
        }
    }

    void HandleDialogMessage(BinaryReader reader, ushort messageLength, IPAddress clientIp)
    {
        // Читаем время
        DateTime time = new DateTime(reader.ReadInt64(), DateTimeKind.Utc);
        // Читаем диалоговое сообщение
        byte[] textBytes = reader.ReadBytes(messageLength - PackageTools.TimeSize);
        string text = Encoding.UTF8.GetString(textBytes);
        // формируем сообщение
        Message receivedMessage = new Message(_userTable[clientIp], text, time);
        // тригерим event, который будет добавлять сообщение в TextBox в ChatLayout
        MessageReceived?.Invoke(receivedMessage.ToString());
        // добавляем в сообщения
        _messages.Add(receivedMessage);
    }

    async Task HandleHistoryRequest(TcpClient tcpClient)
    {
        
        byte[] datagram;
        // отправляем сообщения
        foreach (var message in _messages)
        {
            datagram = PackageTools.FormDatagram(
                MessageType.TCP_HISTORY_RESPONSE,
                PackageTools.FormDialogMessage(message)
            );
            await SendMessageAsync(tcpClient, datagram);
        }
        // формируем заключительное сообщение
        datagram = PackageTools.FormDatagram(
            MessageType.TCP_HISTORY_RESPONSE,
            [0]
        );
        // отправляем заключительное сообщение
        await SendMessageAsync(tcpClient, datagram);
    }
    
    void HandleHistoryResponse(BinaryReader reader, ushort messageLength, IPAddress clientIp)
    {
        // если было доставлено конечное сообщение
        if (messageLength == 1) return;
        
        // Читаем время
        DateTime time = new DateTime(reader.ReadInt64(), DateTimeKind.Utc);
        // Читаем диалоговое сообщение
        byte[] textBytes = reader.ReadBytes(messageLength - PackageTools.TimeSize);
        string text = Encoding.UTF8.GetString(textBytes);
        string username = _userTable[clientIp];
        // формируем сообщение
        Message receivedMessage = new Message(username, text, time);
        // проверяем не было ли уже получено такого сообщения,
        // если не существует, добавляем
        if (!_messages.Exists((message) => 
                message.Text == text && message.Time == time && message.Username == username ))
        {
            // тригерим event, который будет добавлять сообщение в TextBox в ChatLayout
            MessageReceived?.Invoke(receivedMessage.ToString());
            // добавляем в сообщения
            _messages.Add(receivedMessage);    
        }
    }
}