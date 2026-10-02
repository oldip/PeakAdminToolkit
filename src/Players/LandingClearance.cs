using System.Collections;

namespace PeakAdminToolkit.Players
{
    internal static class LandingClearance
    {
        internal static bool CanStand(IEnumerable overlaps, object supportingGround)
        {
            foreach (object obstacle in overlaps)
                if (!object.Equals(obstacle, supportingGround)) return false;
            return true;
        }
    }
}
