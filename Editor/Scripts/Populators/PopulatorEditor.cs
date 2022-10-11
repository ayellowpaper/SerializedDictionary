using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AYellowpaper.SerializedCollections.Populators
{
    [CustomEditor(typeof(Populator), true)]
    public class PopulatorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
        }
    }
}