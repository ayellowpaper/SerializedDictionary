using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        private float _contentHeight;
        private float _remainingOffset;
        private float _remainingHeight;
        private int _previouslyRenderedIndex = int.MaxValue;
        private SerializedProperty _listProperty;
        private List<EntryData> _entryDatas = new List<EntryData>();
        private float _labelWidth;

        private class EntryData
        {
            public readonly float RenderOffset;
            public readonly float DesiredHeight;
            public readonly float ActualHeight;

            public EntryData(float offset, float desiredHeight, float actualHeight)
            {
                RenderOffset = offset;
                DesiredHeight = desiredHeight;
                ActualHeight = actualHeight;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 400;
        }

        private void RecalculateHeights()
        {
            _contentHeight = 0;
            _entryDatas.Clear();
            for (int i = 0; i < _listProperty.arraySize; i++)
            {
                SerializedProperty kvp = _listProperty.GetArrayElementAtIndex(i);
                var keyProperty = kvp.FindPropertyRelative(KeyName);
                var valueProperty = kvp.FindPropertyRelative(ValueName);

                float desiredPropertyHeight = Mathf.Max(EditorGUI.GetPropertyHeight(keyProperty, true), EditorGUI.GetPropertyHeight(valueProperty, true)) + 2;
                float actualHeight = desiredPropertyHeight;
                float offset = 0;

                if (_remainingOffset > 0)
                {
                    float deducted = Mathf.Min(actualHeight, _remainingOffset);
                    actualHeight -= deducted;
                    _remainingOffset -= deducted;
                    if (_remainingOffset <= 0)
                        offset = -deducted;
                }
                if (actualHeight > 0)
                {
                    float usedHeight = Mathf.Min(actualHeight, _remainingHeight);
                    actualHeight = usedHeight;
                    _remainingHeight -= usedHeight;
                }

                _contentHeight += desiredPropertyHeight;
                _entryDatas.Add(new EntryData(offset, desiredPropertyHeight, actualHeight) );
            }
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

            _labelWidth = EditorGUIUtility.labelWidth;

            Rect scrollViewOverlay = position;
            scrollViewOverlay.y += 40;
            scrollViewOverlay.height -= 100;
            _scrollPosition = GUI.BeginScrollView(scrollViewOverlay, _scrollPosition, new Rect(position.x, position.y, position.width - 20, _contentHeight));
            GUI.EndScrollView(true);

            _totalRect = position;
            _labelContent = new GUIContent(label);
            var dict = fieldInfo.GetValue(property.serializedObject.targetObject);
            _conflictChecker = dict as IConflictCheckable;
            var listField = fieldInfo.FieldType.GetField("_serializedList", BindingFlags.Instance | BindingFlags.NonPublic);
            _backingList = (IList)listField.GetValue(dict);
            _keyFieldInfo = listField.FieldType.GetGenericArguments()[0].GetField(KeyName);
            EnsureListExists(_listProperty);
            Rect listRect = position;
            listRect.width -= 13;
            _list.DoList(listRect);
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
            if (_previouslyRenderedIndex + 1 != index)
            {
                _remainingOffset = _scrollPosition.y;
                _remainingHeight = 300;
                RecalculateHeights();
            }
            _previouslyRenderedIndex = index;

            return _entryDatas[index].ActualHeight;
        }

        private void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (rect.height <= 0 && Event.current.type == EventType.Repaint)
                return;

            GUI.BeginClip(rect);
            rect.x = 0;
            rect.y = _entryDatas[index].RenderOffset;

            SerializedProperty kvp = _list.serializedProperty.GetArrayElementAtIndex(index);
            int spacing = 2;
            Rect labelPosition = new Rect(rect.x, rect.y, _labelWidth - spacing, EditorGUIUtility.singleLineHeight);
            Rect lineRect = new Rect(labelPosition.x + labelPosition.width + spacing, labelPosition.y, 1, _entryDatas[index].DesiredHeight);
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

            GUI.EndClip();
        }

        private void OnAddToList(ReorderableList list)
        {
            _list.serializedProperty.InsertArrayElementAtIndex(list.serializedProperty.arraySize);
        }
    }
}