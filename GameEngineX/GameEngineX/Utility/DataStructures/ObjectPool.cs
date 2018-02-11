using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngineX.Utility.DataStructures {
    public class ObjectPool<T> {
        private readonly HashSet<T> objectPool;
        private readonly Func<T> objectGenerator;
        private readonly Func<T, T> objectResetter;

        public ObjectPool(Func<T> objectGenerator)
            : this(objectGenerator, x => x) { }

        public ObjectPool(Func<T> objectGenerator, Func<T, T> objectResetter, int initialPopulation = 0) {
            this.objectPool = new HashSet<T>();

            this.objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
            this.objectResetter = objectResetter;

            if (initialPopulation > 0)
                Populate(initialPopulation);
        }

        public T Get() {
            if (this.objectPool.Count == 0)
                return GenerateObject();

            T item = this.objectPool.First();
            this.objectPool.Remove(item);

            return item;
        }

        public IEnumerable<T> Get(int count) {
            List<T> items = new List<T>();
            for (int i = 0; i < count; i++) {
                items.Add(Get());
            }
            return items;
        }

        public void Put(T item) {
            if (item == null)
                return;

            item = this.objectResetter(item);

            this.objectPool.Add(item);
        }

        public void Put(IEnumerable<T> items) {
            foreach (T item in items) {
                Put(item);
            }
        }

        public void Populate(int count) {
            if (count < 0)
                throw new ArgumentException();

            while (AvailableObjects < count) {
                Put(GenerateObject());
            }
        }

        public int AvailableObjects => this.objectPool.Count;

        private T GenerateObject() {
            return this.objectResetter(this.objectGenerator());
        }
    }
}
