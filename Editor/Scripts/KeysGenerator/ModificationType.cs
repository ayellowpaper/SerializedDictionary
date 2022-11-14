using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AYellowpaper.SerializedCollections
{
    [Flags]
    public enum ModificationType
    {
        None = 0,
        Add = 1 << 0,
        Remove = 1 << 1,
        Confine = 1 << 2,
        All = ~0,
    }
}