using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AYellowpaper.SerializedCollections.Populators
{
    [Populator("Int Range", typeof(int))]
    public class IntRangePopulator : Populator
    {
        [SerializeField]
        private int _startValue = 1;
        [SerializeField]
        private int _endValue = 10;

        public override IEnumerable GetElements(Type type)
        {
            for (int i = _startValue; i <= _endValue; i++)
                yield return i;
        }
    }
}