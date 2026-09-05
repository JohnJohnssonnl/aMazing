using System;
using System.Collections.Generic;
using System.Linq;

public class TopologicalGraphBot : BaseMazeSolver
{
    public override string Name => "Topological Junction Graph Bot";

    private class JunctionNode
    {
        public (int x, int y) Position { get; }
        public List<JunctionEdge> Edges { get; } = new List<JunctionEdge>();

        public JunctionNode(int x, int y)
        {
            Position = (x, y);
        }
    }

    private class JunctionEdge
    {
        public JunctionNode Target { get; }
        public int Distance { get; }
        public List<(int x, int y)> FullPath { get; }

        public JunctionEdge(JunctionNode target, int distance, List<(int x, int y)> fullPath)
        {
            Target = target;
            Distance = distance;
            FullPath = fullPath;
        }
    }

    private class SolverNode
    {
        public JunctionNode GraphNode { get; }
        public int G { get; set; }
        public int H { get; }
        public int F => G + H;
        public SolverNode Parent { get; set; }
        public JunctionEdge EdgeFromParent { get; set; }

        public SolverNode(JunctionNode graphNode, int g, int h, SolverNode parent, JunctionEdge edgeFromParent)
        {
            GraphNode = graphNode; G = g; H = h; Parent = parent; EdgeFromParent = edgeFromParent;
        }
    }

    public override List<(int x, int y)> Solve(char[,] maze, (int x, int y) start, (int x, int y) end)
    {
        var junctionMap = BuildTopologicalGraph(maze, start, end);

        if (!junctionMap.ContainsKey(start) || !junctionMap.ContainsKey(end)) return null;

        JunctionNode startNode = junctionMap[start];
        JunctionNode endNode = junctionMap[end];

        var openQueue = new PriorityQueue<SolverNode, int>();
        var openTracker = new Dictionary<JunctionNode, SolverNode>();
        var closedList = new HashSet<JunctionNode>();

        int startH = Math.Abs(start.x - end.x) + Math.Abs(start.y - end.y);
        var firstNode = new SolverNode(startNode, 0, startH, null, null);

        openQueue.Enqueue(firstNode, firstNode.F);
        openTracker.Add(startNode, firstNode);

        while (openQueue.Count > 0)
        {
            SolverNode current = openQueue.Dequeue();

            if (closedList.Contains(current.GraphNode)) continue;

            AnimateBot(current.GraphNode.Position.x, current.GraphNode.Position.y);

            if (current.GraphNode == endNode)
            {
                return ReconstructGraphPath(current);
            }

            closedList.Add(current.GraphNode);

            foreach (var edge in current.GraphNode.Edges)
            {
                JunctionNode neighbor = edge.Target;
                if (closedList.Contains(neighbor)) continue;

                int newG = current.G + edge.Distance;
                int newH = Math.Abs(neighbor.Position.x - end.x) + Math.Abs(neighbor.Position.y - end.y);

                if (!openTracker.TryGetValue(neighbor, out SolverNode existingNode))
                {
                    var neighborNode = new SolverNode(neighbor, newG, newH, current, edge);
                    openQueue.Enqueue(neighborNode, neighborNode.F);
                    openTracker.Add(neighbor, neighborNode);
                }
                else if (newG < existingNode.G)
                {
                    existingNode.G = newG;
                    existingNode.Parent = current;
                    existingNode.EdgeFromParent = edge;
                    openQueue.Enqueue(existingNode, existingNode.F);
                }
            }

            LeaveBreadcrumb(current.GraphNode.Position.x, current.GraphNode.Position.y);
        }

        return null;
    }

    private Dictionary<(int x, int y), JunctionNode> BuildTopologicalGraph(char[,] maze, (int x, int y) start, (int x, int y) end)
    {
        int height = maze.GetLength(0);
        int width = maze.GetLength(1);
        var junctionMap = new Dictionary<(int x, int y), JunctionNode>();

        for (int r = 1; r < height - 1; r++)
        {
            for (int c = 1; c < width - 1; c++)
            {
                if (maze[r, c] == '#') continue;

                bool isStartOrEnd = (c == start.x && r == start.y) || (c == end.x && r == end.y);

                int openNeighbors = 0;
                if (maze[r - 1, c] != '#') openNeighbors++;
                if (maze[r + 1, c] != '#') openNeighbors++;
                if (maze[r, c - 1] != '#') openNeighbors++;
                if (maze[r, c + 1] != '#') openNeighbors++;

                if (isStartOrEnd || openNeighbors > 2 || (openNeighbors == 1 && !isStartOrEnd))
                {
                    junctionMap[(c, r)] = new JunctionNode(c, r);
                }
            }
        }

        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { -1, 1, 0, 0 };

        foreach (var kvp in junctionMap)
        {
            var startPos = kvp.Key;
            var currentNode = kvp.Value;

            for (int i = 0; i < 4; i++)
            {
                int nx = startPos.x + dx[i];
                int ny = startPos.y + dy[i];

                if (!IsValidMove(maze, nx, ny)) continue;

                var edgePath = new List<(int x, int y)> { startPos };
                int cx = nx, cy = ny;
                int px = startPos.x, py = startPos.y;

                while (!junctionMap.ContainsKey((cx, cy)))
                {
                    edgePath.Add((cx, cy));

                    int nextX = cx, nextY = cy;
                    for (int j = 0; j < 4; j++)
                    {
                        int tx = cx + dx[j];
                        int ty = cy + dy[j];
                        if (IsValidMove(maze, tx, ty) && (tx != px || ty != py))
                        {
                            nextX = tx;
                            nextY = ty;
                            break;
                        }
                    }
                    px = cx; py = cy;
                    cx = nextX; cy = nextY;
                }

                var targetNode = junctionMap[(cx, cy)];

                if (!currentNode.Edges.Any(e => e.Target == targetNode))
                {
                    currentNode.Edges.Add(new JunctionEdge(targetNode, edgePath.Count, edgePath));
                }
            }
        }

        return junctionMap;
    }

    private List<(int x, int y)> ReconstructGraphPath(SolverNode finalSolverNode)
    {
        var segments = new List<List<(int x, int y)>>();
        SolverNode curr = finalSolverNode;

        while (curr != null && curr.EdgeFromParent != null)
        {
            segments.Add(curr.EdgeFromParent.FullPath);
            curr = curr.Parent;
        }
        segments.Reverse();

        var fullPath = new List<(int x, int y)>();
        foreach (var segment in segments)
        {
            fullPath.AddRange(segment);
        }

        if (finalSolverNode != null)
        {
            fullPath.Add(finalSolverNode.GraphNode.Position);
        }

        return fullPath.Distinct().ToList();
    }
}