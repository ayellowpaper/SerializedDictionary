using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AYellowpaper.SerializedCollections.Editor
{
    public static class RectUtility
    {
        public static Rect WithX(this Rect rect, float x) => new Rect(x, rect.y, rect.width, rect.height);
        public static Rect WithY(this Rect rect, float y) => new Rect(rect.x, y, rect.width, rect.height);
        public static Rect WithWidth(this Rect rect, float width) => new Rect(rect.x, rect.y, width, rect.height);
        public static Rect WithHeight(this Rect rect, float height) => new Rect(rect.x, rect.y, rect.width, height);
        public static Rect WithPosition(this Rect rect, Vector2 position) => new Rect(position, rect.size);
        public static Rect WithPosition(this Rect rect, float x, float y) => new Rect(new Vector2(x, y), rect.size);
        public static Rect WithSize(this Rect rect, Vector2 size) => new Rect(rect.position, size);
        public static Rect WithSize(this Rect rect, float width, float height) => new Rect(rect.position, new Vector2(width, height));

        public static Rect WithXAndWidth(this Rect rect, float x, float width) => new Rect(x, rect.y, width, rect.height);
        public static Rect WithYAndHeight(this Rect rect, float y, float height) => new Rect(rect.x, y, rect.width, height);

        public static Rect Append(this Rect rect, float width) => new Rect(rect.x + rect.width, rect.y, width, rect.height);
    }
}