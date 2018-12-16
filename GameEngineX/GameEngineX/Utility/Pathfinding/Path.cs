using System.Collections;
using System.Collections.Generic;

namespace GameEngineX.Utility.Pathfinding {
    public class Path<T> : IEnumerable<T> {

        private readonly T[] path;
        private readonly float cost;

        public Path(T[] path, float pathCost) {
            this.path = path;
            this.cost = pathCost;
        }

        public T this[int i] => this.path[i];

        public int Length => this.path.Length;

        public float Cost => this.cost;

        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)this.path).GetEnumerator(); 

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}