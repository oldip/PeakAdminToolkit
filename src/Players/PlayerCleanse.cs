using System;
using System.Reflection;

namespace PeakAdminToolkit.Players
{
    internal sealed class PlayerCleanse
    {
        private readonly PlayerDirectory directory;
        private readonly MethodInfo clearAll;
        private readonly MethodInfo clearAfflictions;
        private readonly MethodInfo removeThorns;
        internal bool Available { get { return clearAll != null && clearAfflictions != null && removeThorns != null; } }
        internal string FailureReason { get; private set; }

        internal PlayerCleanse(PlayerDirectory directory, Func<string, Type> resolve = null)
        {
            this.directory = directory;
            try
            {
                Type type = resolve == null ? FindType("CharacterAfflictions") : resolve("CharacterAfflictions");
                if (type == null) throw new MissingMemberException("CharacterAfflictions");
                clearAll = type.GetMethod("ClearAll", BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
                clearAfflictions = type.GetMethod("ClearAllAfflictions", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
                removeThorns = type.GetMethod("RemoveAllThorns", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
                if (clearAll == null || clearAll.ReturnType != typeof(void)) throw new MissingMethodException("CharacterAfflictions", "ClearAll");
                if (clearAfflictions == null || clearAfflictions.ReturnType != typeof(void)) throw new MissingMethodException("CharacterAfflictions", "ClearAllAfflictions");
                if (removeThorns == null || removeThorns.ReturnType != typeof(void)) throw new MissingMethodException("CharacterAfflictions", "RemoveAllThorns");
            }
            catch (Exception ex) { clearAll = null; clearAfflictions = null; removeThorns = null; FailureReason = ex.GetType().Name + ": " + ex.Message; }
        }

        internal bool CanRequest(PlayerEntry entry)
        {
            try
            {
                return Available && directory.IsCurrent(entry) && ReferenceEquals(entry.Character, directory.Local()) &&
                    !directory.Dead(entry.Character) &&
                    (bool)PlayerDirectory.ReadMember(PlayerDirectory.ReadMember(entry.Character, "photonView"), "IsMine") &&
                    PlayerDirectory.ReadMember(PlayerDirectory.ReadMember(entry.Character, "refs"), "afflictions") != null;
            }
            catch (Exception) { return false; }
        }

        internal bool Request(PlayerEntry entry)
        {
            if (!CanRequest(entry)) return false;
            try
            {
                object afflictions = PlayerDirectory.ReadMember(PlayerDirectory.ReadMember(entry.Character, "refs"), "afflictions");
                clearAfflictions.Invoke(afflictions, null);
                removeThorns.Invoke(afflictions, null);
                clearAll.Invoke(null, null);
                return true;
            }
            catch (Exception) { return false; }
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
