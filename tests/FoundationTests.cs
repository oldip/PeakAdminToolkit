using System;
using PeakAdminToolkit.Core;
using PeakAdminToolkit.UI;
using UnityEngine;

internal static class FoundationTests
{
    private static int checks;
    private static void Equal<T>(T expected, T actual, string label)
    {
        checks++;
        if (!object.Equals(expected, actual))
        {
            Console.WriteLine("FAIL: " + label);
            Environment.Exit(1);
        }
    }

    private static void Main()
    {
        try { Run(); }
        catch (Exception ex) { Console.WriteLine("FAIL: " + ex.Message); Environment.ExitCode = 1; }
    }

    private static void Run()
    {
        Equal("zh-CN", Localization.Resolve("Auto", "SimplifiedChinese"), "Auto simplified game enum");
        Equal("zh-TW", Localization.Resolve("Auto", "TraditionalChinese"), "Auto traditional game enum");
        Equal("en", Localization.Resolve("Auto", "English"), "Auto English");
        Equal("en", Localization.Resolve("Auto", "French"), "Unsupported game language");
        Equal("en", Localization.Resolve("Auto", null), "Missing game language");
        Equal("zh-TW", Localization.Resolve("zh-TW", "SimplifiedChinese"), "Explicit override wins");
        Equal("zh-CN", Localization.Resolve("zh-CN", null), "Override with missing API");
        Equal("en", Localization.Resolve("en", "TraditionalChinese"), "Explicit English wins");
        Equal("zh-TW", Localization.Resolve("invalid", "TraditionalChinese"), "Invalid preference behaves as Auto");
        var loc = new Localization();
        loc.Update("Auto", "TraditionalChinese");
        Equal("關閉", loc.Text("Close"), "Traditional text");
        loc.Update("Auto", "SimplifiedChinese");
        Equal("关闭", loc.Text("Close"), "Runtime game language change");
        loc.Update("en", "SimplifiedChinese");
        Equal("Close", loc.Text("Close"), "Runtime override");
        foreach (string language in new[] { "en", "zh-CN", "zh-TW" })
        {
            loc.Update(language, null);
            foreach (string key in Localization.Keys)
                Equal(false, string.IsNullOrEmpty(loc.Text(key)), "Missing translation " + language + "/" + key);
        }
        Equal("unknown-key", loc.Text("unknown-key"), "Unknown label remains diagnosable");

        Rect normalWindow = WindowBounds.Clamp(new Rect(80, 40, 620, 360), 800, 600);
        Equal(80f, normalWindow.x, "Window bounds preserve centered x");
        Equal(40f, normalWindow.y, "Window bounds preserve centered y");
        Equal(620f, normalWindow.width, "Window bounds preserve normal width");
        Equal(360f, normalWindow.height, "Window bounds preserve compact height");
        Rect smallScreenWindow = WindowBounds.Clamp(new Rect(500, 400, 620, 360), 400, 300);
        Equal(16f, smallScreenWindow.x, "Window bounds fit right edge on small screen");
        Equal(16f, smallScreenWindow.y, "Window bounds fit bottom edge on small screen");
        Equal(384f, smallScreenWindow.width, "Window bounds shrink width on small screen");
        Equal(284f, smallScreenWindow.height, "Window bounds shrink height on small screen");

        var toast = new Toast();
        toast.Show("saved", 10.0, 2.0);
        Equal("saved", toast.Current(11.99), "Toast before expiry");
        Equal(null, toast.Current(12.0), "Toast expires at boundary");
        toast.Show("new", 13.0, 2.0);
        toast.Clear();
        Equal(null, toast.Current(13.1), "Closing clears toast");

        var tooltip = new Tooltip();
        Equal(null, tooltip.Observe("help", "help", 1.0), "Tooltip waits");
        Equal(null, tooltip.Observe("help", "help", 1.99), "Tooltip delay");
        Equal("help", tooltip.Observe("help", "help", 2.0), "Tooltip appears at boundary");
        Equal(null, tooltip.Observe("different", "different", 2.1), "New target resets timer");
        Equal(null, tooltip.Observe(null, "", 4.0), "No hover hides tooltip");
        Equal(null, tooltip.Observe("different", "different", 4.1), "Returning starts new timer");
        tooltip.Clear();
        Equal(null, tooltip.Observe("different", "different", 8.0), "Closing clears hover history");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        var cursor = new CursorLease();
        cursor.Acquire();
        cursor.Acquire();
        Equal(CursorLockMode.None, Cursor.lockState, "Open releases cursor");
        Equal(true, Cursor.visible, "Open shows cursor");
        cursor.Release(false);
        Equal(CursorLockMode.Locked, Cursor.lockState, "Repeated acquire preserves initial state");
        Equal(false, Cursor.visible, "Close restores visibility");
        Cursor.lockState = CursorLockMode.Confined;
        cursor.Release(false);
        Equal(CursorLockMode.Confined, Cursor.lockState, "Repeated release has no side effects");
        cursor.Acquire();
        cursor.Release(true);
        Equal(CursorLockMode.None, Cursor.lockState, "Other game window keeps cursor");
        Equal(true, Cursor.visible, "Other game window keeps visibility");
        checks += RevisionTests.Run();
        checks += CardLayoutTests.Run();
        checks += HoverTests.Run();
        checks += ItemCatalogTests.Run();
        checks += SpecialItemTests.Run();
        Console.WriteLine("PASS: " + checks + " behavioral checks (Unity rendering not exercised).");
    }
}
