using System.Text.Json;

namespace server.Model;

public class ServerMessage
{
    public ServerMessageType Type { get; set; }
    public string? Content { get; set; }
    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }
}
public enum ServerMessageType
{
    AckJoin,
    PlayerMessage,
    GameJoin,
    GameStart,
    GameStop,
    MapUpdate,
    Message,
    Error,
    Quit
}