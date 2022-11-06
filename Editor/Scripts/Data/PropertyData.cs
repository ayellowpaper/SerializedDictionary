using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AYellowpaper.SerializedCollections.Editor.Data
{
    [System.Serializable]
    internal class PropertyData
    {
        [SerializeField]
        private ElementData _keyData;
        [SerializeField]
        private ElementData _valueData;
        [SerializeField]
        private int _elementsPerPage = 5;

        public int ElementsPerPage
        {
            get => _elementsPerPage;
            set => _elementsPerPage = value;
        }

        public ElementData GetElementData(bool fieldType)
        {
            return fieldType == SCEditorUtility.KeyFlag ? _keyData : _valueData;
        }

        public PropertyData() : this(new ElementSettings(), new ElementSettings()) { }

        public PropertyData(ElementSettings keySettings, ElementSettings valueSettings)
        {
            _keyData = new ElementData(keySettings);
            _valueData = new ElementData(valueSettings);
        }
    }
}