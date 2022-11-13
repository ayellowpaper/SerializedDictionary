using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AYellowpaper.SerializedCollections.Populators
{
    [CustomEditor(typeof(KeysGenerator), true)]
    public class KeysGeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var iterator = serializedObject.GetIterator();
            if (iterator.Next(true))
            {
                // skip script name
                iterator.NextVisible(true);
                while (iterator.NextVisible(true))
                {
                    EditorGUILayout.PropertyField(iterator);
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}