using System;
using System.Reflection;

internal static class HornHudTests
{
    private static int checks;
    private static void Main()
    {
        try { Run(); Console.WriteLine("PASS: " + checks + " horn HUD checks."); }
        catch (Exception ex) { Console.WriteLine("FAIL: " + ex); Environment.ExitCode = 1; }
    }
    private static void Run()
    {
        Type module = Assembly.GetExecutingAssembly().GetType("PeakAdminToolkit.Items.HornFuelHud");
        Check(module != null, "independent horn HUD module exists");
        MethodInfo apply = module.GetMethod("Apply", BindingFlags.Static | BindingFlags.NonPublic);
        Check(apply != null, "horn HUD exposes isolated frame update");
        var character = new Character(); Character.localCharacter = character; Character.observedCharacter = character;
        character.data.currentItem = new Item { bugle = new MagicBugle { currentFuel = 3, totalTootTime = 10 } };
        character.refs.items.currentSelectedSlot = new OptionalSlot { IsSome = true, Value = 1 };
        var gui = new GUIManager { items = new[] { new InventoryItemUI(), new InventoryItemUI() } };
        Apply(apply, gui);
        Check(gui.items[1].fuelBar.active && Math.Abs(gui.items[1].fuelBarFill.fillAmount - 0.3f) < 0.0001f,
            "selected horn slot shows live fuel after native repaint");
        Check(!gui.items[0].fuelBar.active, "unselected slot untouched");
        gui.items[1].fuelBarFill.fillAmount = 0.7f;
        character.data.currentItem = new Item(); Apply(apply, gui);
        Check(Math.Abs(gui.items[1].fuelBarFill.fillAmount - 0.7f) < 0.0001f, "non-horn item untouched");
        character.data.currentItem = new Item { bugle = new MagicBugle { currentFuel = 2, totalTootTime = 0 } };
        Apply(apply, gui);
        Check(Math.Abs(gui.items[1].fuelBarFill.fillAmount - 0.7f) < 0.0001f, "invalid fuel total untouched");
        character.data.currentItem.bugle.totalTootTime = 10;
        Character.observedCharacter = new Character(); Apply(apply, gui);
        Check(Math.Abs(gui.items[1].fuelBarFill.fillAmount - 0.7f) < 0.0001f, "remote observed HUD untouched");
        Character.observedCharacter = character;
        character.refs.items.currentSelectedSlot.IsSome = false; Apply(apply, gui);
        Check(Math.Abs(gui.items[1].fuelBarFill.fillAmount - 0.7f) < 0.0001f, "no selected slot untouched");
        ConstructorInfo constructor = module.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null,
            new[] { typeof(Func<string, Type>), typeof(Action<string>), typeof(Func<bool>) }, null);
        Check(constructor != null, "independent horn module constructor available");
        var missing = (IDisposable)constructor.Invoke(new object[] {
            new Func<string, Type>(n => n == "GUIManager" ? null : Resolve(n)), null, new Func<bool>(() => true) });
        Check(!(bool)module.GetProperty("Available", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(missing, null),
            "missing HUD API disables only horn correction");
        missing.Dispose();
#if REAL_HARMONY
        TestRealHook(module, character, gui);
#endif
    }
#if REAL_HARMONY
    private static void TestRealHook(Type module, Character character, GUIManager gui)
    {
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(HarmonyLib.AccessTools).TypeHandle);
        character.refs.items.currentSelectedSlot.IsSome = true;
        character.data.currentItem = new Item { bugle = new MagicBugle { currentFuel = 4, totalTootTime = 10 } };
        ConstructorInfo constructor = module.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null,
            new[] { typeof(Func<string, Type>), typeof(Action<string>), typeof(Func<bool>) }, null);
        Check(constructor != null, "isolated hook constructor available");
        var hook = (IDisposable)constructor.Invoke(new object[] { new Func<string, Type>(Resolve), null, new Func<bool>(() => true) });
        Check((bool)module.GetProperty("Available", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(hook, null),
            "real Harmony binds GUIManager.UpdateItems");
        gui.items[1].fuelBarFill.fillAmount = 1;
        gui.UpdateItems();
        Check(Math.Abs(gui.items[1].fuelBarFill.fillAmount - 0.4f) < 0.0001f,
            "real Harmony postfix displays live horn fuel");
        hook.Dispose();
        var disabled = (IDisposable)constructor.Invoke(new object[] { new Func<string, Type>(Resolve), null, new Func<bool>(() => false) });
        gui.items[1].fuelBarFill.fillAmount = 0.8f;
        gui.UpdateItems();
        Check(Math.Abs(gui.items[1].fuelBarFill.fillAmount - 0.8f) < 0.0001f,
            "disabled toolkit leaves native HUD unchanged");
        disabled.Dispose();
        gui.items[1].fuelBarFill.fillAmount = 0.8f;
        gui.UpdateItems();
        Check(Math.Abs(gui.items[1].fuelBarFill.fillAmount - 0.8f) < 0.0001f,
            "disposing horn module restores native HUD path");
    }
#endif
    private static void Apply(MethodInfo method, GUIManager gui)
    { method.Invoke(null, new object[] { gui, new Func<string, Type>(Resolve) }); }
    private static Type Resolve(string name)
    {
        if (name == "Character") return typeof(Character);
        if (name == "MagicBugle") return typeof(MagicBugle);
        if (name == "GUIManager") return typeof(GUIManager);
        return null;
    }
    private static void Check(bool value, string name) { checks++; if (!value) throw new Exception(name); }
}
