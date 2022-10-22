using AYellowpaper.SerializedCollections.Editor.Data;
using AYellowpaper.SerializedCollections.Populators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace AYellowpaper.SerializedCollections.Editor
{
    [CustomPropertyDrawer(typeof(SerializedDictionary<,>))]
    public class SerializedDictionaryDrawer : PropertyDrawer
    {
        public const string KeyName = nameof(SerializedKeyValuePair<int, int>.Key);
        public const string ValueName = nameof(SerializedKeyValuePair<int, int>.Value);
        public const string SerializedListName = nameof(SerializedDictionary<int, int>._serializedList);
        public const string LookupTableName = nameof(SerializedDictionary<int, int>.LookupTable);

        private const int NotExpandedHeight = 20;
        private const bool KeyFlag = true;
        private const bool ValueFlag = false;
        private static readonly List<int> NoEntriesList = new List<int>();

        private bool _initialized = false;
        private ReorderableList _expandedList;
        private ReorderableList _unexpandedList;
        private SingleEditingData _singleEditing;
        private Type _entryType;
        private FieldInfo _keyFieldInfo;
        private GUIContent _label;
        private Rect _totalRect;
        private GUIStyle _keyValueStyle;
        private SerializedDictionaryAttribute _dictionaryAttribute;
        private SerializedProperty _listProperty;
        private PropertyData _propertyData;
        private bool _propertyListSettingsInitialized = false;
        private List<int> _listOfIndices;
        private PagingElement _pagingElement;
        private int _elementsPerPage = 5;
        private int _lastArraySize = -1;
        private IReadOnlyList<PopulatorData> _populators;
        private Action _queuedAction;

        private class SingleEditingData
        {
            public bool IsValid => BackingList != null;
            public IList BackingList;
            public ILookupTable LookupTable;

            public void Invalidate()
            {
                BackingList = null;
                LookupTable = null;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            InitializeIfNeeded(property);

            _totalRect = position;
            _label = new GUIContent(label);

            if (_listProperty.isExpanded)
                _expandedList.DoList(position);
            else
            {
                using (new GUI.ClipScope(new Rect(0, position.y, position.width + position.x, NotExpandedHeight)))
                {
                    _unexpandedList.DoList(position.WithY(0));
                }
            }

            if (_queuedAction != null)
            {
                _queuedAction();
                _queuedAction = null;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            InitializeIfNeeded(property);

            if (!_listProperty.isExpanded)
                return NotExpandedHeight;

            float height = 68;
            if (_listProperty.minArraySize == 0)
                height += 20;
            foreach (int index in _listOfIndices)
                height += CalculateHeightOfElement(_listProperty.GetArrayElementAtIndex(index), _propertyData.GetElementData(KeyFlag).EffectiveDisplayType == DisplayType.List ? true : false, _propertyData.GetElementData(ValueFlag).EffectiveDisplayType == DisplayType.List ? true : false);
            return height;
        }

        private SerializedProperty GetElementProperty(SerializedProperty property, bool fieldFlag)
        {
            return property.FindPropertyRelative(fieldFlag == KeyFlag ? KeyName : ValueName);
        }

        private static float CalculateHeightOfElement(SerializedProperty property, bool drawKeyAsList, bool drawValueAsList)
        {
            SerializedProperty keyProperty = property.FindPropertyRelative(KeyName);
            SerializedProperty valueProperty = property.FindPropertyRelative(ValueName);
            return Mathf.Max(SCEditorUtility.CalculateHeight(keyProperty, drawKeyAsList), SCEditorUtility.CalculateHeight(valueProperty, drawValueAsList));
        }

        private void InitializeIfNeeded(SerializedProperty property)
        {
            property.serializedObject.Update();
            _listProperty = property.FindPropertyRelative(SerializedListName);

            if (!_initialized)
            {
                _initialized = true;
                _keyValueStyle = new GUIStyle(EditorStyles.toolbarButton);
                _keyValueStyle.padding = new RectOffset(0, 0, 0, 0);
                _keyValueStyle.border = new RectOffset(0, 0, 0, 0);
                _keyValueStyle.alignment = TextAnchor.MiddleCenter;

                _dictionaryAttribute = fieldInfo.GetCustomAttribute<SerializedDictionaryAttribute>();

                _pagingElement = new PagingElement();
                _listOfIndices = new List<int>();
                UpdatePaging();

                _expandedList = MakeList();
                _unexpandedList = MakeUnexpandedList();

                var listField = fieldInfo.FieldType.GetField(SerializedListName, BindingFlags.Instance | BindingFlags.NonPublic);
                _entryType = listField.FieldType.GetGenericArguments()[0];
                _keyFieldInfo = _entryType.GetField(KeyName);

                _singleEditing = new SingleEditingData();

                _populators = PopulatorCache.GetPopulatorsForType(_keyFieldInfo.FieldType);

                _propertyData = SCEditorUtility.GetPropertyData(_listProperty);
                _propertyData.GetElementData(SCEditorUtility.KeyFlag).Settings.DisplayName = _dictionaryAttribute?.KeyName ?? "Key";
                _propertyData.GetElementData(SCEditorUtility.ValueFlag).Settings.DisplayName = _dictionaryAttribute?.ValueName ?? "Value";
            }

            void InitializeSettings(bool fieldFlag)
            {
                var genericArgs = fieldInfo.FieldType.GetGenericArguments();
                var firstProperty = _listProperty.GetArrayElementAtIndex(0);
                var keySettings = CreateDisplaySettings(GetElementProperty(firstProperty, fieldFlag), genericArgs[fieldFlag == SCEditorUtility.KeyFlag ? 0 : 1]);
                var settings = _propertyData.GetElementData(fieldFlag).Settings;
                settings.DisplayType = keySettings.displayType;
                settings.HasListDrawerToggle = keySettings.canToggleListDrawer;
            }

            if (!_propertyListSettingsInitialized && _listProperty.minArraySize > 0)
            {
                _propertyListSettingsInitialized = true;
                InitializeSettings(SCEditorUtility.KeyFlag);
                InitializeSettings(SCEditorUtility.ValueFlag);
            }

            // TODO: Is there a better solution to check for Revert/delete/add?
            if (_lastArraySize != _listProperty.minArraySize)
            {
                _lastArraySize = _listProperty.minArraySize;
                UpdateSingleEditing();
                UpdatePaging();
            }
        }

        private void UpdateSingleEditing()
        {
            if (_listProperty.serializedObject.isEditingMultipleObjects && _singleEditing.IsValid)
                _singleEditing.Invalidate();
            else if (!_listProperty.serializedObject.isEditingMultipleObjects && !_singleEditing.IsValid)
            {
                _singleEditing.BackingList = GetBackingList(_listProperty.serializedObject.targetObject);
                _singleEditing.LookupTable = GetLookupTableFromObject(_listProperty.serializedObject.targetObject);
            }
        }

        private ILookupTable GetLookupTableFromObject(UnityEngine.Object targetObject)
        {
            var dictionary = fieldInfo.GetValue(targetObject);
            var propInfo = dictionary.GetType().GetProperty(LookupTableName, BindingFlags.Instance | BindingFlags.NonPublic);
            return (ILookupTable)propInfo.GetValue(dictionary);
        }

        private IList GetBackingList(object targetObject)
        {
            var listField = fieldInfo.FieldType.GetField(SerializedListName, BindingFlags.Instance | BindingFlags.NonPublic);
            var dictionary = fieldInfo.GetValue(targetObject);
            return (IList)listField.GetValue(dictionary);
        }

        private void UpdatePaging()
        {
            _pagingElement.PageCount = Mathf.Max(1, Mathf.CeilToInt((float)_listProperty.minArraySize / _elementsPerPage));

            _listOfIndices.Clear();
            _listOfIndices.Capacity = Mathf.Max(_elementsPerPage, _listOfIndices.Capacity);

            int startIndex = (_pagingElement.Page - 1) * _elementsPerPage;
            int endIndex = Mathf.Min(startIndex + _elementsPerPage, _listProperty.minArraySize);
            for (int i = startIndex; i < endIndex; i++)
                _listOfIndices.Add(i);
        }

        private ReorderableList MakeList()
        {
            var list = new ReorderableList(_listOfIndices, typeof(int), true, true, true, true);
            list.onAddCallback += OnAdd;
            list.onRemoveCallback += OnRemove;
            list.onReorderCallbackWithDetails += OnReorder;
            list.drawElementCallback += OnDrawElement;
            list.elementHeightCallback += OnGetElementHeight;
            list.drawHeaderCallback += OnDrawHeader;
            list.headerHeight *= 2;
            return list;
        }

        private ReorderableList MakeUnexpandedList()
        {
            var list = new ReorderableList(NoEntriesList, typeof(int));
            list.drawHeaderCallback = DrawUnexpandedHeader;
            return list;
        }

        private (DisplayType displayType, bool canToggleListDrawer) CreateDisplaySettings(SerializedProperty property, Type type)
        {
            bool hasCustomEditor = SCEditorUtility.HasDrawerForType(type);
            bool isGenericWithChildren = property.propertyType == SerializedPropertyType.Generic && property.hasVisibleChildren;
            bool isArray = property.isArray && property.propertyType != SerializedPropertyType.String;
            bool canToggleListDrawer = isArray || (isGenericWithChildren && hasCustomEditor);
            DisplayType displayType = DisplayType.PropertyNoLabel;
            if (canToggleListDrawer)
                displayType = DisplayType.Property;
            else if (!isArray && isGenericWithChildren && !hasCustomEditor)
                displayType = DisplayType.List;
            return (displayType, canToggleListDrawer);
        }

        private void DrawUnexpandedHeader(Rect rect)
        {
            EditorGUI.BeginProperty(rect, _label, _listProperty);
            _listProperty.isExpanded = EditorGUI.Foldout(rect.WithX(rect.x - 5), _listProperty.isExpanded, _label, true);
            EditorGUI.EndProperty();
        }

        private void DoOptionsButton(Rect rect)
        {
            if (EditorGUI.DropdownButton(rect, EditorGUIUtility.IconContent("d_Grid.FillTool"), FocusType.Passive))
            {
                var gm = new GenericMenu();
                SCEditorUtility.AddGenericMenuItem(gm, true, new GUIContent("Clear"), () => QueueAction(() => _listProperty.ClearArray()));
                gm.AddSeparator(string.Empty);
                SCEditorUtility.AddGenericMenuItem(gm, _singleEditing.IsValid, new GUIContent("Remove Conflicts"), () => QueueAction(RemoveConflicts));
                foreach (var populatorData in _populators)
                {
                    SCEditorUtility.AddGenericMenuItem(gm, _singleEditing.IsValid, new GUIContent(populatorData.Name), OnPopulatorDataSelected, populatorData.PopulatorType);
                }
                gm.DropDown(rect);
            }
        }

        private void DoPaging(Rect rect)
        {
            EditorGUI.BeginChangeCheck();
            _pagingElement.OnGUI(rect);
            if (EditorGUI.EndChangeCheck())
                UpdatePaging();
        }

        private void OnDrawHeader(Rect rect)
        {
            Rect topRect = rect.WithHeight(rect.height / 2);
            Rect lastTopRect = topRect.Append(0);

            if (_pagingElement.PageCount > 1)
            {
                lastTopRect = lastTopRect.Prepend(_pagingElement.GetDesiredWidth());
                DoPaging(lastTopRect);
                lastTopRect = lastTopRect.Prepend(5);
            }

            lastTopRect = lastTopRect.Prepend(30);
            DoOptionsButton(lastTopRect);

            if (!_singleEditing.IsValid)
            {
                lastTopRect = lastTopRect.Prepend(lastTopRect.height + 5);
                var guicontent = EditorGUIUtility.TrIconContent(EditorGUIUtility.Load("d_console.infoicon") as Texture, "Conflict checking, duplicate key removal and populators not supported in multi object editing mode.");
                GUI.Label(lastTopRect, guicontent);
            }

            EditorGUI.BeginProperty(topRect, _label, _listProperty);
            _listProperty.isExpanded = EditorGUI.Foldout(topRect.WithXAndWidth(topRect.x - 5, lastTopRect.x - topRect.x), _listProperty.isExpanded, _label, true);

            Rect bottomRect = new Rect(_totalRect.x + 1, topRect.y + topRect.height, _totalRect.width - 1, rect.height - topRect.height);

            float width = EditorGUIUtility.labelWidth + 22;
            Rect leftRect = new Rect(bottomRect.x, bottomRect.y, width, bottomRect.height);
            Rect rightRect = new Rect(bottomRect.x + width, bottomRect.y, bottomRect.width - width, bottomRect.height);

            if (Event.current.type == EventType.Repaint && _propertyData != null)
            {
                _keyValueStyle.Draw(leftRect, EditorGUIUtility.TrTextContent(_propertyData.GetElementData(KeyFlag).Settings.DisplayName), false, false, false, false);
                _keyValueStyle.Draw(rightRect, EditorGUIUtility.TrTextContent(_propertyData.GetElementData(ValueFlag).Settings.DisplayName), false, false, false, false);
            }

            if (_listProperty.minArraySize > 0)
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

        private void OnPopulatorDataSelected(object userdata)
        {
            var populator = (Populator)ScriptableObject.CreateInstance((Type)userdata);

            if (populator.RequiresWindow)
            {
                var window = EditorWindow.GetWindow<PopulatorWindow>();
                window.Init(populator, x => QueueAction(() => ApplyPopulator(x.Populator)));
                window.ShowModal();
            }
            else
                QueueAction(() => ApplyPopulator(populator));
        }

        private void QueueAction(Action action)
        {
            _queuedAction = action;
        }

        private void ApplyPopulator(Populator populator)
        {
            var elements = populator.GetElements(_keyFieldInfo.FieldType);
            object entry = Activator.CreateInstance(_entryType);

            foreach (var targetObject in _listProperty.serializedObject.targetObjects)
            {
                Undo.RecordObject(targetObject, "Populate");
                var lookupTable = GetLookupTableFromObject(targetObject);
                var list = GetBackingList(targetObject);
                foreach (var key in elements)
                {
                    var occurences = lookupTable.GetOccurences(key);
                    if (occurences.Count > 0)
                        continue;
                    _keyFieldInfo.SetValue(entry, key);
                    list.Add(entry);
                }
                // TODO: This is only done because OnAfterDeserialize doesn't fire. Not really obvious why this has to be called manually here
                lookupTable.RecalculateOccurences();
                PrefabUtility.RecordPrefabInstancePropertyModifications(targetObject);
            }

            _listProperty.serializedObject.Update();
        }

        private void RemoveConflicts()
        {
            foreach (var targetObject in _listProperty.serializedObject.targetObjects)
            {
                Undo.RecordObject(targetObject, "Populate");
                var lookupTable = GetLookupTableFromObject(targetObject);
                var list = GetBackingList(targetObject);

                List<int> duplicateIndices = new List<int>();

                foreach (var key in lookupTable.Keys)
                {
                    var occurences = lookupTable.GetOccurences(key);
                    for (int i = 1; i < occurences.Count; i++)
                        duplicateIndices.Add(occurences[i]);

                }

                foreach (var indexToRemove in duplicateIndices.OrderByDescending(x => x))
                {
                    list.RemoveAt(indexToRemove);
                }

                // TODO: This is only done because OnAfterDeserialize doesn't fire. Not really obvious why this has to be called manually here
                lookupTable.RecalculateOccurences();
                PrefabUtility.RecordPrefabInstancePropertyModifications(targetObject);
            }
        }

        private void DoDisplayTypeToggle(Rect contentRect, bool fieldFlag)
        {
            var displayData = _propertyData.GetElementData(fieldFlag);

            if (displayData.Settings.HasListDrawerToggle)
            {
                Rect rightRectToggle = new Rect(contentRect);
                rightRectToggle.x += rightRectToggle.width - 18;
                rightRectToggle.width = 18;
                EditorGUI.BeginChangeCheck();
                bool newValue = GUI.Toggle(rightRectToggle, displayData.IsListToggleActive, "", EditorStyles.toolbarButton);
                if (EditorGUI.EndChangeCheck())
                    displayData.IsListToggleActive = newValue;
            }
        }

        private float OnGetElementHeight(int index)
        {
            var element = _listProperty.GetArrayElementAtIndex(_listOfIndices[index]);
            return CalculateHeightOfElement(element, _propertyData.GetElementData(KeyFlag).EffectiveDisplayType == DisplayType.List ? true : false, _propertyData.GetElementData(ValueFlag).EffectiveDisplayType == DisplayType.List ? true : false);
        }

        private void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            const int leftSpace = 2;
            const int lineWidth = 1;
            const int rightSpace = 12;
            const int totalSpace = leftSpace + lineWidth + rightSpace;

            int actualIndex = _listOfIndices[index];

            SerializedProperty kvp = _listProperty.GetArrayElementAtIndex(actualIndex);
            Rect keyRect = new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth - leftSpace, EditorGUIUtility.singleLineHeight);
            Rect lineRect = new Rect(keyRect.x + keyRect.width + leftSpace, keyRect.y, lineWidth, rect.height);
            Rect valueRect = new Rect(keyRect.x + keyRect.width + totalSpace, keyRect.y, rect.width - keyRect.width - totalSpace, keyRect.height);

            var keyProperty = kvp.FindPropertyRelative(KeyName);
            var valueProperty = kvp.FindPropertyRelative(ValueName);

            Color prevColor = GUI.color;
            if (_singleEditing.IsValid)
            {
                var keyObject = _keyFieldInfo.GetValue(_singleEditing.BackingList[actualIndex]);
                var occurences = _singleEditing.LookupTable.GetOccurences(keyObject);
                if (occurences.Count > 1)
                {
                    GUI.color = occurences[0] == actualIndex ? Color.yellow : Color.red;
                }
                if (!SerializedCollectionsUtility.IsValidKey(keyObject))
                {
                    GUI.color = Color.red;
                }
            }

            var keyDisplayData = _propertyData.GetElementData(KeyFlag);
            DrawGroupedElement(keyRect, 20, keyProperty, keyDisplayData.EffectiveDisplayType);

            EditorGUI.DrawRect(lineRect, new Color(36 / 255f, 36 / 255f, 36 / 255f));
            GUI.color = prevColor;

            var valueDisplayData = _propertyData.GetElementData(ValueFlag);
            DrawGroupedElement(valueRect, rightSpace, valueProperty, valueDisplayData.EffectiveDisplayType);
        }

        private void DrawGroupedElement(Rect rect, int space, SerializedProperty property, DisplayType displayType)
        {
            const int propertyOffset = 5;

            using (new LabelWidth(rect.width * 0.4f))
            {
                float height = SCEditorUtility.CalculateHeight(property.Copy(), displayType == DisplayType.List ? true : false);
                Rect groupRect = new Rect(rect.x - space, rect.y, rect.width + space, height);
                GUI.BeginGroup(groupRect);

                Rect elementRect = new Rect(space, 0, rect.width, height);
                DrawElement(elementRect, property, displayType);

                // Apply clip for property here as to not show the blue line for modification but still allow reverting the property. Reduces visual noise
                GUI.BeginClip(elementRect.WithXAndWidth(-propertyOffset, propertyOffset + space));
                EditorGUI.BeginProperty(elementRect.WithXAndWidth(propertyOffset, space), GUIContent.none, property);
                EditorGUI.EndProperty();
                GUI.EndClip();

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
                    foreach (SerializedProperty prop in SCEditorUtility.GetDirectChildren(property.Copy()))
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

        private void OnAdd(ReorderableList list)
        {
            int targetIndex = list.index >= 0 ? list.index : _listProperty.minArraySize;
            _listProperty.InsertArrayElementAtIndex(targetIndex);
        }

        private void OnReorder(ReorderableList list, int oldIndex, int newIndex)
        {
            UpdatePaging();
            _listProperty.MoveArrayElement(_listOfIndices[oldIndex], _listOfIndices[newIndex]);
        }

        private void OnRemove(ReorderableList list)
        {
            int actualIndex = _listOfIndices[list.index];
            _listProperty.DeleteArrayElementAtIndex(actualIndex);
            UpdatePaging();
            if (actualIndex >= _listProperty.minArraySize)
                list.index = _listOfIndices.Count - 1;
        }
    }
}