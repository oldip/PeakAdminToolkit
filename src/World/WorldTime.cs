using System;
using System.Reflection;

namespace PeakAdminToolkit.World
{
    internal enum TimePreset { Morning, Noon, Evening, Midnight }

    internal sealed class WorldTime
    {
        private readonly Func<string, Type> resolve;

        internal WorldTime(Func<string, Type> resolve = null) { this.resolve = resolve ?? FindType; }

        internal bool CanSet() { return UnavailableReason() == null; }

        internal string UnavailableReason()
        {
            try
            {
                Type network = resolve("Photon.Pun.PhotonNetwork");
                if (network == null) return "WorldTimeApiUnavailable";
                if (!ReadBool(network, "InRoom") || !ReadBool(network, "IsMasterClient"))
                    return "WorldHostRequired";
                Type game = resolve("GameHandler");
                if (game == null || !ReadBool(game, "IsInGameplayScene")) return "WorldGameplayRequired";
                Type day = resolve("DayNightManager");
                FieldInfo midnight = day == null ? null : day.GetField("passedMidnight", BindingFlags.NonPublic | BindingFlags.Instance);
                if (DayInstance(day) == null ||
                    day.GetMethod("SetTimeOfDay", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(float) }, null) == null ||
                    midnight == null || midnight.FieldType != typeof(bool))
                    return "WorldTimeApiUnavailable";
                return null;
            }
            catch (Exception) { return "WorldTimeApiUnavailable"; }
        }
        internal bool Set(TimePreset preset)
        {
            float hour;
            switch (preset)
            {
                case TimePreset.Morning: hour = 7f; break;
                case TimePreset.Noon: hour = 12f; break;
                case TimePreset.Evening: hour = 18f; break;
                case TimePreset.Midnight: hour = 0f; break;
                default: return false;
            }
            if (!CanSet()) return false;
            try
            {
                Type day = resolve("DayNightManager");
                FieldInfo midnight = day == null ? null : day.GetField("passedMidnight", BindingFlags.NonPublic | BindingFlags.Instance);
                day.GetMethod("SetTimeOfDay", BindingFlags.Public | BindingFlags.Static, null,
                    new[] { typeof(float) }, null).Invoke(null, new object[] { hour });
                midnight.SetValue(DayInstance(day), false);
                return true;
            }
            catch (Exception) { return false; }
        }

        internal bool TryRead(out int dayCount, out float hour)
        {
            dayCount = 0; hour = 0;
            try
            {
                Type type = resolve("DayNightManager");
                object instance = DayInstance(type);
                if (instance == null) return false;
                FieldInfo day = type.GetField("dayCount", BindingFlags.Public | BindingFlags.Instance);
                FieldInfo time = type.GetField("timeOfDay", BindingFlags.Public | BindingFlags.Instance);
                if (day == null || time == null || day.FieldType != typeof(int) || time.FieldType != typeof(float)) return false;
                dayCount = (int)day.GetValue(instance);
                hour = (float)time.GetValue(instance);
                return true;
            }
            catch (Exception) { return false; }
        }

        private bool IsHostInGameplay()
        {
            Type network = resolve("Photon.Pun.PhotonNetwork");
            Type game = resolve("GameHandler");
            return network != null && game != null &&
                ReadBool(network, "InRoom") && ReadBool(network, "IsMasterClient") &&
                ReadBool(game, "IsInGameplayScene");
        }

        private static bool ReadBool(Type type, string name)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Static);
            return property != null && property.PropertyType == typeof(bool) && (bool)property.GetValue(null, null);
        }

        private static object DayInstance(Type type)
        {
            FieldInfo field = type == null ? null : type.GetField("instance", BindingFlags.Public | BindingFlags.Static);
            return field == null ? null : field.GetValue(null);
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






