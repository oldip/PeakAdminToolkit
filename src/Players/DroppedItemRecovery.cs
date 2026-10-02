using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace PeakAdminToolkit.Players
{
    internal sealed class DroppedItemEntry
    {
        internal readonly int ViewId;
        internal readonly string Name;
        internal DroppedItemEntry(int viewId, string name) { ViewId = viewId; Name = name; }
    }

    internal sealed class DroppedItemRecovery
    {
        private const BindingFlags Members = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private readonly PlayerDirectory directory;
        private readonly DroppedItemHistory history;
        private readonly Type itemType;
        private readonly Type rpcTargetType;
        internal bool Available { get; private set; }

        internal DroppedItemRecovery(PlayerDirectory directory, Func<string, Type> resolve = null, DroppedItemHistory history = null)
        {
            this.directory = directory;
            this.history = history;
            resolve = resolve ?? FindType;
            itemType = resolve("Item");
            rpcTargetType = resolve("Photon.Pun.RpcTarget");
            Available = itemType != null && rpcTargetType != null &&
                itemType.GetProperty("itemState", Members) != null &&
                itemType.GetMethod("GetName", Members, null, Type.EmptyTypes, null) != null &&
                itemType.GetMethod("SetKinematicRPC", Members) != null &&
                Enum.IsDefined(rpcTargetType, "AllViaServer");
        }

        internal List<DroppedItemEntry> Read(PlayerEntry owner)
        {
            var result = new List<DroppedItemEntry>();
            if (!CanRecover(owner)) return result;
            var seen = new HashSet<int>();
            foreach (object view in Views(owner.Character))
            {
                object item;
                int id;
                if (!TryGround(view, out item, out id) || !seen.Add(id)) continue;
                try
                {
                    string name = itemType.GetMethod("GetName", Members, null, Type.EmptyTypes, null).Invoke(item, null) as string;
                    result.Add(new DroppedItemEntry(id, string.IsNullOrWhiteSpace(name) ? "#" + id : name));
                }
                catch (Exception) { result.Add(new DroppedItemEntry(id, "#" + id)); }
            }
            return result;
        }

        internal int MoveSelected(PlayerEntry owner, IEnumerable<int> selected)
        {
            if (!CanRecover(owner) || selected == null) return 0;
            var ids = new HashSet<int>(selected);
            if (ids.Count == 0) return 0;
            object center;
            try { center = directory.Position(directory.Local(), "Center"); }
            catch (Exception) { return 0; }
            int moved = 0;
            var seen = new HashSet<int>();
            foreach (object view in Views(owner.Character))
            {
                object item;
                int id;
                if (!TryGround(view, out item, out id) || !ids.Contains(id) || !seen.Add(id)) continue;
                try
                {
                    object targetPosition = Offset(center, moved);
                    object rotation = PlayerDirectory.ReadMember(PlayerDirectory.ReadMember(item, "transform"), "rotation");
                    MethodInfo rpc = view.GetType().GetMethod("RPC", Members, null,
                        new[] { typeof(string), rpcTargetType, typeof(object[]) }, null);
                    if (rpc == null || !CanRecover(owner) || !TryGround(view, out item, out id)) continue;
                    rpc.Invoke(view, new object[] { "SetKinematicRPC", Enum.Parse(rpcTargetType, "AllViaServer"),
                        new object[] { false, targetPosition, rotation } });
                    moved++;
                }
                catch (Exception) { }
            }
            return moved;
        }

        private bool CanRecover(PlayerEntry owner)
        {
            try
            {
                object local = directory.Local();
                return Available && directory.CanManage && directory.IsCurrent(owner) &&
                    local != null && !directory.Dead(local) && !directory.PassedOut(local);
            }
            catch (Exception) { return false; }
        }

        private IEnumerable Views(object character)
        {
            try
            {
                object items = PlayerDirectory.ReadMember(PlayerDirectory.ReadMember(character, "refs"), "items");
                IEnumerable source = PlayerDirectory.ReadMember(items, "droppedItems") as IEnumerable;
                var snapshot = new List<object>();
                if (source != null) foreach (object view in source) snapshot.Add(view);
                if (history != null) foreach (object view in history.Read(character)) snapshot.Add(view);
                return snapshot;
            }
            catch (Exception) { return new object[0]; }
        }

        private bool TryGround(object view, out object item, out int id)
        {
            item = null; id = 0;
            try
            {
                id = Convert.ToInt32(PlayerDirectory.ReadMember(view, "ViewID"));
                if (id <= 0 || !(bool)PlayerDirectory.ReadMember(view, "IsMine")) return false;
                MethodInfo get = view.GetType().GetMethod("GetComponent", Members, null, new[] { typeof(Type) }, null);
                if (get == null) return false;
                item = get.Invoke(view, new object[] { itemType });
                return item != null && string.Equals(Convert.ToString(PlayerDirectory.ReadMember(item, "itemState")), "Ground", StringComparison.Ordinal) &&
                    ReferenceEquals(PlayerDirectory.ReadMember(item, "photonView"), view);
            }
            catch (Exception) { return false; }
        }

        private static object Offset(object center, int index)
        {
            Type type = center.GetType();
            float x = Convert.ToSingle(PlayerDirectory.ReadMember(center, "x"));
            float y = Convert.ToSingle(PlayerDirectory.ReadMember(center, "y"));
            float z = Convert.ToSingle(PlayerDirectory.ReadMember(center, "z"));
            return Activator.CreateInstance(type, x + 1.5f + (index % 4) * 0.7f,
                y + 0.6f, z + 1.5f + (index / 4) * 0.7f);
        }

        private static Type FindType(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type found = assembly.GetType(name, false);
                if (found != null) return found;
            }
            return null;
        }
    }
}
