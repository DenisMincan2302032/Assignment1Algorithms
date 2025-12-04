using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab_Week_1_Friday
{
    public enum Algorithm
    {
        DepthFirst,
        BreadthFirst,
        HillClimbing
    }

    public static class PathFinderFactory
    {
        public static PathFinderInterface NewPathFinder(Algorithm algo)
        {
            switch (algo)
            {
                case Algorithm.DepthFirst:
                    return new DepthFirst();
                case Algorithm.HillClimbing:
                    return new HillClimbing();
                case Algorithm.BreadthFirst:
                    return new BreadthFirst();
                default:
                    throw new ArgumentException("Invalid pathfinding algorithm specified.");
            }
        }
    }
}