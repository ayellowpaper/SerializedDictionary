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
    }
}
