using System;
using System.Collections.Generic;

public class BulletproofBitArrayBot : BaseMazeSolver
{
    public override string Name => "Bulletproof Bit-Array Bot";

    public override List<(int x, int y)> Solve(char[,] maze, (int x, int y) start, (int x, int y) end)
    {
        int height = maze.GetLength(0);
        int width = maze.GetLength(1);

        bool[,] visited = new bool[height, width];

        int[] parents = new int[height * width];
        Array.Fill(parents, -1);

        Queue<int> queue = new Queue<int>();

        int startId = (start.y * width) + start.x;
        int endId = (end.y * width) + end.x;

        queue.Enqueue(startId);
        visited[start.y, start.x] = true;

        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { -1, 1, 0, 0 };

        bool found = false;

        while (queue.Count > 0)
        {
            int currentId = queue.Dequeue();

            if (currentId == endId)
            {
                found = true;
                break;
            }

            int cx = currentId % width;
            int cy = currentId / width;

            for (int i = 0; i < 4; i++)
            {
                int nx = cx + dx[i];
                int ny = cy + dy[i];

                if (nx >= 0 && nx < width && ny >= 0 && ny < height && maze[ny, nx] != '#' && !visited[ny, nx])
                {
                    int neighborId = (ny * width) + nx;
                    visited[ny, nx] = true;
                    parents[neighborId] = currentId;
                    queue.Enqueue(neighborId);
                }
            }
        }

        if (!found) return null;

        var path = new List<(int x, int y)>();
        int curr = endId;
        while (curr != -1)
        {
            path.Add((curr % width, curr / width));
            curr = parents[curr];
        }
        path.Reverse();
        return path;
    }
}