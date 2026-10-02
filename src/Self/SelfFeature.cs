using System;
using System.Reflection;
using HarmonyLib;

namespace PeakAdminToolkit.Self
{
    internal abstract class SelfFeature : IDisposable
    {
        protected readonly SelfApi Api;
        protected object Target;
        private readonly Harmony harmony;
        private readonly Action<string> log;
        private readonly string name;
        private bool disposed;
        internal bool Available { get; private set; }
        internal bool Enabled { get; private set; }
        internal string FailureReason { get; private set; }

        protected SelfFeature(SelfApi api, string name, Action<string> log)
        {
            Api = api; this.name = name; this.log = log;
            harmony = new Harmony("com.oldip.peakadmintoolkit.self." + name);
        }

        protected void Bind(Action bind)
        {
            try { bind(); Available = true; }
            catch (Exception ex) { Fail(ex); }
        }

        protected void Patch(MethodInfo original, string prefix, string postfix)
        {
            Type type = GetType();
            harmony.Patch(original,
                prefix == null ? null : new HarmonyMethod(type.GetMethod(prefix, BindingFlags.Static | BindingFlags.NonPublic)),
                postfix == null ? null : new HarmonyMethod(type.GetMethod(postfix, BindingFlags.Static | BindingFlags.NonPublic)));
        }

        internal bool SetEnabled(bool value)
        {
            if (!value) { Disable(); return true; }
            if (disposed || !Available) return false;
            if (Enabled) { Poll(); return Enabled; }
            try
            {
                object current = Api.Local();
                if (current == null || !CanEnable(current)) return false;
                Target = current;
                Attach();
                Enabled = true;
                return true;
            }
            catch (Exception ex) { Fail(ex); return false; }
        }

        internal void Poll()
        {
            if (!Enabled) return;
            try
            {
                if (!ReferenceEquals(Target, Api.Local()) || !CanEnable(Target)) Disable();
            }
            catch (Exception ex) { Fail(ex); }
        }

        protected bool Applies(object character)
        {
            if (!Enabled || !ReferenceEquals(character, Target)) return false;
            Poll();
            return Enabled;
        }

        protected virtual bool CanEnable(object character) { return true; }
        protected virtual void Attach() { }
        protected virtual void Detach() { }

        private void Disable()
        {
            Enabled = false;
            try { if (Target != null) Detach(); }
            catch (Exception ex) { Available = false; FailureReason = ex.GetType().Name + ": " + ex.Message; Report("restore failed: " + ex.Message); }
            finally { Target = null; }
        }

        protected void Fail(Exception ex)
        {
            Disable();
            Available = false;
            var invocation = ex as TargetInvocationException;
            if (invocation != null && invocation.InnerException != null) ex = invocation.InnerException;
            FailureReason = ex.GetType().Name + ": " + ex.Message;
            try { harmony.UnpatchSelf(); }
            catch (Exception unpatch) { Report("unpatch failed: " + unpatch.Message); }
            Report("unavailable: " + ex.Message);
        }

        private void Report(string message) { if (log != null) log("Self tool " + name + " " + message); }

        public void Dispose()
        {
            if (disposed) return;
            Disable();
            try { harmony.UnpatchSelf(); }
            catch (Exception ex) { Report("unpatch failed: " + ex.Message); }
            Available = false; disposed = true;
        }
    }
}
