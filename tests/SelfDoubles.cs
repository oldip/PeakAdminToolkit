using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnityEngine
{
    public class Object { public bool Destroyed; public static implicit operator bool(Object o) { return o != null && !o.Destroyed; } }
    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 zero { get { return new Vector3(); } }
        public static Vector3 up { get { return new Vector3(0, 1, 0); } }
        public float magnitude { get { return (float)Math.Sqrt(x*x+y*y+z*z); } }
        public Vector3 normalized { get { return magnitude > 0.00001f ? this * (1 / magnitude) : zero; } }
        public static Vector3 operator +(Vector3 a, Vector3 b) { return new Vector3(a.x+b.x,a.y+b.y,a.z+b.z); }
        public static Vector3 operator *(Vector3 a, float b) { return new Vector3(a.x*b,a.y*b,a.z*b); }
    }
    public class Rigidbody : Object { public Vector3 linearVelocity; public bool useGravity = true; public bool isKinematic; }
    public enum KeyCode { W, A, S, D, Space, LeftControl, RightControl, LeftShift, RightShift }
    public static class Input { public static readonly HashSet<KeyCode> Held = new HashSet<KeyCode>(); public static bool GetKey(KeyCode key) { return Held.Contains(key); } }
    public static class Time { public static double unscaledTimeAsDouble; }
}
#if !REAL_HARMONY
namespace HarmonyLib
{
    public class HarmonyMethod { public HarmonyMethod(MethodInfo method) { if (method == null) throw new Exception("missing patch method"); } }
    public class Harmony
    {
        public static readonly HashSet<string> ActiveOwners = new HashSet<string>();
        private readonly string id;
        public Harmony(string id) { this.id = id; }
        public void Patch(MethodBase original, HarmonyMethod prefix = null, HarmonyMethod postfix = null)
        { if (original == null) throw new Exception("missing original"); ActiveOwners.Add(id); }
        public void UnpatchSelf() { ActiveOwners.Remove(id); }
    }
}
#endif
public enum STATUSTYPE { Injury, Hunger, Cold, Poison, Crab, Curse, Drowsy, Weight, Hot, Thorns, Spores, Web, Arrow, Petrify, FlyTrap }
public sealed class FakeView { public bool IsMine { get; set; }
    public FakeView() { IsMine = true; } }
public sealed class FakeData
{
    public FakeCharacter character;
    public bool fullyConscious { get; set; }
    public FakeData() { fullyConscious = true; }
    public bool isClimbingAnything { get; set; }
    public bool isCarried, isKinecmatic;
    public object carrier;
    public float fallSeconds;
    private float stamina = 0.4f;
    public float currentStamina { get { return stamina; } set { if (character == null || !character.infiniteStam || value >= stamina) stamina = value; } }
    public UnityEngine.Vector3 lookDirection_Flat = new UnityEngine.Vector3(0, 0, 1);
    public UnityEngine.Vector3 lookDirection_Right = new UnityEngine.Vector3(1, 0, 0);
}
public sealed class FakeCharacter : UnityEngine.Object
{
    public static FakeCharacter localCharacter;
    public FakeData data = new FakeData();
    public FakeRefs refs;
    public bool infiniteStam { get; set; }
    public float maxStamina = 1f;
    public FakeCharacter() { data.character = this; refs = new FakeRefs(this); }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public void AddStamina(float add) { data.currentStamina += add; ClampStamina(); }
    public void ClampStamina() { data.currentStamina = Math.Max(0f, Math.Min(data.currentStamina, GetMaxStamina())); }
    public float GetMaxStamina() { return maxStamina; }
    private bool CanDoInput() { return true; }
}
public sealed class FakeRefs
{
    public FakeView view = new FakeView();
    public FakeAfflictions afflictions;
    public FakeMovement movement;
    public FakeRagdoll ragdoll = new FakeRagdoll();
    public FakeRefs(FakeCharacter c) { afflictions = new FakeAfflictions(c); movement = new FakeMovement(c); }
}
public sealed class FakeAfflictions
{
    public FakeCharacter character;
    public float existingInjury = 0.3f;
    public FakeAfflictions(FakeCharacter c) { character = c; }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public bool AddStatus(STATUSTYPE status, float amount, bool fromRPC, bool effects, bool notify, bool ignoreInvincibility, bool ignoreSkeleton) { return true; }
}
public sealed class FakeMovement
{
    public FakeCharacter character;
    public int FallCalls;
    public FakeMovement(FakeCharacter c) { character = c; }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private void CheckFallDamage() { FallCalls++; }
    private UnityEngine.Vector3 GetGravityForce() { return new UnityEngine.Vector3(0, -10, 0); }
    private float GetMovementForce() { return 1; }
    private void TryToJump() { }
    private bool TryJetpack() { return true; }
    private void FixedUpdate() { }
}
public sealed class MissingMovement { }
public sealed class FakeRagdoll
{
    public List<FakeBodypart> partList = new List<FakeBodypart> { new FakeBodypart(true), new FakeBodypart(false) };
}
public sealed class FakeBodypart
{
    public UnityEngine.Rigidbody Rig { get; private set; }
    public FakeBodypart(bool gravity) { Rig = new UnityEngine.Rigidbody { useGravity = gravity }; }
}

public sealed class LegacyAfflictions
{
    public FakeCharacter character;
    public bool AddStatus(STATUSTYPE status, float amount, bool fromRPC, bool effects, bool notify, bool ignoreInvincibility) { return true; }
}
