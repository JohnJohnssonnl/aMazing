using System;
using System.Collections.Generic;

public class JumpPointSearchBot : BaseMazeSolver
{
    public override string Name => "Jump Point Search (JPS) Bot";

    private class JpsNode
    {
        public int X { get; }
        public int Y { get; }
        public int G { get; set; }
        public int H { get; }
        public int F => G + H;
        public JpsNode Parent { get; set; }

        public JpsNode(int x, int y, int g, int h, JpsNode parent)
        {
            X = x; Y = y; G = g; H = h; Parent = parent;
        }
    }

    public override List<(int x, int y)> Solve(char[,] maze, (int x, int y) start, (int x, int y) end)
    {
        var openQueue = new PriorityQueue<JpsNode, int>();
        var openTracker = new Dictionary<(int x, int y), JpsNode>();
        var closedList = new HashSet<(int x, int y)>();

        int startH = GetDistance(start.x, start.y, end.x, end.y);
        var startNode = new JpsNode(start.x, start.y, 0, startH, null);

        openQueue.Enqueue(startNode, startNode.F);
        openTracker.Add(start, startNode);

        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { -1, 1, 0, 0 };

        while (openQueue.Count > 0)
        {
            JpsNode currentNode = openQueue.Dequeue();
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
                int directionX = dx[i];
                int directionY = dy[i];

                var jumpPoint = Jump(maze, currentNode.X + directionX, currentNode.Y + directionY, directionX, directionY, start, end);

                if (jumpPoint != null && !closedList.Contains(jumpPoint.Value))
                {
                    int jx = jumpPoint.Value.x;
                    int jy = jumpPoint.Value.y;

                    int newG = currentNode.G + GetDistance(currentNode.X, currentNode.Y, jx, jy);
                    int newH = GetDistance(jx, jy, end.x, end.y);

                    if (!openTracker.TryGetValue((jx, jy), out JpsNode existingNode))
                    {
                        var neighborNode = new JpsNode(jx, jy, newG, newH, currentNode);
                        openQueue.Enqueue(neighborNode, neighborNode.F);
                        openTracker.Add((jx, jy), neighborNode);
                    }
                    else if (newG < existingNode.G)
                    {
                        existingNode.G = newG;
                        existingNode.Parent = currentNode;
                        openQueue.Enqueue(existingNode, existingNode.F);
                    }
                }
            }

            LeaveBreadcrumb(currentNode.X, currentNode.Y);
        }

        return null;
    }

    private (int x, int y)? Jump(char[,] maze, int cx, int cy, int dx, int dy, (int x, int y) start, (int x, int y) end)
    {
        if (!IsValidMove(maze, cx, cy)) return null;

        if (cx == end.x && cy == end.y) return (cx, cy);

        if (dx != 0)
        {
            if ((IsValidMove(maze, cx, cy + 1) && !IsValidMove(maze, cx - dx, cy + 1)) ||
                (IsValidMove(maze, cx, cy - 1) && !IsValidMove(maze, cx - dx, cy - 1)))
            {
                return (cx, cy);
            }
        }
        else if (dy != 0)
        {
            if ((IsValidMove(maze, cx + 1, cy) && !IsValidMove(maze, cx + 1, cy - dy)) ||
                (IsValidMove(maze, cx - 1, cy) && !IsValidMove(maze, cx - 1, cy - dy)))
            {
                return (cx, cy);
            }
        }

        return Jump(maze, cx + dx, cy + dy, dx, dy, start, end);
    }

    private int GetDistance(int x1, int y1, int x2, int y2)
    {
        return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
    }

    private List<(int x, int y)> ReconstructPath(JpsNode node)
    {
        List<(int, int)> path = new List<(int, int)>();
        JpsNode temp = node;
        while (temp != null)
        {
            path.Add((temp.X, temp.Y));
            temp = temp.Parent;
        }
        path.Reverse();

        List<(int, int)> fullPath = new List<(int, int)>();
        for (int i = 0; i < path.Count - 1; i++)
        {
            var p1 = path[i];
            var p2 = path[i + 1];
            int x = p1.Item1;
            int y = p1.Item2;

            while (x != p2.Item1 || y != p2.Item2)
            {
                fullPath.Add((x, y));
                x += Math.Sign(p2.Item1 - x);
                y += Math.Sign(p2.Item2 - y);
            }
        }
        fullPath.Add(path[path.Count - 1]);
        return fullPath;
    }
}