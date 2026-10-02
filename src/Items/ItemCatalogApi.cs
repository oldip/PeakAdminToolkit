using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using PeakAdminToolkit.Core;

namespace PeakAdminToolkit.Items
{
    internal sealed class ItemCatalogApi
    {
        private readonly ItemDescriptions descriptions;
        private readonly Action<string> diagnosticLog;

        public ItemCatalogApi(ItemDescriptions descriptions, Action<string> diagnosticLog = null)
        {
            this.descriptions = descriptions;
            this.diagnosticLog = diagnosticLog;
        }

        public List<ItemCatalogEntry> ReadItemCatalog(bool includeSpecial = false)
        {
            var diagnostics = includeSpecial && diagnosticLog != null ? new ItemValidityDiagnostics(diagnosticLog) : null;
            try { return ReadCatalog(includeSpecial, diagnostics); }
            catch (Exception ex)
            {
                if (diagnostics != null) diagnostics.Write("catalog read failed: " + ex.GetType().Name);
                return new List<ItemCatalogEntry>();
            }
        }

        private List<ItemCatalogEntry> ReadCatalog(bool includeSpecial, ItemValidityDiagnostics diagnostics)
        {
            List<ItemCatalogEntry> result = new List<ItemCatalogEntry>();
            Type databaseType = ItemApiAccess.FindGameType("ItemDatabase");
            if (databaseType == null)
            {
                if (diagnostics != null) diagnostics.Write("ItemDatabase type unavailable");
                return result;
            }
            object database = ItemApiAccess.ReadStaticMember(databaseType, "Instance");
            object lookupObject = ItemApiAccess.ReadMember(database, "itemLookup");
            IDictionary lookup = lookupObject as IDictionary;
            if (lookup == null)
            {
                if (diagnostics != null) diagnostics.Write("ItemDatabase lookup unavailable");
                return result;
            }

            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (DictionaryEntry pair in lookup)
            {
                try
                {
                    ItemCatalogEntry entry = CreateCatalogEntry(pair.Value, pair.Key as string, includeSpecial, diagnostics);
                    if (entry == null || !seen.Add(entry.SpawnName)) continue;
                    result.Add(entry);
                }
                catch (Exception ex)
                {
                    if (diagnostics != null) diagnostics.Record(pair.Key as string, false, false, "candidate read failed: " + ex.GetType().Name);
                    // An incompatible prefab must not hide the remaining catalog.
                }
            }
            result.Sort(delegate(ItemCatalogEntry left, ItemCatalogEntry right)
            {
                int nameOrder = StringComparer.CurrentCultureIgnoreCase.Compare(left.CurrentName, right.CurrentName);
                return nameOrder != 0 ? nameOrder : StringComparer.OrdinalIgnoreCase.Compare(left.SpawnName, right.SpawnName);
            });
            if (diagnostics != null) diagnostics.Write();
            return result;
        }

        private ItemCatalogEntry CreateCatalogEntry(object item, string registeredName, bool includeSpecial, ItemValidityDiagnostics diagnostics)
        {
            string spawnName = GetSpawnName(item);
            if (string.IsNullOrEmpty(spawnName))
            {
                if (diagnostics != null) diagnostics.Record(registeredName, false, false, "prefab or name unavailable");
                return null;
            }
            bool valid;
            string reason;
            bool evaluated = ItemApiAccess.TryIsValidToSpawn(item, out valid, out reason);
            if (diagnostics != null) diagnostics.Record(spawnName, evaluated, valid, reason);
            bool duplicate = ItemApiAccess.ReadMember(item, "isSecretlyOtherItemPrefab") != null;
            if (!evaluated) return null;
            bool canSpawnManually = valid || ItemManualSpawnPolicy.AllowsSoloOverride(item, spawnName);
            if (!ItemVisibility.ShouldShow(spawnName, true, canSpawnManually, duplicate, includeSpecial)) return null;

            string currentName = ItemApiAccess.InvokeStringMethod(item, "GetName");
            if (string.IsNullOrWhiteSpace(currentName)) currentName = spawnName;
            string nameKey = GetLocalizedItemKey(item, "GetNameIndex");
            string englishName = GetLocalizedText(nameKey, "English");
            string simplifiedName = GetLocalizedText(nameKey, "SimplifiedChinese");
            string traditionalName = GetLocalizedText(nameKey, "TraditionalChinese");
            if (string.IsNullOrWhiteSpace(englishName)) englishName = spawnName;
            object uiData = ItemApiAccess.ReadMember(item, "UIData");
            string uiName = ItemApiAccess.ReadMember(uiData, "itemName") as string ?? spawnName;
            object tags = ItemApiAccess.ReadMember(item, "itemTags");
            string nativeTags = tags == null ? string.Empty : tags.ToString();
            string descriptionKey = GetLocalizedItemKey(item, "GetDescriptionIndex");
            SpecialItemKind specialKind = SpecialItems.Classify(spawnName);
            if (specialKind != SpecialItemKind.None)
            {
                var en = new Localization(); en.Update("en", null);
                var cn = new Localization(); cn.Update("zh-CN", null);
                var tw = new Localization(); tw.Update("zh-TW", null);
                ItemCategory categories = ItemCategory.Other;
                ConsumableKind kinds = ConsumableKind.None;
                if (specialKind == SpecialItemKind.MultiplayerItems)
                {
                    categories |= ItemClassification.Categories(uiName, nativeTags);
                    kinds = ItemClassification.Kinds(uiName, nativeTags);
                }
                string englishDisplay = SpecialItems.DisplayName(spawnName, en);
                string simplifiedDisplay = SpecialItems.DisplayName(spawnName, cn);
                string traditionalDisplay = SpecialItems.DisplayName(spawnName, tw);
                string englishDescription = SpecialItems.Description(spawnName, en);
                string simplifiedDescription = SpecialItems.Description(spawnName, cn);
                string traditionalDescription = SpecialItems.Description(spawnName, tw);
                if (string.IsNullOrEmpty(englishDescription)) englishDescription = ReadDescription(descriptionKey, "en", uiName, englishName, spawnName);
                if (string.IsNullOrEmpty(simplifiedDescription)) simplifiedDescription = ReadDescription(descriptionKey, "zh-CN", uiName, englishName, spawnName);
                if (string.IsNullOrEmpty(traditionalDescription)) traditionalDescription = ReadDescription(descriptionKey, "zh-TW", uiName, englishName, spawnName);
                return new ItemCatalogEntry(spawnName, currentName.Trim(),
                    string.IsNullOrEmpty(englishDisplay) ? englishName.Trim() : englishDisplay,
                    string.IsNullOrEmpty(simplifiedDisplay) ? simplifiedName : simplifiedDisplay,
                    string.IsNullOrEmpty(traditionalDisplay) ? traditionalName : traditionalDisplay,
                    ReadItemIcon(uiData), categories, kinds, englishDescription, simplifiedDescription, traditionalDescription,
                    valid, canSpawnManually);
            }
            return new ItemCatalogEntry(spawnName, currentName.Trim(), englishName.Trim(), simplifiedName, traditionalName,
                ReadItemIcon(uiData), ItemClassification.Categories(uiName, nativeTags), ItemClassification.Kinds(uiName, nativeTags),
                ReadDescription(descriptionKey, "en", uiName, englishName, spawnName),
                ReadDescription(descriptionKey, "zh-CN", uiName, englishName, spawnName),
                ReadDescription(descriptionKey, "zh-TW", uiName, englishName, spawnName));
        }

        private string ReadDescription(string key, string language, params string[] names)
        {
            string embedded = descriptions == null ? string.Empty : descriptions.Get(language, names);
            if (!string.IsNullOrEmpty(embedded)) return embedded;
            return GetLocalizedText(key, language == "zh-TW" ? "TraditionalChinese" : language == "zh-CN" ? "SimplifiedChinese" : "English");
        }

        private static Texture2D ReadItemIcon(object uiData)
        {
            if (uiData == null) return null;
            MethodInfo getter = uiData.GetType().GetMethod("GetIcon", ItemApiAccess.InstanceFlags, null, Type.EmptyTypes, null);
            if (getter != null && getter.ReturnType == typeof(Texture2D))
            {
                try
                {
                    Texture2D icon = getter.Invoke(uiData, null) as Texture2D;
                    if (icon != null) return icon;
                }
                catch (Exception) { }
            }
            return ItemApiAccess.ReadMember(uiData, "icon") as Texture2D;
        }

        private static string GetSpawnName(object item)
        {
            object value = ItemApiAccess.ReadMember(item, "name");
            string name = value as string;
            return string.IsNullOrWhiteSpace(name) ? string.Empty : name.Replace("(Clone)", string.Empty).Trim();
        }

        private static string GetLocalizedItemKey(object item, string methodName)
        {
            object uiData = ItemApiAccess.ReadMember(item, "UIData");
            string itemName = ItemApiAccess.ReadMember(uiData, "itemName") as string;
            if (string.IsNullOrWhiteSpace(itemName)) return string.Empty;
            Type localizedText = ItemApiAccess.FindGameType("LocalizedText");
            if (localizedText == null) return string.Empty;
            MethodInfo method = localizedText.GetMethod(methodName, ItemApiAccess.StaticFlags, null, new[] { typeof(string) }, null);
            if (method == null) return string.Empty;
            try { return method.Invoke(null, new object[] { itemName }) as string ?? string.Empty; }
            catch (Exception) { return string.Empty; }
        }

        private static string GetLocalizedText(string key, string language)
        {
            if (string.IsNullOrWhiteSpace(key)) return string.Empty;
            Type localizedText = ItemApiAccess.FindGameType("LocalizedText");
            if (localizedText == null) return string.Empty;
            FieldInfo currentLanguage = localizedText.GetField("CURRENT_LANGUAGE", ItemApiAccess.StaticFlags);
            if (currentLanguage == null || !currentLanguage.FieldType.IsEnum) return string.Empty;
            MethodInfo method = localizedText.GetMethod("GetText", ItemApiAccess.StaticFlags, null,
                new[] { typeof(string), currentLanguage.FieldType }, null);
            if (method == null) return string.Empty;
            try
            {
                object languageValue = Enum.Parse(currentLanguage.FieldType, language);
                return method.Invoke(null, new[] { (object)key, languageValue }) as string ?? string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
