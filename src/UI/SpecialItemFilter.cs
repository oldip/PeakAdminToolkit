using PeakAdminToolkit.Items;

namespace PeakAdminToolkit.UI
{
    internal sealed class SpecialItemFilter
    {
        private ItemCategory category;
        public bool Enabled { get; private set; }
        public SpecialItemKind SelectedKind { get; private set; }

        public void SetCategory(ItemCategory value)
        {
            if (category != value) Reset();
            category = value;
        }

        public void SetEnabled(bool value)
        {
            Enabled = value && category == ItemCategory.Other;
            SelectedKind = SpecialItemKind.None;
        }

        public void SelectKind(SpecialItemKind kind) { SelectedKind = Enabled ? kind : SpecialItemKind.None; }
        public void Reset() { Enabled = false; SelectedKind = SpecialItemKind.None; }

        public bool Matches(ItemCatalogEntry item)
        {
            SpecialItemKind kind = SpecialItems.Classify(item.SpawnName);
            bool needsOptIn = kind != SpecialItemKind.None && (kind != SpecialItemKind.MultiplayerItems || !item.CanSpawnManually);
            if (needsOptIn && (!Enabled || category != ItemCategory.Other)) return false;
            return SelectedKind == SpecialItemKind.None || SelectedKind == kind;
        }
    }
}
