using System;

namespace AYellowpaper.SerializedCollections.Populators
{
    public class KeysGeneratorData
    {
        public string Name { get; set; }
        public Type TargetType { get; set; }
        public Type GeneratorType { get; set; }
        public bool NeedsWindow { get; set; }

        public KeysGeneratorData(string name, Type targetType, Type populatorType, bool needsWindow)
        {
            Name = name;
            TargetType = targetType;
            GeneratorType = populatorType;
            NeedsWindow = needsWindow;
        }
    }
}