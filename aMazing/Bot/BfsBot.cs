public class BfsBot : BaseMazeSolver
{
    public override string Name => "Breadth-First Search (BFS) Bot";

    public override List<(int x, int y)> Solve(char[,] maze, (int x, int y) start, (int x, int y) end)
    {
        Queue<(int x, int y, List<(int, int)> path)> queue = new Queue<(int, int, List<(int, int)>)>();
        HashSet<(int, int)> visited = new HashSet<(int, int)>();

        queue.Enqueue((start.x, start.y, new List<(int, int)> { start }));
        visited.Add(start);

        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { -1, 1, 0, 0 };

        while (queue.Count > 0)
        {
            var (cx, cy, currentPath) = queue.Dequeue();

            AnimateBot(cx, cy);

            if (cx == end.x && cy == end.y)
            {
                return currentPath;
            }

            for (int i = 0; i < 4; i++)
            {
                int nx = cx + dx[i];
                int ny = cy + dy[i];

                if (IsValidMove(maze, nx, ny) && !visited.Contains((nx, ny)))
                {
                    visited.Add((nx, ny));
                    var newPath = new List<(int, int)>(currentPath) { (nx, ny) };
                    queue.Enqueue((nx, ny, newPath));
                }
            }

            LeaveBreadcrumb(cx, cy);
        }

        return null;
    }
}
