using System;
using UnityEditor;

namespace AYellowpaper.SerializedCollections.Editor.Search
{
    public class EnumMatcher : Matcher
    {
        public override string GetMatch(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.Enum && SCEditorUtility.TryGetTypeFromProperty(property, out var type))
            {
                foreach (var text in SCEnumUtility.GetEnumCache(type).GetNamesForValue(property.enumValueFlag))
                {
                    if (text.Contains(SearchString, StringComparison.InvariantCultureIgnoreCase))
                        return text;
                }
            }
            return null;
        }
    }
}