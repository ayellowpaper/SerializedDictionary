using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static AYellowpaper.SerializedCollections.Editor.SerializedHashSetDrawer;

namespace AYellowpaper.SerializedCollections.Editor.States
{
    internal class HashSetDefaultListState : HashSetListState
    {
        public override int ListSize => Drawer.ListProperty.minArraySize;

        public HashSetDefaultListState(SerializedHashSetInstanceDrawer serializedHashSetDrawer) : base(serializedHashSetDrawer)
        {
        }

        public override void OnEnter()
        {
            Drawer.ReorderableList.draggable = true;
        }

        public override void OnExit()
        {
        }

        public override HashSetListState OnUpdate()
        {
            if (Drawer.SearchText.Length > 0)
                return Drawer.SearchState;

            return this;
        }

        public override void DrawElement(Rect rect, SerializedProperty property, DisplayType displayType)
        {
            SerializedHashSetInstanceDrawer.DrawElement(rect, property, displayType);
        }

        public override SerializedProperty GetPropertyAtIndex(int index)
        {
            return Drawer.ListProperty.GetArrayElementAtIndex(index);
        }

        public override void RemoveElementAt(int index)
        {
            Drawer.ListProperty.DeleteArrayElementAtIndex(index);
        }

        public override void InserElementAt(int index)
        {
            Drawer.ListProperty.InsertArrayElementAtIndex(index);
            Drawer.ListProperty.serializedObject.ApplyModifiedProperties();
        }
    }
}