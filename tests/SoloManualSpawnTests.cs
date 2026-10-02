using System;
using PeakAdminToolkit.Core;
using PeakAdminToolkit.Items;
using PeakAdminToolkit.UI;

internal static class SoloManualSpawnTests
{
    private static int checks;
    public static int Run()
    {
        var savedDatabase = ItemDatabase.Instance;
        var savedItems = Character.localCharacter.refs.items;
        bool savedHost = Photon.Pun.PhotonNetwork.IsMasterClient;
        bool savedInRoom = Photon.Pun.PhotonNetwork.InRoom;
        try
        {
            ItemDatabase.Instance = new ItemDatabase();
            Character.localCharacter.refs.items = new CharacterItems();
            Photon.Pun.PhotonNetwork.InRoom = true;
            Photon.Pun.PhotonNetwork.IsMasterClient = true;
            Photon.Pun.PhotonNetwork.CurrentRoom = new Photon.Pun.Room();
            var api = new PeakApi(new Compatibility());
            string[] names = { "Bugle_Magic", "Cursed Skull", "ScoutEffigy", "HealingDart Variant", "RitualDagger", "Fannypack" };
            string[] uiNames = { "Bugle of Friendship", "Cursed Skull", "Scout Effigy", "Blowgun", "RitualDagger", "Fannypack" };
            ushort[] ids = { 16, 25, 67, 70, 173, 166 };
            for (int i = 0; i < names.Length; i++)
            {
                var item = new SoloItem { name=names[i], itemID=ids[i], Valid=false, UIData=new ItemUIData { itemName=uiNames[i] } };
                ItemDatabase.Instance.itemLookup[names[i]] = item;
            }
            Check(api.ReadItemCatalog().Count == 6, "solo manual candidates appear in the normal catalog without opt-in");
            var catalog = api.ReadItemCatalog();
            Check(catalog.Count == 6, "all six solo candidates remain inspectable");
            foreach (var entry in catalog)
            {
                Check(Manual(entry), "solo manual permission enables the card: " + entry.SpawnName);
                Check(api.SpawnItem(entry), "host can manually generate the solo-restricted candidate: " + entry.SpawnName);
                var filter = new SpecialItemFilter();
                filter.SetCategory(ItemCategory.None);
                Check(filter.Matches(entry), "All shows solo-manual entries without opt-in: " + entry.SpawnName);
                filter.SetCategory(ItemCategory.Other);
                Check(filter.Matches(entry) && !filter.Enabled, "normal Other also shows eligible multiplayer item without opt-in");
                Check(!entry.ValidToSpawn, "manual eligibility does not rewrite natural validity: " + entry.SpawnName);
                var loc = new Localization();
                foreach (string language in new[] { "en", "zh-CN", "zh-TW" })
                {
                    loc.Update(language, null);
                    string label = language == "en" ? "Does not spawn naturally in solo" : language == "zh-CN" ? "单人游戏不会自然生成" : "單人遊戲不會自然生成";
                    string tip = ItemTooltip.Format(entry, loc);
                    Check(tip.Contains(label) && !tip.Contains(language == "en" ? "Unavailable" : "目前不可生成"),
                        "solo manual status uses selected language: " + language);
                }
                var source = (SoloItem)ItemDatabase.Instance.itemLookup[entry.SpawnName];
                Check(source.Loot.banInSolo && !source.Valid, "manual request leaves original game fields unchanged");
            }
            Check(Character.localCharacter.refs.items.Requests == 6, "only six explicitly authorized requests reach the game adapter");
            var categoryFilter = new SpecialItemFilter();
            var effigy = catalog.Find(e => e.SpawnName == "ScoutEffigy");
            categoryFilter.SetCategory(ItemCategory.Consumable);
            Check(categoryFilter.Matches(effigy) && ItemClassification.Matches(effigy.Categories, effigy.Kinds, ItemCategory.Consumable, ConsumableKind.Revive),
                "solo manual effigy joins normal Revival category");
            var pack = catalog.Find(e => e.SpawnName == "Fannypack");
            categoryFilter.SetCategory(ItemCategory.Equipment);
            Check(categoryFilter.Matches(pack) && (pack.Categories & ItemCategory.Equipment) != 0, "solo manual fannypack joins normal Equipment category");
            var target = catalog.Find(e => e.SpawnName == "ScoutEffigy");
            var targetItem = (SoloItem)ItemDatabase.Instance.itemLookup[target.SpawnName];
            RunSettings.NameEnabled = false;
            Check(!api.SpawnItem(target), "custom-run name ban blocks stale solo entry");
            Check(!Manual(api.ReadItemCatalog(true).Find(e => e.SpawnName == target.SpawnName)), "custom-run ban disables refreshed solo card");
            Check(api.ReadItemCatalog().Count == 0, "room-banned multiplayer entries stay out of the normal catalog");
            categoryFilter.SetCategory(ItemCategory.None);
            Check(!categoryFilter.Matches(api.ReadItemCatalog(true)[0]), "disabled inspected entries cannot leak into All");
            RunSettings.NameEnabled = true;
            RunSettings.IdEnabled = false;
            Check(!api.SpawnItem(target), "custom-run item ID ban is retained");
            RunSettings.IdEnabled = true;
            RunSettings.Throw = true;
            Check(!api.SpawnItem(target), "failing RunSettings API blocks manual generation");
            RunSettings.Throw = false;
            targetItem.Loot.excludeBasedOnCustomRunSetting = true;
            Check(!api.SpawnItem(target), "changed additional loot rule blocks the narrow exception");
            targetItem.Loot.excludeBasedOnCustomRunSetting = false;
            targetItem.Loot.useOtherItemForSpawningValidity = new object();
            Check(!api.SpawnItem(target), "alternate validity prefab does not inherit the solo exception");
            targetItem.Loot.useOtherItemForSpawningValidity = null;
            targetItem.Loot.banInSolo = false;
            Check(!api.SpawnItem(target), "false validity without solo flag cannot be overridden");
            targetItem.Loot.banInSolo = true;
            var savedLoot = targetItem.Loot;
            targetItem.Loot = null;
            Check(!api.SpawnItem(target), "missing LootData leaves normal features intact but blocks manual exception");
            targetItem.Loot = savedLoot;
            Photon.Pun.PhotonNetwork.CurrentRoom.PlayerCount = 2;
            Check(!api.SpawnItem(target), "false validity in multiplayer is not treated as solo restriction");
            Photon.Pun.PhotonNetwork.OfflineMode = true;
            Check(api.SpawnItem(target), "offline solo mode follows the same inspected banInSolo rule");
            Photon.Pun.PhotonNetwork.OfflineMode = false;
            Photon.Pun.PhotonNetwork.CurrentRoom = null;
            Check(!api.SpawnItem(target), "missing room state cannot assume solo");
            Photon.Pun.PhotonNetwork.CurrentRoom = new Photon.Pun.Room();
            Photon.Pun.PhotonNetwork.IsMasterClient = false;
            Check(api.SpawnItem(target), "Client can submit reviewed solo item through native Master RPC");
            Photon.Pun.PhotonNetwork.IsMasterClient = true;
            Photon.Pun.PhotonNetwork.InRoom = false;
            Check(!api.SpawnItem(target), "solo exception still requires a room");
            Photon.Pun.PhotonNetwork.InRoom = true;
            targetItem.ThrowOnValidity = true;
            Check(!api.SpawnItem(target), "validity API failure never becomes a solo exception");
            targetItem.ThrowOnValidity = false;
            targetItem.isSecretlyOtherItemPrefab = new Item();
            Check(!api.SpawnItem(target), "replacement exclusion is retained for manual requests");
            targetItem.isSecretlyOtherItemPrefab = null;
            ItemDatabase.Instance.itemLookup.Remove(target.SpawnName);
            Check(!api.SpawnItem(target), "registration is rechecked for stale manual candidates");
            var other = new SoloItem { name="Stick", Valid=false };
            ItemDatabase.Instance.itemLookup[other.name] = other;
            Check(!api.SpawnItem(new ItemCatalogEntry("Stick", "Stick", "Stick", "Stick", "Stick", null, ItemCategory.Other, ConsumableKind.None), true),
                "unlisted items cannot use the solo exception");
            Check(Character.localCharacter.refs.items.Requests == 8, "all denied manual requests avoid generation");
            return checks;
        }
        finally
        {
            ItemDatabase.Instance = savedDatabase;
            Character.localCharacter.refs.items = savedItems;
            Photon.Pun.PhotonNetwork.IsMasterClient = savedHost;
            Photon.Pun.PhotonNetwork.InRoom = savedInRoom;
            Photon.Pun.PhotonNetwork.OfflineMode = false;
            Photon.Pun.PhotonNetwork.CurrentRoom = null;
            RunSettings.NameEnabled = RunSettings.IdEnabled = true;
            RunSettings.Throw = false;
        }
    }
    private static bool Manual(ItemCatalogEntry entry) { return entry.CanSpawnManually; }
    private static void Check(bool value, string message) { checks++; if (!value) throw new InvalidOperationException(message); }
}

public sealed class SoloItem : Item
{
    public ushort itemID;
    public LootData Loot = new LootData();
    public object GetComponent(Type type) { return type == typeof(LootData) ? Loot : null; }
}
public sealed class LootData
{
    public bool banInSolo = true;
    public bool excludeBasedOnCustomRunSetting = false;
    public object useOtherItemForSpawningValidity = null;
}
public static class RunSettings
{
    public static bool NameEnabled = true, IdEnabled = true, Throw = false;
    public static bool IsItemEnabled(string name) { if (Throw) throw new InvalidOperationException(); return NameEnabled; }
    public static bool IsItemEnabled(ushort id) { if (Throw) throw new InvalidOperationException(); return IdEnabled; }
}
