using server.Model;

namespace server.Game;

public class Game
{
    private Direction _p1Direction;
    private Direction _p2Direction;
    public Game(Direction p2Direction, Direction p1Direction)
    {
        _p2Direction = p2Direction;
        _p1Direction = p1Direction;
    }


    public void GenerateMap()
    {
        
    }

    public void InitializeSnakes()
    {
        throw new NotImplementedException();
    }

    public void Update(Direction p1Direction, Direction p2Direction)
    {
        throw new NotImplementedException();
    }
}

