public class AStarBot : BaseMazeSolver
{
    public override string Name => "A* (A-Star) Search Bot";

    private class AStarNode
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int G { get; set; }
        public int H { get; set; }
        public int F => G + H;
        public AStarNode Parent { get; set; }

        public AStarNode(int x, int y, int g, int h, AStarNode parent)
        {
            X = x; Y = y; G = g; H = h; Parent = parent;
        }
    }

    public override List<(int x, int y)> Solve(char[,] maze, (int x, int y) start, (int x, int y) end)
    {
        List<AStarNode> openList = new List<AStarNode>();
        HashSet<(int, int)> closedList = new HashSet<(int, int)>();

        int startH = Math.Abs(start.x - end.x) + Math.Abs(start.y - end.y);
        openList.Add(new AStarNode(start.x, start.y, 0, startH, null));

        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { -1, 1, 0, 0 };

        while (openList.Count > 0)
        {
            AStarNode currentNode = openList.OrderBy(n => n.F).ThenBy(n => n.H).First();

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

            openList.Remove(currentNode);
            closedList.Add((currentNode.X, currentNode.Y));

            for (int i = 0; i < 4; i++)
            {
                int nx = currentNode.X + dx[i];
                int ny = currentNode.Y + dy[i];

                if (!IsValidMove(maze, nx, ny) || closedList.Contains((nx, ny)))
                    continue;

                int newG = currentNode.G + 1;
                int newH = Math.Abs(nx - end.x) + Math.Abs(ny - end.y);

                AStarNode existingNode = openList.FirstOrDefault(n => n.X == nx && n.Y == ny);

                if (existingNode == null)
                {
                    openList.Add(new AStarNode(nx, ny, newG, newH, currentNode));
                }
                else if (newG < existingNode.G)
                {
                    existingNode.G = newG;
                    existingNode.Parent = currentNode;
                }
            }

            LeaveBreadcrumb(currentNode.X, currentNode.Y);
        }

        return null;
    }
}