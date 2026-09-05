using aMazing;

public abstract class BaseMazeSolver : IMazeSolver
{
    public abstract string Name { get; }
    protected int StepDelayMs => 1;
    protected int PathDelayMs => 15;

    public abstract List<(int x, int y)> Solve(char[,] maze, (int x, int y) start, (int x, int y) end);

    protected void AnimateBot(int x, int y)
    {
        Console.SetCursorPosition(x, y);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write('@');
        //Thread.Sleep(StepDelayMs);
    }

    protected void LeaveBreadcrumb(int x, int y)
    {
        Console.SetCursorPosition(x, y);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write('.');
    }

    public void DrawFinalPath(List<(int x, int y)> path)
    {
        if (path == null) return;
        Console.ForegroundColor = ConsoleColor.Green;
        foreach (var (px, py) in path)
        {
            Console.SetCursorPosition(px, py);
            Console.Write('*');
            Thread.Sleep(PathDelayMs);
        }
    }

    protected bool IsValidMove(char[,] maze, int x, int y)
    {
        return x >= 0 && x < maze.GetLength(1) && y >= 0 && y < maze.GetLength(0) && maze[y, x] == ' ';
    }
}