using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using System.Linq;

namespace AYellowpaper.SerializedCollections.Editor
{
    internal static class SerializedCollectionsEditorUtility
    {
        public const string EditorPrefsPrefix = "SC_";

        public static bool GetPersistentBool(string path, bool defaultValue)
        {
            return EditorPrefs.GetBool(EditorPrefsPrefix + path, defaultValue);
        }

        public static bool HasKey(string path)
        {
            return EditorPrefs.HasKey( EditorPrefsPrefix + path );
        }

        public static void SetPersistentBool(string path, bool value)
        {
            EditorPrefs.SetBool(EditorPrefsPrefix + path, value);
        }

        public static float CalculateHeight(SerializedProperty property, bool drawAsList)
        {
            if (drawAsList)
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

        public static int GetActualArraySize(SerializedProperty arrayProperty)
        {
            return GetDirectChildren(arrayProperty).Count() - 1;
        }

        public static bool HasDrawerForType(Type type)
        {
            Type attributeUtilityType = typeof(SerializedProperty).Assembly.GetType("UnityEditor.ScriptAttributeUtility");
            if (attributeUtilityType == null)
                return false;
            var getDrawerMethod = attributeUtilityType.GetMethod("GetDrawerTypeForType", BindingFlags.Static | BindingFlags.NonPublic);
            if (getDrawerMethod == null)
                return false;
            return getDrawerMethod.Invoke(null, new object[] { type }) != null;
        }
    }
}