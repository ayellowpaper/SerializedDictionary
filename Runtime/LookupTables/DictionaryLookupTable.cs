using System.Collections;
using System.Collections.Generic;

namespace AYellowpaper.SerializedCollections
{
    internal class DictionaryLookupTable<TKey, TValue> : ILookupTable
    {
        private SerializedDictionary<TKey, TValue> _dictionary;
        private Dictionary<TKey, List<int>> _occurences = new Dictionary<TKey, List<int>>();

        private static readonly List<int> EmptyList = new List<int>();

        public IEnumerable Keys => _dictionary.Keys;

        public DictionaryLookupTable(SerializedDictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary;
        }

        public IReadOnlyList<int> GetOccurences(object key)
        {
            if (key is TKey castKey && _occurences.TryGetValue(castKey, out var list))
                return list;

            return EmptyList;
        }

        public void RecalculateOccurences()
        {
            _occurences.Clear();

            int count = _dictionary._serializedList.Count;
            for (int i = 0; i < count; i++)
            {
                var kvp = _dictionary._serializedList[i];
                if (!SerializedCollectionsUtility.IsValidKey(kvp.Key))
                    continue;

                if (!_occurences.ContainsKey(kvp.Key))
                    _occurences.Add(kvp.Key, new List<int>() { i });
                else
                    _occurences[kvp.Key].Add(i);
            }
        }
    }
}