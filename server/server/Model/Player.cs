using server.Server;

namespace server.Model;

public class Player
{
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; } = 0;
    public GameRoom? Room { get; set; }
    public WebSocketHandler Handler { get; }
    
    public Player(WebSocketHandler webSocketHandler, string name)
    {
        Handler = webSocketHandler;
        Name = name;
    }

    public void EndGame(bool won)
    {
        if (won) Score += 1;
    }
}