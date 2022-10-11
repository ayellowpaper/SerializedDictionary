using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AYellowpaper.SerializedCollections
{
    public interface IConflictCheckable
    {
        void RecalculateConflicts();
        int GetFirstConflict(object key);
    }
}
