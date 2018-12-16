using System.Collections.Generic;
using System.Linq;

namespace GameEngineX.Utility.Pathfinding {
    public class AStar<T> {
        public delegate IEnumerable<T> NeighbourGetter(T node);
        public delegate float EdgeCostFunction(T from, T to);
        public delegate float HeuristicFunction(T node, T goal);

        private readonly HeuristicFunction heuristic;
        private readonly EdgeCostFunction edgeCost;
        private readonly NeighbourGetter neighbourGetter;

        public AStar(HeuristicFunction heuristic, EdgeCostFunction edgeCost, NeighbourGetter neighbourGetter) {
            this.heuristic = heuristic;
            this.edgeCost = edgeCost;
            this.neighbourGetter = neighbourGetter;
        }

        public Path<T> FindPath(T startNode, T goalNode) {
            HashSet<T> visisted = new HashSet<T>();
            HashSet<T> discovered = new HashSet<T>();
            Dictionary<T, float> pathCosts = new Dictionary<T, float>();
            Dictionary<T, T> nodeOrigins = new Dictionary<T, T>();

            float PathCost(T node) => pathCosts.TryGetValue(node, out float value) ? value : float.PositiveInfinity;
            float EstimatedCost(T node) => PathCost(node) + heuristic(node, goalNode);
            (T node, float cost) FindBestCostNode() {
                float minCost = float.PositiveInfinity;
                T minCostNode = default(T);

                foreach (T node in discovered) {
                    float cost = EstimatedCost(node);
                    if (cost < minCost) {
                        minCost = cost;
                        minCostNode = node;
                    }
                }

                return (minCostNode, minCost);
            }

            // init
            pathCosts[startNode] = 0;
            discovered.Add(startNode);

            while (discovered.Count != 0) {
                T currentNode = FindBestCostNode().node;

                if (currentNode.Equals(goalNode))
                    return ConstructPath(nodeOrigins, goalNode, PathCost(goalNode));

                discovered.Remove(currentNode);
                visisted.Add(currentNode);

                float pathCostToCurrentNode = PathCost(currentNode);

                foreach (T neighbourNode in neighbourGetter(currentNode)) {
                    if (visisted.Contains(neighbourNode))
                        continue;

                    discovered.Add(neighbourNode);  // need not check if already contained since it's a set

                    float tentativePathCost = pathCostToCurrentNode + edgeCost(currentNode, neighbourNode);
                    if (tentativePathCost >= PathCost(neighbourNode))
                        continue;

                    nodeOrigins[neighbourNode] = currentNode;
                    pathCosts[neighbourNode] = tentativePathCost;


                }
            }

            return null;
        }

        private Path<T> ConstructPath(Dictionary<T, T> nodeOrigins, T goal, float pathCost) {
            List<T> path = new List<T> { goal };

            T currentNode = goal;
            while (nodeOrigins.TryGetValue(currentNode, out currentNode)) {
                path.Add(currentNode);
            }

            path.Reverse();

            return new Path<T>(path.ToArray(), pathCost);
        }
    }
}