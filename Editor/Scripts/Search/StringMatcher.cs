using System.Globalization;
using UnityEditor;

namespace AYellowpaper.SerializedCollections.Editor.Search
{
    public class StringMatcher : Matcher
    {
        public override string GetMatch(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.String && property.stringValue.Contains(SearchString, System.StringComparison.InvariantCultureIgnoreCase))
                return property.stringValue;
            return null;
        }
    }
}