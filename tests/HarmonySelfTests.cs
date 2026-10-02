using System;
using System.Reflection;
using PeakAdminToolkit.Self;
using UnityEngine;

internal static class HarmonySelfTests
{
    private static int checks;
    private static void Main()
    {
        try { Run(); Console.WriteLine("PASS: " + checks + " real Harmony self-tool hook checks (game-shaped doubles)."); }
        catch (Exception ex) { Console.WriteLine("FAIL: " + ex); Environment.ExitCode = 1; }
    }
    private static void Run()
    {
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(HarmonyLib.AccessTools).TypeHandle);
        var local = new FakeCharacter(); var remote = new FakeCharacter(); FakeCharacter.localCharacter = local;
        using (var tools = new SelfTools(new SelfApi(n => n == "STATUSTYPE" ? null : Resolve(n)), Console.WriteLine, () => false))
        {
            foreach (SelfFeature feature in tools.Features) Check(feature.Available, "real Harmony binds each feature");
            Check(tools.God.SetEnabled(true), "enable God hook");
            Check(!local.refs.afflictions.AddStatus(STATUSTYPE.Injury, 0.1f, false, true, true, false, false), "enum boxing and bool result on actual patch");
            Check(remote.refs.afflictions.AddStatus(STATUSTYPE.Injury, 0.1f, false, true, true, false, false), "remote original runs");
            Check(local.refs.afflictions.AddStatus(STATUSTYPE.Injury, -0.1f, false, true, true, false, false), "healing original runs");
            tools.God.SetEnabled(false);
            Check(local.refs.afflictions.AddStatus(STATUSTYPE.Injury, 0.1f, false, true, true, false, false), "God disabled restores method behavior");
            tools.Fall.SetEnabled(true);
            Invoke(local.refs.movement, "CheckFallDamage"); Invoke(remote.refs.movement, "CheckFallDamage");
            Check(local.refs.movement.FallCalls == 0 && remote.refs.movement.FallCalls == 1, "actual fall prefix ownership");
            tools.Flight.SetEnabled(true);
            var gravity = (Vector3)Invoke(local.refs.movement, "GetGravityForce");
            Check(gravity.magnitude == 0, "actual struct return postfix");
            Check(!(bool)Invoke(local.refs.movement, "TryJetpack"), "flight suppresses local jetpack");
            Check((bool)Invoke(remote.refs.movement, "TryJetpack"), "remote jetpack unaffected");
            Input.Held.Add(KeyCode.W); Invoke(local.refs.movement, "FixedUpdate");
            Check(local.refs.ragdoll.partList[0].Rig.linearVelocity.z == 8, "actual fixed update postfix");
            tools.Flight.SetEnabled(false);
            Check(((Vector3)Invoke(local.refs.movement, "GetGravityForce")).y == -10, "gravity result restored");
            Check((bool)Invoke(local.refs.movement, "TryJetpack"), "jetpack resumes after flight");
            tools.God.SetEnabled(true); tools.Stamina.SetEnabled(true); tools.Flight.SetEnabled(true);
            local.data.currentStamina = 0.9f;
            local.AddStamina(0.2f);
            Check(Math.Abs(local.data.currentStamina - 1f) < 0.001f, "actual stamina hook caps positive gain");
            for (int i = 0; i < 1000; i++) local.AddStamina(0.2f);
            Check(Math.Abs(local.data.currentStamina - 1f) < 0.001f, "repeated regeneration never accumulates");
        }
        Check(!local.infiniteStam && local.refs.ragdoll.partList[0].Rig.useGravity, "dispose restores active flags and gravity");
        Check(local.refs.afflictions.AddStatus(STATUSTYPE.Injury, 1, false, true, true, false, false), "dispose unpatches status");
        Invoke(local.refs.movement, "CheckFallDamage");
        Check(local.refs.movement.FallCalls == 1, "dispose unpatches fall");
        Check(((Vector3)Invoke(local.refs.movement, "GetGravityForce")).y == -10, "dispose unpatches flight");
    }
    private static object Invoke(object instance, string name) { return instance.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(instance, null); }
    private static Type Resolve(string name)
    {
        switch (name)
        {
            case "Character": return typeof(FakeCharacter);
            case "CharacterData": return typeof(FakeData);
            case "CharacterAfflictions": return typeof(FakeAfflictions);
            case "CharacterMovement": return typeof(FakeMovement);
            case "Bodypart": return typeof(FakeBodypart);
            case "STATUSTYPE": return typeof(STATUSTYPE);
            default: return null;
        }
    }
    private static void Check(bool value, string name) { checks++; if (!value) throw new Exception(name); }
}
