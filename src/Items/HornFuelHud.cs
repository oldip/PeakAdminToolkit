using System;
using System.Reflection;
using HarmonyLib;

namespace PeakAdminToolkit.Items
{
    // Local HUD correction only: the held bugle updates its Item data before the
    // player's inventory slot receives that data on drop/pickup.
    internal sealed class HornFuelHud : IDisposable
    {
        private const BindingFlags Members = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private static HornFuelHud current;
        private readonly Harmony harmony = new Harmony("com.oldip.peakadmintoolkit.hornfuelhud");
        private readonly Func<string, Type> resolve;
        private readonly Action<string> log;
        private readonly Func<bool> enabled;
        private bool disposed;
        internal bool Available { get; private set; }

        internal HornFuelHud(Func<string, Type> resolve, Action<string> log, Func<bool> enabled)
        {
            this.resolve = resolve ?? FindGameType;
            this.log = log;
            this.enabled = enabled;
            try
            {
                Type gui = this.resolve("GUIManager");
                if (gui == null || this.resolve("Character") == null || this.resolve("MagicBugle") == null)
                    throw new MissingMemberException("Horn HUD game types");
                MethodInfo update = gui.GetMethod("UpdateItems", Members, null, Type.EmptyTypes, null);
                if (update == null || update.ReturnType != typeof(void))
                    throw new MissingMethodException("GUIManager", "UpdateItems");
                harmony.Patch(update, null, new HarmonyMethod(typeof(HornFuelHud).GetMethod("Postfix", Members)));
                current = this;
                Available = true;
            }
            catch (Exception ex) { Fail(ex); }
        }

        private static void Postfix(object __instance)
        {
            HornFuelHud self = current;
            if (self == null || !self.Available || (self.enabled != null && !self.enabled())) return;
            try { Apply(__instance, self.resolve); }
            catch (Exception ex) { self.Fail(ex); }
        }

        internal static void Apply(object gui, Func<string, Type> resolve)
        {
            if (gui == null || resolve == null) return;
            Type characterType = resolve("Character");
            Type bugleType = resolve("MagicBugle");
            if (characterType == null || bugleType == null) return;
            object local = ReadStatic(characterType, "localCharacter");
            if (local == null || !ReferenceEquals(local, ReadStatic(characterType, "observedCharacter"))) return;
            object item = Read(Read(local, "data"), "currentItem");
            if (item == null) return;
            MethodInfo component = item.GetType().GetMethod("GetComponent", new[] { typeof(Type) });
            if (component == null) return;
            object bugle = component.Invoke(item, new object[] { bugleType });
            if (bugle == null) return;
            object selection = Read(Read(Read(local, "refs"), "items"), "currentSelectedSlot");
            if (selection == null || !Convert.ToBoolean(Read(selection, "IsSome"))) return;
            int index = Convert.ToInt32(Read(selection, "Value"));
            Array slots = Read(gui, "items") as Array;
            if (slots == null || index < 0 || index >= slots.Length) return;
            object slot = slots.GetValue(index);
            object bar = Read(slot, "fuelBar"), fill = Read(slot, "fuelBarFill");
            if (bar == null || fill == null) return;
            float total = Convert.ToSingle(Read(bugle, "totalTootTime"));
            float fuel = Convert.ToSingle(Read(bugle, "currentFuel"));
            if (total <= 0 || float.IsNaN(total) || float.IsNaN(fuel)) return;
            PropertyInfo amount = fill.GetType().GetProperty("fillAmount", Members);
            MethodInfo setActive = bar.GetType().GetMethod("SetActive", new[] { typeof(bool) });
            if (amount == null || !amount.CanWrite || setActive == null) return;
            float percentage = Math.Max(0f, Math.Min(1f, fuel / total));
            setActive.Invoke(bar, new object[] { true });
            amount.SetValue(fill, percentage, null);
        }

        private static object Read(object target, string name)
        {
            if (target == null) return null;
            Type type = target.GetType();
            FieldInfo field = type.GetField(name, Members);
            if (field != null) return field.GetValue(target);
            PropertyInfo property = type.GetProperty(name, Members);
            return property == null ? null : property.GetValue(target, null);
        }

        private static object ReadStatic(Type type, string name)
        {
            FieldInfo field = type.GetField(name, Members);
            if (field != null) return field.GetValue(null);
            PropertyInfo property = type.GetProperty(name, Members);
            return property == null ? null : property.GetValue(null, null);
        }

        private static Type FindGameType(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                if (assembly.GetName().Name == "Assembly-CSharp") return assembly.GetType(name, false);
            return null;
        }

        private void Fail(Exception ex)
        {
            Available = false;
            if (ReferenceEquals(current, this)) current = null;
            try { harmony.UnpatchSelf(); } catch { }
            if (log != null) log("Horn fuel HUD unavailable: " + ex.Message);
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            Available = false;
            if (ReferenceEquals(current, this)) current = null;
            harmony.UnpatchSelf();
        }
    }
}
