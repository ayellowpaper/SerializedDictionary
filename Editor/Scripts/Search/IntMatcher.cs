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

        public override string GetMatch(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.Float)
            {
                var val = property.floatValue.ToString(CultureInfo.InvariantCulture);
                if (val.Contains(SearchString, System.StringComparison.InvariantCulture))
                    return val;
            }
            return null;
        }
    }
}