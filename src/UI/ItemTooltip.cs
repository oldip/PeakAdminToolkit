using System;
using System.Collections.Generic;
using PeakAdminToolkit.Core;
using PeakAdminToolkit.Items;

namespace PeakAdminToolkit.UI
{
    internal static class ItemTooltip
    {
        public static string Format(ItemCatalogEntry item, Localization localization)
        {
            var categories = new List<string>();
            foreach (ItemCategory category in Enum.GetValues(typeof(ItemCategory)))
                if (category != ItemCategory.None && (item.Categories & category) != 0)
                    categories.Add(localization.Text(category.ToString()));
            foreach (ConsumableKind kind in Enum.GetValues(typeof(ConsumableKind)))
                if (kind != ConsumableKind.None && (item.Kinds & kind) != 0)
                    categories.Add(localization.Text(kind.ToString()));
            SpecialItemKind special = SpecialItems.Classify(item.SpawnName);
            if (special != SpecialItemKind.None) categories.Add(localization.Text(special.ToString()));
            string text = item.DisplayName(localization.Language) + "\n"
                + localization.Format("ItemCategory", string.Join(" · ", categories.ToArray()));
            if (!item.CanSpawnManually)
                text += "\n" + localization.Text("ItemUnavailable") + "\n" + localization.Text("ItemUnavailableReason");
            else if (!item.ValidToSpawn)
                text += "\n" + localization.Text("SoloManualSpawn") + "\n" + localization.Text("SoloManualSpawnReason");
            string description = item.DisplayDescription(localization.Language);
            return string.IsNullOrEmpty(description) ? text : text + "\n\n" + description;
        }
    }
}
