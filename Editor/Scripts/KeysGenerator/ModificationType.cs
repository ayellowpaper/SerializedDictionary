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
        Set = 1 << 1,
        Remove = 1 << 2,
        All = ~0,
    }
}