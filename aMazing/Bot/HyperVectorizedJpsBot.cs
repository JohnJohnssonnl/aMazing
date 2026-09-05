using System;
using System.Collections.Generic;

public class HyperVectorizedJpsBot : BaseMazeSolver
{
    public override string Name => "Hyper-Vectorized JPS Bot";

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

    private int[,] _openG;
    private JpsNode[,] _nodeTracker;
    private bool[,] _closed;

    public override List<(int x, int y)> Solve(char[,] maze, (int x, int y) start, (int x, int y) end)
    {
        int height = maze.GetLength(0);
        int width = maze.GetLength(1);

        if (_openG == null || _openG.GetLength(0) != height || _openG.GetLength(1) != width)
        {
            _openG = new int[height, width];
            _nodeTracker = new JpsNode[height, width];
            _closed = new bool[height, width];
        }
        else
        {
            Array.Clear(_openG, 0, _openG.Length);
            Array.Clear(_nodeTracker, 0, _nodeTracker.Length);
            Array.Clear(_closed, 0, _closed.Length);
        }

        var openQueue = new PriorityQueue<JpsNode, int>();

        int startH = Math.Abs(start.x - end.x) + Math.Abs(start.y - end.y);
        var startNode = new JpsNode(start.x, start.y, 0, startH, null);

        openQueue.Enqueue(startNode, startNode.F);
        _nodeTracker[start.y, start.x] = startNode;

        int maxX = width - 1;
        int maxY = height - 1;

        while (openQueue.Count > 0)
        {
            JpsNode currentNode = openQueue.Dequeue();
            int cx = currentNode.X;
            int cy = currentNode.Y;

            if (_closed[cy, cx]) continue;

            if (cx == end.x && cy == end.y)
            {
                return ReconstructPath(currentNode);
            }

            _closed[cy, cx] = true;

            EvaluateDirection(maze, currentNode, 0, -1, end, maxX, maxY, openQueue);
            EvaluateDirection(maze, currentNode, 0, 1, end, maxX, maxY, openQueue);
            EvaluateDirection(maze, currentNode, -1, 0, end, maxX, maxY, openQueue);
            EvaluateDirection(maze, currentNode, 1, 0, end, maxX, maxY, openQueue);

            LeaveBreadcrumb(cx, cy);
        }

        return null;
    }

    private void EvaluateDirection(char[,] maze, JpsNode currentNode, int dx, int dy, (int x, int y) end,
        int maxX, int maxY, PriorityQueue<JpsNode, int> openQueue)
    {
        var jumpPoint = VectorJump(maze, currentNode.X, currentNode.Y, dx, dy, end, maxX, maxY);

        if (jumpPoint != null)
        {
            int jx = jumpPoint.Value.x;
            int jy = jumpPoint.Value.y;

            if (_closed[jy, jx]) return;

            int newG = currentNode.G + (Math.Abs(currentNode.X - jx) + Math.Abs(currentNode.Y - jy));
            int existingG = _openG[jy, jx];

            if (existingG == 0 && _nodeTracker[jy, jx] == null)
            {
                int newH = Math.Abs(jx - end.x) + Math.Abs(jy - end.y);
                var neighborNode = new JpsNode(jx, jy, newG, newH, currentNode);

                _openG[jy, jx] = newG;
                _nodeTracker[jy, jx] = neighborNode;
                openQueue.Enqueue(neighborNode, neighborNode.F);
            }
            else if (newG < existingG)
            {
                var existingNode = _nodeTracker[jy, jx];
                existingNode.G = newG;
                existingNode.Parent = currentNode;

                _openG[jy, jx] = newG;
                openQueue.Enqueue(existingNode, existingNode.F);
            }
        }
    }

    private (int x, int y)? VectorJump(char[,] maze, int cx, int cy, int dx, int dy, (int x, int y) end, int maxX, int maxY)
    {
        while (true)
        {
            cx += dx;
            cy += dy;

            if (cx <= 0 || cx >= maxX || cy <= 0 || cy >= maxY || maze[cy, cx] == '#')
                return null;

            if (cx == end.x && cy == end.y)
                return (cx, cy);

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