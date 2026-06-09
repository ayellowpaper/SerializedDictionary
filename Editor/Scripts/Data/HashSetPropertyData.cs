using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AYellowpaper.SerializedCollections.Editor.Data
{
    [System.Serializable]
    internal class HashSetPropertyData
    {
        [SerializeField]
        private ElementData _keyData;
        [SerializeField]
        private bool _alwaysShowSearch = false;

        public bool AlwaysShowSearch
        {
            get => _alwaysShowSearch;
            set => _alwaysShowSearch = value;
        }

        public ElementData GetElementData()
        {
            return _keyData;
        }

        public HashSetPropertyData() : this(new ElementSettings()) { }

        public HashSetPropertyData(ElementSettings keySettings)
        {
            _keyData = new ElementData(keySettings);
        }
    }
}