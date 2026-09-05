using System;
using System.Collections.Generic;

public class OptimizedAStarBot : BaseMazeSolver
{
    public override string Name => "Optimized A* (Priority Queue) Bot";

    private class AStarNode
    {
        public int X { get; }
        public int Y { get; }
        public int G { get; set; }
        public int H { get; }
        public int F => G + H;
        public AStarNode Parent { get; set; }

        public AStarNode(int x, int y, int g, int h, AStarNode parent)
        {
            X = x; Y = y; G = g; H = h; Parent = parent;
        }
    }

    public override List<(int x, int y)> Solve(char[,] maze, (int x, int y) start, (int x, int y) end)
    {
        var openQueue = new PriorityQueue<AStarNode, (int F, int H)>(
            Comparer<(int F, int H)>.Create((a, b) => {
                int compare = a.F.CompareTo(b.F);
                return compare != 0 ? compare : a.H.CompareTo(b.H);
            })
        );

        var openTracker = new Dictionary<(int x, int y), AStarNode>();
        var closedList = new HashSet<(int x, int y)>();

        int startH = Math.Abs(start.x - end.x) + Math.Abs(start.y - end.y);
        var startNode = new AStarNode(start.x, start.y, 0, startH, null);

        openQueue.Enqueue(startNode, (startNode.F, startNode.H));
        openTracker.Add((start.x, start.y), startNode);

        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { -1, 1, 0, 0 };

        while (openQueue.Count > 0)
        {
            AStarNode currentNode = openQueue.Dequeue();
            var currentPos = (currentNode.X, currentNode.Y);

            if (closedList.Contains(currentPos))
                continue;

            AnimateBot(currentNode.X, currentNode.Y);

            if (currentNode.X == end.x && currentNode.Y == end.y)
            {
                List<(int, int)> finalPath = new List<(int, int)>();
                AStarNode temp = currentNode;
                while (temp != null)
                {
                    finalPath.Add((temp.X, temp.Y));
                    temp = temp.Parent;
                }
                finalPath.Reverse();
                return finalPath;
            }

            openTracker.Remove(currentPos);
            closedList.Add(currentPos);

            for (int i = 0; i < 4; i++)
            {
                int nx = currentNode.X + dx[i];
                int ny = currentNode.Y + dy[i];
                var neighborPos = (nx, ny);

                if (!IsValidMove(maze, nx, ny) || closedList.Contains(neighborPos))
                    continue;

                int newG = currentNode.G + 1;
                int newH = Math.Abs(nx - end.x) + Math.Abs(ny - end.y);

                if (!openTracker.TryGetValue(neighborPos, out AStarNode existingNode))
                {
                    var neighborNode = new AStarNode(nx, ny, newG, newH, currentNode);
                    openQueue.Enqueue(neighborNode, (neighborNode.F, neighborNode.H));
                    openTracker.Add(neighborPos, neighborNode);
                }
                else if (newG < existingNode.G)
                {
                    existingNode.G = newG;
                    existingNode.Parent = currentNode;
                    openQueue.Enqueue(existingNode, (existingNode.F, existingNode.H));
                }
            }

            LeaveBreadcrumb(currentNode.X, currentNode.Y);
        }

        return null;
    }
}