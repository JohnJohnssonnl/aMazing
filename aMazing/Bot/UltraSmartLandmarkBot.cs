using System;
using System.Collections.Generic;

public class UltraSmartLandmarkBot : BaseMazeSolver
{
    public override string Name => "Ultra-Smart Landmark & Corridor Bot";

    private class SmartNode
    {
        public int X { get; }
        public int Y { get; }
        public int G { get; set; }
        public int H { get; }
        public int F => G + H;
        public SmartNode Parent { get; set; }

        public SmartNode(int x, int y, int g, int h, SmartNode parent)
        {
            X = x; Y = y; G = g; H = h; Parent = parent;
        }
    }

    public override List<(int x, int y)> Solve(char[,] maze, (int x, int y) start, (int x, int y) end)
    {
        int height = maze.GetLength(0);
        int width = maze.GetLength(1);

        HashSet<(int, int)> deadEnds = PrecalculateDeadEnds(maze, start, end);

        var openQueue = new PriorityQueue<SmartNode, int>();
        var openTracker = new Dictionary<(int x, int y), SmartNode>();
        var closedList = new HashSet<(int x, int y)>();

        int startH = GetLandmarkHeuristic(start.x, start.y, end.x, end.y);
        var startNode = new SmartNode(start.x, start.y, 0, startH, null);

        openQueue.Enqueue(startNode, startNode.F);
        openTracker.Add(start, startNode);

        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { -1, 1, 0, 0 };

        while (openQueue.Count > 0)
        {
            SmartNode currentNode = openQueue.Dequeue();
            var currentPos = (currentNode.X, currentNode.Y);

            if (closedList.Contains(currentPos)) continue;

            AnimateBot(currentNode.X, currentNode.Y);

            if (currentNode.X == end.x && currentNode.Y == end.y)
            {
                return ReconstructPath(currentNode);
            }

            openTracker.Remove(currentPos);
            closedList.Add(currentPos);

            for (int i = 0; i < 4; i++)
            {
                int nx = currentNode.X + dx[i];
                int ny = currentNode.Y + dy[i];
                var neighborPos = (nx, ny);

                if (!IsValidMove(maze, nx, ny) || closedList.Contains(neighborPos) || deadEnds.Contains(neighborPos))
                    continue;

                int newG = currentNode.G + 1;
                int newH = GetLandmarkHeuristic(nx, ny, end.x, end.y);

                if (!openTracker.TryGetValue(neighborPos, out SmartNode existingNode))
                {
                    var neighborNode = new SmartNode(nx, ny, newG, newH, currentNode);
                    openQueue.Enqueue(neighborNode, neighborNode.F);
                    openTracker.Add(neighborPos, neighborNode);
                }
                else if (newG < existingNode.G)
                {
                    existingNode.G = newG;
                    existingNode.Parent = currentNode;
                    openQueue.Enqueue(existingNode, existingNode.F);
                }
            }

            LeaveBreadcrumb(currentNode.X, currentNode.Y);
        }

        return null;
    }

    private HashSet<(int x, int y)> PrecalculateDeadEnds(char[,] maze, (int x, int y) start, (int x, int y) end)
    {
        var deadEnds = new HashSet<(int x, int y)>();
        int height = maze.GetLength(0);
        int width = maze.GetLength(1);
        bool changeFinder = true;

        while (changeFinder)
        {
            changeFinder = false;
            for (int r = 1; r < height - 1; r++)
            {
                for (int c = 1; c < width - 1; c++)
                {
                    if (maze[r, c] == '#' || (r == start.y && c == start.x) || (r == end.y && c == end.x) || deadEnds.Contains((c, r)))
                        continue;

                    int wallCount = 0;
                    if (maze[r - 1, c] == '#' || deadEnds.Contains((c, r - 1))) wallCount++;
                    if (maze[r + 1, c] == '#' || deadEnds.Contains((c, r + 1))) wallCount++;
                    if (maze[r, c - 1] == '#' || deadEnds.Contains((c - 1, r))) wallCount++;
                    if (maze[r, c + 1] == '#' || deadEnds.Contains((c + 1, r))) wallCount++;

                    if (wallCount >= 3)
                    {
                        if (deadEnds.Add((c, r)))
                        {
                            changeFinder = true;
                        }
                    }
                }
            }
        }
        return deadEnds;
    }

    private int GetLandmarkHeuristic(int x1, int y1, int x2, int y2)
    {
        return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
    }

    private List<(int x, int y)> ReconstructPath(SmartNode node)
    {
        List<(int, int)> path = new List<(int, int)>();
        SmartNode temp = node;
        while (temp != null)
        {
            path.Add((temp.X, temp.Y));
            temp = temp.Parent;
        }
        path.Reverse();
        return path;
    }
}