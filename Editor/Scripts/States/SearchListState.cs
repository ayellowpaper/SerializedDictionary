using AYellowpaper.SerializedCollections.Editor.Search;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System;

namespace AYellowpaper.SerializedCollections.Editor.States
{
    internal class SearchListState : ListState
    {
        public override int ListSize => _searchResults.Count;
        public bool OnlyShowMatchingValues { get; set; }

        private string _lastSearch = string.Empty;
        private List<SearchResultEntry> _searchResults = new List<SearchResultEntry>();
        private HashSet<string> _foundProperties;
        private Color _previousColor;

        public SearchListState(SerializedDictionaryDrawer serializedDictionaryDrawer) : base(serializedDictionaryDrawer)
        {
        }

        public override void DrawElement(Rect rect, SerializedProperty property, DisplayType displayType)
        {
            SerializedDictionaryDrawer.DrawElement(rect, property, displayType, BeforeDrawingProperty, AfterDrawingProperty);
        }

        private void BeforeDrawingProperty(SerializedProperty obj)
        {
            _previousColor = GUI.backgroundColor;
            if (_foundProperties.Contains(obj.propertyPath))
            {
                GUI.backgroundColor = Color.blue;
            }
        }

        private void AfterDrawingProperty(SerializedProperty obj)
        {
            GUI.backgroundColor = _previousColor;
        }

        public override void OnEnter()
        {
        }

        public override void OnExit()
        {
        }

        public override ListState OnUpdate()
        {
            if (Drawer.SearchText.Length == 0)
                return Drawer.DefaultState;

            if (_lastSearch != Drawer.SearchText)
            {
                _lastSearch = Drawer.SearchText;
                SetSearchString(Drawer.SearchText);
            }

            return this;
        }

        public void SetSearchString(string searchString)
        {
            var query = new SearchQuery(Matchers.RegisteredMatchers);
            query.SearchString = searchString;
            _searchResults.Clear();
            _searchResults.AddRange(query.ApplyToArrayProperty(Drawer.ListProperty));

            _foundProperties = _searchResults.SelectMany(x => x.MatchingResults, (x, y) => y.Property.propertyPath).ToHashSet();
        }

        public override SerializedProperty GetPropertyAtIndex(int index)
        {
            return _searchResults[index].Property;
        }

        public override float GetHeightAtIndex(int index, bool drawKeyAsList, bool drawValueAsList)
        {
            return base.GetHeightAtIndex(index, drawKeyAsList, drawValueAsList);
        }
    }
}