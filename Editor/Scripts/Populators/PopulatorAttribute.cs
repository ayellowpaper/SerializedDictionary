using System;

namespace AYellowpaper.SerializedCollections.Populators
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PopulatorAttribute : Attribute
    {
        public readonly string Name;
        public readonly Type TargetType;

        public PopulatorAttribute(string name, Type targetType)
        {
            Name = name;
            TargetType = targetType;
        }
    }
}