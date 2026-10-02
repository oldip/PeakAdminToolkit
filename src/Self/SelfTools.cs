using System;

namespace PeakAdminToolkit.Self
{
    internal sealed class SelfTools : IDisposable
    {
        internal readonly GodMode God;
        internal readonly InfiniteStamina Stamina;
        internal readonly NoFallDamage Fall;
        internal readonly Flight Flight;
        internal readonly SelfFeature[] Features;

        internal SelfTools(SelfApi api, Action<string> log, Func<bool> inputBlocked)
        {
            God = new GodMode(api, log);
            Stamina = new InfiniteStamina(api, log);
            Fall = new NoFallDamage(api, log);
            Flight = new Flight(api, log, inputBlocked);
            Features = new SelfFeature[] { God, Stamina, Fall, Flight };
        }
        internal void Poll() { foreach (SelfFeature feature in Features) feature.Poll(); }
        internal void Reset() { foreach (SelfFeature feature in Features) feature.SetEnabled(false); Flight.ClearFallProtection(); }
        public void Dispose() { foreach (SelfFeature feature in Features) feature.Dispose(); Flight.ClearFallProtection(); }
    }
}
