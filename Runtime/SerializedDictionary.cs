using System.Collections.Generic;
using UnityEngine;

namespace AYellowpaper.SerializedCollections
{
    [System.Serializable]
    public partial class SerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver, IConflictCheckable
    {
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

        public int GetFirstConflict(object key)
        {
#if UNITY_EDITOR
            if (key is TKey castKey && _conflicts.TryGetValue(castKey, out var list))
                return list[0];
#endif
            return -1;
        }

        public void RecalculateConflicts()
        {
#if UNITY_EDITOR
            Clear();
            _conflicts.Clear();

            int count = _serializedList.Count;
            for (int i = 0; i < count; i++)
            {
                var kvp = _serializedList[i];
                if (!SerializedCollectionsUtility.IsValidKey(kvp.Key))
                    continue;

                if (ContainsKey(kvp.Key))
                {
                    if (!_conflicts.ContainsKey(kvp.Key))
                        _conflicts.Add(kvp.Key, new List<int>() { _serializedList.FindIndex(x => x.Key.Equals(kvp.Key)) });
                    _conflicts[kvp.Key].Add(i);
                    continue;
                }

                Add(kvp.Key, kvp.Value);
            }
#endif
        }
    }
}
