using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AYellowpaper.SerializedCollections.Editor.States
{
    internal abstract class ListState
    {
        public abstract int ListSize { get; }

        public readonly SerializedDictionaryDrawer Drawer;

        public ListState(SerializedDictionaryDrawer serializedDictionaryDrawer)
        {
            Drawer = serializedDictionaryDrawer;
        }

        public abstract SerializedProperty GetPropertyAtIndex(int index);
        public abstract ListState OnUpdate();
        public abstract void OnEnter();
        public abstract void OnExit();
        public abstract void DrawElement(Rect rect, SerializedProperty property, DisplayType displayType);

        public virtual float GetHeightAtIndex(int index, bool drawKeyAsList, bool drawValueAsList)
        {
            return SerializedDictionaryDrawer.CalculateHeightOfElement(GetPropertyAtIndex(index), drawKeyAsList, drawValueAsList);
        }
    }
}