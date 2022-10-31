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
        private IEnumerator<SerializedProperty> _foundProperties;
        private SerializedProperty _activeSearchProperty;
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
            _previousColor = GUI.color;
            if (SerializedProperty.EqualContents(_activeSearchProperty, obj))
            {
                GUI.color = Color.yellow;
                MoveNext();
            }
        }

        private void AfterDrawingProperty(SerializedProperty obj)
        {
            GUI.color = _previousColor;
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

            _foundProperties.Reset();

            return this;
        }

        public void SetSearchString(string searchString)
        {
            var query = new SearchQuery(Matchers.RegisteredMatchers);
            query.SearchString = searchString;
            _searchResults.Clear();
            _searchResults.AddRange(query.ApplyToArrayProperty(Drawer.ListProperty));

            _foundProperties = _searchResults.SelectMany(x => x.MatchingResults, (x, y) => y.Property).ToList().GetEnumerator();
            MoveNext();
        }
        
        void MoveNext()
        {
            if (_foundProperties.MoveNext())
                _activeSearchProperty = _foundProperties.Current;
            else
                _activeSearchProperty = null;
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