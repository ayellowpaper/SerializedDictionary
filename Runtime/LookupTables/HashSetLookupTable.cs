using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AYellowpaper.SerializedCollections
{
    internal class HashSetLookupTable<TElement> : IKeyable
    {
        private SerializedHashSet<TElement> _hashSet;
        private Dictionary<TElement, List<int>> _occurences = new Dictionary<TElement, List<int>>();

        private static readonly List<int> EmptyList = new List<int>();

        public IEnumerable Keys => _hashSet._serializedList;

        public HashSetLookupTable(SerializedHashSet<TElement> hashSet)
        {
            _hashSet = hashSet;
        }

        public IReadOnlyList<int> GetOccurences(object element)
        {
            if (element is TElement castElement && _occurences.TryGetValue(castElement, out var list))
                return list;

            return EmptyList;
        }

        public void RecalculateOccurences()
        {
            _occurences.Clear();

            int count = _hashSet._serializedList.Count;
            for (int i = 0; i < count; i++)
            {
                var element = _hashSet._serializedList[i];
                if (!SerializedCollectionsUtility.IsValidKey(element))
                    continue;

                if (!_occurences.ContainsKey(element.Element))
                    _occurences.Add(element.Element, new List<int>() { i });
                else
                    _occurences[element.Element].Add(i);
            }
        }

        public void RemoveKey(object element)
        {
            for (int i = _hashSet._serializedList.Count - 1; i >= 0; i--)
            {
                var dictKey = _hashSet._serializedList[i].Element;
                if ((object)dictKey == element || dictKey.Equals(element))
                    _hashSet._serializedList.RemoveAt(i);
            }
        }

        public void RemoveAt(int index)
        {
            _hashSet._serializedList.RemoveAt(index);
        }

        public object GetKeyAt(int index)
        {
            return _hashSet._serializedList[index];
        }

        public void RemoveDuplicates()
        {
            _hashSet._serializedList = _hashSet._serializedList
                .GroupBy(x => x)
                .Where(x => SerializedCollectionsUtility.IsValidKey(x.Key))
                .Select(x => x.First()).ToList();
        }

        public void AddKey(object element)
        {
            var entry = new SerializedElement<TElement>();
            entry.Element = (TElement) element;
            _hashSet._serializedList.Add(entry);
        }
    }
}