using System;
using System.Reflection;

namespace PeakAdminToolkit.World
{
    internal sealed class WorldAdvance
    {
        private static readonly string[] Segments = { "Beach", "Tropics", "Alpine", "Caldera", "TheKiln", "Peak", "Void" };
        private readonly Func<string, Type> resolve;

        internal WorldAdvance(Func<string, Type> resolve = null) { this.resolve = resolve ?? FindType; }

        internal bool CanAdvance() { return UnavailableReason() == null; }

        internal string UnavailableReason()
        {
            try
            {
                Type network = resolve("Photon.Pun.PhotonNetwork");
                if (network == null) return "WorldMapApiUnavailable";
                if (!ReadBool(network, "InRoom") || !ReadBool(network, "IsMasterClient"))
                    return "WorldHostRequired";
                Type game = resolve("GameHandler");
                if (game == null || !ReadBool(game, "IsInGameplayScene")) return "WorldGameplayRequired";
                Type map = resolve("MapHandler");
                if (map == null || !ReadBool(map, "ExistsAndInitialized"))
                    return "WorldMapApiUnavailable";
                string current, next;
                if (!TryRead(out current, out next)) return "WorldMapApiUnavailable";
                return next == null ? (current == "Void" ? "WorldNoNextSegmentVoid" : "WorldNoNextSegment") : null;
            }
            catch (Exception) { return "WorldMapApiUnavailable"; }
        }
        internal bool TryRead(out string current, out string next)
        {
            current = null; next = null;
            try
            {
                Type map = resolve("MapHandler");
                PropertyInfo property = map == null ? null : map.GetProperty("CurrentSegmentNumber", BindingFlags.Public | BindingFlags.Static);
                if (property == null || !property.PropertyType.IsEnum) return false;
                object value = property.GetValue(null, null);
                int index = Convert.ToInt32(value);
                if (index < 0 || index >= Segments.Length || value.ToString() != Segments[index]) return false;
                current = Segments[index];
                if (index < 5) next = Segments[index + 1];
                return true;
            }
            catch (Exception) { return false; }
        }

        internal bool Advance()
        {
            if (!CanAdvance()) return false;
            try
            {
                string current, next;
                if (!TryRead(out current, out next)) return false;
                return WorldDestinationTeleport.TryTeleport(current, next);
            }
            catch (Exception) { return false; }
        }

        private static bool ReadBool(Type type, string name)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Static);
            return property != null && property.PropertyType == typeof(bool) && (bool)property.GetValue(null, null);
        }

        private static Type FindType(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(name, false);
                if (type != null) return type;
            }
            return null;
        }
    }
}



