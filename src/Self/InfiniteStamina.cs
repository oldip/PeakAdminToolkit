using System;
using System.Reflection;

namespace PeakAdminToolkit.Self
{
    internal sealed class InfiniteStamina : SelfFeature
    {
        private static InfiniteStamina current;
        private PropertyInfo flag;
        private PropertyInfo stamina;
        private MethodInfo maximum, clamp;
        private bool original, captured;
        internal InfiniteStamina(SelfApi api, Action<string> log) : base(api, "InfiniteStamina", log)
        {
            current = this;
            Bind(() =>
            {
                Type character = api.Type("Character");
                flag = character.GetProperty("infiniteStam", SelfApi.Instance);
                if (flag == null || flag.PropertyType != typeof(bool) || !flag.CanRead || !flag.CanWrite)
                    throw new MissingMemberException("Character.infiniteStam");
                maximum = SelfApi.Method(character, "GetMaxStamina", typeof(float));
                clamp = SelfApi.Method(character, "ClampStamina", typeof(void));
                MethodInfo add = SelfApi.Method(character, "AddStamina", typeof(void), typeof(float));
                Type data = api.Type("CharacterData");
                stamina = data == null ? null : data.GetProperty("currentStamina", SelfApi.Instance);
                if (stamina == null || stamina.PropertyType != typeof(float) || !stamina.CanRead)
                    throw new MissingMemberException("CharacterData.currentStamina");
                Patch(add, "AddPrefix", null);
            });
        }
        protected override void Attach()
        {
            original = (bool)flag.GetValue(Target, null); captured = true;
            ClampExcess(Target, (float)maximum.Invoke(Target, null));
            flag.SetValue(Target, true, null);
        }
        protected override void Detach()
        {
            if (captured && SelfApi.Alive(Target)) flag.SetValue(Target, original, null);
            captured = false;
        }
        private float ClampExcess(object character, float limit)
        {
            float value = (float)stamina.GetValue(SelfApi.Read(character, "data"), null);
            if (value <= limit) return value;
            flag.SetValue(character, false, null);
            try { clamp.Invoke(character, null); }
            finally { flag.SetValue(character, true, null); }
            return (float)stamina.GetValue(SelfApi.Read(character, "data"), null);
        }
        internal static void AddPrefix(object __instance, ref float __0)
        {
            InfiniteStamina feature = current;
            if (feature == null || !feature.Applies(__instance)) return;
            float limit = (float)feature.maximum.Invoke(__instance, null);
            float value = feature.ClampExcess(__instance, limit);
            if (__0 <= 0f) return;
            __0 = Math.Max(0f, Math.Min(__0, limit - value));
        }
    }
}
