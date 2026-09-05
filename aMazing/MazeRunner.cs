using System.Diagnostics;
using System.Reflection;

public class MazeRunner
{
    private char[,] _maze;
    private readonly List<BaseMazeSolver> _bots;

    public MazeRunner()
    {
        _bots = Assembly.GetExecutingAssembly()
        .GetTypes()
        .Where(t => t.IsSubclassOf(typeof(BaseMazeSolver)) && !t.IsAbstract)
        .Select(t => (BaseMazeSolver)Activator.CreateInstance(t))
        .ToList();
    }

    public IDictionary<string, double> Run(string filePath)
    {
        IDictionary<string, double> ret = new Dictionary<string, double>();
        Console.CursorVisible = false;

        if (!TryLoadMaze(filePath))
        {
            throw new Exception("Cannot load maze");
        }

        foreach (BaseMazeSolver currentBot in _bots)
        {
            Console.Clear();
            DrawMaze();

            (int, int) startPos = (1, 1);
            (int, int) endPos = (_maze.GetLength(1) - 2, _maze.GetLength(0) - 2);

            Stopwatch stopwatch = Stopwatch.StartNew();

            List<(int x, int y)> solution = currentBot.Solve(_maze, startPos, endPos);
            stopwatch.Stop();

            // Show final draw of path
            //currentBot.DrawFinalPath(solution);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.SetCursorPosition(0, _maze.GetLength(0) + 1);

            ret.Add(currentBot.Name, stopwatch.Elapsed.TotalMicroseconds);

            if (solution != null)
            {
                ExportRouteToTxt(currentBot.Name, stopwatch.Elapsed.TotalMicroseconds, solution);
            }
        }

        return ret;
    }

    private bool TryLoadMaze(string filename)
    {
        if (!File.Exists(filename))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: Cannot find maze '{filename}'. Generator failure?");
            _ = Console.ReadLine();
            return false;
        }

        string[] lines = File.ReadAllLines(filename);
        _maze = new char[lines.Length, lines[0].Length];
        for (int y = 0; y < lines.Length; y++)
        {
            for (int x = 0; x < lines[0].Length; x++)
            {
                _maze[y, x] = lines[y][x];
            }
        }

        return true;
    }

    private void DrawMaze()
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        for (int y = 0; y < _maze.GetLength(0); y++)
        {
            for (int x = 0; x < _maze.GetLength(1); x++)
            {
                Console.Write(_maze[y, x]);
            }

            Console.WriteLine();
        }
    }

    public static void ExportRouteToTxt(string botName, double durationMs, List<(int x, int y)> path)
    {
        string safeBotName = botName.Replace(" ", "_").Replace("*", "Star").Replace("(", "").Replace(")", "");
        string rawFileName = $"{safeBotName}_route.txt";

        string folderName = "Validation";

        try
        {
            if (!Directory.Exists(folderName))
            {
                _ = Directory.CreateDirectory(folderName);
            }

            string fullPath = Path.Combine(folderName, rawFileName);

            using StreamWriter writer = new(fullPath, false);
            writer.WriteLine($"BotName: {botName}");
            writer.WriteLine($"ExecutionTimeMs: {durationMs:F4}");
            writer.WriteLine($"TotalSteps: {path?.Count ?? 0}");
            writer.WriteLine("--- START PATH ---");

            if (path != null)
            {
                foreach ((int x, int y) in path)
                {
                    writer.WriteLine($"{x},{y}");
                }
            }
            else
            {
                writer.WriteLine("No solution found.");
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Cannot write to {folderName}: {ex.Message}");
        }
    }
}