using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Audio.ProcessorInstance.AvailableData;

namespace AYellowpaper.SerializedCollections
{
    [System.Serializable]
    public partial class SerializedHashSet<TElement> : HashSet<TElement>, ISerializationCallbackReceiver
    {
        [SerializeField]
        internal List<SerializedElement<TElement>> _serializedList = new List<SerializedElement<TElement>>();

#if UNITY_EDITOR
        internal IKeyable LookupTable
        {
            get
            {
                if (_lookupTable == null)
                    _lookupTable = new HashSetLookupTable<TElement>(this);
                return _lookupTable;
            }
        }

        private HashSetLookupTable<TElement> _lookupTable;
#endif

        public void OnAfterDeserialize()
        {
            Clear();

            foreach (var element in _serializedList)
            {
#if UNITY_EDITOR
                if (!Contains(element.Element))
                    Add(element.Element);
#else
                    Add(element.Element);
#endif
            }

#if UNITY_EDITOR
            LookupTable.RecalculateOccurences();
#else
            _serializedList.Clear();
#endif
        }

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (UnityEditor.BuildPipeline.isBuildingPlayer)
                LookupTable.RemoveDuplicates();
#endif
        }
    }
}
