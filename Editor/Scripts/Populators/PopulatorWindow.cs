using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AYellowpaper.SerializedCollections.Editor;

namespace AYellowpaper.SerializedCollections.Populators
{
    public class PopulatorWindow : EditorWindow
    {
        [SerializeField]
        private bool _removeOtherKeys = false;

        private Populator _populator;
        private UnityEditor.Editor _populatorEditor;
        private bool _heightWasInitialized = false;
        private GUIContent _descriptionContent;

        private System.Action<PopulatorWindowEventArgs> _callback;

        private void OnGUI()
        {
            GUILayout.Label(_descriptionContent);
            _populatorEditor.OnInspectorGUI();
            GUILayout.FlexibleSpace();
            //_removeOtherKeys = GUILayout.Toggle(_removeOtherKeys, EditorGUIUtility.TrTextContent("remove other keys"));
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel"))
                Close();
            if (GUILayout.Button("Ok"))
            {
                Close();
                _callback?.Invoke(new PopulatorWindowEventArgs(_populator));
            }
            EditorGUILayout.EndHorizontal();
            if (!_heightWasInitialized && Event.current.type == EventType.Repaint)
            {
                _heightWasInitialized = true;
                var rect = GUILayoutUtility.GetLastRect();
                position = position.WithHeight(rect.height);
            }
        }

        public void Init(Populator populator, System.Action<PopulatorWindowEventArgs> callback)
        {
            _populator = populator;
            _populatorEditor = UnityEditor.Editor.CreateEditor(_populator);
            titleContent = new GUIContent(_populator.Title);
            _descriptionContent = new GUIContent(_populator.Description);
            _callback = callback;
        }
    }

    public class PopulatorWindowEventArgs : System.EventArgs
    {
        public Populator Populator { get; private set; }

        public PopulatorWindowEventArgs(Populator populator)
        {
            Populator = populator;
        }
    }
}