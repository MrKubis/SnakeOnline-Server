using server.Model;

namespace server.Server;

public sealed class GameServer
{
    private GameServer() {}
    private static GameServer? _instance;
    private Dictionary<WebSocketHandler,Player> _players;
    
    public static GameServer GetInstance()
    {
        if (_instance == null)
        {
            _instance = new GameServer();
        }
        return _instance;
    }

    public async Task HandlePlayer(WebSocketHandler webSocketHandler, Message message)
    {
        try
        {
            if (message.Type == MessageType.JOIN)
            {
                HandleJoin(webSocketHandler, message);
                return;
            }
            
            //Checking 
            if (!_players.TryGetValue(webSocketHandler, out var player))
            {
                throw new Exception("Join first");
            }
            
            switch (message.Type)
            {
                case MessageType.MESSAGE:
                    HandleMessage(webSocketHandler, message);
                    break;
                case MessageType.QUIT:
                    await HandleQuit(webSocketHandler);
                    break;
            }
        }
        catch (Exception ex)
        {
            webSocketHandler.SendError(ex.Message);
        }

    }
    
    private async Task HandleJoin(WebSocketHandler webSocketHandler, Message message)
    {
        if (_players.TryGetValue(webSocketHandler, out var oldPlayer))
        {
            throw new Exception("Already joined");
        }

        Player player = new Player(webSocketHandler,message.Content);
        
        _players.Add(webSocketHandler,player);
    }
    
    private async Task HandleMessage(WebSocketHandler webSocketHandler, Message message)
    {
    }


    private async Task HandleQuit(WebSocketHandler webSocketHandler)
    {
        if (!_players.TryGetValue(webSocketHandler, out var player))
        {
            webSocketHandler.SendError("Join first");
        }
        
    }
}