namespace Lab2;

public enum MessageType
{
    UDP_BROADCAST_REQUEST = 0,
    UDP_BROADCAST_RESPONSE = 1,
    TCP_SYN = 10,
    TCP_ACK = 11,
    TCP_HISTORY_REQUEST = 16,
    TCP_HISTORY_RESPONSE = 17,
    TCP_DIALOG = 20,
}