using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Lab2;

public class UdpService
{
    private const int PackageCount = 3;
    private const int Сalldown = 250;
    private const int UdpPortReceive = 5000;
    private readonly UdpClient _sendClient;
    private UdpClient _receiveClient;
    private Dictionary<IPAddress, string> _userTable;
    private readonly UserData _user;
    private readonly CancellationTokenSource _cts;
    
    public event Action<IPAddress>? UserDiscovered;

    public UdpService(UserData user, Dictionary<IPAddress, string> userTable)
    {
        _user = user;
        _userTable = userTable;
        _sendClient = new UdpClient(new IPEndPoint(_user.ipAddress,0));
        _receiveClient = new(new IPEndPoint(IPAddress.Any, UdpPortReceive));
        _sendClient.EnableBroadcast = true;
        _cts = new CancellationTokenSource();
    }

    public async Task ListenBroadcastAsync()
    {
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                var receiveMessage = await _receiveClient.ReceiveAsync();
                byte[] message = receiveMessage.Buffer;
                IPAddress ipAddress = receiveMessage.RemoteEndPoint.Address;

                #if DEBUG
                    Console.WriteLine($"{ipAddress} receive message.");
                #endif

                // Отсеять повторные пакеты и loopback
                if (ipAddress.Equals(_user.ipAddress) || _userTable.ContainsKey(ipAddress))
                    continue;

                // Проверим контрольную сумму
                if (!PackageTools.ApproveChecksum(message)) throw new Exception("Invalid checksum");

                // Обрабатываем сообщение по типу
                MessageType type = PackageTools.GetMessageType(message);
                switch (type)
                {
                    case MessageType.UDP_BROADCAST_REQUEST:
                    {
                        await HandleRequestAsync(PackageTools.GetBody(message), ipAddress);
                        Console.WriteLine("Received UDP BROADCAST_REQUEST");
                    }
                        break;
                    case MessageType.UDP_BROADCAST_RESPONSE:
                    {
                        HandleResponseAsync(PackageTools.GetBody(message), ipAddress);
                        Console.WriteLine("Received UDP BROADCAST_RESPONSE");
                    }
                        break;
                    default:
                        throw new Exception("Invalid type");
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
                break;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }


    public async Task SendBroadcastAsync()
    {
        byte[] resBody = PackageTools.FormUserInfoBody(_user.username);
        byte[] datagram = PackageTools.FormDatagram(MessageType.UDP_BROADCAST_REQUEST, resBody);
        for (int i = 0; i < PackageCount; i++)
        {
            await SendAsync(datagram, new IPEndPoint(IPAddress.Broadcast, UdpPortReceive));
            await Task.Delay(Сalldown);
        }
    }

    public void Disable()
    {
        if (_cts.IsCancellationRequested) return;
        
        #if DEBUG
            Console.WriteLine("Shutting down UPD service...");
        #endif
        
        _cts.Cancel();
        
        _receiveClient.Close();
        
        _sendClient.Close();
        
        #if DEBUG
            Console.WriteLine("UDP service stopped.");
        #endif
        
    }
    
    private async Task HandleRequestAsync(byte[] reqBody, IPAddress address)
    {
        // записываем в таблицу информацию
        string username = Encoding.UTF8.GetString(reqBody);
        _userTable.TryAdd(address, username);

        
        // формируем пакет для response
        byte[] resBody = PackageTools.FormUserInfoBody(_user.username);
        byte[] datagram = PackageTools.FormDatagram(MessageType.UDP_BROADCAST_RESPONSE, resBody);
        
        // отправляем пакет
        await SendAsync(datagram, new IPEndPoint(address, UdpPortReceive));
    }

    private void HandleResponseAsync(byte[] resBody, IPAddress address)
    {
        // записываем в таблицу информацию
        string username = Encoding.UTF8.GetString(resBody);
        _userTable.TryAdd(address, username);

        
        // тригерим tcp server
        UserDiscovered?.Invoke(address);
    }

    private async Task SendAsync(byte[] datagram, IPEndPoint endPoint)
    {
        await _sendClient.SendAsync(datagram, datagram.Length, endPoint);
    }
}