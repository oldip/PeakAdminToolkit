using System;
using PeakAdminToolkit.Core;
using PeakAdminToolkit.Items;
using PeakAdminToolkit.UI;

internal static class MultiplayerVisibilityTests
{
    private static int checks;
    public static int Run()
    {
        var savedDatabase = ItemDatabase.Instance;
        bool savedHost = Photon.Pun.PhotonNetwork.IsMasterClient;
        bool savedRoom = Photon.Pun.PhotonNetwork.InRoom;
        var savedItems = Character.localCharacter.refs.items;
        try
        {
            ItemDatabase.Instance = new ItemDatabase();
            Character.localCharacter.refs.items = new CharacterItems();
            Photon.Pun.PhotonNetwork.InRoom = true;
            Photon.Pun.PhotonNetwork.IsMasterClient = true;
            string[] names = { "Bugle_Magic", "Cursed Skull", "ScoutEffigy", "HealingDart Variant", "RitualDagger", "Fannypack" };
            string[] uiNames = { "Bugle of Friendship", "Cursed Skull", "Scout Effigy", "Blowgun", "RitualDagger", "Fannypack" };
            for (int i = 0; i < names.Length; i++)
                ItemDatabase.Instance.itemLookup[names[i]] = new Item { name=names[i], Valid=false, UIData=new ItemUIData { itemName=uiNames[i], icon=new UnityEngine.Texture2D() } };
            var api = new PeakApi(new Compatibility(), new ItemDescriptions());
            Check(api.ReadItemCatalog().Count == 0, "unavailable multiplayer items remain hidden without opt-in");
            var unavailable = api.ReadItemCatalog(true);
            Check(unavailable.Count == 6, "opt-in shows all six measured-false multiplayer candidates");
            var filter = new SpecialItemFilter();
            var loc = new Localization();
            foreach (var entry in unavailable)
            {
                Check(!Spawnable(entry) && entry.Icon != null, "unavailable item retains icon but disables its card: " + entry.SpawnName);
                filter.SetCategory(ItemCategory.None);
                Check(!filter.Matches(entry), "unavailable item stays out of All: " + entry.SpawnName);
                filter.SetCategory(ItemCategory.Other);
                Check(!filter.Matches(entry), "unavailable item requires manual opt-in: " + entry.SpawnName);
                filter.SetEnabled(true);
                filter.SelectKind(SpecialItemKind.MultiplayerItems);
                Check(filter.Matches(entry), "unavailable item is in the multiplayer subfilter: " + entry.SpawnName);
                Check(!api.SpawnItem(entry, true), "viewing unavailable item cannot generate it: " + entry.SpawnName);
                Check(api.MatchesItem(entry.EnglishName, entry), "unavailable item remains searchable: " + entry.SpawnName);
                foreach (string language in new[] { "en", "zh-CN", "zh-TW" })
                {
                    loc.Update(language, null);
                    string status = language == "en" ? "Unavailable" : "目前不可生成";
                    string tip = ItemTooltip.Format(entry, loc);
                    Check(tip.Contains(status) && tip.Contains(loc.Text("MultiplayerItems")) && tip.Contains(entry.DisplayName(language)) && tip.Contains(entry.DisplayDescription(language)),
                        "unavailable tooltip retains translated name, category and description: " + language + " " + entry.SpawnName);
                }
            }
            Check(Character.localCharacter.refs.items.Requests == 0, "inspection never submits a generation request");
            foreach (string name in names) ItemDatabase.Instance.itemLookup[name].Valid = true;
            var available = api.ReadItemCatalog();
            Check(available.Count == 6, "refresh moves all newly valid multiplayer items into the normal catalog");
            filter.SetCategory(ItemCategory.None);
            foreach (var entry in available)
            {
                Check(Spawnable(entry) && filter.Matches(entry), "valid multiplayer item appears in All with an enabled card: " + entry.SpawnName);
                Check(api.SpawnItem(entry), "host can generate valid multiplayer item without opt-in: " + entry.SpawnName);
                loc.Update("zh-TW", null);
                Check(!ItemTooltip.Format(entry, loc).Contains("目前不可生成"), "refreshed valid tooltip drops unavailable status: " + entry.SpawnName);
            }
            Check(Character.localCharacter.refs.items.Requests == 6, "each permitted request reaches the existing spawn API once");
            Check(!api.SpawnItem(unavailable[0], true), "old disabled entry requires refresh even if game now allows it");
            var effigy = available.Find(e => e.SpawnName == "ScoutEffigy");
            filter.SetCategory(ItemCategory.Consumable);
            Check(filter.Matches(effigy) && ItemClassification.Matches(effigy.Categories, effigy.Kinds, ItemCategory.Consumable, ConsumableKind.Revive),
                "valid effigy joins the normal consumable revival filter");
            var pack = available.Find(e => e.SpawnName == "Fannypack");
            filter.SetCategory(ItemCategory.Equipment);
            Check(filter.Matches(pack) && (pack.Categories & ItemCategory.Equipment) != 0, "valid fannypack joins normal equipment");
            Photon.Pun.PhotonNetwork.IsMasterClient = false;
            Check(api.SpawnItem(available[0]), "Client can submit the visible multiplayer item through native Master RPC");
            Photon.Pun.PhotonNetwork.IsMasterClient = true;
            foreach (string name in names) ItemDatabase.Instance.itemLookup[name].Valid = false;
            Check(!api.SpawnItem(available[0]), "stale enabled entry is rechecked before spawning");
            Check(api.ReadItemCatalog().Count == 0, "refresh hides newly invalid multiplayer entries from normal browsing");
            Check(!Spawnable(api.ReadItemCatalog(true)[0]), "refresh restores unavailable state after validity changes");
            Check(Character.localCharacter.refs.items.Requests == 7, "all rejected requests leave generation untouched");
            return checks;
        }
        finally
        {
            ItemDatabase.Instance = savedDatabase;
            Photon.Pun.PhotonNetwork.IsMasterClient = savedHost;
            Photon.Pun.PhotonNetwork.InRoom = savedRoom;
            Character.localCharacter.refs.items = savedItems;
        }
    }

    private static bool Spawnable(ItemCatalogEntry entry) { return entry.ValidToSpawn; }
    private static void Check(bool value, string name)
    {
        checks++;
        if (!value) throw new InvalidOperationException(name);
    }
}
