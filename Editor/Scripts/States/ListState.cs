using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static AYellowpaper.SerializedCollections.Editor.SerializedDictionaryDrawer;

namespace AYellowpaper.SerializedCollections.Editor.States
{
    internal abstract class ListState
    {
        public abstract int ListSize { get; }
        public virtual string NoElementsText => "List is Empty.";

        public readonly DictionaryDrawer Drawer;

        public ListState(DictionaryDrawer serializedDictionaryDrawer)
        {
            Drawer = serializedDictionaryDrawer;
        }

        public abstract SerializedProperty GetPropertyAtIndex(int index);
        public abstract ListState OnUpdate();
        public abstract void OnEnter();
        public abstract void OnExit();
        public abstract void DrawElement(Rect rect, SerializedProperty property, DisplayType displayType);
        public abstract void RemoveElementAt(int index);
        public abstract void InserElementAt(int index);

        public virtual float GetHeightAtIndex(int index, bool drawKeyAsList, bool drawValueAsList)
        {
            return DictionaryDrawer.CalculateHeightOfElement(GetPropertyAtIndex(index), drawKeyAsList, drawValueAsList);
        }
    }
}