using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoAPI.Geometries;
using System.Runtime.CompilerServices;

namespace SearchLocations
{
    public class Quadtree
    {
        private const int MAX_OBJECTS = 10;
        private const int MAX_LEVELS = 5;

        private readonly int level;
        private readonly Envelope bounds;
        private readonly Quadtree[] nodes;
        private List<Location> locations = new List<Location>();
        public Quadtree(int level, Envelope bounds)
        {
            this.level = level;
            this.bounds = bounds;
            nodes = new Quadtree[4];
        }
       
        private void Split()
        {
            int subWidth = (int)(bounds.Width / 2);
            int subHeight = (int)(bounds.Height / 2);
            double x = bounds.MinX;
            double y = bounds.MinY;

            nodes[0] = new Quadtree(level + 1, new Envelope(x + subWidth, y, subWidth, subHeight));
            nodes[1] = new Quadtree(level + 1, new Envelope(x, y, subWidth, subHeight));
            nodes[2] = new Quadtree(level + 1, new Envelope(x, y + subHeight, subWidth, subHeight));
            nodes[3] = new Quadtree(level + 1, new Envelope(x + subWidth, y + subHeight, subWidth, subHeight));
        }

        private int GetIndex(Location obj)
        {
            Envelope objBounds = new Envelope(obj.Latitude, obj.Latitude, obj.Longitude, obj.Longitude);
            int index = -1;
            double verticalMidpoint = bounds.MinX + (bounds.Width / 2);
            double horizontalMidpoint = bounds.MinY + (bounds.Height / 2);

            // Object can completely fit within the top quadrants
            bool topQuadrant = (objBounds.MaxY < horizontalMidpoint && objBounds.MinY < horizontalMidpoint);
            // Object can completely fit within the bottom quadrants
            bool bottomQuadrant = (objBounds.MaxY >= horizontalMidpoint);

            // Object can completely fit within the left quadrants
            if (objBounds.MaxX < verticalMidpoint && objBounds.MinX < verticalMidpoint)
            {
                if (topQuadrant)
                {
                    index = 1;
                }
                else if (bottomQuadrant)
                {
                    index = 2;
                }
            }
            // Object can completely fit within the right quadrants
            else if (objBounds.MaxX >= verticalMidpoint)
            {
                if (topQuadrant)
                {
                    index = 0;
                }
                else if (bottomQuadrant)
                {
                    index = 3;
                }
            }

            return index;
        }

        public void InsertRange(IEnumerable<Location> objects)
        {
            foreach (Location obj in objects)
            {
                Insert(obj);
            }
        }

        public void Insert(Location obj)
        {
            if (nodes[0] != null)
            {
                int index = GetIndex(obj);
                if (index != -1)
                {
                    nodes[index].Insert(obj);
                    return;
                }
            }

            locations.Add(obj);

            if (locations.Count > MAX_OBJECTS && level < MAX_LEVELS)
            {
                Split();
            }
        }

        public List<Location> Query(Envelope range)
        {
            List<Location> results = new List<Location>();
            if (!bounds.Intersects(range))
            {
                return results;
            }

            foreach (Location obj in locations)
            {
                if (range.Contains(new Coordinate(obj.Latitude, obj.Longitude)))
                {
                    results.Add(obj);
                }
            }

            if (nodes[0] != null)
            {
                results.AddRange(nodes[0].Query(range));
                results.AddRange(nodes[1].Query(range));
                results.AddRange(nodes[2].Query(range));
                results.AddRange(nodes[3].Query(range));
            }

            return results;
        }

    }

}
