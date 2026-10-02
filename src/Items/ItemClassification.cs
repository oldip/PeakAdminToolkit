using System;
using System.Collections.Generic;

namespace PeakAdminToolkit.Items
{
    [Flags]
    internal enum ItemCategory { None = 0, Mystical = 1, Consumable = 2, Deployable = 4, Equipment = 8, Other = 16 }
    [Flags]
    internal enum ConsumableKind { None = 0, Food = 1, Healing = 2, Recovery = 4, Buff = 8, Revive = 16 }

    internal static class ItemClassification
    {
        private static readonly Dictionary<string, ItemCategory> Known = BuildCategories();

        public static ItemCategory Categories(string uiName, string nativeTags)
        {
            ItemCategory result;
            bool known = Known.TryGetValue((uiName ?? string.Empty).Trim(), out result);
            if (HasTag(nativeTags, "Mystical") || HasTag(nativeTags, "ScoutAmulet") ||
                HasTag(nativeTags, "GoldenIdol") || HasTag(nativeTags, "BookOfBones")) result |= ItemCategory.Mystical;
            if (!known && IsFoodTag(nativeTags)) result |= ItemCategory.Consumable;
            return result == ItemCategory.None ? ItemCategory.Other : result;
        }

        public static ConsumableKind Kinds(string uiName, string nativeTags)
        {
            if ((Categories(uiName, nativeTags) & ItemCategory.Consumable) == 0) return ConsumableKind.None;
            ConsumableKind kinds = ConsumableKind.None;
            switch ((uiName ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "medicinal root": case "medicinalroot":
                    kinds = ConsumableKind.Food | ConsumableKind.Healing | ConsumableKind.Recovery; break;
                case "napberry":
                    kinds = ConsumableKind.Food | ConsumableKind.Healing | ConsumableKind.Recovery; break;
                case "froglegs":
                    kinds = ConsumableKind.Food; break;
                case "mandrake": kinds = ConsumableKind.Food | ConsumableKind.Recovery; break;
                case "bandages": kinds = ConsumableKind.Healing; break;
                case "first aid kit": case "firstaidkit": case "cure-all": case "remedy fungus":
                    kinds = ConsumableKind.Healing | ConsumableKind.Recovery; break;
                case "antidote": case "heat pack": kinds = ConsumableKind.Recovery; break;
                case "sunscreen": kinds = ConsumableKind.Buff; break;
                case "energy drink": case "sports drink": kinds = ConsumableKind.Buff; break;
                case "fortified milk": case "fortifiedmilk": kinds = ConsumableKind.Healing | ConsumableKind.Buff; break;
                case "scout effigy": case "scouteffigy": kinds = ConsumableKind.Revive; break;
            }
            if (IsFoodTag(nativeTags)) kinds |= ConsumableKind.Food;
            return kinds;
        }

        public static bool Matches(ItemCategory categories, ConsumableKind kinds, ItemCategory category, ConsumableKind kind)
        {
            return (category == ItemCategory.None || (categories & category) != 0)
                && (kind == ConsumableKind.None || (kinds & kind) != 0);
        }

        public static ConsumableKind[] AvailableKinds(IEnumerable<ItemCatalogEntry> catalog)
        {
            ConsumableKind available = ConsumableKind.None;
            foreach (ItemCatalogEntry item in catalog)
                if ((item.Categories & ItemCategory.Consumable) != 0) available |= item.Kinds;
            var result = new List<ConsumableKind> { ConsumableKind.None };
            foreach (ConsumableKind kind in new[] { ConsumableKind.Food, ConsumableKind.Healing, ConsumableKind.Recovery, ConsumableKind.Buff, ConsumableKind.Revive })
                if ((available & kind) != 0) result.Add(kind);
            return result.ToArray();
        }

        private static bool IsFoodTag(string tags)
        {
            return HasTag(tags, "PackagedFood") || HasTag(tags, "Berry") || HasTag(tags, "Mushroom")
                || HasTag(tags, "GourmandRequirement") || HasTag(tags, "Bird");
        }

        private static bool HasTag(string tags, string expected)
        {
            foreach (string tag in (tags ?? string.Empty).Split(','))
                if (string.Equals(tag.Trim(), expected, StringComparison.Ordinal)) return true;
            return false;
        }

        private static Dictionary<string, ItemCategory> BuildCategories()
        {
            // Stable ItemUIData.itemName labels/categories from the 1.8.2 behavior specification.
            // These are item labels, not localization or character-transliteration data.
            var result = new Dictionary<string, ItemCategory>(StringComparer.OrdinalIgnoreCase);
            Add(result, ItemCategory.Mystical, "AMULET_CLONE|AMULET_DOUBLEJUMP|AMULET_HEALING|AMULET_INFINITESTAM|Ancient Idol|anti-rope cannon|anti-rope spool|Bugle of Friendship|Cursed Skull|Faerie Lantern|RitualDagger|Scout Effigy|ScoutEffigy|Scoutmaster's Bugle|SCOUTMASTERSOUL|ScoutsHonor|Strange Gem|THEBOOKOFBONES|VOIDLAUNCHER|Warp Compass|Warp Fungus");
            Add(result, ItemCategory.Consumable, "Airline Food|Aloe Vera|Antidote|Balloon|Balloon Bunch|Bandages|Big Egg|Big Lollipop|Bird|Black Clusterberry|Blue Berrynana|Blue Shroomberry|Brown Berrynana|Bugle Shroom|Button Shroom|Cactus|Chubby Shroom|Cluster Shroom|Coconut|Coconut Half|Cure-All|EARLYWORM|Egg|Energy Drink|First Aid Kit|FirstAidKit|Fortified Milk|FortifiedMilk|FrogLegs|Gold Prickleberry|Granola Bar|Green Clusterberry|Green Crispberry|Green Kingberry|Green Shroomberry|Heat Pack|Honeycomb|Hot Dog|Mandrake|Marshmallow|Medicinal Root|MedicinalRoot|Napberry|Orange Winterberry|Pandora's Lunchbox|Pink Berrynana|Purple Kingberry|Purple Shroomberry|Red Clusterberry|Red Crispberry|Red Prickleberry|Red Shroomberry|Remedy Fungus|Scorchberry|Scout Cookies|Scout Effigy|ScoutEffigy|Small Egg|Sports Drink|Sunscreen|Tick|Trail Mix|Yellow Berrynana|Yellow Clusterberry|Yellow Crispberry|Yellow Kingberry|Yellow Shroomberry|Yellow Winterberry");
            Add(result, ItemCategory.Deployable, "Magic Bean");
            Add(result, ItemCategory.Deployable, "anti-rope cannon|anti-rope spool|Bounce Fungus|Chain Launcher|Checkpoint Flag|Cloud Fungus|Piton|Portable Stove|rope cannon|rope spool|Scout Cannon|Shelf Fungus|VOIDLAUNCHER");
            Add(result, ItemCategory.Equipment, "Backpack|Binoculars|Bugle|Candlestick|Compass|Fannypack|Flare|Glider|Guidebook|Jetpack|Lantern|Megaphone|Parasol|Pirate's Compass|Rescue Claw|Rocketpack|Torch");
            Add(result, ItemCategory.Other, "Beehive|Berrynana Peel|Bing Bong|Blowgun|Conch|Dynamite|Frisbee|Frog|Scorpion|Scroll|SNOWBALL|Stick|Stone|Torn Page");
            return result;
        }

        private static void Add(Dictionary<string, ItemCategory> map, ItemCategory category, string names)
        {
            foreach (string name in names.Split('|'))
            {
                ItemCategory current;
                map.TryGetValue(name, out current);
                map[name] = current | category;
            }
        }
    }
}
