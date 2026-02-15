using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
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
        
        player.Handler.SendMessageAsync(new ServerMessage{Type = ServerMessageType.AckJoin,Content=nickName});
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

    private async Task TryMatch()
    {
        while (_waitlist.Count >= 2 )
        {
            Player p1 = _waitlist.First();
            _waitlist.RemoveAt(0);
            Player p2 = _waitlist.First();
            _waitlist.RemoveAt(0);

            Console.WriteLine($"Matching: {p1} + {p2}");

            using (var newRoom = new GameRoom(p1, p2))
            {
                p2.Room = newRoom;
                p1.Room = newRoom;
                
                var cts = new CancellationTokenSource();

                try
                { 
                    await p1.Handler.SendMessageAsync(new ServerMessage { Type = ServerMessageType.GameJoin });
                    await p2.Handler.SendMessageAsync(new ServerMessage { Type = ServerMessageType.GameJoin });

                await newRoom.StartGame();
                while (!newRoom.GameOver && !cts.IsCancellationRequested)
                {
                    await newRoom.GameLoopAsync(cts.Token);
                }
                    
                }
                catch (Exception ex)
                {
                    await RemoveGameRoom(newRoom);
                    Console.WriteLine($"ERROR in Task.Run: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
                // Dodaj to do TryMatch() po finally:
                finally
                {
                    cts?.Cancel();
                    cts?.Dispose();
                    await RemoveGameRoom(newRoom);
                }
            }
            await HandleQuit(p1);
            await HandleQuit(p2);
        }
    }

    public async Task HandleQuit(Player player)
    {
        if (player.Room != null)
        {
            await RemoveGameRoom(player.Room);
        }
        _waitlist.Remove(player);
        _players.Remove(player.Handler);
        await player.Handler.SendMessageAsync(new ServerMessage { Type = ServerMessageType.Quit });
    }

    public async Task RemoveGameRoom(GameRoom gameRoom)
    {

        if (gameRoom._p1 != null)
        {
            gameRoom._p1.Room = null;

        }

        if (gameRoom._p2 != null)
        {
            gameRoom._p2.Room = null;
        }
            
        gameRoom.Dispose();
        lock (_gameRooms)
        {
            _gameRooms.Remove(gameRoom);
        }
        
    }

    public void RemoveWebSocketHandler(WebSocketHandler webSocketHandler)
    {
        _waitlist.Remove(_players[webSocketHandler]);
        _players.Remove(webSocketHandler);
    }
}