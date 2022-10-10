using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AYellowpaper.SerializedCollections.Populators
{
    [Populator("Populate Enum", typeof(System.Enum))]
    public class EnumPopulator : Populator
    {
        public override IEnumerable GetElements(System.Type type)
        {
            return System.Enum.GetValues(type);
        }
    }
}