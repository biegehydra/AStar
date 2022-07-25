using AStar.Models;

namespace AStar
{
    public class Program
    {
        public static int buffer = 1;
        private static Playground MakePlayground()
        {
            Console.Write("Enter Hegith of Playground: ");
            int heighofPlayground = int.Parse(Console.ReadLine());

            Console.Write("Enter Width of Playground: ");
            int widthofPlayground = int.Parse(Console.ReadLine());

            // Set Starting Node
            Console.Write("Enter starting node position seperated by , : ");
            string[] startingCoordinates = Console.ReadLine().Split(',');
            Coord startCords = new(int.Parse(startingCoordinates[0]) - buffer, int.Parse(startingCoordinates[1]) - buffer);
            var startNode = new Node(startCords);

            // Sets end node
            Console.Write("Enter end node position seperated by , :");
            string[] endCoordinates = Console.ReadLine().Split(',');
            Coord endCords = new(int.Parse(endCoordinates[0]) - buffer, int.Parse(endCoordinates[1]) - buffer);
            var endNode = new Node(endCords);

            Console.Write("Enter coordinates of walls seperated by spaces: ");
            string[] coordinatesOfWalls = Console.ReadLine().Split(' ');

            List<Coord> cordsofwalls = new List<Coord>();
            foreach (var coordinatesOfWall in coordinatesOfWalls)
            {
                var coords = coordinatesOfWall.Split(',');
                cordsofwalls.Add(new Coord(int.Parse(coords[0])-buffer, int.Parse(coords[1]) - buffer));
            }
            List<Node> walls = new List<Node>();
            cordsofwalls.ForEach(x => walls.Add(new Node(x, true)));

            Console.Write("Can move diagonally? Yes/No ");
            var test = Console.ReadLine().ToLower();
            bool canMoveDiagonally = test == "yes";
            return new Playground(heighofPlayground, widthofPlayground, startNode, endNode, walls, canMoveDiagonally);
        }
        public static void Main(string[] args)
        {
            var playground = MakePlayground();
            var path = playground.CalculateFastestPath();
            Console.WriteLine(path);
        }

    }
}