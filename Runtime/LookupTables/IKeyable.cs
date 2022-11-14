using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AYellowpaper.SerializedCollections
{
    internal interface IKeyable
    {
        void RecalculateOccurences();
        IReadOnlyList<int> GetOccurences(object key);
        IEnumerable Keys { get; }
    }
}
