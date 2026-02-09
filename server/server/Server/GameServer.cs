using server.Game;
using server.Model;

namespace server.Server;

public sealed class GameServer
{
    private GameServer() {}
    private static GameServer? _instance;
    private Dictionary<WebSocketHandler,Player> _players = new Dictionary<WebSocketHandler, Player>();
    private readonly List<Player> _waitlist = new List<Player>();
    private List<GameRoom> _gameRooms = new List<GameRoom>();
    public static GameServer GetInstance()
    {
        _instance ??= new GameServer();
        return _instance;
    }

    public async Task HandlePlayer(WebSocketHandler webSocketHandler, ClientMessage message)
    {
        try
        {
            if (message.Type == ClientMessageType.Join)
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
                case ClientMessageType.Move:
                    if (player.Room != null)
                    {
                        HandleMove(player,message);
                    }
                    break;
                case ClientMessageType.Message:
                    await HandleMessage(player, message);
                    break;
                case ClientMessageType.Quit:
                    await HandleQuit(player);
                    break;
            }
        }
        catch (Exception ex)
        {
            await webSocketHandler.SendErrorAsync(ex.Message);
        }
    }

    private void HandleMove(Player player, ClientMessage message)
    {
        Console.WriteLine(message.Content?.ToLower());
        switch (message.Content?.ToLower())
        {
            
            case "up":
                player.Room?.HandleInput(player, Direction.Up);
                break;
            case "down":
                player.Room?.HandleInput(player, Direction.Down);
                break;
            case "left":
                player.Room?.HandleInput(player, Direction.Left);
                break;
            case "right":
                player.Room?.HandleInput(player, Direction.Right);
                break;
        }
    }
    
    
    private async Task HandleJoin(WebSocketHandler webSocketHandler, ClientMessage message)
    {
        if (_players.TryGetValue(webSocketHandler, out var oldPlayer))
        {
            throw new Exception("Already joined");
        }

        var nickName = message.Content ?? "guest"; 
        
        Player player = new Player(webSocketHandler,nickName);
        
        _players.Add(webSocketHandler,player);
        await AddToWaitList(player);
    }
    
    private async Task HandleMessage(Player sender, ClientMessage message)
    {
        var recievers =  _players.Values.Where(entity => entity != sender);
        var serverMessage = new ServerMessage
        {
            Type = ServerMessageType.PlayerMessage,
            Content =  sender.Name+ " : " + message.Content,
        };
        foreach (var reciever in recievers)
        {
            //if(reciever.isOpen)
            await reciever.Handler.SendMessageAsync(serverMessage);
        }
    }

    private async Task AddToWaitList(Player player)
    {
        _waitlist.Add(player);
        await TryMatch();
    }

    private Task TryMatch()
    {
        while (_waitlist.Count >= 2 )
        {
            Player p1 = _waitlist.First();
            _waitlist.RemoveAt(0);
            Player p2 = _waitlist.First();
            _waitlist.RemoveAt(0);

            Console.WriteLine($"Matching: {p1} + {p2}");
            
            GameRoom newRoom = new GameRoom(p1, p2);
            lock (_gameRooms)
            {
                p1.Room = newRoom;
                p2.Room = newRoom;
                _gameRooms.Add(newRoom);
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    await p1.Handler.SendMessageAsync(new ServerMessage { Type = ServerMessageType.GameJoin });
                    await p2.Handler.SendMessageAsync(new ServerMessage { Type = ServerMessageType.GameJoin });
                    await newRoom.StartGame();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR in Task.Run: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }

            });
        }

        return Task.CompletedTask;
    }

    private async Task HandleQuit(Player player)
    {
        _waitlist.Remove(player);
        _players.Remove(player.Handler);
        await player.Handler.SendMessageAsync(new ServerMessage { Type = ServerMessageType.Quit });
    }

    public async Task EndGame(GameRoom gameRoom)
    {
        
    }

    public void RemoveGameRoom(GameRoom gameRoom)
    {
        lock (_gameRooms)
        {
            _gameRooms.Remove(gameRoom);
        }
    }
}