using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AYellowpaper.SerializedCollections.Editor.States
{
    internal class DefaultListState : ListState
    {
        public override int ListSize => Drawer.ListProperty.minArraySize;

        public DefaultListState(SerializedDictionaryDrawer serializedDictionaryDrawer) : base(serializedDictionaryDrawer)
        {
        }

        public override void OnEnter()
        {
        }

        public override void OnExit()
        {
        }

        public override ListState OnUpdate()
        {
            if (Drawer.SearchText.Length > 0)
                return Drawer.SearchState;

            return this;
        }

        public override void DrawElement(Rect rect, SerializedProperty property, DisplayType displayType)
        {
            SerializedDictionaryDrawer.DrawElement(rect, property, displayType);
        }

        public override SerializedProperty GetPropertyAtIndex(int index)
        {
            return Drawer.ListProperty.GetArrayElementAtIndex(index);
        }
    }
}