﻿namespace Lab2;

public enum MessageType
{
    UDP_BROADCAST_REQUEST = 0,
    UDP_BROADCAST_RESPONSE = 1,
    TCP_SYN = 10,
    TCP_ACK = 11,
    TCP_JOIN = 20,
    TCP_HISTORY_REQUEST = 30,
    TCP_HISTORY_RESPONSE = 31,
    TCP_DIALOG = 40,
    TCP_DISCONNECT = 50,
}