using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AYellowpaper.SerializedCollections
{
    public static class SerializedCollectionsEditorUtility
    {
        public static bool IsSomething( string path )
        {
            return UnityEditor.EditorPrefs.GetBool("SC/" + path, false);
        }

        public static void SetIsSomething( string path, bool value )
        {
            UnityEditor.EditorPrefs.SetBool("SC/" + path, value);
        }
    }
}