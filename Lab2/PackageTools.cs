using System.Text;

namespace Lab2;

public static class PackageTools
{
    public const int DatagramHeaderSize = 6;
    public const int TimeSize = 8;

    public static byte[] FormUserInfoBody(string username)
    {
        return Encoding.UTF8.GetBytes(username);
    }

    public static byte[] FormTcpAcceptBody(int randomNumber)
    {
        return BitConverter.GetBytes(randomNumber);
    }
    
    public static byte[] FormDatagram(MessageType type, byte[] message)
    {
        using MemoryStream ms = new MemoryStream(DatagramHeaderSize + message.Length);
        using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8))
        {
            // 2(message type) + 2(checksum) + 2(message length) + (message)
            writer.Write((ushort)((int)type & 0xFF));
            writer.Write((ushort)0);
            writer.Write((ushort)message.Length);
            writer.Write(message);
        }
        byte[] datagram = ms.ToArray();
        ushort checksum = CalculateChecksum(datagram);
        datagram[3] = (byte)(checksum >> 8);
        datagram[2] = (byte)(checksum & 0xFF);

        return datagram;
    }
    
    public static byte[] FormDialogMessage(Message message)
    {
        using MemoryStream memoryStream = new MemoryStream(TimeSize + message.Text.Length * 2);
        using BinaryWriter writer = new BinaryWriter(memoryStream, Encoding.UTF8);
    
        writer.Write(message.Time.Ticks);
        writer.Write(Encoding.UTF8.GetBytes(message.Text));
    
        return memoryStream.ToArray();
    }
    
    public static bool ApproveChecksum(byte[] datagram)
    {
        ushort originalChecksum = (ushort)((datagram[3] << 8) | datagram[2]);
        (datagram[2], datagram[3]) = (0, 0);
        bool result = originalChecksum == CalculateChecksum(GetHeader(datagram));
        (datagram[3], datagram[2]) = ((byte)(originalChecksum >> 8), (byte)(originalChecksum & 0xFF));
        return result;
    }

    public static bool ApproveMessage(byte[] header, MessageType messageType)
    {
        if (GetMessageType(header) != messageType || !ApproveChecksum(header))
            return false;
        
        return true;
    }

    public static MessageType GetMessageType(byte[] datagram)
    {
        return (MessageType)(datagram[1] << 8 | datagram[0]);
    }

    public static ushort GetMessageLength(byte[] datagram)
    {
        return (ushort)((datagram[DatagramHeaderSize - 1] << 8) | datagram[DatagramHeaderSize - 2]);
    }
    
    public static byte[] GetHeader(byte[] datagram)
    {
        byte[] header = new byte[DatagramHeaderSize];
        Array.Copy(datagram, 0, header, 0, DatagramHeaderSize);
        return header;
    }

    public static byte[] GetBody(byte[] datagram)
    {
        int bodyLength = datagram.Length - DatagramHeaderSize;
        byte[] body = new byte[bodyLength];
        Array.Copy(datagram, DatagramHeaderSize, body, 0, bodyLength);
        return body;
    }
    
    private static ushort CalculateChecksum(byte[] header)
    {
        int sum = 0;
        // summarize 16-bit number
        for (int i = 0; i < DatagramHeaderSize; i+=2)
        {
            sum += (ushort)((header[i] << 8) + header[i + 1]);
        }
        // if sum is overflow 16-bit number
        while (sum >> 16 != 0)
        {
            sum += sum & 0xffff + sum >> 16;
        }
        // return invert number
        return (ushort)(~sum);
    }
}