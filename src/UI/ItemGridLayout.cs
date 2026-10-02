using System;
using UnityEngine;

namespace PeakAdminToolkit.UI
{
    internal static class ItemGridLayout
    {
        private const float Gap = 8f;
        private const float MinWidth = 140f;
        private const float Height = 142f;

        public static int Columns(float width)
        {
            return Math.Max(1, (int)((width + Gap) / (MinWidth + Gap)));
        }

        public static Rect CardRect(int index, float width)
        {
            int columns = Columns(width);
            float cardWidth = (width - Gap * (columns - 1)) / columns;
            return new Rect((index % columns) * (cardWidth + Gap), (index / columns) * (Height + Gap), cardWidth, Height);
        }

        public static float ContentHeight(int count, float width)
        {
            return count == 0 ? 0 : ((count - 1) / Columns(width) + 1) * (Height + Gap) - Gap;
        }

        public static float ContentWidth(float viewportWidth) { return Math.Max(1, viewportWidth - 24); }

        public static Rect StatusRect(Rect card, float measuredHeight)
        {
            float height = measuredHeight > 0 ? Math.Max(28, (float)Math.Ceiling(measuredHeight) + 4) : 0;
            return new Rect(card.x + 8, card.y + 92 - height, card.width - 16, height);
        }

        public static Rect IconRect(Rect card, float statusHeight)
        {
            float size = statusHeight > 0 ? Math.Min(80, 80 - statusHeight) : 80;
            return new Rect(card.x + (card.width - size) / 2, card.y + 8, size, size);
        }

        public static int HitTest(Rect viewport, Vector2 scroll, Vector2 pointer, int count)
        {
            float width = ContentWidth(viewport.width);
            if (pointer.x < viewport.x || pointer.x >= viewport.x + width ||
                pointer.y < viewport.y || pointer.y >= viewport.y + viewport.height) return -1;
            float x = pointer.x - viewport.x + scroll.x;
            float y = pointer.y - viewport.y + scroll.y;
            for (int i = 0; i < count; i++)
            {
                Rect card = CardRect(i, width);
                if (x >= card.x && x < card.x + card.width && y >= card.y && y < card.y + card.height) return i;
            }
            return -1;
        }
    }
}
