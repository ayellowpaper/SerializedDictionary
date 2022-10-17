using System.Collections.Generic;
using UnityEngine;

namespace AYellowpaper.SerializedCollections
{
    [System.Serializable]
    public partial class SerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver, ILookupTable
    {
        private static readonly List<int> EmptyList = new List<int>();

        [SerializeField]
        internal List<SerializedKeyValuePair<TKey, TValue>> _serializedList = new List<SerializedKeyValuePair<TKey, TValue>>();

#if !UNITY_EDITOR
        public void OnAfterDeserialize()
        {
            Clear();

            foreach (var kvp in _serializedList)
                Add(kvp.Key, kvp.Value);

            _serializedList.Clear();
        }

        public void OnBeforeSerialize()
        {
        }
#endif

        public IReadOnlyList<int> GetOccurences(object key)
        {
#if UNITY_EDITOR
            if (key is TKey castKey && _occurences.TryGetValue(castKey, out var list))
                return list;
#endif
            return EmptyList;
        }

        public void RecalculateOccurences()
        {
#if UNITY_EDITOR
            Clear();
            _occurences.Clear();

            int count = _serializedList.Count;
            for (int i = 0; i < count; i++)
            {
                var kvp = _serializedList[i];
                if (!SerializedCollectionsUtility.IsValidKey(kvp.Key))
                    continue;

                if (!_occurences.ContainsKey(kvp.Key))
                {
                    _occurences.Add(kvp.Key, new List<int>() { i });
                    Add(kvp.Key, kvp.Value);
                }
                else
                    _occurences[kvp.Key].Add(i);
            }
#endif
        }
    }
}
