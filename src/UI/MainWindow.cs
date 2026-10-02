using System;
using System.Collections.Generic;
using UnityEngine;
using PeakAdminToolkit.Core;
using PeakAdminToolkit.Items;
using PeakAdminToolkit.Self;
using PeakAdminToolkit.Players;

namespace PeakAdminToolkit.UI
{
    internal sealed class MainWindow : IDisposable
    {
        private const int WindowId = 0x504154;
        private static readonly ItemCategory[] Categories = { ItemCategory.None, ItemCategory.Mystical, ItemCategory.Consumable, ItemCategory.Deployable, ItemCategory.Equipment, ItemCategory.Other };
        private ConsumableKind[] availableKinds = { ConsumableKind.None };
        private readonly ToolkitConfig config;
        private readonly PeakApi api;
        private readonly SelfTools selfTools;
        private readonly SelfToolsPanel selfPanel = new SelfToolsPanel();
        private readonly WorldPanel worldPanel = new WorldPanel();
        private readonly PlayerPanel playerPanel;
        private readonly RoomRoleReader roomRole = new RoomRoleReader();
        private readonly RoomHostTransfer hostTransfer = new RoomHostTransfer();
        private readonly Localization localization = new Localization();
        private readonly CursorLease cursor = new CursorLease();
        private readonly Theme theme = new Theme();
        private readonly Toast toast = new Toast();
        private readonly Tooltip tooltip = new Tooltip();
        private readonly SpecialItemFilter specialFilter = new SpecialItemFilter();
        private readonly List<ItemCatalogEntry> catalog = new List<ItemCatalogEntry>();
        private readonly List<ItemCatalogEntry> visibleItems = new List<ItemCatalogEntry>();
        private Rect windowRect;
        private float uiScale = 1f;
        private int screenWidth, screenHeight;
        private bool open, catalogLoaded, languageUnavailable;
        private int activeTab = 1;
        private int categoryIndex, kindIndex;
        private string itemQuery = string.Empty;
        private Vector2 itemScroll = Vector2.zero;
        private string lastGameLanguage, lastPreference;

        public bool IsOpen { get { return open; } }

        public MainWindow(ToolkitConfig config, PeakApi api, SelfTools selfTools, DroppedItemHistory history, Action<string> log)
        {
            playerPanel = new PlayerPanel(history, log);
            this.config = config;
            this.api = api;
            this.selfTools = selfTools;
            lastPreference = config.Language.Value;
            localization.Update(lastPreference, null);
        }

        public void Open()
        {
            if (open || !api.CanOpenWindow) return;
            specialFilter.Reset();
            specialFilter.SetCategory(Categories[categoryIndex]);
            open = true;
            CenterOnScreen();
            InvalidateItemCatalog();
            cursor.Acquire();
            toast.Clear();
            tooltip.Clear();
        }

        public void Close()
        {
            if (!open) return;
            open = false;
            specialFilter.Reset();
            cursor.Release(api.GameWantsCursor());
            toast.Clear();
            tooltip.Clear();
        }

        public void InvalidateItemCatalog()
        {
            tooltip.Clear();
            catalogLoaded = false;
            catalog.Clear();
            visibleItems.Clear();
        }

        public void RefreshLanguage(string gameLanguage)
        {
            bool changed = !string.Equals(gameLanguage, lastGameLanguage, StringComparison.Ordinal);
            languageUnavailable = string.IsNullOrEmpty(gameLanguage) && config.Language.Value == "Auto";
            if (gameLanguage == lastGameLanguage && config.Language.Value == lastPreference) return;
            lastGameLanguage = gameLanguage;
            lastPreference = config.Language.Value;
            localization.Update(lastPreference, gameLanguage);
            tooltip.Clear();
            if (changed && catalogLoaded) RefreshItemCatalog();
        }

        public void Draw()
        {
            if (!open) return;
            if (screenWidth != Screen.width || screenHeight != Screen.height) CenterOnScreen();
            theme.Ensure();
            Matrix4x4 previous = GUI.matrix;
            try
            {
                GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(uiScale, uiScale, 1));
                // The title is laid out inside the content, not in GUI.Window's skin caption.
                windowRect = GUI.Window(WindowId, windowRect, DrawContents, string.Empty, theme.Window);
                windowRect = WindowBounds.Clamp(windowRect, screenWidth / uiScale, screenHeight / uiScale);
            }
            finally { GUI.matrix = previous; }
        }

        private void DrawContents(int id)
        {
            GUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            GUILayout.BeginHorizontal(GUILayout.Height(42));
            GUILayout.Label(localization.Text("Heading"), theme.Heading);
            GUILayout.Label(localization.Text("Foundation"), theme.Small, GUILayout.Width(150));
            GUILayout.FlexibleSpace();
            int nextTab = GUILayout.SelectionGrid(activeTab, new[] { localization.Text("Overview"), localization.Text("Items"), localization.Text("SelfTools"), localization.Text("Players"), localization.Text("World") }, 5, theme.Button, GUILayout.Width(640));
            if (nextTab != activeTab)
            {
                activeTab = nextTab;
                specialFilter.Reset();
                InvalidateItemCatalog();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            ItemCatalogEntry hoveredItem = null;
            if (activeTab == 1) hoveredItem = DrawItems();
            else if (activeTab == 2) selfPanel.Draw(selfTools, theme, localization, toast);
            else if (activeTab == 3) playerPanel.Draw(theme, localization, toast);
            else if (activeTab == 4) worldPanel.Draw(theme, localization, toast);
            else
            {
                DrawOverview();
                GUILayout.FlexibleSpace();
            }

            GUILayout.Label(toast.Current(Time.unscaledTimeAsDouble) ?? string.Empty, theme.Small, GUILayout.Height(26));
            GUILayout.BeginHorizontal();
            GUILayout.Label(localization.Text("CloseHelp"), theme.Small);
            if (GUILayout.Button(localization.Text("Close"), theme.Button, GUILayout.Width(120))) Close();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(12, 12, windowRect.width - 680, 42));

            if (Event.current.type == EventType.Repaint)
            {
                string details = null;
                if (open && hoveredItem != null)
                {
                    details = ItemTooltip.Format(hoveredItem, localization);
                }
                string tip = tooltip.Observe(details == null ? null : hoveredItem.SpawnName, details, Time.unscaledTimeAsDouble);
                if (!string.IsNullOrEmpty(tip))
                {
                    float width = Mathf.Min(380, windowRect.width - 24);
                    float height = theme.Tooltip.CalcHeight(new GUIContent(tip), width);
                    GUI.Box(Tooltip.Bounds(Event.current.mousePosition, width, height, windowRect.width, windowRect.height), tip, theme.Tooltip);
                }
            }
        }

        private void DrawOverview()
        {
            GUILayout.Label(localization.Text("Scope"), theme.Body);
            RoomRole role = roomRole.Read();
            string roleKey = role == RoomRole.Host ? "RoomRoleHost" : role == RoomRole.Client ? "RoomRoleClient" :
                role == RoomRole.OutsideRoom ? "RoomRoleOutside" : "RoomRoleUnavailable";
            GUILayout.Label(localization.Format("RoomRoleLabel", localization.Text(roleKey)), theme.Body);
            bool previous = GUI.enabled;
            try
            {
                GUI.enabled = previous && hostTransfer.CanRequest();
                if (GUILayout.Button(localization.Text("RoomTransferAttempt"), theme.Button, GUILayout.Width(320)))
                    toast.Show(localization.Text(hostTransfer.Request() ? "RoomTransferSubmitted" : "RoomTransferFailed"),
                        Time.unscaledTimeAsDouble, 4.0);
            }
            finally { GUI.enabled = previous; }
            GUILayout.Label(localization.Text("RoomTransferHelp"), theme.Small);
            GUILayout.Space(16);
            GUILayout.Label(localization.Text("Language"), theme.Body);
            GUIContent[] choices = {
                new GUIContent(localization.Text("Auto")),
                new GUIContent(localization.Text("English")),
                new GUIContent(localization.Text("SimplifiedChinese")),
                new GUIContent(localization.Text("TraditionalChinese"))
            };
            int selected = config.Language.Value == "en" ? 1 : config.Language.Value == "zh-CN" ? 2 : config.Language.Value == "zh-TW" ? 3 : 0;
            int next = GUILayout.SelectionGrid(selected, choices, 4, theme.Button);
            if (next != selected)
            {
                config.Language.Value = next == 1 ? "en" : next == 2 ? "zh-CN" : next == 3 ? "zh-TW" : "Auto";
                RefreshLanguage(lastGameLanguage);
                toast.Show(localization.Text("Saved"), Time.unscaledTimeAsDouble, 2.0);
            }
            GUILayout.Label(localization.Text("AutoHelp"), theme.Small);
            GUILayout.Label(localization.Format("ActiveLanguage", LanguageName()), theme.Small);
            if (languageUnavailable) GUILayout.Label(localization.Text("LanguageUnavailable"), theme.Small);
        }

        private ItemCatalogEntry DrawItems()
        {
            if (!catalogLoaded) RefreshItemCatalog();
            GUILayout.BeginHorizontal();
            GUILayout.Label(localization.Text("SearchItems"), theme.Body, GUILayout.Width(180));
            string nextQuery = GUILayout.TextField(itemQuery, theme.TextField);
            if (nextQuery != itemQuery) { itemQuery = nextQuery; ApplyFilter(); }
            if (GUILayout.Button(localization.Text("RefreshItems"), theme.Button, GUILayout.Width(120))) RefreshItemCatalog();
            GUILayout.EndHorizontal();
            GUILayout.Space(6);
            string[] categoryLabels = new string[Categories.Length];
            for (int i = 0; i < categoryLabels.Length; i++) categoryLabels[i] = localization.Text(i == 0 ? "All" : Categories[i].ToString());
            int nextCategory = GUILayout.SelectionGrid(categoryIndex, categoryLabels, categoryLabels.Length, theme.Button);
            if (nextCategory != categoryIndex)
            {
                bool hadSpecial = specialFilter.Enabled;
                categoryIndex = nextCategory; kindIndex = 0;
                specialFilter.SetCategory(Categories[categoryIndex]);
                if (hadSpecial) RefreshItemCatalog(); else ApplyFilter();
            }
            if (Categories[categoryIndex] == ItemCategory.Consumable)
            {
                string[] kindLabels = new string[availableKinds.Length];
                for (int i = 0; i < kindLabels.Length; i++) kindLabels[i] = localization.Text(i == 0 ? "All" : availableKinds[i].ToString());
                int nextKind = GUILayout.SelectionGrid(kindIndex, kindLabels, kindLabels.Length, theme.Button);
                if (nextKind != kindIndex) { kindIndex = nextKind; ApplyFilter(); }
            }
            if (Categories[categoryIndex] == ItemCategory.Other)
            {
                bool showSpecial = GUILayout.Toggle(specialFilter.Enabled, localization.Text("ShowSpecialItems"), theme.Toggle);
                if (showSpecial != specialFilter.Enabled)
                {
                    specialFilter.SetEnabled(showSpecial);
                    RefreshItemCatalog();
                }
                if (specialFilter.Enabled)
                {
                    SpecialItemKind[] kinds = SpecialItems.AvailableKinds(catalog);
                    string[] labels = new string[kinds.Length];
                    for (int i = 0; i < kinds.Length; i++) labels[i] = localization.Text(i == 0 ? "All" : kinds[i].ToString());
                    int selected = Math.Max(0, Array.IndexOf(kinds, specialFilter.SelectedKind));
                    int next = GUILayout.SelectionGrid(selected, labels, labels.Length, theme.Button);
                    if (next != selected) { specialFilter.SelectKind(kinds[next]); ApplyFilter(); }
                }
            }
            GUILayout.Label(localization.Format("ItemCount", visibleItems.Count, catalog.Count), theme.Small);
            bool canSpawn = api.CanSpawnItems;
            if (!canSpawn) GUILayout.Label(localization.Text("SpawnUnavailable"), theme.Small);

            Rect viewport = GUILayoutUtility.GetRect(0, 10000, 0, 10000, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            float gridWidth = ItemGridLayout.ContentWidth(viewport.width);
            float gridHeight = ItemGridLayout.ContentHeight(visibleItems.Count, gridWidth);
            Vector2 nextScroll = GUI.BeginScrollView(viewport, itemScroll, new Rect(0, 0, gridWidth, Mathf.Max(40, gridHeight)), false, true);
            if (nextScroll != itemScroll) tooltip.Clear();
            itemScroll = nextScroll;
            if (visibleItems.Count == 0) GUI.Label(new Rect(0, 0, gridWidth, 40), localization.Text(catalog.Count == 0 ? "NoItems" : "NoMatches"), theme.Body);
            for (int i = 0; i < visibleItems.Count; i++)
            {
                ItemCatalogEntry item = visibleItems[i];
                Rect card = ItemGridLayout.CardRect(i, gridWidth);
                string name = item.DisplayName(localization.Language);
                bool enabled = GUI.enabled;
                try
                {
                    GUI.enabled = enabled && canSpawn && item.CanSpawnManually;
                    if (GUI.Button(card, GUIContent.none, theme.Card))
                        toast.Show(localization.Text(api.SpawnItem(item, specialFilter.Enabled) ? "SpawnRequested" : "SpawnFailed"), Time.unscaledTimeAsDouble, 2.5);
                    var status = item.ValidToSpawn ? GUIContent.none
                        : new GUIContent(localization.Text(item.CanSpawnManually ? "SoloManualSpawn" : "ItemUnavailable"));
                    float measuredHeight = item.ValidToSpawn ? 0 : theme.Small.CalcHeight(status, card.width - 16);
                    Rect statusRect = ItemGridLayout.StatusRect(card, measuredHeight);
                    Rect iconRect = ItemGridLayout.IconRect(card, statusRect.height);
                    if (item.Icon != null) GUI.DrawTexture(iconRect, item.Icon, ScaleMode.ScaleToFit, true);
                    if (statusRect.height > 0) GUI.Label(statusRect, status, theme.Small);
                    GUI.Label(new Rect(card.x + 8, card.y + 92, card.width - 16, 44), name, theme.CardName);
                }
                finally { GUI.enabled = enabled; }
            }
            GUI.EndScrollView();
            int hoveredIndex = Event.current.type == EventType.Repaint
                ? ItemGridLayout.HitTest(viewport, itemScroll, Event.current.mousePosition, visibleItems.Count) : -1;
            return hoveredIndex < 0 ? null : visibleItems[hoveredIndex];
        }

        private void ApplyFilter()
        {
            tooltip.Clear();
            visibleItems.Clear();
            foreach (ItemCatalogEntry item in catalog)
                if (ItemClassification.Matches(item.Categories, item.Kinds, Categories[categoryIndex], availableKinds[kindIndex]) && specialFilter.Matches(item) && api.MatchesItem(itemQuery, item))
                    visibleItems.Add(item);
            itemScroll = Vector2.zero;
        }

        private void RefreshItemCatalog()
        {
            ConsumableKind selectedKind = availableKinds[kindIndex];
            catalog.Clear();
            catalog.AddRange(api.ReadItemCatalog(specialFilter.Enabled));
            if (Array.IndexOf(SpecialItems.AvailableKinds(catalog), specialFilter.SelectedKind) < 0)
                specialFilter.SelectKind(SpecialItemKind.None);
            availableKinds = ItemClassification.AvailableKinds(catalog);
            kindIndex = Math.Max(0, Array.IndexOf(availableKinds, selectedKind));
            catalogLoaded = true;
            ApplyFilter();
        }

        private string LanguageName() { return localization.Language == "zh-CN" ? "简体中文" : localization.Language == "zh-TW" ? "繁體中文" : "English"; }

        private void CenterOnScreen()
        {
            tooltip.Clear();
            screenWidth = Screen.width;
            screenHeight = Screen.height;
            uiScale = WindowBounds.ScaleFor(screenWidth, screenHeight);
            windowRect = WindowBounds.Centered(screenWidth, screenHeight);
        }

        public void Dispose() { Close(); theme.Dispose(); }
    }
}

