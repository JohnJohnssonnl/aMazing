using System;
using System.Collections.Generic;

public class MinimalistLaserJpsBot : BaseMazeSolver
{
    public override string Name => "Minimalist Laser JPS Bot";

    private int[] _g;
    private int[] _p;
    private int _sz = 0;

    public override List<(int x, int y)> Solve(char[,] maze, (int x, int y) start, (int x, int y) end)
    {
        int h = maze.GetLength(0), w = maze.GetLength(1), tot = w * h;
        int ex = end.x, ey = end.y, mx = w - 1, my = h - 1;

        if (_sz < tot) { _g = new int[tot]; _p = new int[tot]; _sz = tot; }
        Array.Fill(_g, int.MaxValue); Array.Clear(_p, 0, _p.Length);

        var q = new PriorityQueue<int, int>();

        int sId = (start.y * w) + start.x, eId = (ey * w) + ex;
        _g[sId] = 0; _p[sId] = -1;
        q.Enqueue(sId, (start.x > ex ? start.x - ex : ex - start.x) + (start.y > ey ? start.y - ey : ey - start.y));

        int[] dx = { 0, 0, -1, 1 }, dy = { -1, 1, 0, 0 };

        while (q.Count > 0)
        {
            int cId = q.Dequeue(), cx = cId % w, cy = cId / w, cg = _g[cId];
            if (cId == eId) break;

            for (int i = 0; i < 4; i++)
            {
                int x = cx, y = cy, sx = dx[i], sy = dy[i];

                while (true)
                {
                    x += sx; y += sy;

                    if (x <= 0 || x >= mx || y <= 0 || y >= my || maze[y, x] == '#') break;

                    if ((x == ex && y == ey) ||
                        (sx != 0 && ((maze[y + 1, x] != '#' && maze[y + 1, x - sx] == '#') || (maze[y - 1, x] != '#' && maze[y - 1, x - sx] == '#'))) ||
                        (sy != 0 && ((maze[y, x + 1] != '#' && maze[y - sy, x + 1] == '#') || (maze[y, x - 1] != '#' && maze[y - sy, x - 1] == '#'))))
                    {
                        int jId = (y * w) + x;
                        int nG = cg + (cx > x ? cx - x : x - cx) + (cy > y ? cy - y : y - cy);

                        if (nG < _g[jId])
                        {
                            _g[jId] = nG; _p[jId] = cId;
                            q.Enqueue(jId, nG + (x > ex ? x - ex : ex - x) + (y > ey ? y - ey : ey - y));
                        }
                        break;
                    }
                }
            }
        }
        return Reconstruct(sId, eId, w);
    }

    private List<(int x, int y)> Reconstruct(int sId, int eId, int w)
    {
        if (_g[eId] == int.MaxValue) return null;
        var jps = new List<(int, int)>();
        for (int c = eId; c != -1; c = _p[c]) jps.Add((c % w, c / w));
        jps.Reverse();

        var res = new List<(int, int)>();
        for (int i = 0; i < jps.Count - 1; i++)
        {
            int x = jps[i].Item1, y = jps[i].Item2, tx = jps[i + 1].Item1, ty = jps[i + 1].Item2;
            while (x != tx || y != ty) { res.Add((x, y)); x += Math.Sign(tx - x); y += Math.Sign(ty - y); }
        }
        res.Add(jps[jps.Count - 1]);
        return res;
    }
}