using System;
using System.Collections.Generic;

namespace Lab_Week_1_Friday
{
    using Grid = System.Int32[,];

    public class HillClimbing : PathFinderInterface
    {
        private int ManhattanDistance(Coordinate current, Coordinate target)
        {
            return Math.Abs(current.Row - target.Row) + Math.Abs(current.Col - target.Col);
        }

        
        public bool FindPath(Grid grid, Coordinate start, Coordinate goal, ref LinkedList<Coordinate> path)
        {
            Coordinate current = start;
            var pathList = new List<Coordinate>();
            var visited = new HashSet<Coordinate>();

            int[] dRow = { -1, 1, 0, 0 };
            int[] dCol = { 0, 0, 1, -1 };

            while (current != goal)
            {
                if (visited.Contains(current)) return false;

                visited.Add(current);
                if (current != start) pathList.Add(current);

                Coordinate bestNeighbor = new Coordinate(-1, -1);
                int currentDistance = ManhattanDistance(current, goal);
                int bestDistance = currentDistance;

                for (int i = 0; i < 4; i++)
                {
                    Coordinate neighbor = new Coordinate(current.Row + dRow[i], current.Col + dCol[i]);

                    if (neighbor.Row < 0 || neighbor.Row >= grid.GetLength(0) ||
                        neighbor.Col < 0 || neighbor.Col >= grid.GetLength(1) ||
                        grid[neighbor.Row, neighbor.Col] != 0)
                    {
                        continue;
                    }

                    int distance = ManhattanDistance(neighbor, goal);

                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestNeighbor = neighbor;
                    }
                }

                if (bestNeighbor.Row == -1)
                {
                    return false;
                }

                current = bestNeighbor;
            }

            pathList.Add(goal);

            path = new LinkedList<Coordinate>();
            foreach (var coord in pathList)
            {
                path.pushFront(coord);
            }

            return true;
        }
    }
}