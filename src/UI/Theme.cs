using UnityEngine;

namespace PeakAdminToolkit.UI
{
    internal sealed class Theme : System.IDisposable
    {
        private bool ready;
        private Font font;
        private Texture2D windowBackground;
        private Texture2D panelBackground;
        private Texture2D buttonBackground;
        private Texture2D buttonHoverBackground;
        private Texture2D buttonActiveBackground;
        private Texture2D buttonSelectedHoverBackground;

        public GUIStyle Window { get; private set; }
        public GUIStyle Heading { get; private set; }
        public GUIStyle Body { get; private set; }
        public GUIStyle Small { get; private set; }
        public GUIStyle Button { get; private set; }
        public GUIStyle Toggle { get; private set; }
        public GUIStyle Card { get; private set; }
        public GUIStyle CardName { get; private set; }
        public GUIStyle Tooltip { get; private set; }
        public GUIStyle TextField { get; private set; }

        public void Ensure()
        {
            if (ready) return;
            font = Font.CreateDynamicFontFromOSFont(
                new[] { "Microsoft JhengHei", "Microsoft YaHei", "Arial Unicode MS", "Arial" }, 20);
            windowBackground = CreateTexture(new Color(0.075f, 0.09f, 0.105f, 0.98f));
            panelBackground = CreateTexture(new Color(0.12f, 0.15f, 0.17f, 0.99f));
            buttonBackground = CreateTexture(new Color(0.22f, 0.27f, 0.30f, 1f));
            buttonHoverBackground = CreateTexture(new Color(0.30f, 0.39f, 0.43f, 1f));
            buttonActiveBackground = CreateTexture(new Color(0.10f, 0.39f, 0.36f, 1f));
            buttonSelectedHoverBackground = CreateTexture(new Color(0.15f, 0.50f, 0.45f, 1f));

            Window = new GUIStyle(GUI.skin.box)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperCenter,
                padding = new RectOffset(16, 16, 16, 16),
                contentOffset = Vector2.zero
            };
            SetBackground(Window, windowBackground);
            SetTextColor(Window, new Color(0.96f, 0.98f, 1f, 1f));

            Body = new GUIStyle(GUI.skin.label)
            {
                wordWrap = true,
                richText = true,
                fontSize = 20
            };
            SetTextColor(Body, new Color(0.94f, 0.96f, 0.98f, 1f));
            Heading = new GUIStyle(Body)
            {
                fontSize = 26,
                fontStyle = FontStyle.Bold,
                wordWrap = false,
                alignment = TextAnchor.MiddleLeft
            };

            Small = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                wordWrap = true,
                richText = true
            };
            SetTextColor(Small, new Color(0.82f, 0.87f, 0.90f, 1f));

            Button = new GUIStyle(GUI.skin.button)
            {
                fixedHeight = 38,
                fontSize = 18
            };
            SetButtonState(Button.normal, buttonBackground);
            SetButtonState(Button.hover, buttonHoverBackground);
            SetButtonState(Button.focused, buttonHoverBackground);
            SetButtonState(Button.active, buttonActiveBackground);
            SetButtonState(Button.onNormal, buttonActiveBackground);
            SetButtonState(Button.onHover, buttonSelectedHoverBackground);
            SetButtonState(Button.onFocused, buttonSelectedHoverBackground);
            SetButtonState(Button.onActive, buttonActiveBackground);

            Toggle = new GUIStyle(GUI.skin.toggle) { fontSize = 18, fixedHeight = 32 };
            SetTextColor(Toggle, new Color(0.94f, 0.96f, 0.98f, 1f));

            Card = new GUIStyle(Button)
            {
                fixedHeight = 0,
                margin = new RectOffset(),
                padding = new RectOffset()
            };
            CardName = new GUIStyle(Body)
            {
                fontSize = 18,
                richText = false,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(),
                clipping = TextClipping.Clip
            };

            TextField = new GUIStyle(GUI.skin.textField)
            {
                fontSize = 20,
                fixedHeight = 42,
                padding = new RectOffset(10, 10, 7, 7)
            };
            SetTextColor(TextField, new Color(0.96f, 0.98f, 1f, 1f));

            Tooltip = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperLeft,
                wordWrap = true,
                richText = true,
                fontSize = 16,
                padding = new RectOffset(8, 8, 6, 6)
            };
            SetBackground(Tooltip, panelBackground);
            SetTextColor(Tooltip, new Color(0.94f, 0.96f, 0.98f, 1f));

            if (font != null)
            {
                Window.font = Heading.font = Body.font = Small.font = Button.font = Toggle.font = Card.font = CardName.font = Tooltip.font = TextField.font = font;
            }
            ready = true;
        }

        private static Texture2D CreateTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private static void SetBackground(GUIStyle style, Texture2D texture)
        {
            style.normal.background = texture;
            style.hover.background = texture;
            style.active.background = texture;
            style.focused.background = texture;
            style.onNormal.background = texture;
            style.onHover.background = texture;
            style.onActive.background = texture;
            style.onFocused.background = texture;
        }

        private static void SetTextColor(GUIStyle style, Color color)
        {
            style.normal.textColor = color;
            style.hover.textColor = color;
            style.active.textColor = color;
            style.focused.textColor = color;
            style.onNormal.textColor = color;
            style.onHover.textColor = color;
            style.onActive.textColor = color;
            style.onFocused.textColor = color;
        }
        private static void SetButtonState(GUIStyleState state, Texture2D background)
        {
            state.background = background;
            state.textColor = new Color(0.97f, 0.98f, 1f, 1f);
        }

        private static void DestroyTexture(Texture2D texture)
        {
            if (texture != null) UnityEngine.Object.Destroy(texture);
        }

        public void Dispose()
        {
            Window = null;
            Heading = null;
            Body = null;
            Small = null;
            Button = null;
            Card = null;
            CardName = null;
            Tooltip = null;
            TextField = null;
            if (font != null) UnityEngine.Object.Destroy(font);
            font = null;
            DestroyTexture(windowBackground);
            DestroyTexture(panelBackground);
            DestroyTexture(buttonBackground);
            DestroyTexture(buttonHoverBackground);
            DestroyTexture(buttonActiveBackground);
            DestroyTexture(buttonSelectedHoverBackground);
            windowBackground = null;
            panelBackground = null;
            buttonBackground = null;
            buttonHoverBackground = null;
            buttonActiveBackground = null;
            buttonSelectedHoverBackground = null;
            ready = false;
        }
    }
}
