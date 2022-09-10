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

        private ReorderableList _list;
        private IList _backingList;
        private IConflictCheckable _conflictChecker;
        private FieldInfo _keyFieldInfo;
        private GUIContent _labelContent;
        private Rect _totalRect;
        private GUIStyle _keyValueStyle;
        private SerializedDictionaryAttribute _dictionaryAttribute;
        private Vector2 _scrollPosition;
        private SerializedProperty _listProperty;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 68;
            foreach (SerializedProperty arrayElement in property.FindPropertyRelative("_serializedList"))
                height += Mathf.Max(EditorGUI.GetPropertyHeight(arrayElement.FindPropertyRelative(KeyName), true), EditorGUI.GetPropertyHeight(arrayElement.FindPropertyRelative(ValueName), true));
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_keyValueStyle == null)
            {
                _keyValueStyle = new GUIStyle(EditorStyles.label);
                _keyValueStyle.padding = new RectOffset(0, 0, 0, 0);
                _keyValueStyle.border = new RectOffset(2, 2, 2, 2);
                _keyValueStyle.alignment = TextAnchor.MiddleCenter;
                _keyValueStyle.normal.background = Resources.Load<Texture2D>("SerializedCollections/KeyValueBackground");

                _dictionaryAttribute = fieldInfo.GetCustomAttribute<SerializedDictionaryAttribute>();
                _listProperty = property.FindPropertyRelative("_serializedList");
            }

            _totalRect = position;
            _labelContent = new GUIContent(label);
            var dict = fieldInfo.GetValue(property.serializedObject.targetObject);
            _conflictChecker = dict as IConflictCheckable;
            var listField = fieldInfo.FieldType.GetField("_serializedList", BindingFlags.Instance | BindingFlags.NonPublic);
            _backingList = (IList)listField.GetValue(dict);
            _keyFieldInfo = listField.FieldType.GetGenericArguments()[0].GetField(KeyName);
            EnsureListExists(_listProperty);
            _list.DoList(position);
        }

        private ReorderableList EnsureListExists(SerializedProperty property)
        {
            if (_list != null && _list.serializedProperty != null && _list.serializedProperty.serializedObject == property.serializedObject && _list.serializedProperty.propertyPath == property.propertyPath)
                return _list;

            _list = new ReorderableList(property.serializedObject, property, true, true, true, true);
            _list.onAddCallback += OnAddToList;
            _list.drawElementCallback += OnDrawElement;
            _list.elementHeightCallback += OnGetElementHeight;
            _list.drawHeaderCallback += OnDrawHeader;
            _list.headerHeight *= 2;
            return _list;
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
            bool newValue = EditorGUI.Toggle(toggleRect, SerializedCollectionsEditorUtility.IsSomething(_list.serializedProperty.propertyPath), EditorStyles.miniButton);
            if (EditorGUI.EndChangeCheck())
                SerializedCollectionsEditorUtility.SetIsSomething(_list.serializedProperty.propertyPath, newValue);

            EditorGUI.EndProperty();
        }

        private float OnGetElementHeight(int index)
        {
            var thing = _listProperty.GetArrayElementAtIndex(index);
            return Mathf.Max(EditorGUI.GetPropertyHeight(thing.FindPropertyRelative(KeyName), true), EditorGUI.GetPropertyHeight(thing.FindPropertyRelative(ValueName), true));
        }

        private void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty kvp = _list.serializedProperty.GetArrayElementAtIndex(index);
            int spacing = 2;
            Rect labelPosition = new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth - spacing, EditorGUIUtility.singleLineHeight);
            Rect lineRect = new Rect(labelPosition.x + labelPosition.width + spacing, labelPosition.y, 1, labelPosition.height);
            Rect result = new Rect(lineRect.x + lineRect.width + spacing, lineRect.y, rect.width - lineRect.width - labelPosition.width - spacing, labelPosition.height);

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
            EditorGUI.PropertyField(labelPosition, keyProperty, GUIContent.none, true);
            EditorGUI.DrawRect(lineRect, new Color(36 / 255f, 36 / 255f, 36 / 255f));
            GUI.color = prevColor;

            if (valueProperty.hasChildren)
            {
                bool overrideCustomDrawer = SerializedCollectionsEditorUtility.IsSomething(_list.serializedProperty.propertyPath);
                if (overrideCustomDrawer)
                {
                    foreach (SerializedProperty prop in valueProperty)
                        EditorGUI.PropertyField(result, prop, true);
                }
                else
                    EditorGUI.PropertyField(result, valueProperty, true);
            }
            else
                EditorGUI.PropertyField(result, valueProperty, GUIContent.none, false);
        }

        private void OnAddToList(ReorderableList list)
        {
            _list.serializedProperty.InsertArrayElementAtIndex(list.serializedProperty.arraySize);
        }
    }
}