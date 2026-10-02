using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using PeakAdminToolkit.Players;

namespace PeakAdminToolkit.World
{
    internal static class WorldDestinationTeleport
    {
        internal static string LastFailure { get; private set; }

        internal static bool TryTeleport(string current, string next)
        {
            LastFailure = null;
            try
            {
                Transform target;
                float heightRange = 3.5f;
                if (current == "Void" && next == "Void")
                    target = WorldVoidEndpoint.Find();
                else
                {
                    PropertyInfo instanceProperty = typeof(MapHandler).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                    MapHandler map = instanceProperty == null ? null : instanceProperty.GetValue(null, null) as MapHandler;
                    int index = Array.IndexOf(new[] { "Beach", "Tropics", "Alpine", "Caldera", "TheKiln" }, current);
                    if (map == null || map.segments == null || index < 0 || index >= map.segments.Length)
                        return Fail("Map segment unavailable");
                    GameObject segment = map.segments[index].segmentParent;
                    if (!segment || !segment.activeInHierarchy) return Fail("Current segment is inactive");
                    target = FindUnlitCampfire(map.segments[index].segmentCampfire, next);
                    if (!target && current == "TheKiln")
                    {
                        target = FindSummitSequence();
                        heightRange = 20f;
                    }
                }
                if (!target) return Fail(current == "Void" ? "No unique active Void endpoint portal" : "No unlit next-area campfire or summit sequence landmark");
                Debug.Log("PEAK Admin Toolkit: destination " + current + " -> " + next + " landmark " + target.name + " at " + target.position);

                var directory = new PlayerDirectory();
                List<PlayerEntry> players = directory.Read();
                var positions = new List<Vector3>();
                bool requested = WorldWarpBatch.TryWarp(players, entry => directory.CanSend(entry.Character),
                    slot => FindLanding(target.position, heightRange, positions),
                    (entry, position) => directory.Send(entry.Character, "WarpPlayerRPC", position, false));
                if (!requested) return Fail("No safe ground or player warp request unavailable");
                Debug.Log("PEAK Admin Toolkit: team warp requested near " + target.name + " for " + players.Count + " player(s)");
                return true;
            }
            catch (Exception ex) { return Fail(ex.GetType().Name + ": " + ex.Message); }
        }

        private static Transform FindUnlitCampfire(GameObject root, string next)
        {
            if (!root || !root.activeInHierarchy) return null;
            Campfire campfire = root.GetComponentInChildren<Campfire>(true);
            return campfire && !campfire.Lit && campfire.advanceToSegment.ToString() == next ? campfire.transform : null;
        }

        private static Transform FindSummitSequence()
        {
            PropertyInfo instanceProperty = typeof(PeakHandler).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            PeakHandler handler = instanceProperty == null ? null : instanceProperty.GetValue(null, null) as PeakHandler;
            return handler && handler.peakSequence ? handler.peakSequence.transform : null;
        }

        private static object FindLanding(Vector3 landmark, float heightRange, List<Vector3> reserved)
        {
            int mask = HelperFunctions.terrainMapMask;
            for (int i = 0; i < 24; i++)
            {
                float angle = (i % 8) * Mathf.PI / 4f;
                float radius = 1.8f + (i / 8) * 1.2f;
                Vector3 probe = landmark + new Vector3(Mathf.Cos(angle) * radius, heightRange, Mathf.Sin(angle) * radius);
                RaycastHit hit;
                if (!Physics.Raycast(probe, Vector3.down, out hit, heightRange * 2f, mask, QueryTriggerInteraction.Ignore)) continue;
                if (hit.normal.y < 0.7f || Mathf.Abs(hit.point.y - landmark.y) > heightRange) continue;
                Vector3 feet = hit.point + Vector3.up * 0.45f;
                if (!LandingClearance.CanStand(
                    Physics.OverlapCapsule(feet, feet + Vector3.up * 1.2f, 0.32f, mask, QueryTriggerInteraction.Ignore), hit.collider)) continue;
                Vector3 point = hit.point + Vector3.up * 1.05f;
                bool occupied = false;
                foreach (Vector3 other in reserved)
                    if (Vector3.Distance(point, other) < 1.3f) { occupied = true; break; }
                if (occupied) continue;
                reserved.Add(point);
                return point;
            }
            return null;
        }

        private static bool Fail(string reason)
        {
            LastFailure = reason;
            Debug.LogWarning("PEAK Admin Toolkit: destination warp unavailable: " + reason);
            return false;
        }
    }
}
