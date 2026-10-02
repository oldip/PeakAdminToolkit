using System;
using System.Reflection;
using HarmonyLib;

namespace PeakAdminToolkit.Players
{
    internal sealed class DroppedItemCapture : IDisposable
    {
        private const BindingFlags Members = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        [ThreadStatic] private static object currentOwner;
        private readonly Harmony harmony = new Harmony("com.oldip.peakadmintoolkit.droppeditemcapture");
        private readonly DroppedItemHistory history;
        private readonly Action<string> log;
        private static DroppedItemCapture current;
        internal bool Available { get; private set; }

        internal DroppedItemCapture(DroppedItemHistory history, Action<string> log)
        {
            this.history = history;
            this.log = log;
            try
            {
                Type items = FindType("CharacterItems"), network = FindType("Photon.Pun.PhotonNetwork");
                MethodInfo drop = items == null ? null : items.GetMethod("DropItemRpc", Members);
                MethodInfo instantiate = network == null ? null : network.GetMethod("InstantiateItemRoom", Members);
                if (drop == null || instantiate == null || items.GetField("character", Members) == null)
                    throw new MissingMemberException("Native hand-drop capture API");
                harmony.Patch(drop, new HarmonyMethod(typeof(DroppedItemCapture).GetMethod("DropPrefix", Members)),
                    new HarmonyMethod(typeof(DroppedItemCapture).GetMethod("DropPostfix", Members)));
                harmony.Patch(instantiate, null, new HarmonyMethod(typeof(DroppedItemCapture).GetMethod("InstantiatePostfix", Members)));
                current = this;
                Available = true;
            }
            catch (Exception ex) { Fail(ex); }
        }

        private static void DropPrefix(object __instance, out object __state)
        {
            __state = currentOwner;
            try { currentOwner = __instance.GetType().GetField("character", Members).GetValue(__instance); }
            catch (Exception) { currentOwner = null; }
        }

        private static void DropPostfix(object __state) { currentOwner = __state; }

        private static void InstantiatePostfix(object __result)
        {
            DroppedItemCapture capture = current;
            if (capture == null || !capture.Available || currentOwner == null || __result == null) return;
            try
            {
                Type viewType = FindType("Photon.Pun.PhotonView");
                MethodInfo get = __result.GetType().GetMethod("GetComponent", new[] { typeof(Type) });
                object view = viewType == null || get == null ? null : get.Invoke(__result, new object[] { viewType });
                capture.history.Record(currentOwner, view);
            }
            catch (Exception ex) { capture.Fail(ex); }
        }

        private static Type FindType(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(name, false);
                if (type != null) return type;
            }
            return null;
        }

        private void Fail(Exception ex)
        {
            Available = false;
            if (ReferenceEquals(current, this)) current = null;
            try { harmony.UnpatchSelf(); } catch (Exception) { }
            if (log != null) log("Hand-drop capture unavailable: " + ex.Message);
        }

        public void Dispose()
        {
            Available = false;
            if (ReferenceEquals(current, this)) current = null;
            harmony.UnpatchSelf();
        }
    }
}
