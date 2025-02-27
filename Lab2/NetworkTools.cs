using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Lab2;

public static class NetworkTools
{
    public static string GetActiveIPAddress()
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .Where(n => n.OperationalStatus == OperationalStatus.Up && 
                        n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .SelectMany(n => n.GetIPProperties().UnicastAddresses)
            .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
            .Select(a => a.Address.ToString())
            .FirstOrDefault() ?? "Не удалось определить IP";
    }
    
    public static IPAddress GetIpV4(EndPoint endPoint)
    {
        IPAddress clientIp = ((IPEndPoint)endPoint).Address;
        return clientIp.IsIPv4MappedToIPv6 ? clientIp.MapToIPv4() : clientIp;
    }
}