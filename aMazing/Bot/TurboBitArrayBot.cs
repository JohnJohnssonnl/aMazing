using System;
using System.Collections.Generic;

public class TurboBitArrayBot : BaseMazeSolver
{
    public override string Name => "Turbo Bit-Array Bot";

    private bool[] _visited;
    private int[] _parents;
    private int[] _queueBuffer;
    private int _currentBufferSize = 0;

    public override List<(int x, int y)> Solve(char[,] maze, (int x, int y) start, (int x, int y) end)
    {
        int height = maze.GetLength(0);
        int width = maze.GetLength(1);
        int totalCells = width * height;

        if (_currentBufferSize < totalCells)
        {
            _visited = new bool[totalCells];
            _parents = new int[totalCells];
            _queueBuffer = new int[totalCells];
            _currentBufferSize = totalCells;
        }
        else
        {
            Array.Clear(_visited, 0, _visited.Length);
            Array.Fill(_parents, -1);
        }

        int startId = (start.y * width) + start.x;
        int endId = (end.y * width) + end.x;

        int head = 0;
        int tail = 0;

        _queueBuffer[tail++] = startId;
        _visited[startId] = true;
        _parents[startId] = -1;

        int maxX = width - 1;
        int maxY = height - 1;

        bool found = false;

        while (head < tail)
        {
            int currentId = _queueBuffer[head++];

            if (currentId == endId)
            {
                found = true;
                break;
            }

            int cx = currentId % width;
            int cy = currentId / width;

            if (cy > 0)
            {
                int ny = cy - 1; int nId = (ny * width) + cx;
                if (maze[ny, cx] != '#' && !_visited[nId]) { _visited[nId] = true; _parents[nId] = currentId; _queueBuffer[tail++] = nId; }
            }
            if (cy < maxY)
            {
                int ny = cy + 1; int nId = (ny * width) + cx;
                if (maze[ny, cx] != '#' && !_visited[nId]) { _visited[nId] = true; _parents[nId] = currentId; _queueBuffer[tail++] = nId; }
            }
            if (cx > 0)
            {
                int nx = cx - 1; int nId = (cy * width) + nx;
                if (maze[cy, nx] != '#' && !_visited[nId]) { _visited[nId] = true; _parents[nId] = currentId; _queueBuffer[tail++] = nId; }
            }
            if (cx < maxX)
            {
                int nx = cx + 1; int nId = (cy * width) + nx;
                if (maze[cy, nx] != '#' && !_visited[nId]) { _visited[nId] = true; _parents[nId] = currentId; _queueBuffer[tail++] = nId; }
            }
        }

        if (!found) return null;

        var path = new List<(int x, int y)>();
        int curr = endId;
        while (curr != -1)
        {
            path.Add((curr % width, curr / width));
            curr = _parents[curr];
        }
        path.Reverse();
        return path;
    }
}