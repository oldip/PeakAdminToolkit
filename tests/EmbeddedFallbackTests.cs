using System;
using PeakAdminToolkit.Items;
using PeakAdminToolkit.Core;

internal static class EmbeddedFallbackTests
{
    private static int checks;
    private static void Main()
    {
        try { Run(); }
        catch (Exception ex) { Console.WriteLine("FAIL: " + ex.Message); Environment.ExitCode = 1; }
    }

    private static void Run()
    {
        var loc = new Localization();
        loc.Update("zh-CN", null);
        Check(loc.Text("Close") == "Close", "malformed translation falls back to English");
        loc.Update("zh-TW", null);
        Check(loc.Text("Heading") == "測試翻譯", "valid entries in a partial language stay usable");
        Check(loc.Text("Close") == "Close", "missing key falls back to English");
        Check(loc.Text("Saved") == "Language preference saved.", "non-string entry falls back to English");
        Check(loc.Format("ActiveLanguage", "English") == "Displayed language: English", "broken translated format falls back to English");
        Check(loc.Text("unknown-key") == "unknown-key", "unknown label remains diagnosable");
        var descriptions = new ItemDescriptions();
        Check(descriptions.Get("zh-TW", "Sunscreen") == "", "malformed description resource allows category/native fallback");
        Check(descriptions.Get("en", "Flare").StartsWith("Found at the crash site"), "one broken description language leaves another usable");
        Console.WriteLine("PASS: " + checks + " translation/description isolation checks.");
    }
    private static void Check(bool value, string name)
    {
        checks++;
        if (!value) { Console.WriteLine("FAIL: " + name); Environment.Exit(1); }
    }
}
