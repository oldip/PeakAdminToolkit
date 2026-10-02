using System;
using System.Collections.Generic;
using PeakAdminToolkit.Items;

namespace PeakAdminToolkit.Core
{
    // Coordinates independently guarded feature adapters for the existing UI.
    internal sealed class PeakApi
    {
        private readonly Compatibility compatibility;
        private readonly ItemCatalogApi catalog;
        private readonly ItemSpawnApi spawning = new ItemSpawnApi();

        public PeakApi(Compatibility compatibility, ItemDescriptions descriptions = null, Action<string> diagnosticLog = null)
        {
            this.compatibility = compatibility;
            catalog = new ItemCatalogApi(descriptions, diagnosticLog);
        }

        public bool CanOpenWindow { get { return compatibility.WindowHooksReady; } }
        public string ReadLanguage() { return compatibility.ReadGameLanguage(); }
        public bool GameWantsCursor() { return compatibility.GameWantsCursor(); }
        public List<ItemCatalogEntry> ReadItemCatalog(bool includeSpecial = false) { return catalog.ReadItemCatalog(includeSpecial); }
        public bool CanSpawnItems { get { return spawning.CanSpawnItems; } }
        public bool SpawnItem(ItemCatalogEntry entry, bool includeSpecial = false) { return spawning.SpawnItem(entry, includeSpecial); }

        public bool MatchesItem(string query, ItemCatalogEntry item)
        {
            return item != null && ItemSearch.Matches(query, item.CurrentName, item.EnglishName, item.SimplifiedName, item.TraditionalName);
        }
    }
}
