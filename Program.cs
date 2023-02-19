using SearchLocations;

namespace ConsoleApplication1
{
    public class Program
    {

        static void Main(string[] args)
        {
            LocationHelper locHelper = new LocationHelper();
           
            // Read locations from the file
            List<Location> list = locHelper.LoadDataFromCsv(@"D:\Test.csv");

            //Insert into Quadtree in batches
            locHelper.InsertInBatches(list);

            // Search 
            Location loc = new Location(52.2165425, 5.4778534);
            var nearByLocations = locHelper.GetLocations(loc, 10, 10);
           

        }

    }
}