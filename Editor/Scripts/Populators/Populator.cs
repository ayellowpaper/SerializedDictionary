using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AYellowpaper.SerializedCollections.Populators
{
    public abstract class Populator : ScriptableObject
    {
        public abstract IEnumerable GetElements(System.Type type);
    }
}