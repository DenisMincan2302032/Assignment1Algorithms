using System.Collections.Generic;

namespace Lab_Week_1_Friday
{
    using Grid = System.Int32[,];

    public class DepthFirst : PathFinderInterface
    {
       
        public bool FindPath(Grid grid, Coordinate start, Coordinate goal, ref LinkedList<Coordinate> path)
        {
            // DFS uses a Stack (LIFO). 
            var open = new LinkedList<Coordinate>();
            var closed = new HashSet<Coordinate>();
            var parent = new Dictionary<Coordinate, Coordinate>();
            int[] dRow = { -1, 1, 0, 0 }; // N, S, E, W
            int[] dCol = { 0, 0, 1, -1 };

            open.pushFront(start);

            while (open.Count() > 0)
            {
                Coordinate current = new Coordinate();
                open.GetFront(ref current);
                open.popFront();

                if (current == goal)
                {
                    path = new LinkedList<Coordinate>();
                    Coordinate step = current;
                    while (parent.ContainsKey(step))
                    {
                        path.pushFront(step);
                        step = parent[step];
                    }
                    return true;
                }

                closed.Add(current);

                for (int i = 0; i < 4; i++)
                {
                    Coordinate neighbor = new Coordinate(current.Row + dRow[i], current.Col + dCol[i]);

                    if (neighbor.Row < 0 || neighbor.Row >= grid.GetLength(0) ||
                        neighbor.Col < 0 || neighbor.Col >= grid.GetLength(1) ||
                        grid[neighbor.Row, neighbor.Col] != 0 ||
                        closed.Contains(neighbor))
                    {
                        continue;
                    }

                    if (!parent.ContainsKey(neighbor))
                    {
                        open.pushFront(neighbor);
                        parent[neighbor] = current;
                    }
                }
            }
            return false;
        }
    }
}