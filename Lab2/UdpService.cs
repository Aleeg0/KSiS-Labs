using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Lab2;

public class UdpService
{
    private const int UdpPortReceive = 5000;
    private const int UdpPortSend = 54000;
    private readonly IPAddress _broadcast = IPAddress.Parse("192.168.100.255");
    private readonly UdpClient _sendClient;
    private Dictionary<IPAddress, string> _userTable;
    private readonly UserData _user;
    
    public event Action<IPAddress>? UserDiscovered;

    public UdpService(UserData user, Dictionary<IPAddress, string> userTable)
    {
        _user = user;
        _userTable = userTable;
        _sendClient = new UdpClient(UdpPortSend);
    }


    public async Task ListenBroadcastAsync()
    {
        using UdpClient receiverClient = new UdpClient(new IPEndPoint(IPAddress.Any, UdpPortReceive));
        while (true)
        {
            try
            {
                // получить сообщение
                var receiveMessage = await receiverClient.ReceiveAsync();
                byte[] message = receiveMessage.Buffer;
                IPAddress ipAddress = receiveMessage.RemoteEndPoint.Address;
        
                #if DEBUG
                    Console.WriteLine($"{ipAddress} receive message.");
                #endif
                
                // отсеять повторные пакеты и loopback
                if (ipAddress.Equals(_user.ipAddress) || _userTable.ContainsKey(ipAddress))
                    continue;
                
                // проверим контрольную сумму
                if (!PackageTools.ApproveChecksum(message)) throw new Exception("Invalid checksum");
        
                // по типу вызывать метод обработчик
                MessageType type = PackageTools.GetMessageType(message);
                switch (type)
                {
                    case MessageType.UDP_BROADCAST_REQUEST:
                        await HandleRequestAsync(PackageTools.GetBody(message), ipAddress);
                        break;
                    case MessageType.UDP_BROADCAST_RESPONSE:
                        HandleResponseAsync(PackageTools.GetBody(message), ipAddress);
                        break;
                    default:
                        throw new Exception("Invalid type");
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
        }
    }
    
    async Task HandleRequestAsync(byte[] reqBody, IPAddress address)
    {
        // записываем в таблицу информацию
        string username = Encoding.UTF8.GetString(reqBody);
        _userTable.Add(address, username);
        
        // формируем пакет для response
        byte[] resBody = PackageTools.FormUserInfoBody(_user.username);
        byte[] datagram = PackageTools.FormDatagram(MessageType.UDP_BROADCAST_RESPONSE, resBody);
        
        // отправляем пакет
        await SendAsync(datagram, new IPEndPoint(address, UdpPortReceive));
    }

    void HandleResponseAsync(byte[] resBody, IPAddress address)
    {
        // записываем в таблицу информацию
        string username = Encoding.UTF8.GetString(resBody);
        _userTable.Add(address, username);
        
        // тригерим tcp server
        UserDiscovered?.Invoke(address);
    }

    public async Task SendBroadcastAsync()
    {
        byte[] resBody = PackageTools.FormUserInfoBody(_user.username);
        byte[] datagram = PackageTools.FormDatagram(MessageType.UDP_BROADCAST_REQUEST, resBody);
        await SendAsync(datagram, new IPEndPoint(_broadcast, UdpPortReceive));
    }

    private async Task<int> SendAsync(byte[] datagram, IPEndPoint endPoint)
    {
        return await _sendClient.SendAsync(datagram, datagram.Length, endPoint);
    }
}