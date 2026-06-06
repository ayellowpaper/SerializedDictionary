using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static AYellowpaper.SerializedCollections.Editor.SerializedHashSetDrawer;

namespace AYellowpaper.SerializedCollections.Editor.States
{
    internal abstract class HashSetListState
    {
        public abstract int ListSize { get; }
        public virtual string NoElementsText => "List is Empty.";

        public readonly SerializedHashSetInstanceDrawer Drawer;

        public HashSetListState(SerializedHashSetInstanceDrawer serializedHashSetDrawer)
        {
            Drawer = serializedHashSetDrawer;
        }

        public abstract SerializedProperty GetPropertyAtIndex(int index);
        public abstract HashSetListState OnUpdate();
        public abstract void OnEnter();
        public abstract void OnExit();
        public abstract void DrawElement(Rect rect, SerializedProperty property, DisplayType displayType);
        public abstract void RemoveElementAt(int index);
        public abstract void InserElementAt(int index);

        public virtual float GetHeightAtIndex(int index, bool drawKeyAsList, bool drawValueAsList)
        {
            return SerializedHashSetInstanceDrawer.CalculateHeightOfElement(GetPropertyAtIndex(index), drawKeyAsList, drawValueAsList);
        }
    }
}