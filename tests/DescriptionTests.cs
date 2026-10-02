using System;
using System.IO;
using System.Collections.Generic;
using PeakAdminToolkit.Items;
using PeakAdminToolkit.Core;
using PeakAdminToolkit.UI;
using Newtonsoft.Json.Linq;

internal static class DescriptionTests
{
    private static int checks;
    public static void Main(string[] args)
    {
        string directory = Path.Combine(Path.GetTempPath(), "PeakAdminToolkit-descriptions-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string previous = Environment.CurrentDirectory;
        try
        {
            Environment.CurrentDirectory = directory;
            var warnings = new List<string>();
            var descriptions = new ItemDescriptions(warnings.Add);
            Check(descriptions.Get("en", "Flare") == "Found at the crash site and peak. Summons the rescue helicopter when used at the PEAK",
                "built-in descriptions work without external config");
            Check(Directory.GetFileSystemEntries(directory).Length == 0, "loading built-in data creates no config files");
            Check(descriptions.Get("zh-CN", "Sunscreen") == "可使用3次。每次喷雾提供90秒防晒", "built-in simplified text remains unchanged");
            Check(descriptions.Get("zh-TW", "Sunscreen") == "可使用3次。每次噴霧提供90秒防曬", "traditional conversion preserves numbers");
            Check(descriptions.Get("en", " sunscreen ") == descriptions.Get("en", "Sunscreen"), "lookup ignores case and surrounding whitespace");
            Check(descriptions.Get("en", "Other UI key", "Medicinal Root", "MedicinalRoot").StartsWith("Found on the shore"), "English alias fallback");
            Check(descriptions.Get("zh-TW", "Unlisted item") == "", "unknown item allows category-only fallback");

            string fakeConfig = Path.Combine(directory, "BepInEx", "config");
            Directory.CreateDirectory(fakeConfig);
            const string fake = "{\"items\":{\"Sunscreen\":{\"description\":\"EXTERNAL OVERRIDE\"}}}";
            foreach (string name in new[] { "peak-item-tooltip.descriptions.json", "peak-item-tooltip.descriptions.zh-CN.json", "peak-item-tooltip.descriptions.zh-TW.json" })
            {
                File.WriteAllText(Path.Combine(directory, name), fake);
                File.WriteAllText(Path.Combine(fakeConfig, name), fake);
            }
            Check(descriptions.Get("zh-TW", "Sunscreen") == "可使用3次。每次噴霧提供90秒防曬", "external files cannot override embedded text");
            Check(File.ReadAllText(Path.Combine(fakeConfig, "peak-item-tooltip.descriptions.zh-CN.json")) == fake, "another mod's config is untouched");
            Check(Directory.GetFiles(directory, "*", SearchOption.AllDirectories).Length == 6, "reload creates no description files");
            Check(warnings.Count == 0, "valid embedded data produces no warnings");

            var loc = new Localization();
            loc.Update("zh-TW", null);
            var sunscreen = new ItemCatalogEntry("Sunscreen", "防曬噴霧", "Sunscreen", "防晒喷雾", "防曬噴霧", null,
                ItemCategory.Consumable, ConsumableKind.Buff, descriptions.Get("en", "Sunscreen"), descriptions.Get("zh-CN", "Sunscreen"), descriptions.Get("zh-TW", "Sunscreen"));
            string tip = ItemTooltip.Format(sunscreen, loc);
            Check(tip.Contains("防曬噴霧") && tip.Contains("消耗品") && tip.Contains("增益") && tip.Contains("每次噴霧提供90秒防曬"), "tooltip combines name, category and built-in description");
            Check(!tip.Contains("Sunscreen") && !tip.Contains(descriptions.Get("en", "Sunscreen")), "Chinese tooltip has no extra English");
            var unknown = new ItemCatalogEntry("Unknown", "未知物品", "Unknown", "未知物品", "未知物品", null, ItemCategory.Other, ConsumableKind.None);
            Check(ItemTooltip.Format(unknown, loc) == "未知物品\n分類：其他", "missing description shows category only");

            foreach (string language in new[] { "en", "zh-CN" })
            {
                string name = language == "en" ? "peak-item-tooltip.descriptions.json" : "peak-item-tooltip.descriptions.zh-CN.json";
                JObject items = (JObject)JObject.Parse(File.ReadAllText(Path.Combine(args[0], "data", "descriptions", name)))["items"];
                bool allMatch = true;
                foreach (JProperty item in items.Properties())
                    allMatch &= descriptions.Get(language, item.Name) == ((string)item.Value["description"]).Trim();
                Check(allMatch && items.Count == 131, "all supplied descriptions load exactly for " + language);
            }
            JObject simplified = (JObject)JObject.Parse(File.ReadAllText(Path.Combine(args[0], "data", "descriptions", "peak-item-tooltip.descriptions.zh-CN.json")))["items"];
            bool allTraditional = true;
            foreach (JProperty item in simplified.Properties()) allTraditional &= descriptions.Get("zh-TW", item.Name).Length > 0;
            Check(allTraditional, "all 131 traditional descriptions resolve");
            Console.WriteLine("PASS: " + checks + " embedded description checks (real JSON and Windows conversion).");
        }
        catch (Exception ex) { Console.WriteLine("FAIL: " + ex.Message); Environment.ExitCode = 1; }
        finally
        {
            Environment.CurrentDirectory = previous;
            Directory.Delete(directory, true);
        }
    }
    private static void Check(bool value, string name)
    {
        checks++;
        if (!value) throw new InvalidOperationException(name);
    }
}
