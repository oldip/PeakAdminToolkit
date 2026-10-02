using System;

namespace PeakAdminToolkit.Self
{
    internal sealed class NoFallDamage : SelfFeature
    {
        private static NoFallDamage instance;
        internal NoFallDamage(SelfApi api, Action<string> log) : base(api, "NoFallDamage", log)
        {
            instance = this;
            Bind(() => Patch(SelfApi.Method(api.Type("CharacterMovement"), "CheckFallDamage", typeof(void)), "Prefix", null));
        }
        internal static bool Prefix(object __instance)
        {
            NoFallDamage self = instance;
            if (self == null) return true;
            try
            {
                object character = SelfApi.Read(__instance, "character");
                return !(self.Enabled && self.Applies(character)) && !Flight.ProtectsFromFall(character);
            }
            catch (Exception ex) { self.Fail(ex); return true; }
        }
    }
}
