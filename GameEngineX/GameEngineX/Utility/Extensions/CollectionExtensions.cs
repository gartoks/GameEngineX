using System;
using System.Collections.Generic;

namespace GameEngineX.Utility.Extensions {
    public static class CollectionExtensions {

        public static Dictionary<K, V> AddToDictionary<K, V>(this (K, V)[] set, Dictionary<K, V> dictionary = null, Func<K, V, bool> dataFilter = null) {
            if (dictionary == null)
                dictionary = new Dictionary<K, V>();

            foreach ((K key, V value) data in set) {
                if (dictionary.ContainsKey(data.key))
                    continue;

                if (dataFilter != null && !dataFilter(data.key, data.value))
                    continue;

                dictionary[data.key] = data.value;
            }

            return dictionary;
        }

        public static (K, V) ToTuple<K, V>(this KeyValuePair<K, V> pair) => (pair.Key, pair.Value);

    }
}