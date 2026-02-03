using server.Server;

namespace server.Model;

public class Player
{
    public string Name { get; set; } = string.Empty;
    public WebSocketHandler Handler { get; }
    
    public Player(WebSocketHandler webSocketHandler, string name)
    {
        Handler = webSocketHandler;
        Name = name;
    }
}