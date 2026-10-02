using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PeakAdminToolkit.Self
{
    internal sealed class Flight : SelfFeature
    {
        private static Flight instance;
        private readonly Func<bool> inputBlocked;
        private readonly List<Rigidbody> bodies = new List<Rigidbody>();
        private readonly List<bool> gravity = new List<bool>();
        private MethodInfo canDoInput;
        private object movement, data;
        private object fallProtectedTarget;
        private double fallProtectedUntil;
        private float speed = 8;
        internal float Speed { get { return speed; } set { speed = FlightMotion.ClampSpeed(value); } }

        internal Flight(SelfApi api, Action<string> log, Func<bool> inputBlocked) : base(api, "Flight", log)
        {
            instance = this; this.inputBlocked = inputBlocked;
            Bind(() =>
            {
                Type type = api.Type("CharacterMovement");
                canDoInput = SelfApi.Method(api.Type("Character"), "CanDoInput", typeof(bool));
                PropertyInfo rig = api.Type("Bodypart").GetProperty("Rig", SelfApi.Instance);
                if (rig == null || rig.PropertyType != typeof(Rigidbody)) throw new MissingMemberException("Bodypart.Rig");
                Patch(SelfApi.Method(type, "GetGravityForce", typeof(Vector3)), null, "GravityPostfix");
                Patch(SelfApi.Method(type, "GetMovementForce", typeof(float)), null, "MovementPostfix");
                Patch(SelfApi.Method(type, "TryToJump", typeof(void)), "JumpPrefix", null);
                Patch(SelfApi.Method(type, "TryJetpack", typeof(bool)), "JetpackPrefix", null);
                Patch(SelfApi.Method(type, "FixedUpdate", typeof(void)), null, "FixedPostfix");
            });
        }

        protected override bool CanEnable(object character)
        {
            object state = SelfApi.Read(character, "data");
            if ((bool)SelfApi.Read(state, "isClimbingAnything") || (bool)SelfApi.Read(state, "isCarried") ||
                (bool)SelfApi.Read(state, "isKinecmatic") || SelfApi.Alive(SelfApi.Read(state, "carrier"))) return false;
            if ((float)SelfApi.Read(state, "fallSeconds") > 0) return false;
            foreach (Rigidbody body in bodies) if (!body || body.isKinematic) return false;
            if (bodies.Count == 0)
            {
                var parts = (IEnumerable)SelfApi.Read(SelfApi.Read(SelfApi.Read(character, "refs"), "ragdoll"), "partList");
                int count = 0;
                foreach (object part in parts)
                {
                    Rigidbody body = SelfApi.Read(part, "Rig") as Rigidbody;
                    if (!body || body.isKinematic) return false;
                    count++;
                }
                if (count == 0) return false;
            }
            return true;
        }

        protected override void Attach()
        {
            object references = SelfApi.Read(Target, "refs");
            data = SelfApi.Read(Target, "data");
            movement = SelfApi.Read(references, "movement");
            var parts = (IEnumerable)SelfApi.Read(SelfApi.Read(references, "ragdoll"), "partList");
            foreach (object part in parts)
            {
                Rigidbody body = SelfApi.Read(part, "Rig") as Rigidbody;
                if (!body || body.isKinematic) throw new InvalidOperationException("Flight needs active dynamic bodyparts.");
                bodies.Add(body); gravity.Add(body.useGravity);
            }
            if (bodies.Count == 0) throw new InvalidOperationException("No bodyparts available for flight.");
        }

        protected override void Detach()
        {
            // Restore every captured body even if a different body has been destroyed.
            Exception failure = null;
            if (bodies.Count > 0 && Target != null)
            {
                fallProtectedTarget = Target;
                fallProtectedUntil = Time.unscaledTimeAsDouble + 2.0;
            }
            for (int i = 0; i < bodies.Count; i++)
            {
                try
                {
                    if (!bodies[i]) continue;
                    bodies[i].useGravity = gravity[i];
                    if (!bodies[i].isKinematic) bodies[i].linearVelocity = Vector3.zero;
                }
                catch (Exception ex) { failure = ex; }
            }
            bodies.Clear(); gravity.Clear(); movement = null; data = null;
            if (failure != null) throw failure;
        }

        internal void ClearFallProtection()
        {
            fallProtectedTarget = null;
            fallProtectedUntil = 0;
        }

        internal static bool ProtectsFromFall(object character)
        {
            Flight self = instance;
            if (self == null || character == null) return false;
            try
            {
                if (self.Enabled && self.Applies(character)) return true;
                return ReferenceEquals(character, self.fallProtectedTarget) &&
                    ReferenceEquals(character, self.Api.Local()) &&
                    Time.unscaledTimeAsDouble < self.fallProtectedUntil;
            }
            catch (Exception ex) { self.Fail(ex); return false; }
        }

        private bool Active(object component)
        {
            try { return ReferenceEquals(component, movement) && Applies(Target); }
            catch (Exception ex) { Fail(ex); return false; }
        }

        internal static void GravityPostfix(object __instance, ref Vector3 __result)
        { if (instance != null && instance.Active(__instance)) __result = Vector3.zero; }
        internal static void MovementPostfix(object __instance, ref float __result)
        { if (instance != null && instance.Active(__instance)) __result = 0; }
        internal static bool JumpPrefix(object __instance)
        { return instance == null || !instance.Active(__instance); }
        internal static bool JetpackPrefix(object __instance, ref bool __result)
        {
            if (instance == null || !instance.Active(__instance)) return true;
            __result = false; return false;
        }

        internal static void FixedPostfix(object __instance)
        {
            Flight self = instance;
            if (self == null || !self.Active(__instance)) return;
            try
            {
                bool blocked = (self.inputBlocked != null && self.inputBlocked()) || !(bool)self.canDoInput.Invoke(self.Target, null);
                float x = Axis(KeyCode.D, KeyCode.A), z = Axis(KeyCode.W, KeyCode.S);
                float y = (Input.GetKey(KeyCode.Space) ? 1 : 0) - (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ? 1 : 0);
                Vector3 velocity = FlightMotion.Velocity((Vector3)SelfApi.Read(self.data, "lookDirection_Flat"),
                    (Vector3)SelfApi.Read(self.data, "lookDirection_Right"), x, z, y, self.Speed,
                    Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift), blocked);
                foreach (Rigidbody body in self.bodies)
                {
                    body.useGravity = false;
                    body.linearVelocity = velocity;
                }
            }
            catch (Exception ex) { self.Fail(ex); }
        }
        private static int Axis(KeyCode positive, KeyCode negative)
        { return (Input.GetKey(positive) ? 1 : 0) - (Input.GetKey(negative) ? 1 : 0); }
    }
}
