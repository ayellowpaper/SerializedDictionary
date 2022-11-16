using System;

namespace AYellowpaper.SerializedCollections.Populators
{
    [AttributeUsage(AttributeTargets.Class)]
    public class KeysGeneratorAttribute : Attribute
    {
        public readonly string Name;
        public readonly Type TargetType;
        public readonly bool NeedsWindow;

        public KeysGeneratorAttribute(string name, Type targetType, bool needsWindow = true)
        {
            Name = name;
            TargetType = targetType;
            NeedsWindow = needsWindow;
        }
    }
}