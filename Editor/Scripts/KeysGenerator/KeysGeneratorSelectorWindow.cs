using AYellowpaper.SerializedCollections.Editor;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace AYellowpaper.SerializedCollections.Populators
{
    public class KeysGeneratorSelectorWindow : EditorWindow
    {
        private static readonly Color BorderColor = new Color(36 / 255f, 36 / 255f, 36 / 255f);

        [SerializeField]
        private int _selectedIndex;
        [SerializeField]
        private ModificationType _modificationType;

        private KeysGenerator _generator;
        private UnityEditor.Editor _editor;
        private List<KeysGeneratorData> _generatorsData;
        private int _undoStart;
        private Dictionary<Type, KeysGenerator> _keysGenerators = new Dictionary<Type, KeysGenerator>();

        private void OnGUI()
        {
            Rect rect = position.WithPosition(Vector2.zero);

            DrawBorders(rect);
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));

            EditorGUILayout.BeginVertical(EditorStyles.toolbar, GUILayout.Width(100), GUILayout.ExpandHeight(true));
            DoGeneratorsToggles();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginScrollView(Vector2.zero, GUILayout.ExpandHeight(true));
            _editor.serializedObject.UpdateIfRequiredOrScript();
            EditorGUI.BeginChangeCheck();
            _editor.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                //var enumerable = _generator.GetElements(_generatorsData[_selectedIndex].TargetType);
                //int i = 0;
                //var enumerator = enumerable.GetEnumerator();
                //while (enumerator.MoveNext())
                //    i++;
                //Debug.Log(i);
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            DoModificationToggles();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("3 Elements");
            GUILayout.Button("Apply");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void OnDisable()
        {
            Undo.RevertAllDownToGroup(_undoStart);
            foreach (var test in _keysGenerators)
            {
                DestroyImmediate(test.Value);
            }
        }

        private void DoModificationToggles()
        {
            DoModificationToggle("Add", ModificationType.Add);
            DoModificationToggle("Set", ModificationType.Set);
            DoModificationToggle("Remove", ModificationType.Remove);
        }

        private void DoGeneratorsToggles()
        {
            for (int i = 0; i < _generatorsData.Count; i++)
            {
                var generatorData = _generatorsData[i];
                EditorGUI.BeginChangeCheck();
                GUILayout.Toggle(i == _selectedIndex, generatorData.Name, EditorStyles.toolbarButton);
                if (EditorGUI.EndChangeCheck())
                    SetGeneratorIndex(i);
            }
        }

        private void DoModificationToggle(string label, ModificationType modificationType)
        {
            if (GUILayout.Toggle(modificationType == _modificationType, label, EditorStyles.toolbarButton))
                _modificationType = modificationType;
        }

        private static void DrawBorders(Rect rect)
        {
            EditorGUI.DrawRect(rect.AppendLeft(1, -1), BorderColor);
            EditorGUI.DrawRect(rect.AppendDown(1, -1), BorderColor);
            EditorGUI.DrawRect(rect.AppendRight(1, -1), BorderColor);
            EditorGUI.DrawRect(rect.AppendUp(1, -1), BorderColor);
        }

        public void Initialize(IEnumerable<KeysGeneratorData> generatorsData)
        {
            _selectedIndex = 0;
            _modificationType = ModificationType.Add;
            _undoStart = Undo.GetCurrentGroup();
            _generatorsData = new List<KeysGeneratorData>(generatorsData);
            SetGeneratorIndex(0);
            Undo.undoRedoPerformed += HandleUndoCallback;
        }

        private void HandleUndoCallback()
        {
            UpdateGeneratorAndEditorIfNeeded();
            Repaint();
        }

        private void SetGeneratorIndex(int index)
        {
            Undo.RecordObject(this, "Change Window");
            _selectedIndex = index;
            UpdateGeneratorAndEditorIfNeeded();

        }

        private void UpdateGeneratorAndEditorIfNeeded()
        {
            var targetType = _generatorsData[_selectedIndex].GeneratorType;
            if (_generator != null && _generator.GetType() == targetType)
                return;

            _generator = GetOrCreateKeysGenerator(targetType);
            if (_editor != null)
                DestroyImmediate(_editor);
            _editor = UnityEditor.Editor.CreateEditor(_generator);
        }

        private KeysGenerator GetOrCreateKeysGenerator(Type type)
        {
            if (!_keysGenerators.ContainsKey(type))
            {
                var so = (KeysGenerator) CreateInstance(type);
                _keysGenerators.Add(type, so);
            }
            return _keysGenerators[type];
        }
    }
}