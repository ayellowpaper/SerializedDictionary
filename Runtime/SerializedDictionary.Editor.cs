using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AYellowpaper.SerializedCollections
{
#if UNITY_EDITOR
    public partial class SerializedDictionary<TKey, TValue>
    {
        private Dictionary<TKey, List<int>> _occurences = new Dictionary<TKey, List<int>>();

        public void OnAfterDeserialize()
        {
            RecalculateOccurences();
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