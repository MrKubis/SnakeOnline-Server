namespace server.Model;

public class Message
{
    public MessageType Type { get; set; }
    public string Content { get; set; }
}

public enum MessageType
{
    JOIN,
    QUIT,
    MOVE,
    MESSAGE
}