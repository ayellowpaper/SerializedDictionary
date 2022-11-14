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
        private string _detailsText;

        public event Action<KeysGenerator, ModificationType> OnApply;

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
                UpdateDetailsText();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.LabelField(_detailsText);

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            DoModificationToggles();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Result Count: 10 (3 Added, 2 Removed)");
            if (GUILayout.Button("Apply"))
            {
                EditorApplication.delayCall += Apply;

            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void UpdateDetailsText()
        {
            var enumerable = _generator.GetElements(_generatorsData[_selectedIndex].TargetType);
            int count = 0;
            var enumerator = enumerable.GetEnumerator();
            while (enumerator.MoveNext())
            {
                count++;
                if (count > 100)
                {
                    _detailsText = "over 100 Elements";
                    return;
                }
            }
            _detailsText = $"{count} Elements";
        }

        private void Apply()
        {
            OnApply?.Invoke(_editor.target as KeysGenerator, _modificationType);
            OnApply = null;
            Close();
        }

        private void OnDestroy()
        {
            Undo.undoRedoPerformed -= HandleUndoCallback;
            Undo.RevertAllDownToGroup(_undoStart);
            foreach (var keyGenerator in _keysGenerators)
                DestroyImmediate(keyGenerator.Value);
        }

        private void DoModificationToggles()
        {
            DoModificationToggle(EditorGUIUtility.TrTextContent("Add", "Add the generated missing keys to the target."), ModificationType.Add);
            DoModificationToggle(EditorGUIUtility.TrTextContent("Remove", "Remove the generated keys form the target."), ModificationType.Remove);
            DoModificationToggle(EditorGUIUtility.TrTextContent("Confine", "Remove all keys that are not part of the generated keys from the target."), ModificationType.Confine);
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

        private void DoModificationToggle(GUIContent content, ModificationType modificationType)
        {
            if (GUILayout.Toggle(modificationType == _modificationType, content, EditorStyles.toolbarButton))
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

            UpdateDetailsText();
        }

        private KeysGenerator GetOrCreateKeysGenerator(Type type)
        {
            if (!_keysGenerators.ContainsKey(type))
            {
                var so = (KeysGenerator) CreateInstance(type);
                so.hideFlags = HideFlags.DontSave;
                _keysGenerators.Add(type, so);
            }
            return _keysGenerators[type];
        }
    }
}