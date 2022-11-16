using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AYellowpaper.SerializedCollections.Populators
{
    [KeysGenerator("Populate Enum", typeof(System.Enum), false)]
    public class EnumGenerator : KeysGenerator
    {
        public override IEnumerable GetElements(System.Type type)
        {
            return System.Enum.GetValues(type);
        }
    }
}