using System;
using System.Collections.Generic;

public class VectorizedJpsBot : BaseMazeSolver
{
    public override string Name => "Vectorized JPS Bot";

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

        int startH = Math.Abs(start.x - end.x) + Math.Abs(start.y - end.y);
        var startNode = new JpsNode(start.x, start.y, 0, startH, null);

        openQueue.Enqueue(startNode, startNode.F);
        openTracker.Add(start, startNode);

        while (openQueue.Count > 0)
        {
            JpsNode currentNode = openQueue.Dequeue();
            var currentPos = (currentNode.X, currentNode.Y);

            if (closedList.Contains(currentPos)) continue;

            if (currentNode.X == end.x && currentNode.Y == end.y)
            {
                return ReconstructPath(currentNode);
            }

            openTracker.Remove(currentPos);
            closedList.Add(currentPos);

            EvaluateDirection(maze, currentNode, 0, -1, end, openQueue, openTracker, closedList); 
            EvaluateDirection(maze, currentNode, 0, 1, end, openQueue, openTracker, closedList); 
            EvaluateDirection(maze, currentNode, -1, 0, end, openQueue, openTracker, closedList); 
            EvaluateDirection(maze, currentNode, 1, 0, end, openQueue, openTracker, closedList);

            LeaveBreadcrumb(currentNode.X, currentNode.Y);
        }

        return null;
    }

    private void EvaluateDirection(char[,] maze, JpsNode currentNode, int dx, int dy, (int x, int y) end,
        PriorityQueue<JpsNode, int> openQueue, Dictionary<(int x, int y), JpsNode> openTracker, HashSet<(int x, int y)> closedList)
    {
        var jumpPoint = VectorJump(maze, currentNode.X, currentNode.Y, dx, dy, end);

        if (jumpPoint != null && !closedList.Contains(jumpPoint.Value))
        {
            int jx = jumpPoint.Value.x;
            int jy = jumpPoint.Value.y;

            int newG = currentNode.G + (Math.Abs(currentNode.X - jx) + Math.Abs(currentNode.Y - jy));
            int newH = Math.Abs(jx - end.x) + Math.Abs(jy - end.y);

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

    private (int x, int y)? VectorJump(char[,] maze, int cx, int cy, int dx, int dy, (int x, int y) end)
    {
        int maxY = maze.GetLength(0) - 1;
        int maxX = maze.GetLength(1) - 1;

        while (true)
        {
            cx += dx;
            cy += dy;

            if (cx <= 0 || cx >= maxX || cy <= 0 || cy >= maxY) return null;

            if (maze[cy, cx] == '#') return null;

            if (cx == end.x && cy == end.y) return (cx, cy);

            if (dx != 0)
            {
                if ((maze[cy + 1, cx] != '#' && maze[cy + 1, cx - dx] == '#') ||
                    (maze[cy - 1, cx] != '#' && maze[cy - 1, cx - dx] == '#'))
                {
                    return (cx, cy);
                }
            }
            else
            {
                if ((maze[cy, cx + 1] != '#' && maze[cy - dy, cx + 1] == '#') ||
                    (maze[cy, cx - 1] != '#' && maze[cy - dy, cx - 1] == '#'))
                {
                    return (cx, cy);
                }
            }
        }
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