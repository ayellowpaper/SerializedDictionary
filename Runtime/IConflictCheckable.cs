using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AYellowpaper.SerializedCollections
{
    public interface IConflictCheckable
    {
        int GetFirstConflict(object key);
    }
}
