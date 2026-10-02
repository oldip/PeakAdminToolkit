using System;
using System.Collections.Generic;

namespace PeakAdminToolkit.Items
{
    internal static class ItemVisibility
    {
        private static readonly HashSet<string> HiddenPrefabs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ClimbingChalk", "Climbing Chalk", "Clusterberry_UNUSED", "FireWood",
            "GuidebookPage_13_FirstTeams", "Megaphone", "Mushroom Glow", "Weird Shroom",
            "Stone", "Skull", "Warpsketball", "Basketball", "Cheat Compass", "Cheat Compass 1",
            "BingBong_Prop Variant", "Binoculars_Prop", "Bugle_Prop Variant", "GuidebookPage",
            "Lollipop_Prop", "Mandrake_Hidden", "Parasol_Roots Variant", "Parachute",
            "AUTOPARACHUTE", "ScoutCookies_Vanilla", "Warp Compass", "RescueHook_Infinite",
            "ScoutmasterSoul"
        };

        public static bool ShouldShow(string spawnName, bool registered, bool canSpawn, bool duplicatePrefab, bool includeSpecial = false)
        {
            if (string.IsNullOrWhiteSpace(spawnName) || !registered || duplicatePrefab)
                return false;
            SpecialItemKind kind = SpecialItems.Classify(spawnName);
            if (kind == SpecialItemKind.MultiplayerItems) return canSpawn || includeSpecial;
            if (!canSpawn) return false;
            if (kind != SpecialItemKind.None) return includeSpecial;
            string name = NormalizePrefabName(spawnName);
            return !HiddenPrefabs.Contains(name)
                && !name.StartsWith("C_", StringComparison.OrdinalIgnoreCase)
                && !name.StartsWith("GuidebookPage", StringComparison.OrdinalIgnoreCase)
                && !name.StartsWith("Basketball", StringComparison.OrdinalIgnoreCase)
                && name.IndexOf("_Prop", StringComparison.OrdinalIgnoreCase) < 0
                && !name.StartsWith("Cheat ", StringComparison.OrdinalIgnoreCase)
                && !name.EndsWith("_UNUSED", StringComparison.OrdinalIgnoreCase)
                && !name.EndsWith("_TEMP", StringComparison.OrdinalIgnoreCase)
                && !name.EndsWith("_Hidden", StringComparison.OrdinalIgnoreCase);
        }

        public static bool CanRequestSpawn(bool inRoom)
        {
            return inRoom;
        }

        private static string NormalizePrefabName(string value)
        {
            return (value ?? string.Empty).Replace("(Clone)", string.Empty).Trim();
        }
    }
}

