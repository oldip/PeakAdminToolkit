using UnityEngine;
using System;
namespace PeakAdminToolkit.Self
{
    internal static class FlightMotion
    {
        internal static float ClampSpeed(float speed) { return float.IsNaN(speed) || float.IsInfinity(speed) ? 8 : Math.Max(2, Math.Min(20, speed)); }
        internal static Vector3 Velocity(Vector3 forward, Vector3 right, float x, float z, float y, float speed, bool boost, bool blocked)
        {
            if (blocked) return Vector3.zero;
            forward.y = 0; right.y = 0;
            Vector3 direction = forward.normalized * z + right.normalized * x + Vector3.up * y;
            if (direction.magnitude > 1) direction = direction.normalized;
            return direction * (ClampSpeed(speed) * (boost ? 2 : 1));
        }
    }
}
