using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace PeakAdminToolkit.Players
{
    internal sealed class PlayerEntry
    {
        internal readonly object Character;
        internal readonly string Name;
        internal PlayerEntry(object character, string name) { Character = character; Name = name; }
    }

    internal sealed class PlayerDirectory
    {
        private const BindingFlags Instance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private const BindingFlags Static = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        private readonly Func<string, Type> resolve;
        internal PlayerDirectory(Func<string, Type> resolve = null) { this.resolve = resolve ?? FindType; }
        internal Type CharacterType { get { return resolve("Character"); } }

        internal List<PlayerEntry> Read()
        {
            var result = new List<PlayerEntry>();
            try
            {
                Type type = CharacterType;
                IEnumerable characters = type.GetField("AllCharacters", Static).GetValue(null) as IEnumerable;
                if (characters == null) return result;
                foreach (object character in characters)
                {
                    try
                    {
                        if (character == null || !IsPlayer(character)) continue;
                        string name = ReadMember(character, "characterName") as string;
                        result.Add(new PlayerEntry(character, string.IsNullOrEmpty(name) ? "Player" : name));
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception) { result.Clear(); }
            return result;
        }

        internal bool IsCurrent(PlayerEntry entry)
        {
            if (entry == null || entry.Character == null || !IsPlayer(entry.Character)) return false;
            try
            {
                IEnumerable characters = CharacterType.GetField("AllCharacters", Static).GetValue(null) as IEnumerable;
                if (characters != null)
                    foreach (object current in characters) if (ReferenceEquals(current, entry.Character)) return true;
            }
            catch (Exception) { }
            return false;
        }

        internal object Local()
        {
            try { return CharacterType.GetField("localCharacter", Static).GetValue(null); }
            catch (Exception) { return null; }
        }

        internal object SpectatedLivingTeammate()
        {
            try
            {
                object spectated = ReadStatic(resolve("MainCameraMovement"), "specCharacter");
                return spectated != null && !ReferenceEquals(spectated, Local()) &&
                    IsCurrent(new PlayerEntry(spectated, string.Empty)) && !Dead(spectated) ? spectated : null;
            }
            catch (Exception) { return null; }
        }

        internal bool CanManage
        {
            get
            {
                try
                {
                    Type network = resolve("Photon.Pun.PhotonNetwork");
                    object local = Local();
                    return (bool)ReadStatic(network, "InRoom") && (bool)ReadStatic(network, "IsMasterClient") &&
                        local != null && IsCurrent(new PlayerEntry(local, string.Empty));
                }
                catch (Exception) { return false; }
            }
        }

        internal bool CanRequestPlayerRpc
        {
            get
            {
                try
                {
                    Type network = resolve("Photon.Pun.PhotonNetwork");
                    object local = Local();
                    return (bool)ReadStatic(network, "InRoom") && local != null &&
                        IsCurrent(new PlayerEntry(local, string.Empty));
                }
                catch (Exception) { return false; }
            }
        }

        internal bool CanSend(object character)
        {
            try
            {
                object view = ReadMember(character, "photonView");
                return Convert.ToInt32(ReadMember(view, "ViewID")) > 0 && RpcMethod(view.GetType()) != null;
            }
            catch (Exception) { return false; }
        }

        internal bool Send(object character, string method, params object[] arguments)
        {
            if (!CanRequestPlayerRpc || !CanSend(character)) return false;
            try
            {
                object view = ReadMember(character, "photonView");
                MethodInfo rpc = RpcMethod(view.GetType());
                object all = Enum.Parse(resolve("Photon.Pun.RpcTarget"), "All");
                rpc.Invoke(view, new object[] { method, all, arguments });
                return true;
            }
            catch (Exception) { return false; }
        }

        internal bool Dead(object character) { return (bool)ReadMember(ReadMember(character, "data"), "dead"); }
        internal bool PassedOut(object character) { return (bool)ReadMember(ReadMember(character, "data"), "fullyPassedOut"); }
        internal object Position(object character, string property) { return ReadMember(character, property); }

        private bool IsPlayer(object character)
        {
            try { return (bool)ReadMember(character, "IsPlayerControlled") && (bool)ReadMember(character, "IsRegisteredToPlayer") && !(bool)ReadMember(character, "isBot"); }
            catch (Exception) { return false; }
        }

        private MethodInfo RpcMethod(Type view)
        {
            Type target = resolve("Photon.Pun.RpcTarget");
            return target == null ? null : view.GetMethod("RPC", Instance, null, new[] { typeof(string), target, typeof(object[]) }, null);
        }

        internal static object ReadMember(object instance, string name)
        {
            if (instance == null) throw new MissingMemberException(name);
            Type type = instance.GetType();
            PropertyInfo property = type.GetProperty(name, Instance);
            if (property != null) return property.GetValue(instance, null);
            FieldInfo field = type.GetField(name, Instance);
            if (field != null) return field.GetValue(instance);
            throw new MissingMemberException(type.Name, name);
        }

        private static object ReadStatic(Type type, string name)
        {
            if (type == null) throw new MissingMemberException(name);
            PropertyInfo property = type.GetProperty(name, Static);
            if (property != null) return property.GetValue(null, null);
            FieldInfo field = type.GetField(name, Static);
            if (field != null) return field.GetValue(null);
            throw new MissingMemberException(type.Name, name);
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
