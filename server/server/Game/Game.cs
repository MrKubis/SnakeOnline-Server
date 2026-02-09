
using server.Model;

namespace server.Game;

public class Game
{
    private Player? _winner = null;
    private bool _gameover = false;

    private readonly Player _p1;
    private readonly Player _p2;
    private Direction _p1Direction;
    private Direction _p2Direction;
    
    private List<SnakeBody> _snake1;
    private List<SnakeBody> _snake2;
    private readonly Random _random;
    private CellType[][] _map { get; set; }
    private int _width;
    private int _height;
    
    public Game(Player p1, Player p2, int height, int width)
    {
        _p1 = p1;
        _p2 = p2;
        _random = new Random();
        _p2Direction = Direction.Up;
        _p1Direction = Direction.Up;
        _height = height;
        _width = width;
    }
    

    public void GenerateMap()
    {
        _map = new CellType[_height][];
        for (int i = 0; i < _height; i++)
        {
            _map[i] = new CellType[_width];
            for (int j = 0; j < _width; j++)
            {
                _map[i][j] = CellType.Empty;
            }
        }
    }

    public void InitializeSnakes()
    {
        _snake1 = new List<SnakeBody>();
        _snake2 = new List<SnakeBody>();
        int x1 = 2;
        int y1 = _height/2;
        SnakeBody head1 = new SnakeBody
        {
            X = x1,
            Y = y1
        };
        _snake1.Add(head1);
        
        int x2 = _width - 2;
        int y2 = _height/2;
        SnakeBody head2 = new SnakeBody
        {
            X = x2,
            Y = y2
        };
        _snake2.Add(head2);
    }

    public void InitializeFruits()
    {
        int x1 = _random.Next(0, _width/2);
        int y1 = _random.Next(0, _height);
        
        int x2 = _random.Next(_width/2, _width);
        int y2 = _random.Next(0, _height);
        _map[y1][x1] = CellType.Fruit; 
        _map[y2][x2] = CellType.Fruit; 
    }
    public void InitializeFruit()
    {
        int count = 0;
        int selectedX = 0;
        int selectedY = 0;
        for (int x = 0; x < _height; x++)
        {
            for (int y = 0; y < _width; y++)
            {
                if (_map[x][y] == CellType.Empty)
                {
                    count++;
                    if (_random.Next(0, count) == 0)
                    {
                        selectedX = x;
                        selectedY = y;
                    }
                }
            }
        }
        _map[selectedY][selectedX] = CellType.Fruit;
    }
    
    public void Update()
    {
        if (CheckBoundaries(_snake1, _p1Direction))
        {
            GameOver(winner:_p2);
            Console.WriteLine("Out of bounds for p1");
            return;
        }
        if (CheckBoundaries(_snake2, _p2Direction))
        {
            Console.WriteLine("Out of bounds for p2");
            GameOver(winner:_p1);
            return;
        }

        if (CheckIfFruit(_snake1, _p1Direction))
        {
            EatFruit(_snake1);
            int lastX = _snake1.Last().X;
            int lastY = _snake1.Last().Y;
            GrowSnake(_snake1,lastX,lastY);
            InitializeFruit();
        }

        if (CheckIfFruit(_snake2,_p2Direction))
        {
            EatFruit(_snake2);
            int lastX = _snake2.Last().X;
            int lastY = _snake2.Last().Y;
            GrowSnake(_snake2,lastX,lastY);
            InitializeFruit();
        }
        
        HandleCollision(_p1,_snake1, _p1Direction, _p2,_snake2,_p2Direction);

        Move(_snake1,_p1Direction);
        Move(_snake2,_p2Direction);
    }

    private bool CheckIfFruit(List<SnakeBody> snake, Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                if (_map [snake.First().Y - 1] [snake.First().X] == CellType.Fruit) return true;
                break;
            case Direction.Down:
                if (_map [snake.First().Y + 1] [snake.First().X] == CellType.Fruit) return true;
                break;            
            case Direction.Left:
                if (_map [snake.First().Y] [snake.First().X - 1] == CellType.Fruit) return true;
                break;
            case Direction.Right:
                if (_map [snake.First().Y] [snake.First().X + 1] == CellType.Fruit) return true;
                break;        
        }
        return false;
    }

    private bool CheckBoundaries(List<SnakeBody> snake, Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                if (snake.First().Y - 1 < 0) return true;
                break;
            case Direction.Down:
                if (snake.First().Y + 1 > _height - 1 ) return true;
                break;            
            case Direction.Left:
                if (snake.First().X - 1 < 0) return true;
                break;
            case Direction.Right:
                if (snake.First().X + 1 > _width - 1) return true;
                break;        
        }
        return false;
    }
    
    private void HandleCollision(Player p1,List<SnakeBody> snake1, Direction snake1Direction, Player p2, List<SnakeBody> snake2, Direction snake2Direction)
    {
        var head1 = snake1[0];
        var head2 = snake2[0];
        
        var nextPos1 = GetNextPosition(head1, snake1Direction);
        var nextPos2 = GetNextPosition(head2, snake2Direction);

        //If they hit eachother with head
        if (IsSamePosition(nextPos1, nextPos2))
        {
            GameOver(null);
        }

        if (_map[nextPos1.Y][nextPos1.X] == CellType.Snake
            && !IsSamePosition(nextPos1, snake1[^1])
            && !IsSamePosition(nextPos1, snake2[^1]))
        {
            Console.WriteLine("p1 crashed on p2");
            GameOver(p1);
        }
        if (_map[nextPos2.Y][nextPos2.X] == CellType.Snake
            && !IsSamePosition(nextPos2, snake1[^1])
            && !IsSamePosition(nextPos2, snake2[^1]))
        {
            Console.WriteLine("p2 crashed on p1");

            GameOver(p2);
        }
    }

    private void GameOver(Player? winner)
    {
        _winner = winner;
        _gameover = true;
    }
    private bool IsSamePosition(SnakeBody s1, SnakeBody s2)
    {
        return s1.X == s2.X && s1.Y == s2.Y;
    }
    
    private SnakeBody GetNextPosition(SnakeBody snakeBody, Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return new SnakeBody{X = snakeBody.X, Y = snakeBody.Y-1};
            case Direction.Down:
                return new SnakeBody{X = snakeBody.X, Y = snakeBody.Y+1};
            case Direction.Left:
                return new SnakeBody{X = snakeBody.X-1, Y = snakeBody.Y};
            case Direction.Right:
                return new SnakeBody{X = snakeBody.X+1, Y = snakeBody.Y};
        }
        return snakeBody;
    }
    private void EatFruit(List<SnakeBody> snake)
    {
        var head = snake.First();
        _map[head.Y][head.X] = CellType.Empty;
    }
    
    private void GrowSnake(List<SnakeBody> snake, int lastX, int lastY)
    {
        snake.Add(new SnakeBody{X = lastX, Y = lastY});
        _map[lastY][lastX] = CellType.Snake;
    }

    private void Move(List<SnakeBody> snake, Direction direction)
    {
        _map[snake[0].Y][snake[0].X] = CellType.Empty;
        _map[snake[^1].Y][snake[^1].X] = CellType.Empty;
        for (int i = snake.Count - 1; i >= 1; i--)
        {
            snake[i].X = snake[i - 1].X;
            snake[i].Y = snake[i - 1].Y;
            _map[snake[i].Y][snake[i].X] = CellType.Snake;
        }

        switch (direction)
        {
            case Direction.Up:
                snake[0].Y -= 1;
                break;
            case Direction.Down:
                snake[0].Y += 1;
                break;
            case Direction.Left:
                snake[0].X  -= 1;
                break;
            case Direction.Right:
                snake[0].X  += 1;
                break;
        }
        _map[snake[0].Y][snake[0].X] = CellType.Snake;
    }

    public bool CheckGameOver(out Player? winner)
    {
        winner = _winner;
        return _gameover;
    }

    public void HandleInput(Player player, Direction direction)
    {
        if (_p1 == player)
        {
            Console.WriteLine("Changing direction for p1");
            _p1Direction = direction;
        }
        else if(_p2 == player)
        {
            Console.WriteLine("Changing direction for p2");
            _p2Direction = direction;
        }
    }

    public string ConvertMapToString()
    {
        string result = "";
        for (int i = 0; i < _height; i++)
        {
            for (int j = 0; j < _width; j++)
            {
                switch (_map[i][j])
                {
                    case CellType.Fruit:
                        result += "F";
                        break;
                    case CellType.Empty:
                        result += "0";
                        break;
                    case CellType.Snake:
                        result += "S";
                        break;
                }
            }
            result += "\n";
        }
        return result;
    }
}
public enum Direction
{
    Up,
    Down,
    Left,
    Right
}
public enum CellType
{
    Empty,
    Snake,
    Fruit
}

