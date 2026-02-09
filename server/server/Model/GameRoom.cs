using server.Game;
using server.Server;

namespace server.Model;

public class GameRoom(Player p1, Player p2)
{
    public readonly Player _p1 = p1;
    public readonly Player _p2 = p2;

    private Direction _p1Direction = Direction.Up;
    private Direction _p2Direction = Direction.Up;
    
    private bool _gameover = false;
    private Game.Game _game;
    private Timer _gameTimer { get; set; }
    public async Task StartGame()
    {
        var gameServer = GameServer.GetInstance();
        await _p1.Handler.SendMessageAsync(new ServerMessage { Type = ServerMessageType.GameStart });
        await _p2.Handler.SendMessageAsync(new ServerMessage { Type = ServerMessageType.GameStart });

        //ffff
        _game = new Game.Game(_p1,_p2,20,20);
        _game.GenerateMap();
        _game.InitializeSnakes();

        _gameTimer = new Timer(Update, null, 0, 2000);
    }

    private void Update(object? state)
    {
        if (_gameover) return;

        _game.Update();
        
        _ = Task.Run(async () =>
        {
            try
            {
                await SendMapAsync(_p1);
                await SendMapAsync(_p2);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending map: {ex.Message}");
            }
        });
        if (_game.CheckGameOver(out var winner))
        {
            EndGame(winner).Wait();
        }
    }

    private async Task SendMapAsync(Player player)
    {
        string response = _game.ConvertMapToString();
        await player.Handler.SendMessageAsync(
            new ServerMessage
            {
                Type = ServerMessageType.GameStart,
                Content = response
            });
    }

    private async Task EndGame(Player? winner)
    {
        _gameover = true;
        _gameTimer.Dispose();
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

            _p1.Room = null;
            _p2.Room = null;
            var gameServer = GameServer.GetInstance();
            gameServer.RemoveGameRoom(this);
        }
    }

    public void HandleInput(Player player, Direction direction)
    {
        _game.HandleInput(player,direction);
    }
}


