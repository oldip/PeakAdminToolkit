namespace UnityEngine
{
    public sealed class Texture2D {}
    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
    }
    public enum CursorLockMode { None, Locked, Confined }
    public struct Rect
    {
        public float x, y, width, height;
        public Rect(float x, float y, float width, float height) { this.x = x; this.y = y; this.width = width; this.height = height; }
    }
    public static class Mathf
    {
        public static float Max(float a, float b) { return a > b ? a : b; }
        public static float Min(float a, float b) { return a < b ? a : b; }
        public static float Clamp(float value, float min, float max) { return value < min ? min : value > max ? max : value; }
    }
    public static class Cursor
    {
        public static CursorLockMode lockState;
        public static bool visible;
    }
}
