using System.Collections;
using System.ComponentModel.Design;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AStar.Models
{
    public record Coord(int x, int y);
    internal class AStarPathFinder
    {
        private static int Buffer = 1;
        private List<Node> _visitableNodes { get; set; } = new ();
        private List<Node> _visitedNodes { get; set; } = new();
        private Terrain _terrain;
        public AStarPathFinder(Terrain terrain)
        {
            _terrain = terrain;
        }
        public bool CalculateFastestPath()
        {
            var currentNode = _terrain.StartNode;
            DetermineVisitableNodes();
            do
            {
                CalculateNeighborCosts(currentNode);
                DetermineNextNode();
                DetermineVisitableNodes();
                if (_visitableNodes.Count == 0)
                {
                    Console.WriteLine("No solution possible");
                    return false;
                }
            } while (currentNode.Coord != _terrain.EndNode.Coord);
            return true;

            void DetermineNextNode()
            {
                var nextNode = _visitableNodes.OrderBy(x => x.TotalCost).ThenBy(x => x.HCost).First();
                currentNode.HasBeenVisited = true;
                _visitedNodes.Add(currentNode);
                _visitableNodes.Remove(currentNode);
                nextNode.Parent = _visitedNodes.Where(node => nextNode.AreNeighbors(node)).OrderBy(x => x.GCost).First();
                currentNode = nextNode;
            }
            void DetermineVisitableNodes()
            {
                DetermineNodeNeighbors(currentNode);
                currentNode.NeighboringNodes.ForEach(AddToVisitableNodes);
            }
        }
        private void DetermineNodeNeighbors(Node node)
        {
            var coords = node.Coord;
            bool canLeft = coords.x - 1 >= 0;
            bool canRight = coords.x + 1 <= _terrain.Width - Buffer;
            bool canUp = coords.y - 1 >= 0;
            bool canDown = coords.y + 1 <= _terrain.Height - Buffer;
            if (canUp)
                node.NeighboringNodes.Add(_terrain.Layout[coords.x, coords.y - 1]);
            if (canDown)
                node.NeighboringNodes.Add(_terrain.Layout[coords.x, coords.y + 1]);
            if (canLeft)
                node.NeighboringNodes.Add(_terrain.Layout[coords.x - 1, coords.y]);
            if (canRight)
                node.NeighboringNodes.Add(_terrain.Layout[coords.x + 1, coords.y]);
            node.NeighboringNodes = node.NeighboringNodes.Where(x => !x.IsWall).ToList();
        }
        private void AddToVisitableNodes(Node node)
        {
            if (node.HasBeenVisited) return;
            if (node.IsWall) return;
            if (_visitableNodes.Contains(node)) return;
            if (node.Coord == _terrain.StartNode.Coord) return;
            _visitableNodes.Add(node);
        }
        private void CalculateNeighborCosts(Node nodea)
        {
            foreach (var nodeb in nodea.NeighboringNodes)
            {
                if (nodeb.Coord == _terrain.StartNode.Coord)
                {
                    nodeb.GCost = 0;
                    nodeb.HCost = nodeb.CalculateDistanceTo(_terrain.EndNode);
                    continue;
                }
                nodeb.GCost = nodea.GCost + nodea.CalculateDistanceTo(nodeb);
                nodeb.HCost = nodeb.CalculateDistanceTo(_terrain.EndNode);
            }
        }
        private List<Direction> PathToEnd()
        {
            var nodesToFinish = NodesToEndRecursive(_terrain.EndNode);
            var path = new List<Direction>();
            for (int i = 0; i < nodesToFinish.Count - 1; i++)
            {
                if(i + 1 > nodesToFinish.Count) continue;
                path.Add(nodesToFinish[i].GetDirectionTo(nodesToFinish[i + 1]));
            }
            return path;

            List<Node> NodesToEndRecursive(Node node)
            {
                if (node.Coord == _terrain.StartNode.Coord)
                {
                    return new List<Node> { node };
                }
                if (node.NeighboringNodes.Count == 0)
                {
                    DetermineNodeNeighbors(node);
                }
                var parentPath = NodesToEndRecursive(node.Parent);
                parentPath.Add(node);
                return parentPath;
            }
        }
        public string[] CreatePlayground()
        {
            var pg = new string[_terrain.Height];
            for (int ycord = 0; ycord < _terrain.Width; ycord++)
            {
                var sb = new StringBuilder();
                for (int xcord = 0; xcord < _terrain.Height; xcord++)
                {
                    var coord = new Coord(xcord , ycord);
                    if (coord == _terrain.StartNode.Coord)
                    {
                        sb.Append("S ");
                    }
                    else if (coord == _terrain.EndNode.Coord)
                    {
                        sb.Append("E ");
                    }
                    else if (_terrain.Layout[xcord, ycord].IsWall)
                    {
                        sb.Append("W ");
                    }
                    else
                    {
                        sb.Append("X ");
                    }
                }
                pg[ycord] = sb.ToString();
            }
            return pg;
        }
        public string[] CreateSolution()
        {
            var pathToEnd = PathToEnd();
            var playground = CreatePlayground();
            var solution = CreateBlankSolution();
            var currentCoord = _terrain.EndNode.Coord;
            while (pathToEnd.Count > 0)
            {
                var directonToMove = pathToEnd[pathToEnd.Count-1];
                var doubleCurrentCord = new Coord(currentCoord.x * 2, currentCoord.y * 2);
                var newCoord = doubleCurrentCord.MoveTo(directonToMove);
                solution[newCoord.y] = solution[newCoord.y].Remove(newCoord.x, 1).Insert(newCoord.x, GetSymbol(directonToMove));
                pathToEnd.RemoveAt(pathToEnd.Count - 1);
                currentCoord = currentCoord.MoveTo(directonToMove);
            }
            return solution;

            string[] CreateBlankSolution()
            {
                var solution = new string[(playground.Length * 2) - 1];
                for (int i = 0; i < solution.Length; i++)
                {
                    if (i % 2 == 0)
                    {
                        solution[i] = playground[i / 2];
                    }
                    else
                    {
                        solution[i] = string.Concat(Enumerable.Repeat(" ", _terrain.Width * 2));
                    }
                }
                return solution;
            }
        }
        public static string GetSymbol(Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    return "^";
                case Direction.South:
                    return "v";
                case Direction.West:
                    return ">";
                case Direction.East:
                    return "<";
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
    }
    internal class Terrain
    {
        public int Height;
        public int Width;
        public Node[,] Layout { get; set; }
        public Node EndNode { get; set; }
        public Node StartNode { get; set; }
        public List<Coord> WallCoords { get; set; } = new List<Coord>();
        public Terrain(int heightoflayout, int widthoflayout, Node start, Node end, List<Coord> wallCoords)
        {
            Layout = new Node[heightoflayout, widthoflayout];
            Height = heightoflayout;
            Width = widthoflayout;
            StartNode = start;
            EndNode = end;
            WallCoords = wallCoords;
            GeneratePlayground();
        }
        private void GeneratePlayground()
        {
            for (int ycord = 0; ycord < Width; ycord++)
            {
                for (int xcord = 0; xcord < Height; xcord++)
                {
                    var coord = new Coord(xcord, ycord);
                    var node = new Node(coord);
                    if(CoordIsWall(coord))
                    {
                        node.IsWall = true;
                    }
                    else if (coord == EndNode.Coord)
                    {
                        node = EndNode;
                    }
                    else if (coord == StartNode.Coord)
                    {
                        node = StartNode;
                    }
                    Layout[xcord,ycord] = node;
                }
            }
            bool CoordIsWall(Coord coord)
        {
            return WallCoords.Any(x => x == coord);
        }
        }
    }
    internal class Node
    {
        // Distance from starting node
        public float GCost { get; set; }
        // Distance From the end node
        public float HCost { get; set; }
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
        public bool IsWall { get; set; }
        public bool HasBeenVisited { get; set; }
        public Coord Coord { get; init; }
        public Node Parent { get; set; }
        public List<Node> NeighboringNodes { get; set; } = new List<Node>();

        public bool AreNeighbors(Node node)
        {
            if (node.Coord == Coord) return false;
            return (MathF.Abs(Coord.x - node.Coord.x) <= 1 && Coord.y - node.Coord.y == 0) || MathF.Abs(Coord.y - node.Coord.y) <= 1 && Coord.x - node.Coord.x == 0;
        }
        public float CalculateDistanceTo(Node node)
        {
            float xDistance = MathF.Abs(Coord.x - node.Coord.x);
            float yDistance = MathF.Abs(Coord.y - node.Coord.y);
            return xDistance + yDistance;
        }
        public Direction GetDirectionTo(Node node)
        {
            if (Coord.y - node.Coord.y >= 1)
            {
                return Direction.North;
            }
            else if (Coord.y - node.Coord.y <= -1)
            {
                return Direction.South;
            }
            else
            {
                if (Coord.x - node.Coord.x >= 1)
                {
                    return Direction.East;
                }
                else if (Coord.x - node.Coord.x <= -1)
                {
                    return Direction.West;
                }
            }
            return default;
        }
        public Node(Coord coord, bool isWall = false)
        {
            Coord = coord;
            IsWall = isWall;
        }
    }
    public enum Direction
    {
        Null,
        North,
        South,
        West,
        East
    }

    static class ExtensionMethods
    {
        public static Coord MoveTo(this Coord startingCoord, Direction directionToMove)
        {
            switch (directionToMove)
            {
                case Direction.East:
                    return new Coord(startingCoord.x + 1, startingCoord.y);
                case Direction.West:
                    return new Coord(startingCoord.x - 1, startingCoord.y);
                case Direction.South:
                    return new Coord(startingCoord.x, startingCoord.y - 1);
                case Direction.North:
                    return new Coord(startingCoord.x, startingCoord.y + 1);
                default: return default;
            }
        }
    }
}