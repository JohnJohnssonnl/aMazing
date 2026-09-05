namespace aMazing
{
    internal interface IMazeSolver
    {
        string Name { get; }
        List<(int x, int y)> Solve(char[,] maze, (int x, int y) start, (int x, int y) end);
    }
}
