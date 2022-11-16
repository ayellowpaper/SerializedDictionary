using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AYellowpaper.SerializedCollections.Populators
{
    public abstract class KeysGenerator : ScriptableObject
    {
        public abstract IEnumerable GetElements(System.Type type);
    }
}