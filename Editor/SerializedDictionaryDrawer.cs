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

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            InitializeIfNeeded(property);
            float height = 68;
            if (_listProperty.arraySize == 0)
                height += 20;
            foreach (SerializedProperty arrayElement in _listProperty)
                height += CalculateHeightOfElement(arrayElement, GetOverride(KeyFlag), GetOverride(ValueFlag));
            return height;
        }

        private static float CalculateHeightOfElement(SerializedProperty property, bool drawKeyCustom, bool drawValueCustom)
        {
            SerializedProperty keyProperty = property.FindPropertyRelative(KeyName);
            SerializedProperty valueProperty = property.FindPropertyRelative(ValueName);
            return Mathf.Max(SerializedCollectionsEditorUtility.CalculateHeight(keyProperty, drawKeyCustom), SerializedCollectionsEditorUtility.CalculateHeight(valueProperty, drawValueCustom));
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
            bool hasChildren = _listProperty.GetArrayElementAtIndex(0).FindPropertyRelative(fieldFlag == KeyFlag ? KeyName : ValueName).hasVisibleChildren;
            if (!hasChildren)
                return;

            Rect rightRectToggle = new Rect(contentRect);
            rightRectToggle.x += rightRectToggle.width - 18;
            rightRectToggle.width = 18;
            EditorGUI.BeginChangeCheck();
            bool newValue = GUI.Toggle(rightRectToggle, GetOverride(fieldFlag), "", EditorStyles.toolbarButton);
            if (EditorGUI.EndChangeCheck())
                SetOverride(fieldFlag, newValue);
        }

        private float OnGetElementHeight(int index)
        {
            var element = _listProperty.GetArrayElementAtIndex(index);
            return CalculateHeightOfElement(element, GetOverride(KeyFlag), GetOverride(ValueFlag));
        }

        private bool GetOverride(bool fieldFlag)
        {
            return _list != null ? SerializedCollectionsEditorUtility.GetPersistentBool(_list.serializedProperty.propertyPath + (fieldFlag == KeyFlag ? "_Key" : "_Value")) : false;
        }

        private void SetOverride(bool fieldFlag, bool flag)
        {
            if (_list != null)
                SerializedCollectionsEditorUtility.SetPersistentBool(_list.serializedProperty.propertyPath + (fieldFlag == KeyFlag ? "_Key" : "_Value"), flag);
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

            DrawElement(keyRect, keyProperty, GetOverride(KeyFlag) ? DisplayType.ListDrawer : DisplayType.FieldNoLabel);

            EditorGUI.DrawRect(lineRect, new Color(36 / 255f, 36 / 255f, 36 / 255f));
            GUI.color = prevColor;

            DrawElement(valueRect, valueProperty, GetOverride(ValueFlag) ? DisplayType.ListDrawer : DisplayType.Field);
        }

        private void DrawElement(Rect valueRect, SerializedProperty property, DisplayType displayType)
        {
            switch (displayType)
            {
                case DisplayType.Field:
                    EditorGUI.PropertyField(valueRect, property, true);
                    break;
                case DisplayType.FieldNoLabel:
                    EditorGUI.PropertyField(valueRect, property, GUIContent.none, true);
                    break;
                case DisplayType.ListDrawer:
                    float prevLabelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = valueRect.width * 0.4f;
                    Rect childRect = new Rect(valueRect);
                    foreach (SerializedProperty prop in SerializedCollectionsEditorUtility.GetDirectChildren(property))
                    {
                        float height = EditorGUI.GetPropertyHeight(prop, true);
                        childRect.height = height;
                        EditorGUI.PropertyField(childRect, prop, true);
                        childRect.y += childRect.height;
                    }
                    EditorGUIUtility.labelWidth = prevLabelWidth;
                    break;
            }
        }

        private void OnAddToList(ReorderableList list)
        {
            _list.serializedProperty.InsertArrayElementAtIndex(list.serializedProperty.arraySize);
        }

        public enum DisplayType
        {
            Field,
            FieldNoLabel,
            ListDrawer
        }
    }
}