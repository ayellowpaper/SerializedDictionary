using UnityEditor;

namespace AYellowpaper.SerializedCollections.Editor.Search
{
    public abstract class Matcher
    {
        public string SearchString { get; private set; }

        public void Prepare(string searchString)
        {
            SearchString = ProcessSearchString(searchString);
        }

        public virtual string ProcessSearchString(string searchString)
        {
            return searchString;
        }
        public virtual bool IsMatch(SerializedProperty property)
        {
            return GetMatch(property) == null;
        }
        public abstract string GetMatch(SerializedProperty property);
    }
}