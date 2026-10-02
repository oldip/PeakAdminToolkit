using UnityEngine;

namespace PeakAdminToolkit.World
{
    internal static class WorldVoidEndpoint
    {
        internal static Transform Find()
        {
            Peak.VoidBiome biome = Peak.VoidBiome.instance;
            if (!biome || !biome.isActive || biome.segment == null) return null;
            GameObject root = biome.segment.segmentParent;
            if (!root || !root.activeInHierarchy) return null;
            Transform target = null;
            foreach (Peak.PeakGatePortal portal in root.GetComponentsInChildren<Peak.PeakGatePortal>(true))
            {
                if (!portal || !portal.isActiveAndEnabled) continue;
                if (target) return null;
                target = portal.transform;
            }
            return target;
        }
    }
}
