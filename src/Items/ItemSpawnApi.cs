using System;
using System.Reflection;

namespace PeakAdminToolkit.Items
{
    internal sealed class ItemSpawnApi
    {
        public bool CanSpawnItems
        {
            get
            {
                try
                {
                    object characterItems = GetLocalCharacterItems();
                    return IsInRoom() && HasSpawnCapability(characterItems);
                }
                catch (Exception) { return false; }
            }
        }

        public bool SpawnItem(ItemCatalogEntry entry, bool includeSpecial = false)
        {
            try { return RequestSpawn(entry, includeSpecial); }
            catch (Exception) { return false; }
        }

        private bool RequestSpawn(ItemCatalogEntry entry, bool includeSpecial)
        {
            if (entry == null || !entry.CanSpawnManually || !CanSpawnItems) return false;
            object item;
            if (!TryGetDatabaseItem(entry.SpawnName, out item)) return false;
            bool valid;
            string reason;
            if (!ItemApiAccess.TryIsValidToSpawn(item, out valid, out reason)) return false;
            bool canSpawnManually = valid || ItemManualSpawnPolicy.AllowsSoloOverride(item, entry.SpawnName);
            if (!canSpawnManually) return false;
            bool duplicate = ItemApiAccess.ReadMember(item, "isSecretlyOtherItemPrefab") != null;
            if (!ItemVisibility.ShouldShow(entry.SpawnName, true, canSpawnManually, duplicate, includeSpecial)) return false;

            object characterItems = GetLocalCharacterItems();
            if (characterItems == null) return false;
            MethodInfo spawn = FindSpawnMethod(characterItems.GetType());
            if (spawn == null) return false;
            try
            {
                spawn.Invoke(characterItems, new object[] { entry.SpawnName });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool HasSpawnCapability(object characterItems)
        {
            if (characterItems == null) return false;
            Type network = ItemApiAccess.FindLoadedType("Photon.Pun.PhotonNetwork");
            if (!ItemApiAccess.HasStaticBooleanMember(network, "InRoom")) return false;
            Type databaseType = ItemApiAccess.FindGameType("ItemDatabase");
            Type itemType = ItemApiAccess.FindGameType("Item");
            if (databaseType == null || itemType == null || ItemApiAccess.ReadStaticMember(databaseType, "Instance") == null) return false;
            if (FindTryGetItemMethod(databaseType, itemType) == null || FindSpawnMethod(characterItems.GetType()) == null) return false;
            MethodInfo valid = itemType.GetMethod("IsValidToSpawn", ItemApiAccess.InstanceFlags, null, Type.EmptyTypes, null);
            return valid != null && valid.ReturnType == typeof(bool);
        }

        private static bool TryGetDatabaseItem(string spawnName, out object item)
        {
            item = null;
            Type databaseType = ItemApiAccess.FindGameType("ItemDatabase");
            Type itemType = ItemApiAccess.FindGameType("Item");
            if (databaseType == null || itemType == null) return false;
            MethodInfo method = FindTryGetItemMethod(databaseType, itemType);
            if (method == null) return false;
            object[] arguments = { spawnName, null };
            try
            {
                if (!(bool)method.Invoke(null, arguments)) return false;
                item = arguments[1];
                return item != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static MethodInfo FindTryGetItemMethod(Type databaseType, Type itemType)
        {
            foreach (MethodInfo candidate in databaseType.GetMethods(ItemApiAccess.StaticFlags))
            {
                if (candidate.Name != "TryGetItem" || candidate.ReturnType != typeof(bool)) continue;
                ParameterInfo[] parameters = candidate.GetParameters();
                if (parameters.Length == 2 && parameters[0].ParameterType == typeof(string) &&
                    parameters[1].ParameterType.IsByRef && parameters[1].ParameterType.GetElementType() == itemType)
                    return candidate;
            }
            return null;
        }

        private static bool IsInRoom()
        {
            Type network = ItemApiAccess.FindLoadedType("Photon.Pun.PhotonNetwork");
            return ItemVisibility.CanRequestSpawn(ItemApiAccess.ReadStaticBoolean(network, "InRoom"));
        }

        private static object GetLocalCharacterItems()
        {
            Type characterType = ItemApiAccess.FindGameType("Character");
            object localCharacter = ItemApiAccess.ReadStaticMember(characterType, "localCharacter");
            object references = ItemApiAccess.ReadMember(localCharacter, "refs");
            return ItemApiAccess.ReadMember(references, "items");
        }

        private static MethodInfo FindSpawnMethod(Type type)
        {
            foreach (MethodInfo method in type.GetMethods(ItemApiAccess.InstanceFlags))
            {
                if (method.Name != "SpawnItemInHand" || method.ReturnType != typeof(void)) continue;
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string)) return method;
            }
            return null;
        }
    }
}
