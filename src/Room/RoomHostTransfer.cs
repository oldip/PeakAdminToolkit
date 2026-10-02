using System;
using System.Reflection;

namespace PeakAdminToolkit.Core
{
    internal sealed class RoomHostTransfer
    {
        private readonly Func<string, Type> resolve;
        internal RoomHostTransfer(Func<string, Type> resolve = null) { this.resolve = resolve ?? FindType; }

        internal bool CanRequest()
        {
            try
            {
                Type network = resolve("Photon.Pun.PhotonNetwork");
                PropertyInfo player = network.GetProperty("LocalPlayer", BindingFlags.Public | BindingFlags.Static);
                return (bool)network.GetProperty("InRoom").GetValue(null, null) &&
                    !(bool)network.GetProperty("IsMasterClient").GetValue(null, null) &&
                    !(bool)network.GetProperty("OfflineMode").GetValue(null, null) &&
                    player != null && player.GetValue(null, null) != null &&
                    TransferMethod(network, player.PropertyType) != null;
            }
            catch (Exception) { return false; }
        }

        internal bool Request()
        {
            if (!CanRequest()) return false;
            try
            {
                Type network = resolve("Photon.Pun.PhotonNetwork");
                PropertyInfo player = network.GetProperty("LocalPlayer", BindingFlags.Public | BindingFlags.Static);
                return (bool)TransferMethod(network, player.PropertyType).Invoke(null, new[] { player.GetValue(null, null) });
            }
            catch (Exception) { return false; }
        }

        private static MethodInfo TransferMethod(Type network, Type player)
        {
            MethodInfo method = network.GetMethod("SetMasterClient", BindingFlags.Public | BindingFlags.Static,
                null, new[] { player }, null);
            return method != null && method.ReturnType == typeof(bool) ? method : null;
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
