using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AYellowpaper.SerializedCollections.Populators
{
    [KeysGenerator("Populate Enum", typeof(System.Enum))]
    public class EnumGenerator : KeysGenerator
    {
        public override bool RequiresWindow => false;

        public override IEnumerable GetElements(System.Type type)
        {
            return System.Enum.GetValues(type);
        }
    }
}