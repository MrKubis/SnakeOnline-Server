using server.Game;
using server.Server;

namespace server.Model;

public class GameRoom(Player p1, Player p2): IDisposable
{
    public Player? _p1 = p1;
    public Player? _p2 = p2;
    
    private Direction _p1Direction = Direction.Up;
    private Direction _p2Direction = Direction.Up;
    
    private Task? _gameLoopTask;
    
    private bool _disposed;
    public bool GameOver { get; set; }
    private Game.Game? _game;
    public async Task StartGame()
    {
        await _p1.Handler.SendMessageAsync(new ServerMessage { Type = ServerMessageType.GameStart });
        await _p2.Handler.SendMessageAsync(new ServerMessage { Type = ServerMessageType.GameStart });

        _game = new Game.Game(_p1,_p2,20,20);
        _game.GenerateMap();
        _game.InitializeSnakes();
        _game.InitializeFruits();
        
        Console.WriteLine("starting game");
    }

    public async Task GameLoopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Starting game loop");
        try
        {
            while (!cancellationToken.IsCancellationRequested && !GameOver)
            {
                if (GameOver || _disposed) return;

                _game.Update();
                await SendMapAsync(_p1);
                await SendMapAsync(_p2);

                if (_game.CheckGameOver(out var winner))
                {
                    await EndGame(winner);
                }
                await Task.Delay(100, cancellationToken);
            }

        }
        catch (OperationCanceledException)
        {
            var gameServer = GameServer.GetInstance();
            await gameServer.RemoveGameRoom(this); 
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending map: {ex.Message}");
        }

    }

    private async Task SendMapAsync(Player player)
    {
        string response = _game.ConvertMapToString();
        await player.Handler.SendMessageAsync(
            new ServerMessage
            {
                Type = ServerMessageType.MapUpdate,
                Content = response
            });
    }

    private async Task EndGame(Player? winner)
    {
        if(GameOver) return;
        GameOver = true;

        Console.WriteLine("Ending game");

        if (winner == null)
        {
            await p1.Handler.SendMessageAsync(new ServerMessage
            {
                Type = ServerMessageType.GameStop,
                Content = "TIE"
            });
            await p2.Handler.SendMessageAsync(new ServerMessage
            {
                Type = ServerMessageType.GameStop,
                Content = "TIE"
            });
        }
        else
        {
            winner.EndGame(won:true);
            if (p1 == winner)
            {
                await p1.Handler.SendMessageAsync(new ServerMessage
                {
                    Type = ServerMessageType.GameStop,
                    Content = "WIN"
                });
                await p2.Handler.SendMessageAsync(new ServerMessage
                {
                    Type = ServerMessageType.GameStop,
                    Content = "LOSE"
                });
            }
            else
            {
                await p1.Handler.SendMessageAsync(new ServerMessage
                {
                    Type = ServerMessageType.GameStop,
                    Content = "LOSE"
                });
                await p2.Handler.SendMessageAsync(new ServerMessage
                {
                    Type = ServerMessageType.GameStop,
                    Content = "WIN"
                });
            }
        }
    }
    
    public void HandleInput(Player player, Direction direction)
    {
        _game.HandleInput(player,direction);
    }
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        
        _game?.CleanUp();
        _game = null;
        _disposed = true;
    }

    public void Dispose()
    {
        if (_disposed) return;

        _game?.CleanUp();
        _game = null;

        if (_p1 != null)
        {
            _p1.Room = null;
            _p1 = null;
        }
        if (_p2 != null)
        {
            _p2.Room = null;
            _p2 = null;
        }
        _disposed = true;

    }

}


