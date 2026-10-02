using System;
using System.Collections.Generic;
using PeakAdminToolkit.Core;

namespace PeakAdminToolkit.Items
{
    internal enum SpecialItemKind { None, SpecialVariants, UnusedItems, MultiplayerItems, LobbyToys, MiscellaneousItems }

    internal static class SpecialItems
    {
        private static readonly HashSet<string> Chess = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "C_King B", "C_King W", "C_Queen B", "C_Queen W", "C_Rook B", "C_Rook W",
            "C_Bishop B", "C_Bishop W", "C_Knight B", "C_Knight W", "C_Pawn B", "C_Pawn W"
        };
        private static readonly HashSet<string> LobbyProps = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Binoculars_Prop", "Lollipop_Prop", "Bugle_Prop Variant", "BingBong_Prop Variant", "Passport"
        };
        private static readonly HashSet<string> Multiplayer = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Fannypack", "Cursed Skull", "Bugle_Magic", "ScoutEffigy", "HealingDart Variant", "RitualDagger"
        };

        private static readonly HashSet<string> Miscellaneous = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "GuidebookPage",
            "GuidebookPageScroll Variant",
            "GuidebookPage_0_Intro",
            "GuidebookPage_10_Sleepy",
            "GuidebookPage_11_Awake",
            "GuidebookPage_12_Crashout",
            "GuidebookPage_13_FirstTeams",
            "GuidebookPage_1_Mushrooms",
            "GuidebookPage_2_Campfire",
            "GuidebookPage_3_Revival",
            "GuidebookPage_4_Poison",
            "GuidebookPage_5_Zombies",
            "GuidebookPage_6_BodyHeat",
            "GuidebookPage_7_BurningSun",
            "GuidebookPage_8_Magma",
            "GuidebookPage_9_Gloom"
        };

        public static SpecialItemKind Classify(string spawnName)
        {
            string name = Normalize(spawnName);
            if (EqualsName(name, "RescueHook_Infinite") || EqualsName(name, "ScoutCookies_Vanilla")) return SpecialItemKind.SpecialVariants;
            if (EqualsName(name, "Parachute")) return SpecialItemKind.UnusedItems;
            if (LobbyProps.Contains(name) || Chess.Contains(name) || EqualsName(name, "Basketball") || EqualsName(name, "Warpsketball")) return SpecialItemKind.LobbyToys;
            if (Miscellaneous.Contains(name)) return SpecialItemKind.MiscellaneousItems;
            if (Multiplayer.Contains(name)) return SpecialItemKind.MultiplayerItems;
            return SpecialItemKind.None;
        }

        public static SpecialItemKind[] AvailableKinds(IEnumerable<ItemCatalogEntry> catalog)
        {
            var available = new HashSet<SpecialItemKind>();
            foreach (ItemCatalogEntry item in catalog) available.Add(Classify(item.SpawnName));
            var result = new List<SpecialItemKind> { SpecialItemKind.None };
            foreach (SpecialItemKind kind in new[] { SpecialItemKind.SpecialVariants, SpecialItemKind.UnusedItems, SpecialItemKind.MultiplayerItems, SpecialItemKind.LobbyToys, SpecialItemKind.MiscellaneousItems })
                if (available.Contains(kind)) result.Add(kind);
            return result.ToArray();
        }

        public static string DisplayName(string spawnName, Localization loc)
        {
            string name = Normalize(spawnName);
            if (EqualsName(name, "RescueHook_Infinite")) return loc.Text("InfiniteHookName");
            if (EqualsName(name, "ScoutCookies_Vanilla")) return loc.Text("VanillaCookiesName");
            if (EqualsName(name, "Parachute")) return loc.Text("ParachuteName");
            if (EqualsName(name, "Basketball")) return loc.Text("BasketballName");
            if (EqualsName(name, "Warpsketball")) return loc.Text("WarpsketballName");
            if (EqualsName(name, "GuidebookPageScroll Variant")) return loc.Text("ScrollName");
            if (EqualsName(name, "Binoculars_Prop")) return loc.Text("LobbyBinocularsName");
            if (EqualsName(name, "Lollipop_Prop")) return loc.Text("LobbyLollipopName");
            if (EqualsName(name, "Bugle_Prop Variant")) return loc.Text("LobbyBugleName");
            if (EqualsName(name, "BingBong_Prop Variant")) return loc.Text("LobbyBingBongName");
            if (EqualsName(name, "Passport")) return loc.Text("LobbyPassportName");
            if (EqualsName(name, "Cursed Skull")) return loc.Text("CursedSkullName");
            if (EqualsName(name, "Bugle_Magic")) return loc.Text("FriendshipBugleName");
            if (EqualsName(name, "ScoutEffigy")) return loc.Text("ScoutEffigyName");
            if (EqualsName(name, "HealingDart Variant")) return loc.Text("BlowgunName");
            if (EqualsName(name, "RitualDagger")) return loc.Text("RitualDaggerName");
            if (EqualsName(name, "Fannypack")) return loc.Text("FannypackName");
            if (!Chess.Contains(name)) return string.Empty;
            string piece = name.Substring(2).Split(' ', '_')[0].ToLowerInvariant();
            string suffix = name.EndsWith(" B", StringComparison.OrdinalIgnoreCase) ? "ChessBlack" : "ChessWhite";
            return loc.Format(suffix, loc.Text("Chess_" + piece));
        }

        public static string Description(string spawnName, Localization loc)
        {
            string name = Normalize(spawnName);
            if (EqualsName(name, "RescueHook_Infinite")) return loc.Text("InfiniteHookDescription");
            if (EqualsName(name, "ScoutCookies_Vanilla")) return loc.Text("VanillaCookiesDescription");
            if (EqualsName(name, "Parachute")) return loc.Text("ParachuteDescription");
            if (EqualsName(name, "Basketball")) return loc.Text("BasketballDescription");
            if (EqualsName(name, "Warpsketball")) return loc.Text("WarpsketballDescription");
            return Chess.Contains(name) ? loc.Text("ChessDescription") : string.Empty;
        }

        private static bool EqualsName(string name, string expected) { return string.Equals(name, expected, StringComparison.OrdinalIgnoreCase); }
        private static string Normalize(string name) { return (name ?? string.Empty).Replace("(Clone)", string.Empty).Trim(); }
    }
}
