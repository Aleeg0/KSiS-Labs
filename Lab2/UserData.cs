using System.Net;

namespace Lab2;

public record UserData
{
    public readonly string username;
    public readonly IPAddress ipAddress;

    public UserData(string username, IPAddress ipAddress)
    {
        this.username = username;
        this.ipAddress = ipAddress;
    }
    
    public override string ToString()
    {
        return $"UserInfo({username}, {ipAddress})";
    }
}