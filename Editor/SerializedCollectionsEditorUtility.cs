using System.Collections.Generic;
using UnityEditor;

namespace AYellowpaper.SerializedCollections
{
    internal static class SerializedCollectionsEditorUtility
    {
        public const string EditorPrefsPrefix = "SC_";

        public static bool GetPersistentBool(string path)
        {
            return EditorPrefs.GetBool(EditorPrefsPrefix + path, false);
        }

        public static void SetPersistentBool(string path, bool value)
        {
            EditorPrefs.SetBool(EditorPrefsPrefix + path, value);
        }

        public static float CalculateHeight(SerializedProperty property, bool isDrawOverride)
        {
            if (isDrawOverride)
            {
                float height = 0;
                foreach (SerializedProperty child in GetDirectChildren(property))
                    height += EditorGUI.GetPropertyHeight(child, true);
                return height;
            }

            return EditorGUI.GetPropertyHeight(property, true);
        }

        public static IEnumerable<SerializedProperty> GetDirectChildren(SerializedProperty property)
        {
            if (!property.hasVisibleChildren)
            {
                yield return property;
                yield break;
            }

            SerializedProperty end = property.GetEndProperty();
            property.NextVisible(true);
            do
            {
                yield return property;
            } while (property.NextVisible(false) && !SerializedProperty.EqualContents(property, end));
        }
    }
}