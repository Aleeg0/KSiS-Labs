namespace Lab2;

public class Message
{
    private readonly DateTime _time;
    private readonly string _username;
    private readonly string _text;

    public DateTime Time => _time;

    public string Username => _username;

    public string Text => _text;

    public Message(string username, string text, DateTime time)
    {
        _username = username;
        _text = text;
        _time = time;
    }

    public override string ToString()
    {
        return $"{_username}[{_time:HH:mm:ss}]: {_text}";
    }
}