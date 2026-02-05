namespace server.Model;

public class ServerMessage
{
    public ServerMessageType Type { get; set; }
    public string? Content { get; set; }
}
public enum ServerMessageType
{
    PLAYERMESSAGE,
    GAMEJOIN,
    GAMESTART,
    GAMESTOP,
    BOARDUPDATE,
    MESSAGE,
    ERROR
}