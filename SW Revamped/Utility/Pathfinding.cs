using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.SDK.Tools;
using SharpDX;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWRevamped.Utility
{
    public class PathfindingNode
    {
        public GameObjectBase Target { get; set; }
        public List<PathfindingNode> Neighbors { get; set; }
        public float GScore { get; set; }
        public float FScore { get; set; }
        public PathfindingNode CameFrom { get; set; }

        public PathfindingNode(GameObjectBase target)
        {
            Target = target;
            Neighbors = new List<PathfindingNode>();
            GScore = float.MaxValue;
            FScore = float.MaxValue;
            CameFrom = null;
        }
    }


    public class Pathfinding
    {
        public List<GameObjectBase> FindShortestPathOutOfTower(List<GameObjectBase> gameObjects, GameObjectBase start, int maxRange = 600)
        {
            PriorityQueue<Node> openNodes = new PriorityQueue<Node>();
            openNodes.Enqueue(new Node(start, 0, 0, null));
            Dictionary<GameObjectBase, int> visited = new Dictionary<GameObjectBase, int>();
            visited[start] = 0;
            while (openNodes.Count > 0)
            {
                var currentNode = openNodes.Dequeue();
                if (!General.InNexusRange(currentNode.GameObject) && !General.InTowerRange(currentNode.GameObject))
                {
                    return ConstructPath(currentNode);
                }

                foreach (var neighbor in GetNeighbors(currentNode.GameObject, maxRange, gameObjects))
                {
                    var newCost = currentNode.Cost + 1;

                    if (!visited.ContainsKey(neighbor) || newCost < visited[neighbor])
                    {
                        visited[neighbor] = newCost;
                        var priority = newCost + Heuristic(neighbor, currentNode.GameObject);
                        openNodes.Enqueue(new Node(neighbor, newCost, priority, currentNode));
                    }
                }
            }

            // No path found
            return null;
        }

        public List<GameObjectBase> FindShortestPathToVector(List<GameObjectBase> gameObjects, GameObjectBase start, Vector3 end, int maxRange = 600, int tolerance = 50)
        {
            PriorityQueue<Node> openNodes = new PriorityQueue<Node>();
            openNodes.Enqueue(new Node(start, 0, 0, null));
            Dictionary<GameObjectBase, int> visited = new Dictionary<GameObjectBase, int>();
            visited[start] = 0;
            while (openNodes.Count > 0)
            {
                var currentNode = openNodes.Dequeue();
                if (currentNode.GameObject.Position.Distance(end) < tolerance)
                {
                    return ConstructPath(currentNode);
                }

                foreach (var neighbor in GetNeighbors(currentNode.GameObject, maxRange, gameObjects))
                {
                    var newCost = currentNode.Cost + 1;

                    if (!visited.ContainsKey(neighbor) || newCost < visited[neighbor])
                    {
                        visited[neighbor] = newCost;
                        var priority = newCost + Heuristic(neighbor, currentNode.GameObject);
                        openNodes.Enqueue(new Node(neighbor, newCost, priority, currentNode));
                    }
                }
            }

            // No path found
            return null;
        }
        public List<GameObjectBase> FindShortestPath(List<GameObjectBase> gameObjects, GameObjectBase start, GameObjectBase end, int maxRange = 600)
        {
            // Create a priority queue for open nodes
            var openNodes = new PriorityQueue<Node>();
            openNodes.Enqueue(new Node(start, 0, 0, null));

            // Create a dictionary to track visited nodes and their best cost
            Dictionary<GameObjectBase, int> visited = new Dictionary<GameObjectBase, int>();
            visited[start] = 0;
            while (openNodes.Count > 0)
            {
                var currentNode = openNodes.Dequeue();

                if (currentNode.GameObject == end)
                {
                    // We reached the destination, construct and return the path
                    return ConstructPath(currentNode);
                }

                foreach (var neighbor in GetNeighbors(currentNode.GameObject, maxRange, gameObjects))
                {
                    var newCost = currentNode.Cost + 1; // Assuming each move has a cost of 1

                    if (!visited.ContainsKey(neighbor) || newCost < visited[neighbor])
                    {
                        visited[neighbor] = newCost;
                        var priority = newCost + Heuristic(neighbor, end);
                        openNodes.Enqueue(new Node(neighbor, newCost, priority, currentNode));
                    }
                }
            }

            // No path found
            return null;
        }

        public List<GameObjectBase> FindLongestPath(List<GameObjectBase> gameObjects, GameObjectBase start, GameObjectBase end, int maxRange = 600)
        {
            var unvisitedObjects = new HashSet<GameObjectBase>(gameObjects);
            var visitedPath = new List<GameObjectBase>();

            if (FindLongestPathRecursive(start, end, maxRange, unvisitedObjects, visitedPath))
            {
                visitedPath.Add(end);
                return visitedPath;
            }

            return null;
        }

        private bool FindLongestPathRecursive(GameObjectBase currentObject, GameObjectBase end, int maxRange, HashSet<GameObjectBase> unvisitedObjects, List<GameObjectBase> visitedPath)
        {
            unvisitedObjects.Remove(currentObject);
            visitedPath.Add(currentObject);

            if (currentObject == end)
                return true;

            foreach (var neighbor in GetNeighbors(currentObject, maxRange, unvisitedObjects.ToList()))
            {
                if (FindLongestPathRecursive(neighbor, end, maxRange, unvisitedObjects, visitedPath))
                    return true;
            }

            visitedPath.Remove(currentObject);
            unvisitedObjects.Add(currentObject);

            return false;
        }

        private List<GameObjectBase> GetNeighbors(GameObjectBase gameObject, int maxRange, List<GameObjectBase> gameObjects)
        {
            var neighbors = new List<GameObjectBase>();

            foreach (var otherObject in gameObjects)
            {
                if (otherObject != gameObject)
                {
                    var distance = CalculateDistance(gameObject.Position, otherObject.Position);

                    if (distance <= maxRange)
                    {
                        neighbors.Add(otherObject);
                    }
                }
            }

            return neighbors;
        }

        private float CalculateDistance(Vector3 position1, Vector3 position2)
        {
            // Calculate the Manhattan distance between two positions
            return position1.Distance(position2);
        }

        private int Heuristic(GameObjectBase start, GameObjectBase end)
        {
            // Use the Manhattan distance as the heuristic for A* algorithm
            return (int)CalculateDistance(start.Position, end.Position);
        }

        private List<GameObjectBase> ConstructPath(Node node)
        {
            var path = new List<GameObjectBase>();
            var currentNode = node;

            while (currentNode != null)
            {
                path.Insert(0, currentNode.GameObject);
                currentNode = currentNode.Parent;
            }

            return path;
        }

        private class Node : IComparable<Node>
        {
            public GameObjectBase GameObject { get; }
            public int Cost { get; }
            public int Priority { get; }
            public Node Parent { get; }

            public Node(GameObjectBase gameObject, int cost, int priority, Node parent)
            {
                GameObject = gameObject;
                Cost = cost;
                Priority = priority;
                Parent = parent;
            }

            public int CompareTo(Node other)
            {
                return Priority.CompareTo(other.Priority);
            }
        }

        private class PriorityQueue<T> where T : IComparable<T>
        {
            private List<T> data;

            public int Count => data.Count;

            public PriorityQueue()
            {
                data = new List<T>();
            }

            public void Enqueue(T item)
            {
                data.Add(item);
                int childIndex = data.Count - 1;
                while (childIndex > 0)
                {
                    int parentIndex = (childIndex - 1) / 2;
                    if (data[childIndex].CompareTo(data[parentIndex]) >= 0)
                        break;

                    Swap(childIndex, parentIndex);
                    childIndex = parentIndex;
                }
            }

            public T Dequeue()
            {
                int lastIndex = data.Count - 1;
                T frontItem = data[0];
                data[0] = data[lastIndex];
                data.RemoveAt(lastIndex);

                lastIndex--;
                int parentIndex = 0;

                while (true)
                {
                    int childIndex = parentIndex * 2 + 1;
                    if (childIndex > lastIndex)
                        break;

                    int rightChild = childIndex + 1;
                    if (rightChild <= lastIndex && data[rightChild].CompareTo(data[childIndex]) < 0)
                        childIndex = rightChild;

                    if (data[parentIndex].CompareTo(data[childIndex]) <= 0)
                        break;

                    Swap(parentIndex, childIndex);
                    parentIndex = childIndex;
                }

                return frontItem;
            }

            private void Swap(int i, int j)
            {
                T temp = data[i];
                data[i] = data[j];
                data[j] = temp;
            }
        }
    }
}
