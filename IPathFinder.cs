using System.Collections.Generic;

namespace Lab_Week_1_Friday
{
    // Alias for the map data type
    using Grid = System.Int32[,];

    public interface PathFinderInterface
    {
       
        bool FindPath(Grid grid, Coordinate start, Coordinate goal, ref LinkedList<Coordinate> path);
    }
}