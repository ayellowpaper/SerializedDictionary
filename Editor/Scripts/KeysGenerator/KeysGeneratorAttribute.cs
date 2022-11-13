using System;

namespace AYellowpaper.SerializedCollections.Populators
{
    [AttributeUsage(AttributeTargets.Class)]
    public class KeysGeneratorAttribute : Attribute
    {
        public readonly string Name;
        public readonly Type TargetType;

        public KeysGeneratorAttribute(string name, Type targetType)
        {
            Name = name;
            TargetType = targetType;
        }
    }
}