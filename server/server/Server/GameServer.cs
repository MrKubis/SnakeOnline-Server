using server.Model;

namespace server.Server;

public sealed class GameServer
{
    private GameServer() {}
    private static GameServer? _instance;
    private Dictionary<WebSocketHandler,Player> _players = new Dictionary<WebSocketHandler, Player>();
    
    public static GameServer GetInstance()
    {
        if (_instance == null)
        {
            _instance = new GameServer();
        }
        return _instance;
    }

    public async Task HandlePlayer(WebSocketHandler webSocketHandler, ClientMessage message)
    {
        try
        {
            if (message.Type == ClientMessageType.JOIN)
            {
                await HandleJoin(webSocketHandler, message);
                return;
            }
            
            //Checking 
            if (!_players.TryGetValue(webSocketHandler, out var player))
            {
                throw new Exception("Join first");
            }
            
            switch (message.Type)
            {
                case ClientMessageType.MESSAGE:
                    await HandleMessage(webSocketHandler, message);
                    break;
                case ClientMessageType.QUIT:
                    await HandleQuit(webSocketHandler);
                    break;
            }
        }
        catch (Exception ex)
        {
            await webSocketHandler.SendErrorAsync(ex.Message);
        }
    }
    
    private async Task HandleJoin(WebSocketHandler webSocketHandler, ClientMessage message)
    {
        if (_players.TryGetValue(webSocketHandler, out var oldPlayer))
        {
            throw new Exception("Already joined");
        }

        Player player = new Player(webSocketHandler,message.Content);
        
        _players.Add(webSocketHandler,player);
    }
    
    private async Task HandleMessage(WebSocketHandler webSocketHandler, ClientMessage message)
    {
        var sender = _players.FirstOrDefault(k => k.Key == webSocketHandler).Value;
        var recievers =  _players.Keys.Where(key => key != webSocketHandler);
        var serverMessage = new ServerMessage
        {
            Type = ServerMessageType.PLAYERMESSAGE,
            Content =  sender.Name+ " : " + message.Content,
        };
        foreach (var reciever in recievers)
        {
            //if(reciever.isOpen)
            await reciever.SendMessageAsync(serverMessage);
        }
    }


    private async Task HandleQuit(WebSocketHandler webSocketHandler)
    {
        if (!_players.TryGetValue(webSocketHandler, out var player))
        {
            webSocketHandler.SendErrorAsync("Join first");
        }
        
    }
}