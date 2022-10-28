using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AYellowpaper.SerializedCollections.Editor.Search
{
    public class PropertySearchResult
    {
        public SerializedProperty Property;
        public string MatchingText;

        public PropertySearchResult(SerializedProperty property, string matchingText)
        {
            Property = property;
            MatchingText = matchingText;
        }

        public override string ToString()
        {
            return $"{MatchingText} found in {Property.propertyPath}";
        }
    }
}