using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AYellowpaper.SerializedCollections.Populators
{
    [Populator("Clear", typeof(object))]
    public class ClearPopulator : Populator
    {
        public override IEnumerable GetElements(Type type)
        {
            yield break;
        }
    }
}