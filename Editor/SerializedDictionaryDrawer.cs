using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace AYellowpaper.SerializedCollections
{
    [CustomPropertyDrawer(typeof(SerializedDictionary<,>))]
    public class SerializedDictionaryDrawer : PropertyDrawer
    {
        public const string KeyName = nameof(SerializedKeyValuePair<int, int>.Key);
        public const string ValueName = nameof(SerializedKeyValuePair<int, int>.Value);
        public const string SerializedListName = nameof(SerializedDictionary<int, int>._serializedList);

        private const bool KeyFlag = true;
        private const bool ValueFlag = false;

        private bool _initialized = false;
        private ReorderableList _list;
        private IList _backingList;
        private IConflictCheckable _conflictChecker;
        private FieldInfo _keyFieldInfo;
        private GUIContent _labelContent;
        private Rect _totalRect;
        private GUIStyle _keyValueStyle;
        private SerializedDictionaryAttribute _dictionaryAttribute;
        private SerializedProperty _listProperty;
        private ElementDisplaySettings _keySettings;
        private ElementDisplaySettings _valueSettings;

        private class ElementDisplaySettings
        {
            public readonly string PersistentPath;
            public readonly DisplayType DisplayType;
            public readonly bool HasListDrawerToggle;
            public bool ShowAsList => HasListDrawerToggle && IsListToggleActive;
            public bool IsListToggleActive
            {
                get => SerializedCollectionsEditorUtility.GetPersistentBool(PersistentPath, false);
                set => SerializedCollectionsEditorUtility.SetPersistentBool(PersistentPath, value);
            }
            public DisplayType EffectiveDisplayType => ShowAsList ? DisplayType.List : DisplayType;

            public ElementDisplaySettings(string persistentPath, DisplayType displayType, bool hasListDrawerToggle)
            {
                PersistentPath = persistentPath;
                DisplayType = displayType;
                HasListDrawerToggle = hasListDrawerToggle;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            InitializeIfNeeded(property);
            float height = 68;
            if (_listProperty.arraySize == 0)
                height += 20;
            foreach (SerializedProperty arrayElement in _listProperty)
                height += CalculateHeightOfElement(arrayElement, GetDisplaySettings(KeyFlag).EffectiveDisplayType == DisplayType.List ? true : false, GetDisplaySettings(ValueFlag).EffectiveDisplayType == DisplayType.List ? true : false);
            return height;
        }

        private SerializedProperty GetElementProperty(SerializedProperty property, bool fieldFlag)
        {
            return property.FindPropertyRelative(fieldFlag == KeyFlag ? KeyName : ValueName);
        }

        private ElementDisplaySettings GetDisplaySettings(bool fieldFlag)
        {
            return fieldFlag == KeyFlag ? _keySettings : _valueSettings;
        }

        private static float CalculateHeightOfElement(SerializedProperty property, bool drawKeyAsList, bool drawValueAsList)
        {
            SerializedProperty keyProperty = property.FindPropertyRelative(KeyName);
            SerializedProperty valueProperty = property.FindPropertyRelative(ValueName);
            return Mathf.Max(SerializedCollectionsEditorUtility.CalculateHeight(keyProperty, drawKeyAsList), SerializedCollectionsEditorUtility.CalculateHeight(valueProperty, drawValueAsList));
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            InitializeIfNeeded(property);

            _totalRect = position;
            _labelContent = new GUIContent(label);
            _list.DoList(position);
        }

        private void InitializeIfNeeded(SerializedProperty property)
        {
            if (!_initialized)
            {
                _initialized = true;
                _keyValueStyle = new GUIStyle(EditorStyles.toolbarButton);
                _keyValueStyle.padding = new RectOffset(0, 0, 0, 0);
                _keyValueStyle.border = new RectOffset(0, 0, 0, 0);
                _keyValueStyle.alignment = TextAnchor.MiddleCenter;
                //_keyValueStyle.normal.background = Resources.Load<Texture2D>("SerializedCollections/KeyValueBackground");

                _dictionaryAttribute = fieldInfo.GetCustomAttribute<SerializedDictionaryAttribute>();
                _listProperty = property.FindPropertyRelative(SerializedListName);

                _list = new ReorderableList(property.serializedObject, _listProperty, true, true, true, true);
                _list.onAddCallback += OnAddToList;
                _list.drawElementCallback += OnDrawElement;
                _list.elementHeightCallback += OnGetElementHeight;
                _list.drawHeaderCallback += OnDrawHeader;
                _list.headerHeight *= 2;

                var dict = fieldInfo.GetValue(property.serializedObject.targetObject);
                _conflictChecker = dict as IConflictCheckable;
                var listField = fieldInfo.FieldType.GetField(SerializedListName, BindingFlags.Instance | BindingFlags.NonPublic);
                _keyFieldInfo = listField.FieldType.GetGenericArguments()[0].GetField(KeyName);
                _backingList = (IList)listField.GetValue(dict);
            }

            if (_keySettings == null && _listProperty.arraySize > 0)
            {
                var genericArgs = fieldInfo.FieldType.GetGenericArguments();
                var firstProperty = _listProperty.GetArrayElementAtIndex(0);
                _keySettings = CreateDisplaySettings(GetElementProperty(firstProperty, true), genericArgs[0]);
                _valueSettings = CreateDisplaySettings(GetElementProperty(firstProperty, false), genericArgs[1]);
            }
        }

        private ElementDisplaySettings CreateDisplaySettings(SerializedProperty property, System.Type type)
        {
            bool hasCustomEditor = SerializedCollectionsEditorUtility.HasDrawerForType(type);
            bool isGenericWithChildren = property.propertyType == SerializedPropertyType.Generic && property.hasVisibleChildren;
            DisplayType displayType = DisplayType.PropertyNoLabel;
            if (property.isArray && !hasCustomEditor)
                displayType = DisplayType.Property;
            else if (!property.isArray && isGenericWithChildren && !hasCustomEditor)
                displayType = DisplayType.List;
            bool canToggleListDrawer = property.isArray || (isGenericWithChildren && hasCustomEditor);
            return new ElementDisplaySettings(property.propertyPath, displayType, canToggleListDrawer);
        }

        private void OnDrawHeader(Rect rect)
        {
            Rect topRect = rect;
            topRect.height /= 2;
            _labelContent = EditorGUI.BeginProperty(topRect, _labelContent, _list.serializedProperty);
            EditorGUI.LabelField(topRect, _labelContent);

            Rect bottomRect = _totalRect;
            bottomRect.x += 1;
            bottomRect.width -= 1;
            bottomRect.y = topRect.y + topRect.height;
            bottomRect.height = rect.height - topRect.height;

            float width = EditorGUIUtility.labelWidth + 22;
            Rect leftRect = new Rect(bottomRect.x, bottomRect.y, width, bottomRect.height);
            Rect rightRect = new Rect(bottomRect.x + width, bottomRect.y, bottomRect.width - width, bottomRect.height);

            if (Event.current.type == EventType.Repaint)
            {
                _keyValueStyle.Draw(leftRect, EditorGUIUtility.TrTextContent(_dictionaryAttribute?.KeyName ?? "Key"), false, false, false, false);
                _keyValueStyle.Draw(rightRect, EditorGUIUtility.TrTextContent(_dictionaryAttribute?.ValueName ?? "Value"), false, false, false, false);
            }

            if (_listProperty.arraySize > 0)
            {
                DoDisplayTypeToggle(leftRect, KeyFlag);
                DoDisplayTypeToggle(rightRect, ValueFlag);
            }

            Rect bottomLineRect = new Rect(bottomRect);
            bottomLineRect.y = bottomLineRect.y + bottomLineRect.height;
            bottomLineRect.height = 1;
            EditorGUI.DrawRect(bottomLineRect, new Color(36 / 255f, 36 / 255f, 36 / 255f));

            EditorGUI.EndProperty();
        }

        private void DoDisplayTypeToggle(Rect contentRect, bool fieldFlag)
        {
            var displaySettings = GetDisplaySettings(fieldFlag);

            if (displaySettings.HasListDrawerToggle)
            {
                Rect rightRectToggle = new Rect(contentRect);
                rightRectToggle.x += rightRectToggle.width - 18;
                rightRectToggle.width = 18;
                EditorGUI.BeginChangeCheck();
                bool newValue = GUI.Toggle(rightRectToggle, displaySettings.IsListToggleActive, "", EditorStyles.toolbarButton);
                if (EditorGUI.EndChangeCheck())
                    displaySettings.IsListToggleActive = newValue;
            }
        }

        private float OnGetElementHeight(int index)
        {
            var element = _listProperty.GetArrayElementAtIndex(index);
            return CalculateHeightOfElement(element, GetDisplaySettings(KeyFlag).EffectiveDisplayType == DisplayType.List ? true : false, GetDisplaySettings(ValueFlag).EffectiveDisplayType == DisplayType.List ? true : false);
        }

        private void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty kvp = _list.serializedProperty.GetArrayElementAtIndex(index);
            int leftSpace = 2;
            int lineWidth = 1;
            int rightSpace = 12;
            int totalSpace = leftSpace + lineWidth + rightSpace;
            Rect keyRect = new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth - leftSpace, EditorGUIUtility.singleLineHeight);
            Rect lineRect = new Rect(keyRect.x + keyRect.width + leftSpace, keyRect.y, lineWidth, rect.height);
            Rect valueRect = new Rect(keyRect.x + keyRect.width + totalSpace, keyRect.y, rect.width - keyRect.width - totalSpace, keyRect.height);

            var keyProperty = kvp.FindPropertyRelative(KeyName);
            var valueProperty = kvp.FindPropertyRelative(ValueName);
            var keyObject = _keyFieldInfo.GetValue(_backingList[index]);
            Color prevColor = GUI.color;
            int firstConflict = _conflictChecker.GetFirstConflict(keyObject);
            if (firstConflict >= 0)
            {
                GUI.color = firstConflict == index ? Color.yellow : Color.red;
            }
            if (!SerializedCollectionsUtility.IsValidKey(keyObject))
            {
                GUI.color = Color.red;
            }

            var keyDisplaySettings = GetDisplaySettings(KeyFlag);
            DrawGroupedElement(keyRect, 20, keyProperty, keyDisplaySettings.EffectiveDisplayType);

            EditorGUI.DrawRect(lineRect, new Color(36 / 255f, 36 / 255f, 36 / 255f));
            GUI.color = prevColor;

            var valueDisplaySettings = GetDisplaySettings(ValueFlag);
            DrawGroupedElement(valueRect, rightSpace, valueProperty, valueDisplaySettings.EffectiveDisplayType);
        }

        private void DrawGroupedElement(Rect rect, int space, SerializedProperty property, DisplayType displayType)
        {
            using (new LabelWidth(rect.width * 0.4f))
            {
                float height = SerializedCollectionsEditorUtility.CalculateHeight(property.Copy(), displayType == DisplayType.List ? true : false);
                GUI.BeginGroup(new Rect(rect.x - space, rect.y, rect.width + space, height));
                DrawElement(new Rect(space, 0, rect.width, rect.height), property, displayType);
                GUI.EndGroup();
            }
        }

        private void DrawElement(Rect rect, SerializedProperty property, DisplayType displayType)
        {
            switch (displayType)
            {
                case DisplayType.Property:
                    EditorGUI.PropertyField(rect, property, true);
                    break;
                case DisplayType.PropertyNoLabel:
                    EditorGUI.PropertyField(rect, property, GUIContent.none, true);
                    break;
                case DisplayType.List:

                    Rect childRect = new Rect(rect);
                    foreach (SerializedProperty prop in SerializedCollectionsEditorUtility.GetDirectChildren(property))
                    {
                        float height = EditorGUI.GetPropertyHeight(prop, true);
                        childRect.height = height;
                        EditorGUI.PropertyField(childRect, prop, true);
                        childRect.y += childRect.height;
                    }
                    break;
                default:
                    break;
            }
        }

        private void OnAddToList(ReorderableList list)
        {
            _list.serializedProperty.InsertArrayElementAtIndex(list.serializedProperty.arraySize);
        }

        public enum DisplayType
        {
            Property,
            PropertyNoLabel,
            List
        }
    }
}