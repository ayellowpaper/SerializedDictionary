using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AYellowpaper.SerializedCollections.Editor
{
    internal class ElementDisplaySettings
    {
        public string PersistentPath;
        public DisplayType DisplayType;
        public bool HasListDrawerToggle;
        public string DisplayName;

        public bool ShowAsList => HasListDrawerToggle && IsListToggleActive;
        public bool IsListToggleActive
        {
            get => SerializedCollectionsEditorUtility.GetPersistentBool(PersistentPath, false);
            set => SerializedCollectionsEditorUtility.SetPersistentBool(PersistentPath, value);
        }
        public DisplayType EffectiveDisplayType => ShowAsList ? DisplayType.List : DisplayType;

        public ElementDisplaySettings(string persistentPath, DisplayType displayType, bool hasListDrawerToggle)
        {
            PersistentPath = persistentPath;
            DisplayType = displayType;
            HasListDrawerToggle = hasListDrawerToggle;
        }
    }
}