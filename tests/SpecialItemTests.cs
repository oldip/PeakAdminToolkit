using System;
using PeakAdminToolkit.Items;
using PeakAdminToolkit.Core;
using PeakAdminToolkit.UI;

internal static class SpecialItemTests
{
    private static int checks;
    public static int Run()
    {
        foreach (string name in new[] { "RescueHook_Infinite", "ScoutCookies_Vanilla", "Parachute", "C_King B", "C_Queen W", "Basketball", "Warpsketball", "GuidebookPageScroll Variant", "Binoculars_Prop", "Lollipop_Prop", "Bugle_Prop Variant", "BingBong_Prop Variant", "Passport" })
        {
            Check(!ItemVisibility.ShouldShow(name, true, true, false), "special hidden by default: " + name);
            Check(ItemVisibility.ShouldShow(name, true, true, false, true), "explicit opt-in allows reviewed prefab: " + name);
            Check(!ItemVisibility.ShouldShow(name, true, false, false, true), "opt-in retains game validity check: " + name);
            Check(!ItemVisibility.ShouldShow(name, false, true, false, true), "opt-in retains registration check: " + name);
            Check(!ItemVisibility.ShouldShow(name, true, true, true, true), "opt-in retains replacement exclusion: " + name);
        }
        foreach (string name in new[] { "Cursed Skull", "Bugle_Magic", "ScoutEffigy", "HealingDart Variant", "RitualDagger", "Fannypack" })
        {
            Check(ItemVisibility.ShouldShow(name, true, true, false), "valid multiplayer item appears without opt-in: " + name);
            Check(!ItemVisibility.ShouldShow(name, true, false, false), "invalid multiplayer item requires opt-in: " + name);
            Check(ItemVisibility.ShouldShow(name, true, false, false, true), "invalid multiplayer item can be inspected after opt-in: " + name);
            Check(!ItemVisibility.ShouldShow(name, false, false, false, true), "inspection still requires registration: " + name);
            Check(!ItemVisibility.ShouldShow(name, true, false, true, true), "inspection still excludes replacement: " + name);
        }
        foreach (string name in new[] { "C_King", "C_King Variant", "C_Pawn_m Variant", "C_Pawn_f Variant", "C_NewPiece", "BasketballVariant", "New_UNUSED", "Artifact_TEMP", "Mandrake_Hidden", "Luggage_Prop" })
            Check(!ItemVisibility.ShouldShow(name, true, true, false, true), "opt-in does not open unknown/internal prototypes: " + name);
        Check(ItemVisibility.ShouldShow("RescueHook_Infinite(Clone)", true, true, false, true), "clone name uses the same reviewed policy");
        Check(SpecialItems.Classify("RescueHook_Infinite") == SpecialItemKind.SpecialVariants, "infinite hook subtype");
        Check(SpecialItems.Classify("ScoutCookies_Vanilla") == SpecialItemKind.SpecialVariants, "cookie subtype");
        Check(SpecialItems.Classify("Parachute") == SpecialItemKind.UnusedItems, "parachute subtype");
        Check(SpecialItems.Classify("C_King B") == SpecialItemKind.LobbyToys, "regular chess subtype");
        Check(SpecialItems.Classify("C_King Variant") == SpecialItemKind.None, "chess variants are not admitted");
        Check(SpecialItems.Classify("GuidebookPageScroll Variant").ToString() == "MiscellaneousItems", "scroll belongs to miscellaneous items");
        Check(SpecialItems.Classify("BingBong_Prop Variant") == SpecialItemKind.LobbyToys, "lobby Bing Bong prop is admitted");
        Check(SpecialItems.Classify("Passport") == SpecialItemKind.LobbyToys, "lobby passport is admitted");
        Check(SpecialItems.Classify("ScoutEffigy").ToString() == "MultiplayerItems", "Scout Effigy multiplayer subtype");

        var normal = Entry("Stick");
        var hook = Entry("RescueHook_Infinite");
        var chute = Entry("Parachute");
        var filter = new SpecialItemFilter();
        Check(!filter.Enabled && !filter.Matches(hook), "special filter starts closed");
        filter.SetEnabled(true);
        Check(!filter.Enabled, "cannot opt in outside Other");
        filter.SetCategory(ItemCategory.Other);
        Check(!filter.Matches(hook) && filter.Matches(normal), "Other still hides specials until checked");
        filter.SetEnabled(true);
        Check(filter.Matches(hook) && filter.Matches(chute) && filter.Matches(normal), "checked Other offers regular and special items");
        filter.SelectKind(SpecialItemKind.SpecialVariants);
        Check(filter.Matches(hook) && !filter.Matches(chute) && !filter.Matches(normal), "special subtype selects only its members");
        filter.SetCategory(ItemCategory.None);
        Check(!filter.Enabled && filter.SelectedKind == SpecialItemKind.None && !filter.Matches(hook), "switching to All clears opt-in");
        filter.SetCategory(ItemCategory.Other);
        Check(!filter.Enabled, "returning to Other requires another manual opt-in");
        filter.SetEnabled(true);
        filter.Reset();
        Check(!filter.Enabled && !filter.Matches(hook), "close/open reset hides specials");
        Check(SpecialItems.AvailableKinds(new[] { normal }).Length == 1, "no empty special subfilters");
        Check(SpecialItems.AvailableKinds(new[] { hook, chute }).Length == 3, "only populated special subfilters");
        Check(Array.Exists(SpecialItems.AvailableKinds(new[] { Entry("GuidebookPageScroll Variant") }), k => k.ToString() == "MiscellaneousItems"),
            "miscellaneous subfilter appears when populated");

        var scroll = Entry("GuidebookPageScroll Variant");
        var page = Entry("GuidebookPage_0_Intro");
        Check(!Array.Exists(SpecialItems.AvailableKinds(new[] { scroll, page }), k => k == SpecialItemKind.MultiplayerItems),
            "scroll and pages alone never create an empty multiplayer filter");
        filter.SetCategory(ItemCategory.Other);
        filter.SetEnabled(true);
        filter.SelectKind(SpecialItems.Classify(scroll.SpawnName));
        Check(filter.Matches(scroll) && filter.Matches(page) && !filter.Matches(Entry("ScoutEffigy")),
            "miscellaneous filter groups scroll and torn pages separately from multiplayer items");
        foreach (string name in new[] { "GuidebookPage", "GuidebookPage_0_Intro", "GuidebookPage_13_FirstTeams" })
        {
            Check(ItemVisibility.ShouldShow(name, true, true, false, true), "reviewed guidebook page accepts opt-in: " + name);
            Check(!ItemVisibility.ShouldShow(name, true, true, false), "guidebook page stays hidden by default: " + name);
            Check(!ItemVisibility.ShouldShow(name, true, false, false, true), "guidebook page still requires validity: " + name);
        }
        Check(!ItemVisibility.ShouldShow("GuidebookPage_NewUnknown", true, true, false, true), "unknown pages remain excluded");
        Check(ItemVisibility.ShouldShow("Fannypack", true, true, false), "valid fannypack is a normal item");
        Check(SpecialItems.Classify("Fannypack") == SpecialItemKind.MultiplayerItems, "fannypack is a multiplayer candidate");

        filter.Reset();
        filter.SetCategory(ItemCategory.None);
        Check(filter.Matches(Entry("ScoutEffigy")), "All accepts a valid multiplayer item without opt-in");
        filter.SetCategory(ItemCategory.Consumable);
        Check(filter.Matches(Entry("ScoutEffigy")), "normal usage categories accept valid multiplayer items");

        var loc = new Localization();
        loc.Update("zh-TW", null);
        Check(SpecialItems.DisplayName("RescueHook_Infinite", loc) == "救援鉤爪（無限版）", "infinite version has a distinct translated label");
        Check(SpecialItems.DisplayName("C_King B", loc) == "國王（黑棋）", "chess color is translated");
        Check(SpecialItems.DisplayName("C_Queen W", loc) == "皇后（白棋）", "white chess name is translated");
        Check(SpecialItems.DisplayName("GuidebookPageScroll Variant", loc) == "卷軸", "scroll uses the selected Traditional Chinese name");
        Check(SpecialItems.DisplayName("Binoculars_Prop", loc) == "雙筒望遠鏡（大廳）", "lobby binoculars are distinguished from equipment");
        Check(SpecialItems.DisplayName("Bugle_Magic", loc) == "友情軍號", "friendship bugle uses the selected Traditional Chinese name");
        Check(loc.Text(SpecialItems.Classify(scroll.SpawnName).ToString()) == "雜物", "Traditional Chinese miscellaneous label is loaded");
        loc.Update("zh-CN", null);
        Check(loc.Text(SpecialItems.Classify(scroll.SpawnName).ToString()) == "杂物", "Simplified Chinese miscellaneous label is loaded");
        loc.Update("en", null);
        Check(loc.Text(SpecialItems.Classify(scroll.SpawnName).ToString()) == "Miscellaneous", "English miscellaneous label is loaded");
        return checks;
    }
    private static ItemCatalogEntry Entry(string name) { return new ItemCatalogEntry(name,name,name,name,name,null,ItemCategory.Other,ConsumableKind.None); }
    private static void Check(bool value, string name)
    {
        checks++;
        if (!value) throw new InvalidOperationException("FAIL: " + name);
    }
}
