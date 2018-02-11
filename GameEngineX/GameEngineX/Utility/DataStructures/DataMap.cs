using System.Collections;
using System.Collections.Generic;

namespace GameEngineX.Utility.DataStructures {
    public class DataMap<T> : IEnumerable<KeyValuePair<long, T>> {
        private readonly Dictionary<long, T> data;
        private int minX;
        private int maxX;
        private int minY;
        private int maxY;

        public DataMap() {
            this.data = new Dictionary<long, T>();

            this.minX = 0;
            this.maxX = 0;
            this.minY = 0;
            this.maxY = 0;
        }

        public void Add(int x, int y, T data) {
            this[x, y] = data;
        }

        public T Remove(int x, int y) {
            T d = this[x, y];
            this[x, y] = default(T);

            return d;
        }

        public T Get(int x, int y) {
            return this[x, y];
        }

        public bool Has(int x, int y) {
            return this[x, y] != null;
        }

        public T this[int x, int y] {
            get => data.TryGetValue(ToCombined(x, y), out T res) ? res : default(T);
            set {
                this.data[ToCombined(x, y)] = value;

                this.minX = System.Math.Min(x, this.minX);
                this.maxX = System.Math.Max(x, this.maxX);
                this.minY = System.Math.Min(y, this.minY);
                this.maxY = System.Math.Max(y, this.maxY);
            }

        }

        public IEnumerable<T> Values => this.data.Values;

        private static long ToCombined(int x, int y) {
            return (long)x << 32 | y;
        }

        private static void FromCombined(long c, out int x, out int y) {
            x = (int)(c >> 32);
            y = (int)(c & 0xffffffffL);
        }

        //public XMLElement ToXml(string tag, Func<T, XMLElement> converter) {
        //    XMLElement e = new XMLElement(tag);
        //    foreach (KeyValuePair<long, T> item in this.data) {
        //        long c = item.Key;

        //        int x, y;
        //        FromCombined(c, out x, out y);

        //        XMLElement dp = new XMLElement("DataPoint");
        //        dp.SetAttribute("x", x.ToString());
        //        dp.SetAttribute("y", y.ToString());

        //        XMLElement de = converter(item.Value);
        //        dp.AddElement(de);

        //        e.AddElement(dp);
        //    }

        //    return e;
        //}

        //public static DataMap<E> FromXML<E>(XMLElement e, Func<XMLElement, E> converter) {
        //    DataMap<E> dm = new DataMap<E>();

        //    foreach (XMLElement c in e.GetElements("DataPoints")) {
        //        int x = int.Parse(c.GetAttribute("x"));
        //        int y = int.Parse(c.GetAttribute("y"));
        //        // TODO
        //        E d = converter(c.NestedElements.First());
        //    }

        //    return dm;
        //}

        public IEnumerator<KeyValuePair<long, T>> GetEnumerator() {
            return this.data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.data.GetEnumerator();
        }
    }
}
