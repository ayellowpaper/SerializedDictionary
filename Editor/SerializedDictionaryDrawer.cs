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
                _keyValueStyle = new GUIStyle(EditorStyles.label);
                _keyValueStyle.padding = new RectOffset(0, 0, 0, 0);
                _keyValueStyle.border = new RectOffset(2, 2, 2, 2);
                _keyValueStyle.alignment = TextAnchor.MiddleCenter;
                _keyValueStyle.normal.background = Resources.Load<Texture2D>("SerializedCollections/KeyValueBackground");

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

            if (Event.current.type == EventType.Repaint)
            {
                Rect bottomRect = _totalRect;
                bottomRect.y = topRect.y + topRect.height - 1;
                bottomRect.height = rect.height - topRect.height + 2;

                float width = EditorGUIUtility.labelWidth + 22;
                Rect leftRect = new Rect(bottomRect.x, bottomRect.y, width, bottomRect.height);
                Rect rightRect = new Rect(bottomRect.x + width, bottomRect.y, bottomRect.width - width, bottomRect.height);

                _keyValueStyle.Draw(leftRect, EditorGUIUtility.TrTextContent(_dictionaryAttribute?.KeyName ?? "Key"), 0, false);
                _keyValueStyle.Draw(rightRect, EditorGUIUtility.TrTextContent(_dictionaryAttribute?.ValueName ?? "Value"), 0, false);
            }

            Rect toggleRect = topRect;
            toggleRect.width = 14;
            toggleRect.height = 14;
            toggleRect.x = topRect.x + topRect.width - 14;
            EditorGUI.BeginChangeCheck();
            bool newValue = EditorGUI.Toggle(toggleRect, GetOverride(ValueFlag), EditorStyles.miniButton);
            if (EditorGUI.EndChangeCheck())
                SetOverride(ValueFlag, newValue);

            EditorGUI.EndProperty();
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
            EditorGUI.PropertyField(keyRect, keyProperty, GUIContent.none, true);
            EditorGUI.DrawRect(lineRect, new Color(36 / 255f, 36 / 255f, 36 / 255f));
            GUI.color = prevColor;

            if (valueProperty.hasVisibleChildren)
            {
                bool overrideCustomDrawer = GetOverride(ValueFlag);
                if (overrideCustomDrawer)
                {
                    Rect childRect = new Rect(valueRect);
                    foreach (SerializedProperty prop in SerializedCollectionsEditorUtility.GetDirectChildren(valueProperty))
                    {
                        float height = EditorGUI.GetPropertyHeight(prop, true);
                        childRect.height = height;
                        EditorGUI.PropertyField(childRect, prop, true);
                        childRect.y += childRect.height;
                    }
                }
                else
                    EditorGUI.PropertyField(valueRect, valueProperty, true);
            }
            else
                EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none, false);
        }

        private void OnAddToList(ReorderableList list)
        {
            _list.serializedProperty.InsertArrayElementAtIndex(list.serializedProperty.arraySize);
        }
    }
}