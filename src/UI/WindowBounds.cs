using UnityEngine;

namespace PeakAdminToolkit.UI
{
    internal static class WindowBounds
    {
        private const float ScreenMargin = 16f;

        public static float ScaleFor(float screenWidth, float screenHeight)
        {
            if (screenWidth <= 0 || screenHeight <= 0) return 1f;
            return Mathf.Min(screenWidth / 1920f, screenHeight / 1080f);
        }

        public static Rect Centered(float screenWidth, float screenHeight)
        {
            float scale = ScaleFor(screenWidth, screenHeight);
            return Clamp(new Rect((screenWidth / scale - 1200f) / 2f, (screenHeight / scale - 800f) / 2f, 1200f, 800f),
                screenWidth / scale, screenHeight / scale);
        }

        public static Rect Clamp(Rect window, float screenWidth, float screenHeight)
        {
            window.width = Mathf.Min(window.width, Mathf.Max(1f, screenWidth - ScreenMargin));
            window.height = Mathf.Min(window.height, Mathf.Max(1f, screenHeight - ScreenMargin));
            window.x = Mathf.Clamp(window.x, 0f, Mathf.Max(0f, screenWidth - window.width));
            window.y = Mathf.Clamp(window.y, 0f, Mathf.Max(0f, screenHeight - window.height));
            return window;
        }
    }
}
