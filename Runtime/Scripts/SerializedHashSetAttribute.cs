using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace AYellowpaper.SerializedCollections
{
    [Conditional("UNITY_EDITOR")]
    public class SerializedHashSetAttribute : Attribute
    {
        public readonly string ElementName;

        public SerializedHashSetAttribute(string elementName = null)
        {
            ElementName = elementName;
        }
    }
}