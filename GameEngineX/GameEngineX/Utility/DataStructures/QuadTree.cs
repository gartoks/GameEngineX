using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngineX.Utility.DataStructures {
    public class QuadTree<T> {
        public readonly double MinX;
        public readonly double MinY;
        public readonly double MaxX;
        public readonly double MaxY;

        private readonly int splitMargin;
        private QuadTree<T>[] quads;

        private List<Item> items;

        public QuadTree(int splitMargin, double minX, double minY, double maxX, double maxY) {
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;

            this.splitMargin = splitMargin;
            this.quads = null;

            this.items = new List<Item>();
        }

        public void ForEach(Action<T> action) {
            if (IsLeaf) {
                foreach (Item item in this.items) {
                    action(item.Data);
                }
            } else {
                foreach (QuadTree<T> qt in this.quads) {
                    qt.ForEach(action);
                }
            }
        }

        public void Add(double x, double y, T item) {
            if (!Contains(x, y))
                throw new InvalidOperationException("Cannot add item outside of quad tree's area.");

            if (IsLeaf) {
                this.items.Add(new Item(x, y, item));
            } else {
                int qIdx = GetContainingQuadIndex(x, y);
                this.quads[qIdx].Add(x, y, item);
            }

            if (IsLeaf && Count >= this.splitMargin)
                Split();
        }

        public void Remove(T item) {
            if (IsLeaf) {

                int idx = -1;
                for (int i = 0; i < this.items.Count; i++) {
                    if (!this.items[i].Data.Equals(item))
                        continue;

                    idx = i;
                    break;
                }

                if (idx >= 0)
                    this.items.RemoveAt(idx);

            } else {
                foreach (QuadTree<T> qt in this.quads) {
                    qt.Remove(item);
                }
            }
        }

        public void Move(double x, double y, T item) {
            Remove(item);
            Add(x, y, item);
        }

        private void Split() {
            double sizeX = (MaxX - MinX) / 2.0;
            double sizeY = (MaxY - MinY) / 2.0;

            this.quads = new[] {    new QuadTree<T>(this.splitMargin, MinX, MinY, MinX + sizeX, MinY + sizeY),
                                    new QuadTree<T>(this.splitMargin, MinX + sizeX, MinY, MaxX, MinY + sizeY),
                                    new QuadTree<T>(this.splitMargin, MinX, MinY + sizeY, MinX + sizeX, MaxY),
                                    new QuadTree<T>(this.splitMargin, MinX + sizeX, MinY + sizeY, MaxX, MaxY)};

            foreach (var item in this.items) {
                Add(item.X, item.Y, item.Data);
            }

            this.items = null;
        }

        private int GetContainingQuadIndex(double x, double y) {
            if (IsLeaf || !Contains(x, y))
                return -1;

            for (int i = 0; i < this.quads.Length; i++) {
                if (this.quads[i].Contains(x, y))
                    return i;
            }

            return this.quads.Length - 1;   // ensure it is in one quad if it is inside this quad
        }

        public ICollection<T> GetItemsIn(double x, double y, double width, double height, ICollection<T> result = null) {
            if (result == null)
                result = new List<T>();

            if (IsLeaf) {
                foreach (Item item in this.items) {
                    if (item.X >= x && item.X <= x + width && item.Y >= y && item.Y <= y + height)
                        result.Add(item.Data);
                }
            } else {

                IEnumerable<QuadTree<T>> qs = this.quads.Where(tmp => tmp.Contains(x, y) || tmp.Contains(x + width, y) || tmp.Contains(x, y + height) || tmp.Contains(x + width, y + height));
                foreach (QuadTree<T> qt in qs) {
                    qt.GetItemsIn(x, y, width, height, result);
                }

            }

            return result;
        }

        public ICollection<T> GetItemsIn(double x, double y, double radius, ICollection<T> result = null) {
            if (result == null)
                result = new List<T>();

            double r2 = radius * radius;

            if (IsLeaf) {
                foreach (Item item in this.items) {
                    double dx = x - item.X;
                    double dy = y - item.Y;
                    if (dx * dx + dy * dy <= r2)
                        result.Add(item.Data);
                }
            } else {
                double xMin = x - radius;
                double yMin = y - radius;
                double width = 2f * radius;
                double height = 2f * radius;

                IEnumerable<QuadTree<T>> qs = this.quads.Where(tmp => tmp.Contains(xMin, yMin) || tmp.Contains(xMin + width, yMin) || tmp.Contains(xMin, yMin + height) || tmp.Contains(xMin + width, yMin + height));
                foreach (QuadTree<T> qt in qs) {
                    qt.GetItemsIn(x, y, radius, result);
                }

            }

            return result;
        }

        public ICollection<T> GetItems(ICollection<T> result = null) {
            if (result == null)
                result = new List<T>();

            if (IsLeaf) {
                foreach (Item item in this.items) {
                    result.Add(item.Data);
                }
            } else {
                foreach (QuadTree<T> qt in this.quads) {
                    qt.GetItems(result);
                }
            }

            return result;
        }

        public bool Contains(double x, double y) {
            return x >= MinX && x < MaxX && y >= MinY && y < MaxY;
        }

        public int Count {
            get {
                if (IsLeaf) {
                    return this.items.Count;
                } else {
                    return this.quads.Sum(t => t.Count);
                }
            }
        }

        public bool IsLeaf => this.quads == null;

        private class Item {
            public readonly double X;
            public readonly double Y;
            public readonly T Data;

            public Item(double x, double y, T data) {
                this.X = x;
                this.Y = y;
                this.Data = data;
            }
        }
    }
}
