using System;
using System.Collections.Generic;

public class SubMillisecondLaserJpsBot : BaseMazeSolver
{
    public override string Name => "Sub-Millisecond Laser JPS Bot";

    private class JpsNode
    {
        public int X { get; }
        public int Y { get; }
        public int G { get; set; }
        public int H { get; }
        public int F => G + H; public JpsNode Parent { get; set; }
        public JpsNode(int x, int y, int g, int h, JpsNode parent) { X = x; Y = y; G = g; H = h; Parent = parent; }
    }

    private int[,] _gScores;
    private bool[,] _closed;

    public override List<(int x, int y)> Solve(char[,] maze, (int x, int y) start, (int x, int y) end)
    {
        int height = maze.GetLength(0), width = maze.GetLength(1);
        int ex = end.x, ey = end.y;

        if (_gScores == null || _gScores.GetLength(0) != height || _gScores.GetLength(1) != width)
        {
            _gScores = new int[height, width];
            _closed = new bool[height, width];
        }
        else
        {
            Array.Clear(_gScores, 0, _gScores.Length);
            Array.Clear(_closed, 0, _closed.Length);
        }

        var openQueue = new PriorityQueue<JpsNode, int>();
        int startH = (start.x > ex ? start.x - ex : ex - start.x) + (start.y > ey ? start.y - ey : ey - start.y);
        var startNode = new JpsNode(start.x, start.y, 0, startH, null);
        openQueue.Enqueue(startNode, startNode.F);

        int maxX = width - 1, maxY = height - 1;
        int[] dx = { 0, 0, -1, 1 }, dy = { -1, 1, 0, 0 };

        while (openQueue.Count > 0)
        {
            JpsNode curr = openQueue.Dequeue();
            int cx = curr.X, cy = curr.Y;

            if (_closed[cy, cx]) continue;
            _closed[cy, cx] = true;

            if (cx == ex && cy == ey) return ReconstructPath(curr);

            for (int i = 0; i < 4; i++)
            {
                int x = cx, y = cy, stepX = dx[i], stepY = dy[i];

                while (true) 
                {
                    x += stepX; y += stepY;

                    if (x <= 0 || x >= maxX || y <= 0 || y >= maxY || maze[y, x] == '#') break;

                    if (x == ex && y == ey)
                    {
                        if (!_closed[y, x])
                        {
                            int newG = curr.G + (cx > x ? cx - x : x - cx) + (cy > y ? cy - y : y - cy);
                            if (_gScores[y, x] == 0 || newG < _gScores[y, x])
                            {
                                _gScores[y, x] = newG;
                                int newH = (x > ex ? x - ex : ex - x) + (y > ey ? y - ey : ey - y);
                                openQueue.Enqueue(new JpsNode(x, y, newG, newH, curr), newG + newH);
                            }
                        }
                        break;
                    }

                    if (stepX != 0)
                    {
                        if ((maze[y + 1, x] != '#' && maze[y + 1, x - stepX] == '#') || (maze[y - 1, x] != '#' && maze[y - 1, x - stepX] == '#'))
                        {
                            if (!_closed[y, x])
                            {
                                int newG = curr.G + (cx > x ? cx - x : x - cx) + (cy > y ? cy - y : y - cy);
                                if (_gScores[y, x] == 0 || newG < _gScores[y, x])
                                {
                                    _gScores[y, x] = newG;
                                    int newH = (x > ex ? x - ex : ex - x) + (y > ey ? y - ey : ey - y);
                                    openQueue.Enqueue(new JpsNode(x, y, newG, newH, curr), newG + newH);
                                }
                            }
                            break;
                        }
                    }
                    else
                    {
                        if ((maze[y, x + 1] != '#' && maze[y - stepY, x + 1] == '#') || (maze[y, x - 1] != '#' && maze[y - stepY, x - 1] == '#'))
                        {
                            if (!_closed[y, x])
                            {
                                int newG = curr.G + (cx > x ? cx - x : x - cx) + (cy > y ? cy - y : y - cy);
                                if (_gScores[y, x] == 0 || newG < _gScores[y, x])
                                {
                                    _gScores[y, x] = newG;
                                    int newH = (x > ex ? x - ex : ex - x) + (y > ey ? y - ey : ey - y);
                                    openQueue.Enqueue(new JpsNode(x, y, newG, newH, curr), newG + newH);
                                }
                            }
                            break;
                        }
                    }
                }
            }
            LeaveBreadcrumb(cx, cy);
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