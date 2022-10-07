using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AYellowpaper.SerializedCollections.Editor
{
    internal class ElementSettings
    {
        public string PersistentPath;
        public string DisplayName;
        public DisplayType DisplayType;
        public bool HasListDrawerToggle;

        public bool ShowAsList => HasListDrawerToggle && IsListToggleActive;
        public bool IsListToggleActive
        {
            get => SerializedCollectionsEditorUtility.GetPersistentBool(PersistentPath, false);
            set => SerializedCollectionsEditorUtility.SetPersistentBool(PersistentPath, value);
        }
        public DisplayType EffectiveDisplayType => ShowAsList ? DisplayType.List : DisplayType;

        public ElementSettings(string persistentPath, DisplayType displayType, bool hasListDrawerToggle)
        {
            PersistentPath = persistentPath;
            DisplayType = displayType;
            HasListDrawerToggle = hasListDrawerToggle;
        }
    }
}