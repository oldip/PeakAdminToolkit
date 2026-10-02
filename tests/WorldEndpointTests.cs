using System;
using PeakAdminToolkit.World;

internal static class WorldEndpointTests
{
    private static int checks;
    private static void Check(bool value, string name) { checks++; if (!value) throw new Exception(name); }
    private static void Main()
    {
        try
        {
            Peak.VoidBiome.instance = null;
            Check(WorldVoidEndpoint.Find() == null, "missing Void scene");
            var root = new UnityEngine.GameObject();
            var biome = new Peak.VoidBiome { isActive = true, segment = new MapSegment { segmentParent = root } };
            Peak.VoidBiome.instance = biome;
            Check(WorldVoidEndpoint.Find() == null, "missing portal");
            var portal = new Peak.PeakGatePortal();
            root.Portals = new[] { portal };
            Check(WorldVoidEndpoint.Find() == portal.transform, "active portal in current Void root");
            Check(root.IncludeInactive, "lookup includes then filters inactive children");
            portal.isActiveAndEnabled = false;
            Check(WorldVoidEndpoint.Find() == null, "disabled portal excluded");
            portal.isActiveAndEnabled = true;
            root.Portals = new[] { portal, new Peak.PeakGatePortal() };
            Check(WorldVoidEndpoint.Find() == null, "ambiguous active portals rejected");
            root.Portals[1].isActiveAndEnabled = false;
            Check(WorldVoidEndpoint.Find() == portal.transform, "inactive duplicate ignored");
            biome.isActive = false;
            Check(WorldVoidEndpoint.Find() == null, "inactive Void biome excluded");
            biome.isActive = true; root.activeInHierarchy = false;
            Check(WorldVoidEndpoint.Find() == null, "inactive root excluded");
            root.activeInHierarchy = true; biome.segment.segmentParent = null;
            Check(WorldVoidEndpoint.Find() == null, "missing segment root excluded");
            biome.segment = null;
            Check(WorldVoidEndpoint.Find() == null, "missing map segment excluded");
            Check(Peak.PeakGatePortal.Interactions == 0, "lookup does not interact with or finish portal");
            Console.WriteLine("PASS: " + checks + " Void endpoint landmark checks.");
        }
        catch (Exception ex) { Console.WriteLine("FAIL: " + ex); Environment.ExitCode = 1; }
    }
}

namespace UnityEngine
{
    public class Object { public static implicit operator bool(Object value) { return value != null; } }
    public class Transform : Object { }
    public class GameObject : Object
    {
        public bool activeInHierarchy = true;
        public Peak.PeakGatePortal[] Portals = new Peak.PeakGatePortal[0];
        public bool IncludeInactive;
        public T[] GetComponentsInChildren<T>(bool includeInactive) { IncludeInactive = includeInactive; return Portals as T[]; }
    }
}
public class MapSegment { public UnityEngine.GameObject segmentParent; }
namespace Peak
{
    public class VoidBiome : UnityEngine.Object
    {
        public static VoidBiome instance;
        public bool isActive;
        public MapSegment segment;
    }
    public class PeakGatePortal : UnityEngine.Object
    {
        public bool isActiveAndEnabled = true;
        public UnityEngine.Transform transform = new UnityEngine.Transform();
        public static int Interactions;
        public void Interact() { Interactions++; }
    }
}
