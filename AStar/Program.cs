using System.Security.Cryptography.X509Certificates;
using AStar.Models;

namespace AStar
{
    public class Program
    {
        public static int buffer = 1;
        private static Terrain MakeTerrainFromCommandLine()
        {
            Console.Write("Enter Heigth of Terrain: ");
            int heighofPlayground = int.Parse(Console.ReadLine());

            Console.Write("Enter Width of Terrain: ");
            int widthofPlayground = int.Parse(Console.ReadLine());

            // Set Starting Node
            Console.Write("Enter starting node position seperated by , : ");
            string[] startingCoordinates = Console.ReadLine().Split(',');
            Coord startCords = new(int.Parse(startingCoordinates[0]) - buffer, int.Parse(startingCoordinates[1]) - buffer);
            var startNode = new Node(startCords);

            // Sets end node
            Console.Write("Enter end node position seperated by , : ");
            string[] endCoordinates = Console.ReadLine().Split(',');
            Coord endCords = new(int.Parse(endCoordinates[0]) - buffer, int.Parse(endCoordinates[1]) - buffer);
            var endNode = new Node(endCords);

            Console.Write("Enter coordinates of walls seperated by spaces: ");
            string[] coordinatesOfWalls = Console.ReadLine().Split(' ');

            List<Coord> coordsofwalls = new List<Coord>();
            foreach (var coordinatesOfWall in coordinatesOfWalls)
            {
                var coords = coordinatesOfWall.Split(',');
                try
                {
                    coordsofwalls.Add(new Coord(int.Parse(coords[0]) - buffer, int.Parse(coords[1]) - buffer));
                }
                catch
                {
                    new Exception("Incorrect format");
                }
            }
            return new Terrain(heighofPlayground, widthofPlayground, startNode, endNode, coordsofwalls);
        }
        public static void Main(string[] args)
        {
            Console.WriteLine(Console.OutputEncoding);
            var playground = MakeTerrainFromCommandLine();
            var aStarPathFinder = new AStarPathFinder(playground);
            if (aStarPathFinder.CalculateFastestPath())
            {
                var pg = aStarPathFinder.CreateSolution();
                foreach (var s in pg)
                {
                    Console.WriteLine(s);
                }
            }
        }

    }
}