//imports
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

// args
string target = "google.com";
bool isResolveDns = true;
int maxHops = 30;
int timeout = 4000;
int requestCount = 3;

for (int i = 0; i < args.Length - 1; i++)
{
    switch (args[i])
    {
        case "-d":
            isResolveDns = false;
            break;
        case "-h":
            maxHops = int.Parse(args[++i]);
            break;
        case "-w":
            timeout = int.Parse(args[++i]);
            break;
        case "-n":
            requestCount = int.Parse(args[++i]);
            break;
    }
}

if (args.Length > 0)
{
    target = args[^1];
}
else
{
    PrintHelp();
    return;
}


ushort pid = (ushort)Environment.ProcessId;
ushort sequenceNumber = 0;
int PORT = 8000;

// useful information
IPHostEntry targetInfo  =  Dns.GetHostEntry(target);
IPAddress targetIp = targetInfo.AddressList[0];
IPEndPoint targetEndPoint = new IPEndPoint(targetIp, 0);
IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 0);

// init stopwatch
Stopwatch sw = new Stopwatch();

// init sockets
var socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
socket.Bind(localEndPoint);
socket.ReceiveTimeout = timeout;

// output start info 
Console.WriteLine($"Трассировка маршрута к {targetInfo.HostName} [{targetIp}]\n" +
                  $"с максимальным числом прыжков {maxHops}: ");

// iterates from 1 to maxHops TTL to get each path point
for (int ttl = 1; ttl <= maxHops; ttl++)
{
    // form icmp datagram 
    byte[] message = FormIcmpDatagram();
    // change ttl of socket message
    socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, ttl);
    try
    {
        EndPoint curEndPoint = new IPEndPoint(IPAddress.Any, 0);
        byte[] response = new byte[256];
        short[] time = new short[requestCount];
        // send n - 1 messages
        for (int i = 0; i < requestCount - 1; i++)
        {
            sw.Start();
            // send message
            socket.SendTo(message, targetEndPoint);
            // waiting receive from a router
            socket.ReceiveFrom(response, ref curEndPoint);
            sw.Stop();
            time[i] = (short)sw.ElapsedMilliseconds;
            sw.Reset();
        }
        // send last message and get extra info
        sw.Start();
        // send message
        socket.SendTo(message, targetEndPoint);
        // waiting receive from a router
        socket.ReceiveFrom(response, ref curEndPoint);
        sw.Stop();
        time[^1] = (short)sw.ElapsedMilliseconds;
        sw.Reset();
        
        // getting type from icmp reply
        byte responseType = response[20];
        IPAddress receiverIp = ((IPEndPoint)curEndPoint).Address;
        // output message
        FormMessage(ttl, time, FormIpHostInfo(receiverIp));
        // check if
        if (responseType == 0)
            break;
    }
    catch (SocketException e) when (e.SocketErrorCode == SocketError.TimedOut)
    {
        Console.WriteLine($"{ttl,4}\t*\t*\t*\tПревышен интервал ожидания для запроса.");
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
    }   
}
Console.WriteLine("Трассировка завершена.");

byte[] FormIcmpDatagram()
{
    byte[] datagram = new byte[8];
    ++sequenceNumber;
    datagram[0] = 8; // Type
    datagram[1] = 0; // Code
    datagram[2] = 0; // Checksum before calculating
    datagram[3] = 0; // Checksum before calculating
    datagram[4] = (byte)(pid >> 8); // Identifier HB
    datagram[5] = (byte)pid; // Identifier LB
    datagram[6] = (byte)(sequenceNumber >> 8); // SN HB
    datagram[7] = (byte)sequenceNumber; // SN LB
    // calculating Checksum
    ushort checksum = CalculateChecksum(datagram);
    datagram[2] = (byte)(checksum >> 8);
    datagram[3] = (byte)checksum;
    
    return datagram;
}

ushort CalculateChecksum(byte[] datagram)
{
    int sum = 0;
    // summarize 16-bit number
    for (int i = 0; i < datagram.Length; i+=2)
    {
        sum += (ushort)((datagram[i] << 8) + datagram[i + 1]);
    }
    // if sum is overflow 16-bit number
    while (sum >> 16 != 0)
    {
        sum += sum & 0xffff + sum >> 16;
    }
    // return invert number
    return (ushort)(~sum);
}

void FormMessage(int iteration, short[] time, string ipHostInfo)
{
    StringBuilder stringBuilder = new StringBuilder();
    stringBuilder.Append($"{iteration,4}");
    if (time.Length > 0 && time[0] <= 1)
    {
        stringBuilder.Append($"{"<1",4} ms{"<1",4} ms{"<1",4} ms");
    }
    else
    {
        foreach (var t in time)
        {
            stringBuilder.Append($"{t,4} ms");
        }
    }
    stringBuilder.Append($"  {ipHostInfo}");
    Console.WriteLine(stringBuilder.ToString());
}

string FormIpHostInfo(IPAddress address)
{
    if (isResolveDns)
    {
        try
        {
            var info = Dns.GetHostEntry(address);
            return $"{info.HostName} [{address}]";
        }
        catch
        {
            return $"  {address}";
        }
    }
    // if not
    return $"  {address}";
}

void PrintHelp()
{
    Console.WriteLine(
        "\nИспользование: mytracert [-d] [h максЧисло] [-w таймаут]\n" +
        "                         [-n число] конечноеИмя\n\n" +
        "Параметры:\n" +
        $"{"-d",4} {"",-10}Без разрешения в имена узлов.\n" +
        $"{"-h",4} {"максЧисло",-10}Максимальное число прыжков при поиске узла.\n" +
        $"{"-w",4} {"таймаут",-10}Таймаут каждого ответа в миллисекундах.\n" +
        $"{"-n",4} {"число",-10}Число отправляемых пакетов.\n"
    );
}