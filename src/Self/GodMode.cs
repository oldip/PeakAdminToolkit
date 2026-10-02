using System;
using System.Reflection;

namespace PeakAdminToolkit.Self
{
    internal sealed class GodMode : SelfFeature
    {
        private static GodMode instance;
        internal GodMode(SelfApi api, Action<string> log) : base(api, "GodMode", log)
        {
            instance = this;
            Bind(() => Patch(FindAddStatus(api.Type("CharacterAfflictions")), "Prefix", null));
        }

        private static MethodInfo FindAddStatus(Type type)
        {
            if (type == null) throw new MissingMemberException("CharacterAfflictions");
            foreach (MethodInfo method in type.GetMethods(SelfApi.Instance))
            {
                if (method.Name != "AddStatus" || method.ReturnType != typeof(bool)) continue;
                ParameterInfo[] args = method.GetParameters();
                if ((args.Length != 6 && args.Length != 7) || !args[0].ParameterType.IsEnum || args[0].ParameterType.Name != "STATUSTYPE" ||
                    args[1].ParameterType != typeof(float)) continue;
                bool remainingBooleans = true;
                for (int i = 2; i < args.Length; i++) remainingBooleans &= args[i].ParameterType == typeof(bool);
                if (remainingBooleans) return method;
            }
            throw new MissingMethodException(type.Name, "AddStatus(STATUSTYPE, float, bool, bool, bool, bool[, bool])");
        }

        internal static bool Blocks(string status, float amount, bool ignoreInvincibility)
        {
            if (amount <= 0 || ignoreInvincibility) return false;
            switch (status)
            {
                case "Injury": case "Hunger": case "Cold": case "Poison": case "Crab": case "Curse":
                case "Drowsy": case "Hot": case "Thorns": case "Spores": case "Web": case "Arrow":
                case "Petrify": case "FlyTrap": return true;
                default: return false;
            }
        }

        internal static bool Prefix(object __instance, object __0, float __1, bool __5, ref bool __result)
        {
            GodMode self = instance;
            if (self == null || !self.Enabled) return true;
            try
            {
                if (self.Applies(SelfApi.Read(__instance, "character")) && Blocks(__0.ToString(), __1, __5))
                { __result = false; return false; }
            }
            catch (Exception ex) { self.Fail(ex); }
            return true;
        }
    }
}
