using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AYellowpaper.SerializedCollections.Populators
{
    public abstract class KeysGenerator : ScriptableObject
    {
        public virtual string Title => GetType().Name;
        public virtual string Description => "";
        public virtual bool RequiresWindow => true;
        public abstract IEnumerable GetElements(System.Type type);
    }
}