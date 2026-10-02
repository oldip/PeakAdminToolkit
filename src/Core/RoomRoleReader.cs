using System;
using System.Reflection;

namespace PeakAdminToolkit.Core
{
    internal enum RoomRole { Unavailable, OutsideRoom, Client, Host }

    internal sealed class RoomRoleReader
    {
        private readonly Func<string, Type> resolve;
        private Type network;
        internal RoomRoleReader(Func<string, Type> resolve = null) { this.resolve = resolve ?? FindType; }

        internal RoomRole Read()
        {
            try
            {
                if (network == null) network = resolve("Photon.Pun.PhotonNetwork");
                if (network == null) return RoomRole.Unavailable;
                PropertyInfo inRoom = network.GetProperty("InRoom", BindingFlags.Public | BindingFlags.Static);
                PropertyInfo master = network.GetProperty("IsMasterClient", BindingFlags.Public | BindingFlags.Static);
                if (inRoom == null || master == null || inRoom.PropertyType != typeof(bool) || master.PropertyType != typeof(bool))
                    return RoomRole.Unavailable;
                if (!(bool)inRoom.GetValue(null, null)) return RoomRole.OutsideRoom;
                return (bool)master.GetValue(null, null) ? RoomRole.Host : RoomRole.Client;
            }
            catch (Exception) { return RoomRole.Unavailable; }
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
