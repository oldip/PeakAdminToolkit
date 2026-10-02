using System;
using System.Collections.Generic;
using PeakAdminToolkit.Self;
using UnityEngine;

internal static class SelfToolTests
{
    private static int checks;
    private static void Main()
    {
        try { Run(); Console.WriteLine("PASS: " + checks + " self-tool checks."); }
        catch (Exception ex) { Console.WriteLine("FAIL: " + ex); Environment.ExitCode = 1; }
    }

    private static void Run()
    {
        foreach (string status in new[] { "Injury", "Hunger", "Cold", "Poison", "Crab", "Curse", "Drowsy", "Hot", "Thorns", "Spores", "Web", "Arrow", "Petrify", "FlyTrap" })
            Check(GodMode.Blocks(status, 0.1f, false), "God Mode blocks new " + status);
        Check(!GodMode.Blocks("Weight", 1, false), "weight still works");
        Check(!GodMode.Blocks("FutureStatus", 1, false), "unknown status keeps game behavior");
        Check(!GodMode.Blocks("Injury", -1, false), "healing is not blocked");
        Check(!GodMode.Blocks("Injury", 0, false), "zero unchanged");
        Check(!GodMode.Blocks("Injury", 1, true), "explicit scripted bypass retained");
        Vector3 forward = new Vector3(0, 0, 1), right = new Vector3(1, 0, 0);
        Near(FlightMotion.Velocity(forward, right, 0, 1, 0, 8, false, false).z, 8, "forward speed");
        Near(FlightMotion.Velocity(forward, right, 1, 1, 1, 8, false, false).magnitude, 8, "diagonal normalized");
        Near(FlightMotion.Velocity(forward, right, 0, 0, -1, 8, true, false).y, -16, "descend and boost");
        Near(FlightMotion.Velocity(forward, right, 1, 1, 1, 8, true, true).magnitude, 0, "blocked input holds");
        Near(FlightMotion.ClampSpeed(-1), 2, "min speed");
        Near(FlightMotion.ClampSpeed(100), 20, "max speed");
        Near(FlightMotion.ClampSpeed(float.NaN), 8, "NaN speed rejected");

        var log = new List<string>();
        var api = new SelfApi(Resolve);
        FakeCharacter.localCharacter = null;
        using (var tools = new SelfTools(api, log.Add, () => false))
        {
            foreach (SelfFeature feature in tools.Features)
            {
                Check(feature.Available, "each feature API resolved");
                Check(!feature.Enabled, "default off");
                Check(!feature.SetEnabled(true), "no character rejected");
            }
            var local = new FakeCharacter(); FakeCharacter.localCharacter = local;
            var remote = new FakeCharacter();
            Check(tools.God.SetEnabled(true), "God enabled");
            bool result = true;
            Check(!GodMode.Prefix(local.refs.afflictions, STATUSTYPE.Injury, 0.2f, false, ref result) && !result, "local positive status skipped");
            Check(GodMode.Prefix(remote.refs.afflictions, STATUSTYPE.Injury, 0.2f, false, ref result), "remote status untouched");
            Check(GodMode.Prefix(local.refs.afflictions, STATUSTYPE.Injury, -0.2f, false, ref result), "local healing allowed");
            Near(local.refs.afflictions.existingInjury, 0.3f, "existing injury preserved");
            Check(tools.Stamina.SetEnabled(true) && local.infiniteStam, "native stamina flag enabled");
            Near(local.data.currentStamina, 0.4f, "no stamina refill");
            local.data.currentStamina = 0.9f;
            float gain = 0.2f;
            InfiniteStamina.AddPrefix(local, ref gain);
            local.AddStamina(gain);
            Near(local.data.currentStamina, 1f, "stamina gain stops at native maximum");
            tools.Stamina.SetEnabled(false);
            local.maxStamina = 0.65f; local.data.currentStamina = 0.6f;
            Check(tools.Stamina.SetEnabled(true), "stamina available below reduced maximum");
            gain = 0.2f; InfiniteStamina.AddPrefix(local, ref gain); local.AddStamina(gain);
            Near(local.data.currentStamina, 0.65f, "stamina gain respects reduced native maximum");
            local.maxStamina = 0.5f;
            gain = 0f; InfiniteStamina.AddPrefix(local, ref gain); local.AddStamina(gain);
            Near(local.data.currentStamina, 0.5f, "later maximum reduction clears excess");
            Check(tools.Stamina.SetEnabled(false) && !local.infiniteStam, "native stamina flag restored false");
            local.maxStamina = 0.65f;
            local.data.currentStamina = 1.4f;
            Check(tools.Stamina.SetEnabled(true), "stamina recovers from prior excess");
            Near(local.data.currentStamina, 0.65f, "prior excess clamps once on enable");
            tools.Stamina.SetEnabled(false);
            local.maxStamina = 1f; local.data.currentStamina = 0.4f;
            local.infiniteStam = true;
            tools.Stamina.SetEnabled(true); tools.Stamina.SetEnabled(false);
            Check(local.infiniteStam, "pre-existing native flag restored true");
            local.infiniteStam = false;
            Check(tools.Fall.SetEnabled(true), "fall enabled");
            Check(!NoFallDamage.Prefix(local.refs.movement), "local fall check skipped");
            Check(NoFallDamage.Prefix(remote.refs.movement), "remote fall check retained");
            tools.Fall.SetEnabled(false);
            Time.unscaledTimeAsDouble = 10;

            Check(tools.Flight.SetEnabled(true), "flight enabled");
            Check(!NoFallDamage.Prefix(local.refs.movement), "flight protects local character from fall injury");
            Check(NoFallDamage.Prefix(remote.refs.movement), "flight does not protect remote character");
            Input.Held.Add(KeyCode.W);
            Flight.FixedPostfix(local.refs.movement);
            Near(local.refs.ragdoll.partList[0].Rig.linearVelocity.z, 8, "flight moves owned body");
            Check(!local.refs.ragdoll.partList[0].Rig.useGravity, "flight gravity disabled");
            Vector3 gravity = new Vector3(0, -10, 0);
            Flight.GravityPostfix(local.refs.movement, ref gravity);
            Near(gravity.magnitude, 0, "native gravity suppressed while flying");
            Check(!Flight.JumpPrefix(local.refs.movement), "flight ascent does not jump");
            Check(Flight.JumpPrefix(remote.refs.movement), "remote jump unaffected");
            tools.Flight.SetEnabled(false);
            Check(!NoFallDamage.Prefix(local.refs.movement), "flight exit protects immediately");
            Time.unscaledTimeAsDouble = 11.99;
            Check(!NoFallDamage.Prefix(local.refs.movement), "flight exit protects before two seconds");
            Time.unscaledTimeAsDouble = 12;
            Check(NoFallDamage.Prefix(local.refs.movement), "flight fall immunity expires after two seconds");
            Check(tools.Fall.SetEnabled(true) && !NoFallDamage.Prefix(local.refs.movement), "independent no-fall switch still protects after flight grace expires");
            tools.Fall.SetEnabled(false);
            Check(local.refs.ragdoll.partList[0].Rig.useGravity, "gravity true restored");
            Check(!local.refs.ragdoll.partList[1].Rig.useGravity, "gravity false restored");
            Near(local.refs.ragdoll.partList[0].Rig.linearVelocity.magnitude, 0, "flight velocity stopped");
            Check(Flight.JumpPrefix(local.refs.movement), "jump resumes");
            gravity = new Vector3(0, -10, 0); Flight.GravityPostfix(local.refs.movement, ref gravity);
            Near(gravity.y, -10, "native gravity resumes");
            Input.Held.Clear();
            Check(tools.Flight.SetEnabled(true), "flight reenabled for scene reset");
            Time.unscaledTimeAsDouble = 20;
            tools.Reset();
            Check(NoFallDamage.Prefix(local.refs.movement), "scene or plugin reset clears flight fall immunity");

            tools.Stamina.SetEnabled(true); tools.Flight.SetEnabled(true);
            local.refs.view.IsMine = false; tools.Poll();
            foreach (SelfFeature feature in tools.Features) Check(!feature.Enabled, "ownership loss resets feature");
            Check(!local.infiniteStam, "ownership loss restores original flag");
            Check(!tools.Stamina.SetEnabled(true), "remote-owned local pointer rejected");
            local.refs.view.IsMine = true;
            tools.Stamina.SetEnabled(true);
            FakeCharacter.localCharacter = remote; tools.Poll();
            Check(!local.infiniteStam && !tools.Stamina.Enabled && !remote.infiniteStam, "replacement restores old character without enabling new");
            FakeCharacter.localCharacter = local;
            local.data.fullyConscious = false;
            Check(!tools.God.SetEnabled(true), "dead/downed cannot enable");
            local.data.fullyConscious = true;
            local.data.isClimbingAnything = true;
            Check(!tools.Flight.SetEnabled(true), "climbing cannot enable flight");
            Check(tools.Flight.Available, "temporary ineligibility does not break capability");
            local.data.isClimbingAnything = false;
            tools.Flight.SetEnabled(true); local.data.isCarried = true; tools.Poll();
            Check(!tools.Flight.Enabled, "carried state exits flight");
            local.data.isCarried = false;
            local.refs.ragdoll.partList[0].Rig.isKinematic = true;
            Check(!tools.Flight.SetEnabled(true) && tools.Flight.Available, "temporary kinematic body rejects without disabling capability");
            local.refs.ragdoll.partList[0].Rig.isKinematic = false;
            tools.Flight.SetEnabled(true);
            local.refs.ragdoll.partList[0].Rig.Destroyed = true;
            tools.Poll();
            Check(!tools.Flight.Enabled && tools.Flight.Available, "destroyed body exits without disabling capability");
            Check(!local.refs.ragdoll.partList[1].Rig.useGravity, "surviving body restored after another body destroyed");
            local.refs.ragdoll.partList[0].Rig.Destroyed = false;
            foreach (SelfFeature feature in tools.Features) Check(feature.SetEnabled(true), "enable before lifecycle reset");
            tools.Reset(); tools.Reset();
            foreach (SelfFeature feature in tools.Features) Check(!feature.Enabled, "idempotent lifecycle reset");
            Check(!local.infiniteStam, "reset restores stamina flag");
        }
        Check(HarmonyLib.Harmony.ActiveOwners.Count == 0, "dispose unpatches own owners");

        // Live 0.3.0 reported ArgumentNullException("types") while binding God Mode.
        // The method's parameter types are available even when a separate status
        // type-name lookup is not, so this must not disable the feature.
        using (var unnamedStatus = new SelfTools(new SelfApi(n => n == "STATUSTYPE" ? null : Resolve(n)), log.Add, () => false))
        {
            Check(unnamedStatus.God.Available, "God Mode binds using AddStatus signature without status type-name lookup");
            Check(unnamedStatus.God.SetEnabled(true), "God Mode can enable after signature binding");
            Check(unnamedStatus.Stamina.Available && unnamedStatus.Fall.Available && unnamedStatus.Flight.Available,
                "unrelated self tools remain available");
        }

        using (var legacy = new SelfTools(new SelfApi(n => n == "CharacterAfflictions" ? typeof(LegacyAfflictions) : Resolve(n)), log.Add, () => false))
            Check(legacy.God.Available, "God Mode retains six-argument AddStatus compatibility");

        using (var broken = new SelfTools(new SelfApi(n => n == "CharacterMovement" ? typeof(MissingMovement) : Resolve(n)), log.Add, () => false))
        {
            Check(!broken.Fall.Available && !broken.Flight.Available, "missing movement APIs disable only affected tools");
            var reasonProperty = typeof(SelfFeature).GetProperty("FailureReason", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Check(reasonProperty != null, "disabled module exposes its failure reason to the UI");
            string reason = (string)reasonProperty.GetValue(broken.Fall, null);
            Check(!string.IsNullOrEmpty(reason) && reason.Contains("CheckFallDamage"), "missing fall API names the unavailable member");
            Check(reasonProperty.GetValue(broken.God, null) == null, "healthy module has no incompatible API reason");
            Check(broken.God.Available && broken.Stamina.Available, "other features survive missing API");
            Check(broken.God.SetEnabled(true) && broken.Stamina.SetEnabled(true), "other features still usable");
        }
        using (var blocked = new SelfTools(new SelfApi(Resolve), log.Add, () => true))
        {
            blocked.Flight.SetEnabled(true); Input.Held.Add(KeyCode.W);
            Flight.FixedPostfix(FakeCharacter.localCharacter.refs.movement);
            Near(FakeCharacter.localCharacter.refs.ragdoll.partList[0].Rig.linearVelocity.magnitude, 0, "menu/focus block prevents movement");
            Input.Held.Clear();
        }
        Check(log.Count >= 2, "capability failure logged");
        int typeLookups = 0;
        var cached = new SelfApi(name => { typeLookups++; return typeof(FakeCharacter); });
        Check(cached.Type("Character") == typeof(FakeCharacter) && cached.Type("Character") == typeof(FakeCharacter) && typeLookups == 1,
            "repeated self-tool polls reuse resolved game types");
        var first = new FakeCharacter(); var second = new FakeCharacter();
        FakeCharacter.localCharacter = first;
        Check(ReferenceEquals(cached.Local(), first), "metadata cache reads current local character");
        FakeCharacter.localCharacter = second;
        Check(ReferenceEquals(cached.Local(), second) && typeLookups == 1, "metadata cache does not retain former character");
        second.refs.view.IsMine = false;
        Check(cached.Local() == null, "cached member read observes latest ownership");
        second.refs.view.IsMine = true; second.data.fullyConscious = false;
        Check(cached.Local() == null, "cached member read observes latest consciousness");
        bool loaded = false;
        var late = new SelfApi(name => loaded ? typeof(FakeCharacter) : null);
        Check(late.Type("Character") == null, "unloaded type remains unavailable");
        loaded = true;
        Check(late.Type("Character") == typeof(FakeCharacter), "unavailable types are retried after loading");
    }

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
    private static void Near(float actual, float expected, string name) { Check(Math.Abs(actual - expected) < 0.001, name); }
    private static void Check(bool value, string name) { checks++; if (!value) throw new Exception(name); }
}
