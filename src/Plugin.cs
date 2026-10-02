using System;
using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;
using PeakAdminToolkit.Core;
using PeakAdminToolkit.UI;
using PeakAdminToolkit.Items;
using PeakAdminToolkit.Self;
using PeakAdminToolkit.Players;

namespace PeakAdminToolkit
{
    [BepInPlugin(Guid, Name, Version)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string Guid = "com.oldip.peakadmintoolkit";
        public const string Name = "PEAK Admin Toolkit";
        public const string Version = "0.8.1";

        private ToolkitConfig config;
        private Compatibility compatibility;
        private PeakApi api;
        private MainWindow window;
        private SelfTools selfTools;
        private HornFuelHud hornFuelHud;
        private DroppedItemHistory droppedItems;
        private DroppedItemCapture droppedItemCapture;
        private bool hasFocus = true;
        private double nextLanguagePoll;
        private bool guiFailureReported;

        private bool WindowIsOpen { get { return window != null && window.IsOpen; } }

        private void Awake()
        {
            config = new ToolkitConfig(Config);
            compatibility = new Compatibility(() => WindowIsOpen, Logger);
            api = new PeakApi(compatibility, new ItemDescriptions(message => Logger.LogWarning(message)), message => Logger.LogInfo(message));
            selfTools = new SelfTools(new SelfApi(), message => Logger.LogWarning(message),
                () => WindowIsOpen || !hasFocus || !config.Enabled.Value);
            droppedItems = new DroppedItemHistory();
            window = new MainWindow(config, api, selfTools, droppedItems, message => Logger.LogWarning(message));
            droppedItemCapture = new DroppedItemCapture(droppedItems, message => Logger.LogWarning(message));
            hornFuelHud = new HornFuelHud(null, message => Logger.LogWarning(message),
                () => isActiveAndEnabled && config.Enabled.Value);
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            Logger.LogInfo(Name + " " + Version + " loaded. Window hooks: " + compatibility.WindowHooksReady);
        }

        private void Update()
        {
            if (config == null || window == null) return;
            if (!config.Enabled.Value)
            {
                window.Close();
                selfTools.Reset();
                return;
            }
            selfTools.Poll();
            if (config.ToggleKey.Value != KeyCode.None && Input.GetKeyDown(config.ToggleKey.Value))
            {
                if (WindowIsOpen) window.Close();
                else if (api.CanOpenWindow) window.Open();
            }
            if (Time.unscaledTimeAsDouble >= nextLanguagePoll)
            {
                nextLanguagePoll = Time.unscaledTimeAsDouble + 1.0;
                window.RefreshLanguage(api.ReadLanguage());
            }
        }

        private void OnGUI()
        {
            if (window == null) return;
            try { window.Draw(); }
            catch (Exception ex)
            {
                window.Close();
                if (!guiFailureReported) Logger.LogWarning("Admin window closed after a UI error: " + ex.Message);
                guiFailureReported = true;
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            this.hasFocus = hasFocus;
            if (!hasFocus && window != null) window.Close();
        }

        private void OnDisable()
        {
            if (selfTools != null) selfTools.Reset();
            if (window != null) window.Close();
        }

        private void OnSceneUnloaded(Scene scene)
        {
            if (selfTools != null) selfTools.Reset();
            if (droppedItems != null) droppedItems.Clear();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (selfTools != null) selfTools.Reset();
            if (droppedItems != null) droppedItems.Clear();
            if (window != null)
            {
                window.Close();
                window.InvalidateItemCatalog();
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            if (selfTools != null) selfTools.Dispose();
            if (hornFuelHud != null) hornFuelHud.Dispose();
            if (droppedItemCapture != null) droppedItemCapture.Dispose();
            if (window != null) window.Dispose();
            if (compatibility != null) compatibility.Dispose();
            window = null;
            api = null;
            compatibility = null;
            selfTools = null;
            hornFuelHud = null;
        }
    }
}



