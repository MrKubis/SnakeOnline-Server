using server.Server;

namespace server.Model;

public class GameRoom(Player p1, Player p2)
{
    private readonly Player _p1 = p1;
    private readonly Player _p2 = p2;

    private Direction _p1Direction = Direction.UP;
    private Direction _p2Direction =Direction.UP;
    
    private bool _gameover = false;
    private Game.Game _game;
    private Timer _gameTimer { get; set; }
    public async Task StartGame()
    {
        var gameServer = GameServer.GetInstance();
        await _p1.Handler.SendMessageAsync(new ServerMessage { Type = ServerMessageType.GAMESTART });
        await _p1.Handler.SendMessageAsync(new ServerMessage { Type = ServerMessageType.GAMESTART });

        //ffff
        _game = new Game.Game(_p1Direction,_p2Direction);
        _game.GenerateMap();
        _game.InitializeSnakes();

        _gameTimer = new Timer(Update, null, 0, 100);
    }

    private void Update(object? state)
    {
        if (_gameover) return;

        _game.Update(_p1Direction, _p2Direction);

            if (CheckGameOver(out var winner, out var loser))
            {
                EndGame(winner, loser);
            }
    }

    private bool CheckGameOver(out Player? winner, out Player? loser)
    {
        throw new NotImplementedException();
    }

    private async Task EndGame(Player winner, Player loser)
    {
        _gameover = true;
        _gameTimer.Dispose();
        
        await winner.Handler.SendMessageAsync(new ServerMessage { Type = ServerMessageType.GAMESTOP, Content = "WIN"});
        await loser.Handler.SendMessageAsync(new ServerMessage { Type = ServerMessageType.GAMESTOP, Content = "LOST"});
    }   
}


public enum Direction
{
    UP,
    DOWN,
    LEFT,
    RIGHT
}