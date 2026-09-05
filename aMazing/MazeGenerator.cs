namespace aMazing
{
    public class MazeGenerator
    {
        private const char Wall = '█';
        private const char Path = ' ';
        private char[,]? _maze;
        private readonly Random _random = new();

        public void GenerateAndSave(string filePath, int Width, int Height)
        {
            if (Width % 2 == 0)
            {
                Width++;
            }

            if (Height % 2 == 0)
            {
                Height++;
            }

            _maze = new char[Height, Width];

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    _maze[y, x] = Wall;
                }
            }

            Stack<(int x, int y)> stack = new();
            _maze[1, 1] = Path;
            stack.Push((1, 1));

            int[] dx = { 0, 0, -2, 2 }, dy = { -2, 2, 0, 0 };

            while (stack.Count > 0)
            {
                var (cx, cy) = stack.Peek();
                List<(int nx, int ny, int wx, int wy)> neighbors = new();

                for (int i = 0; i < 4; i++)
                {
                    int nx = cx + dx[i], ny = cy + dy[i];
                    if (nx > 0 && nx < Width - 1 && ny > 0 && ny < Height - 1 && _maze[ny, nx] == Wall)
                    {
                        neighbors.Add((nx, ny, cx + (dx[i] / 2), cy + (dy[i] / 2)));
                    }
                }

                if (neighbors.Count > 0)
                {
                    var (nx, ny, wx, wy) = neighbors[_random.Next(neighbors.Count)];
                    _maze[wy, wx] = Path;
                    _maze[ny, nx] = Path;
                    stack.Push((nx, ny));
                }
                else
                {
                    _ = stack.Pop();
                }
            }

            _maze[1, 0] = Path;
            _maze[Height - 2, Width - 1] = Path;

            using (StreamWriter writer = new(filePath))
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        writer.Write(_maze[y, x]);
                    }

                    writer.WriteLine();
                }
            }

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Gray;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Console.Write(_maze[y, x]);
                }

                Console.WriteLine();
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
        }
    }
}
