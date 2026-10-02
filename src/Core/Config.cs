using BepInEx.Configuration;
using UnityEngine;

namespace PeakAdminToolkit.Core
{
    internal sealed class ToolkitConfig
    {
        public readonly ConfigEntry<bool> Enabled;
        public readonly ConfigEntry<KeyCode> ToggleKey;
        public readonly ConfigEntry<string> Language;

        public ToolkitConfig(ConfigFile file)
        {
            Enabled = file.Bind("General", "Enabled", true,
                new ConfigDescription("Enable the toolkit. Disabling closes the window, resets self tools and stops the horn HUD correction. / 停用时关闭窗口并恢复自身工具 / 停用時關閉視窗並恢復自身工具"));
            ToggleKey = file.Bind("Interface", "ToggleKey", KeyCode.F8,
                new ConfigDescription("Open or close the window. None disables the shortcut. / 窗口快捷键 / 視窗快捷鍵"));
            Language = file.Bind("Interface", "Language", "Auto",
                new ConfigDescription("UI language. Auto follows PEAK; unsupported languages use English. / 界面语言 / 介面語言",
                    new AcceptableValueList<string>("Auto", "en", "zh-CN", "zh-TW")));
        }
    }
}
