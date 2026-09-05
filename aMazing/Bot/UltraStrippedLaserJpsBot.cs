using System;
using System.Collections.Generic;

public class UltraStrippedLaserJpsBot : BaseMazeSolver
{
    public override string Name => "Ultra-Stripped Laser JPS Bot";

    private class JpsNode
    {
        public int X { get; }
        public int Y { get; }
        public int G { get; set; }
        public int H { get; }
        public int F => G + H; public JpsNode Parent { get; set; }
        public JpsNode(int x, int y, int g, int h, JpsNode parent) { X = x; Y = y; G = g; H = h; Parent = parent; }
    }

    private int[,] _visitedG;

    public override List<(int x, int y)> Solve(char[,] maze, (int x, int y) start, (int x, int y) end)
    {
        int height = maze.GetLength(0), width = maze.GetLength(1);

        if (_visitedG == null || _visitedG.GetLength(0) != height || _visitedG.GetLength(1) != width)
            _visitedG = new int[height, width];
        else
            Array.Clear(_visitedG, 0, _visitedG.Length);

        var queue = new PriorityQueue<JpsNode, int>();
        var startNode = new JpsNode(start.x, start.y, 0, Math.Abs(start.x - end.x) + Math.Abs(start.y - end.y), null);
        queue.Enqueue(startNode, startNode.F);

        int maxX = width - 1, maxY = height - 1;
        int[] dx = { 0, 0, -1, 1 }, dy = { -1, 1, 0, 0 };

        while (queue.Count > 0)
        {
            JpsNode curr = queue.Dequeue();
            if (_visitedG[curr.Y, curr.X] == -1) continue;
            _visitedG[curr.Y, curr.X] = -1;

            if (curr.X == end.x && curr.Y == end.y) return ReconstructPath(curr);

            for (int i = 0; i < 4; i++)
            {
                int x = curr.X, y = curr.Y, sx = dx[i], sy = dy[i];

                while (true)
                {
                    x += sx; y += sy;

                    if (x <= 0 || x >= maxX || y <= 0 || y >= maxY || maze[y, x] == '#') break;

                    if ((x == end.x && y == end.y) ||
                        (sx != 0 && ((maze[y + 1, x] != '#' && maze[y + 1, x - sx] == '#') || (maze[y - 1, x] != '#' && maze[y - 1, x - sx] == '#'))) ||
                        (sy != 0 && ((maze[y, x + 1] != '#' && maze[y - sy, x + 1] == '#') || (maze[y, x - 1] != '#' && maze[y - sy, x - 1] == '#'))))
                    {
                        if (_visitedG[y, x] == -1) break;

                        int newG = curr.G + (Math.Abs(curr.X - x) + Math.Abs(curr.Y - y));
                        int oldG = _visitedG[y, x];

                        if (oldG == 0 || newG < oldG)
                        {
                            _visitedG[y, x] = newG;
                            var node = new JpsNode(x, y, newG, Math.Abs(x - end.x) + Math.Abs(y - end.y), curr);
                            queue.Enqueue(node, node.F);
                        }
                        break;
                    }
                }
            }
            LeaveBreadcrumb(curr.X, curr.Y);
        }
        return null;
    }

    private List<(int x, int y)> ReconstructPath(JpsNode node)
    {
        var path = new List<(int, int)>();
        for (JpsNode t = node; t != null; t = t.Parent) path.Add((t.X, t.Y));
        path.Reverse();

        var fullPath = new List<(int, int)>();
        for (int i = 0; i < path.Count - 1; i++)
        {
            int x = path[i].Item1, y = path[i].Item2;
            while (x != path[i + 1].Item1 || y != path[i + 1].Item2)
            {
                fullPath.Add((x, y));
                x += Math.Sign(path[i + 1].Item1 - x); y += Math.Sign(path[i + 1].Item2 - y);
            }
        }
        fullPath.Add(path[path.Count - 1]);
        return fullPath;
    }
}