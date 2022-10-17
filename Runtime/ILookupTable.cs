using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AYellowpaper.SerializedCollections
{
    public interface ILookupTable
    {
        void RecalculateOccurences();
        IReadOnlyList<int> GetOccurences(object key);
    }
}
