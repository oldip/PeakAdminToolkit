using UnityEngine;

namespace PeakAdminToolkit.Items
{
    internal sealed class ItemCatalogEntry
    {
        public readonly string SpawnName;
        public readonly string CurrentName;
        public readonly string EnglishName;
        public readonly string SimplifiedName;
        public readonly string TraditionalName;
        public readonly Texture2D Icon;
        public readonly ItemCategory Categories;
        public readonly ConsumableKind Kinds;
        public readonly bool ValidToSpawn;
        public readonly bool CanSpawnManually;
        private readonly string englishDescription, simplifiedDescription, traditionalDescription;

        public string DisplayName(string language)
        {
            string name = language == "zh-TW" ? TraditionalName : language == "zh-CN" ? SimplifiedName
                : language == "en" ? EnglishName : CurrentName;
            return string.IsNullOrWhiteSpace(name) ? CurrentName : name;
        }

        public string DisplayDescription(string language)
        {
            string description = language == "zh-TW" ? traditionalDescription : language == "zh-CN" ? simplifiedDescription : englishDescription;
            return string.IsNullOrWhiteSpace(description) ? string.Empty : description.Trim();
        }

        public ItemCatalogEntry(string spawnName, string currentName, string englishName, string simplifiedName, string traditionalName,
            Texture2D icon, ItemCategory categories, ConsumableKind kinds,
            string englishDescription = null, string simplifiedDescription = null, string traditionalDescription = null,
            bool validToSpawn = true, bool? canSpawnManually = null)
        {
            SpawnName = spawnName;
            CurrentName = currentName;
            EnglishName = englishName;
            SimplifiedName = simplifiedName;
            TraditionalName = traditionalName;
            Icon = icon;
            Categories = categories;
            Kinds = kinds;
            ValidToSpawn = validToSpawn;
            CanSpawnManually = canSpawnManually ?? validToSpawn;
            this.englishDescription = englishDescription;
            this.simplifiedDescription = simplifiedDescription;
            this.traditionalDescription = traditionalDescription;
        }
    }
}
