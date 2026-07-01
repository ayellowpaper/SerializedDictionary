using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AYellowpaper.SerializedCollections
{
    [System.Serializable]
    public struct SerializedElement<TElement>
    {
        public TElement Element;

        public SerializedElement(TElement element)
        {
            Element = element;
        }
    }
}
