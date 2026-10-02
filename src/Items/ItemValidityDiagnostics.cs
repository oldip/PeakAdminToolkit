using System;
using System.Collections.Generic;

namespace PeakAdminToolkit.Items
{
    // One read-only report per special catalog load; no state survives a refresh.
    internal sealed class ItemValidityDiagnostics
    {
        private readonly Action<string> log;
        private readonly HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> details = new List<string>();
        private int validCount, invalidCount, unknownCount;

        public ItemValidityDiagnostics(Action<string> log) { this.log = log; }

        public void Record(string spawnName, bool evaluated, bool valid, string reason)
        {
            if (SpecialItems.Classify(spawnName) == SpecialItemKind.None || !seen.Add(spawnName)) return;
            if (!evaluated)
            {
                unknownCount++;
                details.Add("unknown: " + spawnName + " (" + reason + ")");
            }
            else if (valid) validCount++;
            else
            {
                invalidCount++;
                details.Add("false: " + spawnName);
            }
        }

        public void Write(string failure = null)
        {
            try
            {
                log("[ItemValidity] status=" + (failure == null ? "complete" : "incomplete reason=" + failure)
                    + " registered=" + seen.Count + " true=" + validCount + " false=" + invalidCount + " unknown=" + unknownCount);
                foreach (string detail in details) log("[ItemValidity] " + detail);
            }
            catch (Exception) { /* Logging must not affect catalog access. */ }
        }
    }
}
