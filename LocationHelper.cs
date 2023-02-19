using CsvHelper;
using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchLocations
{
    public class LocationHelper
    {
        // Create a new Quadtree object
        Quadtree quadtree = new Quadtree(10, new Envelope(-180, -90, 180, 90));
      
        public List<Location> LoadDataFromCsv(string path)
        {
            List<SearchLocations.Location> locations = new List<SearchLocations.Location>();
            using (var reader = new StreamReader(path))
            {
                using (var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    if (csvReader != null)
                    {
                        csvReader.Context.RegisterClassMap<LocationDataMap>();
                        locations = csvReader.GetRecords<Location>().ToList();

                    }
                }
            }
            return locations;
        }

        public void InsertInBatches(List<Location> list)
        {
            // Insert the objects in batches of 1000
            int batchSize = 1000;
            for (int i = 0; i < list.Count; i += batchSize)
            {
                var batch = list.Skip(i).Take(batchSize);
                quadtree.InsertRange(batch);
            }
        }

        public List<Location> GetLocations(Location location, int maxDistance, int maxResults)
        {
            List<Location> locations = new List<Location>();

            // Create an envelope around the search location with the given max distance
            double halfDistance = maxDistance / 2.0;
            Envelope searchEnvelope = new Envelope(
                location.Latitude - halfDistance,
                location.Longitude - halfDistance,
                location.Latitude + halfDistance,
                location.Longitude + halfDistance);

            // Search the quadtree for objects within the search envelope
            foreach (Location obj in quadtree.Query(searchEnvelope))
            {
                // Calculate the distance between the search location and the object
                double distance = location.CalculateDistance(obj);

                // If the distance is less than the max distance, add the object to the results list
                if (distance <= maxDistance)
                {
                    locations.Add(obj);
                }

                // If the results list has reached the maximum number of results, break out of the loop
                if (locations.Count >= maxResults)
                {
                    break;
                }
            }

            return locations;
        }
    }
}
