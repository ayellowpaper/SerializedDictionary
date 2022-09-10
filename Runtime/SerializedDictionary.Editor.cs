using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AYellowpaper.SerializedCollections
{
#if UNITY_EDITOR
    public partial class SerializedDictionary<TKey, TValue>
    {
        private Dictionary<TKey, List<int>> _conflicts = new Dictionary<TKey, List<int>>();

        public void OnAfterDeserialize()
        {
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
        }

        public void OnBeforeSerialize()
        {
            if (UnityEditor.BuildPipeline.isBuildingPlayer)
                RemoveDuplicates();
        }

        private void RemoveDuplicates()
        {
            _serializedList = _serializedList
                .GroupBy(x => x.Key)
                .Where(x => SerializedCollectionsUtility.IsValidKey(x.Key))
                .Select(x => x.First()).ToList();
        }
    }
#endif
}