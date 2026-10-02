using System;
using UnityEngine;
using PeakAdminToolkit.Players;

namespace PeakAdminToolkit.UI
{
    internal sealed class UnityPlayerLanding : IPlayerLanding
    {
        private readonly Action<string> log;
        internal UnityPlayerLanding(Action<string> log = null) { this.log = log; }

        public bool TryNear(object anchor, out object position)
        {
            position = null;
            try
            {
                Vector3 center = (Vector3)PlayerDirectory.ReadMember(anchor, "Center");
                int mask = HelperFunctions.terrainMapMask;
                int noGround = 0, rejectedGround = 0, blocked = 0;
                for (int i = 0; i < 16; i++)
                {
                    float angle = (i % 8) * Mathf.PI / 4f;
                    float radius = i < 8 ? 1.7f : 2.4f;
                    Vector3 probe = center + new Vector3(Mathf.Cos(angle) * radius, 2f, Mathf.Sin(angle) * radius);
                    RaycastHit hit;
                    if (!Physics.Raycast(probe, Vector3.down, out hit, 6f, mask, QueryTriggerInteraction.Ignore)) { noGround++; continue; }
                    if (hit.normal.y < 0.7f || Mathf.Abs(hit.point.y - center.y) > 3.5f) { rejectedGround++; continue; }
                    Vector3 feet = hit.point + Vector3.up * 0.45f;
                    Collider[] overlaps = Physics.OverlapCapsule(feet, feet + Vector3.up * 1.2f, 0.32f, mask, QueryTriggerInteraction.Ignore);
                    if (!LandingClearance.CanStand(overlaps, hit.collider)) { blocked++; continue; }
                    position = hit.point + Vector3.up * 1.05f;
                    return true;
                }
                if (log != null) log("No adjacent landing: mask=" + mask + ", noGround=" + noGround + ", rejectedGround=" + rejectedGround + ", blocked=" + blocked);
            }
            catch (Exception ex) { if (log != null) log("Adjacent landing error: " + ex.Message); }
            return false;
        }

        public bool TryInFront(object anchor, out object position)
        {
            position = null;
            try
            {
                Vector3 center = (Vector3)PlayerDirectory.ReadMember(anchor, "Center");
                object transform = PlayerDirectory.ReadMember(anchor, "transform");
                Vector3 forward = (Vector3)PlayerDirectory.ReadMember(transform, "forward");
                forward.y = 0f;
                if (forward.sqrMagnitude < 0.01f) return false;
                position = center + forward.normalized * 2f;
                return true;
            }
            catch (Exception ex) { if (log != null) log("Forward revival fallback unavailable: " + ex.Message); return false; }
        }
    }
}
