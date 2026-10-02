using System;
using System.Collections.Generic;
using System.Reflection;

#if !REAL_HARMONY
namespace HarmonyLib
{
    public sealed class HarmonyMethod { public HarmonyMethod(MethodInfo method) { if (method == null) throw new Exception("missing postfix"); } }
    public sealed class Harmony
    {
        public static readonly HashSet<string> Active = new HashSet<string>();
        private readonly string id;
        public Harmony(string id) { this.id = id; }
        public void Patch(MethodBase original, HarmonyMethod prefix = null, HarmonyMethod postfix = null)
        { if (original == null || postfix == null) throw new Exception("missing patch"); Active.Add(id); }
        public void UnpatchSelf() { Active.Remove(id); }
    }
}

#endif

public sealed class Character
{
    public static Character localCharacter, observedCharacter;
    public CharacterData data = new CharacterData();
    public CharacterRefs refs = new CharacterRefs();
}
public sealed class CharacterData { public Item currentItem; }
public sealed class CharacterRefs { public CharacterItems items = new CharacterItems(); }
public sealed class CharacterItems { public OptionalSlot currentSelectedSlot = new OptionalSlot(); }
public sealed class OptionalSlot { public bool IsSome; public byte Value; }
public sealed class Item
{
    public MagicBugle bugle;
    public object GetComponent(Type type) { return type == typeof(MagicBugle) ? bugle : null; }
}
public sealed class MagicBugle { public float currentFuel { get; set; } public float totalTootTime; }
public sealed class GUIManager { public InventoryItemUI[] items; [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)] public void UpdateItems() { } }
public sealed class InventoryItemUI { public FuelBar fuelBar = new FuelBar(); public FuelFill fuelBarFill = new FuelFill(); }
public sealed class FuelBar { public bool active; public void SetActive(bool value) { active = value; } }
public sealed class FuelFill { public float fillAmount { get; set; } }
