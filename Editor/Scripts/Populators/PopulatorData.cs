using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace AYellowpaper.SerializedCollections.Populators
{
    public class PopulatorData
    {
        public string Name { get; set; }
        public Type TargetType { get; set; }
        public Type PopulatorType { get; set; }

        public PopulatorData(string name, Type targetType, Type populatorType)
        {
            Name = name;
            TargetType = targetType;
            PopulatorType = populatorType;
        }
    }
}