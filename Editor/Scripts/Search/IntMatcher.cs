using System.Globalization;
using UnityEditor;

namespace AYellowpaper.SerializedCollections.Editor.Search
{
    public class IntMatcher : Matcher
    {
        public override string ProcessSearchString(string searchString)
        {
            return searchString.Replace(',', '.');
        }

        public override bool IsMatch(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.Float)
            {
                if (property.floatValue.ToString(CultureInfo.InvariantCulture).Contains(SearchString))
                    return true;
            }
            return false;
        }
    }
}