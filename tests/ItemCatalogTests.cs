using System;
using PeakAdminToolkit.Items;

internal static class ItemCatalogTests
{
    private static int checks;
    public static int Run()
    {
        Check(Match(""), "empty search shows all names");
        Check(Match("  "), "whitespace search shows all names");
        Check(Match("MEDICINAL ROOT"), "English search ignores case");
        Check(Match("medicinalroot"), "English search ignores spacing");
        Check(Match("药根草"), "simplified translation matches traditional current name");
        Check(Match("藥根草"), "traditional translation is searchable");
        Check(ItemSearch.Matches("便携炉", "便攜爐", "Portable Stove", "便携式炉具", "便攜爐"), "cross-script search works even when translations use different words");
        Check(ItemSearch.Matches("药根草", "藥根草", "Medicinal Root", null, null), "cross-script search works when alternate translation is missing");
        Check(ItemSearch.Matches("醫疗", "醫療箱", "First Aid Kit", null, null), "mixed-script query matches canonical Chinese name");
        Check(ItemSearch.Matches("藥根草", "药根草", "Medicinal Root", "药根草", "藥根草"), "traditional query matches simplified game language");
        Check(ItemSearch.Matches("傘", "滑翔翼", "Glider", "滑翔翼", "滑翔翼"), "umbrella query also finds glider");
        Check(ItemSearch.Matches("umbrella", "滑翔翼", "Glider", "滑翔翼", "滑翔翼"), "English umbrella synonym finds glider");
        Check(!Match("ygc"), "search does not synthesize initials");
        Check(!Match("yaogencao"), "search does not synthesize pinyin");
        Check(!Match("nonexistent"), "unrelated query does not match");
        Check(ItemSearch.Matches("ygc", "YGCamera", null, null, null), "literal Latin item names are still searchable");
        Check(!ItemSearch.Matches("root", null, null, null, null), "missing translations do not invent matches");
        Check(Visible("NormalItem", true, true, false), "registered valid item is visible");
        Check(!Visible("NormalItem", true, false, false), "unspawnable item is hidden");
        Check(!Visible("NormalItem", false, true, false), "unregistered item is hidden");
        foreach (string hidden in new[] { "Mandrake_Hidden", "C_King", "BasketballVariant", "Luggage_Prop", "Artifact_TEMP", "Cheat Beacon", "Warpsketball", "New_UNUSED", "GuidebookPage_99" })
            Check(!Visible(hidden, true, true, false), "hidden prefab: " + hidden);
        Check(!Visible("OtherItem", true, true, true), "replacement prefab is hidden");
        Check(!Visible("Mandrake_Hidden(Clone)", true, true, false), "clone suffix does not bypass hidden rule");
        Check(ItemVisibility.CanRequestSpawn(true), "room host can request spawn");
        Check(ItemVisibility.CanRequestSpawn(true), "room client can request spawn through Master RPC");
        Check(!ItemVisibility.CanRequestSpawn(false), "spawn requires a room");
        var categories=ItemClassification.Categories("Scout Effigy", "Mystical");
        var kinds=ItemClassification.Kinds("Scout Effigy", "Mystical");
        Check(ItemClassification.Matches(categories, kinds, ItemCategory.None, ConsumableKind.None), "all categories includes effigy");
        Check(ItemClassification.Matches(categories, kinds, ItemCategory.Mystical, ConsumableKind.None), "effigy is shown in mystical");
        Check(ItemClassification.Matches(categories, kinds, ItemCategory.Consumable, ConsumableKind.Revive), "effigy matches consumable revival filter");
        Check(!ItemClassification.Matches(categories, kinds, ItemCategory.Equipment, ConsumableKind.None), "equipment filter excludes effigy");
        Check(!ItemClassification.Matches(categories, kinds, ItemCategory.Consumable, ConsumableKind.Food), "food filter excludes effigy without food tags");
        return checks;
    }
    private static bool Match(string query) { return ItemSearch.Matches(query, "藥根草", "Medicinal Root", "药根草", "藥根草"); }
    private static bool Visible(string name, bool registered, bool valid, bool duplicate) { return ItemVisibility.ShouldShow(name, registered, valid, duplicate); }
    private static void Check(bool condition, string name)
    {
        checks++;
        if (!condition) { Console.WriteLine("FAIL: " + name); Environment.Exit(1); }
    }
}
