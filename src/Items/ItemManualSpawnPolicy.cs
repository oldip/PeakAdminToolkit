using System;
using System.Reflection;

namespace PeakAdminToolkit.Items
{
    internal static class ItemManualSpawnPolicy
    {
        // Called only after a successful false IsValidToSpawn evaluation.
        internal static bool AllowsSoloOverride(object item, string spawnName)
        {
            if (item == null || SpecialItems.Classify(spawnName) != SpecialItemKind.MultiplayerItems) return false;
            try
            {
                Type network = ItemApiAccess.FindLoadedType("Photon.Pun.PhotonNetwork");
                if (!ItemApiAccess.HasStaticBooleanMember(network, "OfflineMode") ||
                    !ItemApiAccess.HasStaticBooleanMember(network, "InRoom")) return false;
                bool solo = ItemApiAccess.ReadStaticBoolean(network, "OfflineMode");
                if (!solo && ItemApiAccess.ReadStaticBoolean(network, "InRoom"))
                {
                    object room = ItemApiAccess.ReadStaticMember(network, "CurrentRoom");
                    object count = ItemApiAccess.ReadMember(room, "PlayerCount");
                    solo = count is int && (int)count == 1;
                }
                if (!solo) return false;

                Type lootType = ItemApiAccess.FindGameType("LootData");
                MethodInfo getComponent = item.GetType().GetMethod("GetComponent", ItemApiAccess.InstanceFlags, null, new[] { typeof(Type) }, null);
                if (lootType == null || getComponent == null) return false;
                object loot = getComponent.Invoke(item, new object[] { lootType });
                if (loot == null) return false;
                object ban = ItemApiAccess.ReadMember(loot, "banInSolo");
                object extraRule = ItemApiAccess.ReadMember(loot, "excludeBasedOnCustomRunSetting");
                if (!(ban is bool) || !(bool)ban || !(extraRule is bool) || (bool)extraRule) return false;
                FieldInfo alternate = lootType.GetField("useOtherItemForSpawningValidity", ItemApiAccess.InstanceFlags);
                if (alternate == null || alternate.GetValue(loot) != null) return false;

                Type settings = ItemApiAccess.FindGameType("RunSettings");
                if (settings == null) return false;
                MethodInfo byName = settings.GetMethod("IsItemEnabled", ItemApiAccess.StaticFlags, null, new[] { typeof(string) }, null);
                MethodInfo byId = settings.GetMethod("IsItemEnabled", ItemApiAccess.StaticFlags, null, new[] { typeof(ushort) }, null);
                object id = ItemApiAccess.ReadMember(item, "itemID");
                if (byName == null || byId == null || byName.ReturnType != typeof(bool) || byId.ReturnType != typeof(bool) || !(id is ushort)) return false;
                return (bool)byName.Invoke(null, new object[] { spawnName }) && (bool)byId.Invoke(null, new[] { id });
            }
            catch (Exception) { return false; }
        }
    }
}
