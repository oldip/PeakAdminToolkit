using System;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;

namespace PeakAdminToolkit.Core
{
    internal sealed class Compatibility : IDisposable
    {
        private readonly Harmony harmony;
        private readonly ManualLogSource log;
        private readonly FieldInfo languageField;
        private readonly FieldInfo guiInstance;
        private readonly PropertyInfo cursorProperty;
        private static Func<bool> windowOpen;
        private bool reportedLanguageFailure;

        public bool WindowHooksReady { get; private set; }

        public Compatibility(Func<bool> isWindowOpen, ManualLogSource logger)
        {
            log = logger;
            harmony = new Harmony(Plugin.Guid);
            Type text = FindGameType("LocalizedText");
            Type gui = FindGameType("GUIManager");
            if (text != null) languageField = text.GetField("CURRENT_LANGUAGE", BindingFlags.Public | BindingFlags.Static);
            if (gui != null)
            {
                guiInstance = gui.GetField("instance", BindingFlags.Public | BindingFlags.Static);
                cursorProperty = gui.GetProperty("windowShowingCursor", BindingFlags.Public | BindingFlags.Instance);
            }
            PropertyInfo input = gui == null ? null : gui.GetProperty("windowBlockingInput", BindingFlags.Public | BindingFlags.Instance);
            if (!IsBooleanGetter(input) || !IsBooleanGetter(cursorProperty) || guiInstance == null)
            {
                log.LogWarning("Window unavailable: PEAK input/cursor API is incompatible. Expected GUIManager.windowBlockingInput and windowShowingCursor.");
                return;
            }
            windowOpen = isWindowOpen;
            try
            {
                var postfix = new HarmonyMethod(typeof(Compatibility).GetMethod("IncludeToolkitWindow", BindingFlags.NonPublic | BindingFlags.Static));
                harmony.Patch(input.GetGetMethod(), postfix: postfix);
                harmony.Patch(cursorProperty.GetGetMethod(), postfix: postfix);
                WindowHooksReady = true;
            }
            catch (Exception ex)
            {
                Dispose();
                log.LogWarning("Window unavailable: input hooks could not be installed. " + ex.Message);
            }
        }

        private static Type FindGameType(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                if (assembly.GetName().Name == "Assembly-CSharp") return assembly.GetType(name, false);
            return null;
        }

        private static bool IsBooleanGetter(PropertyInfo property)
        {
            return property != null && property.PropertyType == typeof(bool) && property.GetGetMethod() != null;
        }

        private static void IncludeToolkitWindow(ref bool __result)
        {
            __result = __result || (windowOpen != null && windowOpen());
        }

        public string ReadGameLanguage()
        {
            try
            {
                object value = languageField == null ? null : languageField.GetValue(null);
                if (value != null) return value.ToString();
            }
            catch (Exception ex)
            {
                if (!reportedLanguageFailure) log.LogWarning("PEAK language read failed: " + ex.Message);
            }
            if (!reportedLanguageFailure)
                log.LogWarning("PEAK language unavailable; Auto uses English. An explicit UI language still works.");
            reportedLanguageFailure = true;
            return null;
        }

        // Query after our window closes, so our postfix no longer adds cursor ownership.
        public bool GameWantsCursor()
        {
            if (guiInstance == null || cursorProperty == null) return false;
            try
            {
                object instance = guiInstance.GetValue(null);
                var unityObject = instance as UnityEngine.Object;
                if (unityObject == null) return false;
                return (bool)cursorProperty.GetValue(instance, null);
            }
            catch (Exception) { return false; }
        }

        public void Dispose()
        {
            WindowHooksReady = false;
            windowOpen = null;
            harmony.UnpatchSelf();
        }
    }
}
