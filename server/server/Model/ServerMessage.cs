namespace server.Model;

public class ServerMessage
{
    public ServerMessageType Type { get; set; }
    public string? Content { get; set; }
}
public enum ServerMessageType
{
    PlayerMessage,
    GameJoin,
    GameStart,
    GameStop,
    MapUpdate,
    Message,
    Error,
    Quit
}