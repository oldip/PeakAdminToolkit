using UnityEngine;

namespace PeakAdminToolkit.UI
{
    internal sealed class Tooltip
    {
        private string target;
        private string content;
        private double started;
        public string Observe(string id, string text, double now)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(text)) { Clear(); return null; }
            if (target != id || content != text) { target = id; content = text; started = now; }
            return now - started >= 1.0 ? content : null;
        }
        public void Clear() { target = null; content = null; }

        public static Rect Bounds(Vector2 pointer, float width, float height, float windowWidth, float windowHeight)
        {
            width = Mathf.Min(width, windowWidth - 24);
            height = Mathf.Min(height, windowHeight - 24);
            float x = pointer.x + 14, y = pointer.y + 14;
            if (x + width > windowWidth - 12) x = pointer.x - width - 14;
            if (y + height > windowHeight - 12) y = pointer.y - height - 14;
            return new Rect(Mathf.Clamp(x, 12, windowWidth - width - 12),
                Mathf.Clamp(y, 12, windowHeight - height - 12), width, height);
        }
    }
}
