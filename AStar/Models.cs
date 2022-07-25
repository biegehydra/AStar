using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace AStar.Models
{
    public record Coord(int xvalue, int yvalue);
    class Playground
    {
        private static int Buffer = 1;
        private int _height;
        private int _width;
        private bool _canMoveDiagonally;
        public Node[,] Layout { get; set; }
        public Node EndNode { get; set; }
        public Node StartNode { get; set; }
        public List<Node> Walls { get; set; } = new List<Node>();
        public List<Node> VisitableNodes { get; set; } = new List<Node>();
        public Playground(int heightoflayout, int widthoflayout, Node start, Node end, List<Node> walls,
            bool canMoveDiagonally)
        {
            Layout = new Node[heightoflayout, widthoflayout];
            _height = heightoflayout;
            _width = widthoflayout;
            StartNode = start;
            EndNode = end;
            Walls = walls;
            _canMoveDiagonally = canMoveDiagonally;
            GeneratePlayground();
        }

        private void GeneratePlayground()
        {
            for (int xcord = 0; xcord < _width; xcord++)
            {
                for (int ycord = 0; ycord < _height; ycord++)
                {
                    var coord = new Coord(xcord, ycord);
                    var node = new Node(coord);
                    if(CoordIsWall(coord))
                    {
                        node.IsWall = true;
                    };
                    Layout[xcord,ycord] = node;
                }
            }
        }
        private bool CoordIsWall(Coord coord)
        {
            return Walls.Any(x => x.Coord == coord);
        }
        public string CalculateFastestPath()
        {
            var currentNode = StartNode;
            do
            {
                DetermineNeighbors(currentNode);
                currentNode.NeighboringNodes.ForEach(AddToVisitableNodes);
                CalculateNeighborCosts(currentNode);
                if (currentNode.Coord != StartNode.Coord)
                {
                    VisitableNodes.Remove(currentNode);
                }
                var nextNode = VisitableNodes.OrderBy(x => x.TotalCost).ThenBy(x => x.HCost).First();
                nextNode.Parent = currentNode;
                currentNode = nextNode;
            } while (currentNode.Coord != EndNode.Coord);
            return PathToStart(currentNode);
        }
        private void DetermineNeighbors(Node node)
        {
            var coords = node.Coord;
            bool canLeft = coords.xvalue - 1 >= 0;
            bool canRight = coords.xvalue + 1 <= _width - Buffer;
            bool canUp = coords.yvalue - 1 >= 0;
            bool canDown = coords.yvalue + 1 <= _height - Buffer;
            if (canUp)
                node.NeighboringNodes.Add(Layout[coords.xvalue, coords.yvalue - 1]);
            if (canDown)
                node.NeighboringNodes.Add(Layout[coords.xvalue, coords.yvalue + 1]);
            if (canLeft)
                node.NeighboringNodes.Add(Layout[coords.xvalue -1, coords.yvalue]);
            if (canRight)
                node.NeighboringNodes.Add(Layout[coords.xvalue +1, coords.yvalue]);
            if (!_canMoveDiagonally) return;
            if(canUp && canRight)
                node.NeighboringNodes.Add(Layout[coords.xvalue + 1, coords.yvalue - 1]);
            if(canUp && canLeft)
                node.NeighboringNodes.Add(Layout[coords.xvalue - 1, coords.yvalue - 1]);
            if(canDown && canRight)
                node.NeighboringNodes.Add(Layout[coords.xvalue + 1, coords.yvalue + 1]);
            if(canDown && canLeft)
                node.NeighboringNodes.Add(Layout[coords.xvalue -1, coords.yvalue - 1]);
        }
        private void AddToVisitableNodes(Node node)
        {
            if (node.NeighboringNodes.Count > 0) return;
            if (CoordIsWall(node.Coord)) return;
            if (VisitableNodes.Contains(node)) return;
            if (node.Coord == StartNode.Coord) return;
            VisitableNodes.Add(node);
        }
        private void CalculateNeighborCosts(Node nod)
        {
            foreach (var node in nod.NeighboringNodes)
            {
                node.GCost = CalculateDistance(node.Coord, StartNode.Coord);
                node.HCost = CalculateDistance(node.Coord, EndNode.Coord);
            }
        }
        private float CalculateDistance(Coord coord1, Coord coord2)
        {
            float xDistance = coord2.xvalue - coord1.xvalue;
            float yDistance = coord2.yvalue - coord1.yvalue;
            return (float) Math.Sqrt((xDistance * xDistance) + (yDistance * yDistance));
        }
        private string PathToStart(Node node)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{node.Coord.xvalue},{node.Coord.yvalue}");
            if (node.Coord == StartNode.Coord)
            {
                return sb.ToString();
            }
            return sb.Append(" => " + PathToStart(node.Parent)).ToString();
        }
    }
    internal class Node
    {
        // Distance from starting node
        public float GCost { get; set; }
        // Distance From the end node
        public float HCost { get; set; }
        public bool IsWall { get; set; }
        public Coord Coord { get; init; }
        public float TotalCost
        {
            get
            {
                if (GCost + HCost < 0)
                {
                    return int.MaxValue;
                }
                else
                {
                    return GCost + HCost;
                }
            }
        }
        public Node Parent { get; set; }
        public List<Node> NeighboringNodes { get; set; } = new List<Node>();
        public static int CompareDistanceToEndNode(Node nodea, Node nodeb)
        {
            if (nodea.TotalCost == nodeb.TotalCost)
            {
                return nodea.HCost.CompareTo(nodeb.HCost);
            }
            return nodea.TotalCost.CompareTo(nodeb.TotalCost);
        }
        public Node(Coord coord, bool isWall = false)
        {
            Coord = coord;
        }
    }
}