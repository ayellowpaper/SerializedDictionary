using AYellowpaper.SerializedCollections.Editor.Data;
using AYellowpaper.SerializedCollections.Editor.States;
using AYellowpaper.SerializedCollections.KeysGenerators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;

namespace AYellowpaper.SerializedCollections.Editor
{
    [CustomPropertyDrawer(typeof(SerializedHashSet<>))]
    public class SerializedHashSetDrawer : PropertyDrawer
    {
        public const string ElementName = nameof(SerializedElement<int>.Element);
        public const string SerializedListName = nameof(SerializedHashSet<int>._serializedList);
        public const string LookupTableName = nameof(SerializedHashSet<int>.LookupTable);

        public const int TopHeaderClipHeight = 20;
        public const int TopHeaderHeight = 19;
        public const int SearchHeaderHeight = 20;
        public const int ElementHeaderHeight = 18;
        public const bool KeyFlag = true;
        public const bool ValueFlag = false;
        public static readonly Color BorderColor = new Color(36 / 255f, 36 / 255f, 36 / 255f);
        public static readonly List<int> NoEntriesList = new List<int>();
        internal static GUIContent DisplayTypeToggleContent
        {
            get
            {
                if (_displayTypeToggleContent == null)
                {
                    var texture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Plugins/SerializedCollections/Editor/Assets/BurgerMenu@2x.png");
                    _displayTypeToggleContent = new GUIContent(texture, "Toggle to either draw existing editor or draw properties manually.");
                }
                return _displayTypeToggleContent;
            }
        }
        private static GUIContent _displayTypeToggleContent;

        private Dictionary<string, SerializedHashSetInstanceDrawer> _arrayData = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!_arrayData.ContainsKey(property.propertyPath))
                _arrayData.Add(property.propertyPath, new SerializedHashSetInstanceDrawer(property, fieldInfo));
            _arrayData[property.propertyPath].OnGUI(position, label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!_arrayData.ContainsKey(property.propertyPath))
                _arrayData.Add(property.propertyPath, new SerializedHashSetInstanceDrawer(property, fieldInfo));
            return _arrayData[property.propertyPath].GetPropertyHeight(label);
        }
    }
}