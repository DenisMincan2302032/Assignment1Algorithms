using System.Collections.Generic;
using System.Linq;

namespace Lab_Week_1_Friday
{
    using Grid = System.Int32[,];

    public class BreadthFirst : PathFinderInterface
    {
        public bool FindPath(Grid grid, Coordinate start, Coordinate goal, ref LinkedList<Coordinate> path)
        {
            // BFS uses a Queue (FIFO). Using C#'s built-in Queue<T>.
            var open = new Queue<Coordinate>();
            var closed = new HashSet<Coordinate>();
            var parent = new Dictionary<Coordinate, Coordinate>();
            int[] dRow = { -1, 1, 0, 0 };
            int[] dCol = { 0, 0, 1, -1 };

            open.Enqueue(start);

            while (open.Count > 0)
            {
                Coordinate current = open.Dequeue();

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
                        open.Enqueue(neighbor);
                        parent[neighbor] = current;
                    }
                }
            }
            return false;
        }
    }
}