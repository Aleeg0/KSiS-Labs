using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Lab2;

public class TcpService
{
    private const int SynMessageOffset = 2;
    private const int TcpPortReceive = 5001;
    private const int TcpPortSend = 54001;
    private readonly UserData _user;
    private List<TcpClient> _clients;
    private List<Message> _messages;
    private Dictionary<IPAddress, string> _userTable;
    private TcpListener _tcpListener;
    private readonly CancellationTokenSource _cts;
    
    public event Action<string>? MessageReceived;
    public event Action? OnConnect;

    public TcpService(UserData user, List<Message> messages, Dictionary<IPAddress, string> userTable)
    {
         _user = user;
        _clients = new List<TcpClient>();
        _messages = messages;
        _userTable = userTable;
        _tcpListener = new TcpListener(new IPEndPoint(_user.ipAddress, TcpPortReceive));
        _cts = new CancellationTokenSource();
    }


    public async Task ListenAsync()
    {
        // инициализируем прослушивателя
        _tcpListener.Start();
        // инициализируем рандомайзер
        Random random = new Random();

        while (!_cts.IsCancellationRequested)
        {
            try
            {
                // принимаем запросы от пользователей
                TcpClient tcpClient = await _tcpListener.AcceptTcpClientAsync();
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
                // тригерим ивент перезаписи списка подключенных
                OnConnect?.Invoke();
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
            // тригерим ивент перезаписи списка подключенных
            OnConnect?.Invoke();
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

    public async Task SendJoinMessage(Message message)
    {
        try
        {
            // переводи сообщение в байты
            byte[] datagram = PackageTools.FormDatagram(
                MessageType.TCP_JOIN, 
                PackageTools.FormDialogMessage(message)
            );
            // отправляем созданное сообщение
            await SendMessageAsync(_clients[^1], datagram);
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
    
    public async Task SendDialogMessageAsync(Message message, MessageType type)
    {
        // формируем сообщение
        byte[] datagram = PackageTools.FormDatagram(type, PackageTools.FormDialogMessage(message));
        
        // по каждому проходимся отправляем сообщение
        foreach (var client in _clients)
        {
            try
            {
                if (client.Connected && client.GetStream().CanWrite)
                {
                    await SendMessageAsync(client, datagram);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to send message to {client.Client.RemoteEndPoint}: {e.Message}");
            }
        }
    }

    public void Disable()
    {
        if (_cts.IsCancellationRequested) return;
        
        #if DEBUG
            Console.WriteLine("Shutting down TCP service...");
        #endif
        
        // отключаем tcpListener 
        _cts.Cancel();
        // закрываем прослушивателя
        _tcpListener.Stop();
        // очищаем массив клиентов
        _clients.Clear();
        // очищаем таблицу
        _userTable.Clear();

        #if DEBUG
            Console.WriteLine("TCP service stopped.");
        #endif
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
        // получаем IpV4
        IPAddress clientIp = NetworkTools.GetIpV4(tcpClient.Client.RemoteEndPoint!);
        
        #if DEBUG
            Console.WriteLine($"Start handling messages from {clientIp}");
        #endif
        
        try 
        {
            NetworkStream stream = tcpClient.GetStream();
            using BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true);

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
                    case MessageType.TCP_JOIN:
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
                        _ = Task.Run(async () => await HandleHistoryRequest(tcpClient));
                    }
                        break;
                    case MessageType.TCP_HISTORY_RESPONSE:
                    {
                        HandleHistoryResponse(
                            reader,
                            PackageTools.GetMessageLength(header)
                        );
                    }
                        break;
                    case MessageType.TCP_DISCONNECT:
                    {
                        HandleDialogMessage(
                            reader, 
                            PackageTools.GetMessageLength(header),
                            clientIp
                        );
                        // корректно закрываем поток
                        tcpClient.Close();
                        tcpClient.Dispose();
                        // удаляем из активных пользователей
                        _clients.Remove(tcpClient);
                        // удаляем из таблицы информацию
                        _userTable.Remove(clientIp);
                        // тригерим ивент перезаписи списка подключенных
                        OnConnect?.Invoke();
                    }
                        break;
                }
            }
        }
        catch (IOException)
        {
            Console.WriteLine($"Client {clientIp} disconnected.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    void HandleDialogMessage(BinaryReader reader, ushort messageLength, IPAddress clientIp)
    {
        // Читаем время
        DateTime time = new DateTime(reader.ReadInt64(), DateTimeKind.Local);
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
        try
        {
            byte[] datagram;
            // отправляем сообщения
            foreach (var message in _messages)
            {
                datagram = PackageTools.FormDatagram(
                    MessageType.TCP_HISTORY_RESPONSE,
                    PackageTools.FormHistoryMessage(message)
                );
                if (tcpClient.Connected)
                    await SendMessageAsync(tcpClient, datagram);
            }
            // формируем заключительное сообщение
            datagram = PackageTools.FormDatagram(
                MessageType.TCP_HISTORY_RESPONSE,
                [0]
            );
            // отправляем заключительное сообщение
            if (tcpClient.Connected)
                await SendMessageAsync(tcpClient, datagram);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    void HandleHistoryResponse(BinaryReader reader, ushort messageLength)
    {
        // если было доставлено конечное сообщение
        if (messageLength == 1 && reader.ReadByte() == 0) return;
        
        // Читаем время
        DateTime time = new DateTime(reader.ReadInt64(), DateTimeKind.Local);
        
        // Читаем размер имя пользователя
        int usernameLen = reader.ReadInt32();
        // Читаем имя пользователя
        byte[] usernameBytes = reader.ReadBytes(usernameLen);
        // декодируем имя
        string username = Encoding.UTF8.GetString(usernameBytes);
        
        // Читаем диалоговое сообщение
        byte[] textBytes = reader.ReadBytes(messageLength - PackageTools.TimeSize - usernameLen - 4);
        string text = Encoding.UTF8.GetString(textBytes);
        
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