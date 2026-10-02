using System;
using System.Collections.Generic;
using PeakAdminToolkit.Core;
using PeakAdminToolkit.Items;
using UnityEngine;

// Compiled as Assembly-CSharp.exe so the real reflection adapter discovers this
// narrow boundary double. This does not load PEAK or simulate Photon replication.
internal static class CatalogApiTests
{
    private static int checks;
    private static void Check(bool value, string name)
    {
        checks++;
        if (!value) { Console.WriteLine("FAIL: " + name); Environment.Exit(1); }
    }

    public static void Main(string[] args)
    {
        try { Run(args); }
        catch (Exception ex) { Console.WriteLine("FAIL: " + ex.Message); Environment.ExitCode = 1; }
    }

    private static void Run(string[] args)
    {
        var icon = new Texture2D();
        var root = new Item { name="MedicinalRoot", UIData=new ItemUIData { itemName="Medicinal Root", icon=icon } };
        var duplicate = new Item { name="UnlistedReplacement", isSecretlyOtherItemPrefab=root };
        ItemDatabase.Instance.itemLookup.Add(root.name, root);
        ItemDatabase.Instance.itemLookup.Add(duplicate.name, duplicate);
        var api = new PeakApi(new Compatibility());
        var catalog = api.ReadItemCatalog();
        Check(catalog.Count == 1, "item replacement reference excludes duplicates even without a hidden prefab name");
        var entry=catalog[0];
        Check(entry.CurrentName == "藥根草", "current game language name reaches the catalog");
        Check(api.MatchesItem("药根草", entry), "simplified game translation is searchable in a traditional UI");
        Check(api.MatchesItem("medicinal", entry), "English translation remains searchable");
        Check(entry.DisplayName("zh-TW") == "藥根草", "traditional card contains only its selected translation");
        Check(entry.DisplayName("zh-CN") == "药根草", "simplified interface override controls the card name");
        Check(entry.DisplayName("en") == "Medicinal Root", "English interface override controls the card name");
        Check(entry.DisplayDescription("zh-TW") == "治療傷勢的根草。", "catalog carries the game's traditional description");
        Check(entry.DisplayDescription("zh-CN") == "治疗伤势的根草。", "description follows simplified interface override");
        Check(entry.DisplayDescription("en") == "A root for treating injuries.", "description follows English interface override");
        Check(LocalizedText.CURRENT_LANGUAGE == Language.TraditionalChinese, "reading translations does not change the game language");
        LocalizedText.MissingTraditionalDescription = true;
        Check(api.ReadItemCatalog()[0].DisplayDescription("zh-TW") == string.Empty, "missing traditional description does not mix in English");
        LocalizedText.MissingTraditionalDescription = false;
        LocalizedText.ThrowOnDescription = true;
        var noDescriptionCatalog = api.ReadItemCatalog();
        Check(noDescriptionCatalog.Count == 1 && noDescriptionCatalog[0].DisplayDescription("en") == string.Empty,
            "description lookup failure leaves the catalog item usable");
        LocalizedText.ThrowOnDescription = false;
        var embeddedApi = new PeakApi(new Compatibility(), new ItemDescriptions());
        var embeddedEntry = embeddedApi.ReadItemCatalog()[0];
        Check(embeddedEntry.DisplayDescription("en") == "Found on the shore, tropics, roots and mesa. Removes poison and spores, and heals injury",
            "real adapter prefers built-in description over native description");
        Check(embeddedEntry.DisplayDescription("zh-TW").Contains("發現") && embeddedEntry.DisplayDescription("zh-TW").Contains("受傷"),
            "real adapter matches stable UI name and carries traditional description");
        var untranslated = new ItemCatalogEntry("FallbackPrefab", "目前名稱", "English fallback", "", null,
            null, ItemCategory.Other, ConsumableKind.None);
        Check(untranslated.DisplayName("zh-TW") == "目前名稱", "missing translation keeps one usable current name");
        Check(untranslated.DisplayName("zh-CN") == "目前名稱", "empty translation keeps one usable current name");
        Check(!api.MatchesItem("ygc", entry), "single-language cards do not reintroduce pinyin aliases");
        var available = ItemClassification.AvailableKinds(catalog);
        Check(Array.IndexOf(available, ConsumableKind.Revive) < 0, "empty revival filter is not offered");
        Check(Array.IndexOf(available, ConsumableKind.Healing) >= 0, "populated healing filter is offered");
        Check(available[0] == ConsumableKind.None, "all remains the first consumable filter");
        var emptyKinds = ItemClassification.AvailableKinds(new List<ItemCatalogEntry>());
        Check(emptyKinds.Length == 1 && emptyKinds[0] == ConsumableKind.None, "empty catalog exposes only all");
        var effigy = new ItemCatalogEntry("ScoutEffigy", "雕像", "Scout Effigy", "雕像", "雕像",
            null, ItemCategory.Mystical | ItemCategory.Consumable, ConsumableKind.Revive);
        catalog.Add(effigy);
        Check(Array.IndexOf(ItemClassification.AvailableKinds(catalog), ConsumableKind.Revive) >= 0,
            "revival filter appears when a visible revival consumable exists");
        var nonConsumable = new ItemCatalogEntry("Equipment", "Equipment", "Equipment", "", "",
            null, ItemCategory.Equipment, ConsumableKind.Revive);
        Check(ItemClassification.AvailableKinds(new[] { nonConsumable }).Length == 1,
            "other categories cannot create an empty consumable subfilter");
        Check((entry.Categories & ItemCategory.Consumable) != 0, "adapter classifies by stable UI name");
        Check((entry.Kinds & ConsumableKind.Healing) != 0, "adapter carries consumable use into the catalog");
        Check(object.ReferenceEquals(icon, entry.Icon), "catalog borrows the PEAK icon without copying it");
        root.UIData.ThrowOnIcon=true;
        Check(object.ReferenceEquals(icon, api.ReadItemCatalog()[0].Icon), "icon field is fallback when PEAK icon getter fails");
        root.UIData.icon=null;
        Check(api.ReadItemCatalog().Count == 1, "missing icon does not hide an otherwise valid item");

        var incompatible = new IncompatibleItem { name="NewApiItem" };
        ((Item)incompatible).UIData.itemName = "New API item";
        ItemDatabase.Instance.itemLookup.Add(incompatible.name, incompatible);
        List<ItemCatalogEntry> surviving = null;
        try { surviving = api.ReadItemCatalog(); } catch (Exception) { }
        Check(surviving != null && surviving.Count == 1, "one incompatible item API leaves other catalog entries usable");
        Check(api.CanOpenWindow && api.ReadLanguage() == "TraditionalChinese", "catalog incompatibility leaves basic window and language usable");
        ItemDatabase.Instance.itemLookup.Remove(incompatible.name);
        var savedItems = Character.localCharacter.refs.items;
        Character.localCharacter.refs.items = null;
        Check(!api.CanSpawnItems && !api.SpawnItem(entry), "missing generation API disables generation");
        Check(api.ReadItemCatalog().Count == 1 && api.CanOpenWindow, "generation API failure does not disable browsing or window");
        Character.localCharacter.refs.items = savedItems;
        var savedDatabase = ItemDatabase.Instance;
        ItemDatabase.Instance = null;
        Check(api.ReadItemCatalog().Count == 0 && !api.CanSpawnItems, "missing database disables item capabilities safely");
        Check(api.CanOpenWindow && api.ReadLanguage() == "TraditionalChinese", "missing item database leaves overview capabilities usable");
        ItemDatabase.Instance = savedDatabase;

        Photon.Pun.PhotonNetwork.IsMasterClient=true;
        Photon.Pun.PhotonNetwork.InRoom=true;
        Check(api.SpawnItem(entry), "host can submit a valid spawn request");
        Check(Character.localCharacter.refs.items.LastSpawn == "MedicinalRoot", "request uses the registered prefab name");
        Photon.Pun.PhotonNetwork.IsMasterClient=false;
        Check(api.CanSpawnItems && api.SpawnItem(entry), "client can submit the native Master-targeted spawn request");
        Photon.Pun.PhotonNetwork.IsMasterClient=true;
        Photon.Pun.PhotonNetwork.InRoom=false;
        Check(!api.SpawnItem(entry), "spawn outside a room is blocked");
        Photon.Pun.PhotonNetwork.InRoom=true;
        root.Valid=false;
        Check(!api.SpawnItem(entry), "stale catalog entry is rechecked for current spawnability");
        root.Valid=true;
        root.isSecretlyOtherItemPrefab=duplicate;
        Check(!api.SpawnItem(entry), "stale entry now marked duplicate cannot spawn");
        root.isSecretlyOtherItemPrefab=null;
        ItemDatabase.Instance.itemLookup.Remove(root.name);
        Check(!api.SpawnItem(entry), "removed registration cannot spawn from a stale entry");
        Check(Character.localCharacter.refs.items.Requests == 2, "rejected requests never invoke the game's spawn method");
        var reviewed = Newtonsoft.Json.Linq.JArray.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(args[0], "tests", "fixtures", "special-prefabs.json")));
        foreach (var prefab in reviewed)
        {
            string name = (string)prefab["prefab"];
            Item reviewedItem = name.StartsWith("GuidebookPage_") || name == "GuidebookPage" ? new Guidebook() :
                name == "Fannypack" ? (Item)new Backpack() : new Item();
            reviewedItem.name = name;
            reviewedItem.UIData = new ItemUIData { itemName=(string)prefab["uiName"] };
            ItemDatabase.Instance.itemLookup[name] = reviewedItem;
        }
        Check(api.ReadItemCatalog().Count == 6, "valid multiplayer items enter the normal catalog without opt-in");
        var extras = api.ReadItemCatalog(true);
        Check(extras.Count == 44, "all 44 reviewed prefabs can enter the opt-in catalog");
        bool otherOnly = true;
        foreach (var specialEntry in extras)
        {
            bool multiplayer = SpecialItems.Classify(specialEntry.SpawnName).ToString() == "MultiplayerItems";
            otherOnly &= (specialEntry.Categories & ItemCategory.Other) != 0 && (multiplayer || specialEntry.Categories == ItemCategory.Other);
        }
        Check(otherOnly, "special catalog entries are available under Other");
        var pageEntry = extras.Find(e => e.SpawnName == "GuidebookPage_0_Intro");
        Check(pageEntry != null && pageEntry.Categories == ItemCategory.Other, "Item-derived guidebook page enters miscellaneous catalog");
        var packEntry = extras.Find(e => e.SpawnName == "Fannypack");
        Check(packEntry != null && (packEntry.Categories & ItemCategory.Equipment) != 0, "Item-derived fannypack retains equipment classification");
        ItemDatabase.Instance.itemLookup["GuidebookPage_0_Intro"].Valid = false;
        Check(!api.ReadItemCatalog(true).Exists(e => e.SpawnName == "GuidebookPage_0_Intro"), "invalid derived guidebook page is still excluded");
        ItemDatabase.Instance.itemLookup["GuidebookPage_0_Intro"].Valid = true;
        var effigyEntry = extras.Find(e => e.SpawnName == "ScoutEffigy");
        Check(effigyEntry != null && (effigyEntry.Categories & ItemCategory.Consumable) != 0 &&
            (effigyEntry.Categories & ItemCategory.Mystical) != 0 && (effigyEntry.Kinds & ConsumableKind.Revive) != 0,
            "multiplayer Scout Effigy retains its revival classification");
        var embeddedSpecialApi = new PeakApi(new Compatibility(), new ItemDescriptions());
        var blowgun = embeddedSpecialApi.ReadItemCatalog(true).Find(e => e.SpawnName == "HealingDart Variant");
        Check(blowgun != null && blowgun.DisplayDescription("en").Contains("Does not spawn in solo play"),
            "special multiplayer item keeps the built-in solo-play description");
        Check(blowgun.DisplayDescription("zh-CN").Length > 0, "special multiplayer item has the embedded Simplified Chinese description");
        var infinite = extras.Find(e => e.SpawnName == "RescueHook_Infinite");
        Check(infinite != null && infinite.DisplayName("zh-TW") == "救援鉤爪（無限版）", "real adapter uses translated variant name");
        Check(api.MatchesItem("无限", infinite), "variant label is searchable across Chinese scripts");
        var glider = new ItemCatalogEntry("Glider", "滑翔翼", "Glider", "滑翔翼", "滑翔翼", null, ItemCategory.Equipment, ConsumableKind.None);
        Check(api.MatchesItem("傘", glider), "Chinese umbrella alias reaches glider through the catalog adapter");
        Check(!infinite.DisplayDescription("en").Contains("4"), "special claw does not inherit normal claw usage limits");
        Check(!api.SpawnItem(infinite), "stale special entry cannot spawn after opt-in closes");
        Photon.Pun.PhotonNetwork.IsMasterClient=false;
        Check(api.SpawnItem(infinite, true), "Client special request still requires explicit opt-in");
        Photon.Pun.PhotonNetwork.IsMasterClient=true;
        ItemDatabase.Instance.itemLookup["RescueHook_Infinite"].Valid=false;
        Check(!api.SpawnItem(infinite, true), "special request rechecks current validity");
        ItemDatabase.Instance.itemLookup["RescueHook_Infinite"].Valid=true;
        Check(api.SpawnItem(infinite, true), "Host submits reviewed special request with explicit opt-in");
        Check(Character.localCharacter.refs.items.LastSpawn == "RescueHook_Infinite", "special request preserves variant prefab identity");
        Check(Character.localCharacter.refs.items.Requests == 4, "rejected special requests never reach game spawn");
        Check(api.ReadItemCatalog().Count == 6, "closing opt-in leaves only valid multiplayer items in normal catalog");
        checks += ItemValidityDiagnosticTests.Run();
        checks += MultiplayerVisibilityTests.Run();
        checks += SoloManualSpawnTests.Run();
        Console.WriteLine("PASS: " + checks + " adapter checks (metadata-shaped doubles; no live PEAK/Photon test).");
    }
}

namespace UnityEngine { public sealed class Texture2D {} }
namespace PeakAdminToolkit.Core
{
    internal sealed class Compatibility
    {
        public bool WindowHooksReady { get { return true; } }
        public string ReadGameLanguage() { return "TraditionalChinese"; }
        public bool GameWantsCursor() { return false; }
    }
}
public class Item
{
    public string name=string.Empty;
    public Item isSecretlyOtherItemPrefab=null;
    public ItemUIData UIData { get; set; }
    public Item() { UIData = new ItemUIData(); }
    public ItemTags itemTags=ItemTags.None;
    public bool Valid=true;
    public bool ThrowOnValidity=false;
    public int ValidityCalls=0;
    public bool IsValidToSpawn()
    {
        ValidityCalls++;
        if (ThrowOnValidity) throw new InvalidOperationException();
        return Valid;
    }
    public string GetName() { return LocalizedText.GetText(LocalizedText.GetNameIndex(UIData.itemName), LocalizedText.CURRENT_LANGUAGE); }
}
public sealed class IncompatibleItem : Item
{
    public new string UIData { get; set; }
}
[Flags] public enum ItemTags { None=0, Mystical=1, PackagedFood=2, Berry=4, Mushroom=8 }
public sealed class ItemUIData
{
    public string itemName=string.Empty;
    public Texture2D icon=null;
    public bool ThrowOnIcon=false;
    public Texture2D GetIcon() { if (ThrowOnIcon) throw new InvalidOperationException(); return icon; }
}
public sealed class ItemDatabase
{
    public static ItemDatabase Instance=new ItemDatabase();
    public Dictionary<string, Item> itemLookup=new Dictionary<string, Item>();
    public static bool TryGetItem(string name, out Item item) { return Instance.itemLookup.TryGetValue(name, out item); }
}
public enum Language { English, SimplifiedChinese, TraditionalChinese }
public static class LocalizedText
{
    public static bool MissingTraditionalDescription;
    public static bool ThrowOnDescription;
    public static Language CURRENT_LANGUAGE=Language.TraditionalChinese;
    public static string GetNameIndex(string name) { return "NAME_"+name; }
    public static string GetDescriptionIndex(string name) { return "DESC_"+name; }
    public static string GetText(string key, Language language)
    {
        if (key == "DESC_Medicinal Root")
        {
            if (ThrowOnDescription) throw new InvalidOperationException();
            if (MissingTraditionalDescription && language == Language.TraditionalChinese) return string.Empty;
            return language == Language.English ? "A root for treating injuries." : language == Language.SimplifiedChinese ? "治疗伤势的根草。" : "治療傷勢的根草。";
        }
        if (key != "NAME_Medicinal Root") return key;
        return language == Language.English ? "Medicinal Root" : language == Language.SimplifiedChinese ? "药根草" : "藥根草";
    }
}
public sealed class Character
{
    public static Character localCharacter=new Character();
    public CharacterReferences refs=new CharacterReferences();
}
public sealed class CharacterReferences { public CharacterItems items=new CharacterItems(); }
public sealed class CharacterItems
{
    public string LastSpawn=null;
    public int Requests=0;
    internal void SpawnItemInHand(string name) { LastSpawn=name; Requests++; }
}
namespace Photon.Pun
{
    public sealed class Room { public int PlayerCount=1; }
    public static class PhotonNetwork
    {
        public static bool OfflineMode { get; set; }
        public static Room CurrentRoom { get; set; }
        public static bool InRoom { get; set; }
        public static bool IsMasterClient { get; set; }
    }
}

public sealed class Guidebook : Item {}
public sealed class Backpack : Item {}
