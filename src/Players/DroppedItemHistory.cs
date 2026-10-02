using System;
using System.Collections.Generic;

namespace PeakAdminToolkit.Players
{
    internal sealed class DroppedItemHistory
    {
        private readonly Dictionary<object, List<object>> views = new Dictionary<object, List<object>>();

        internal void Record(object character, object view)
        {
            if (character == null || view == null) return;
            List<object> owned;
            if (!views.TryGetValue(character, out owned)) views[character] = owned = new List<object>();
            foreach (object previous in owned) if (ReferenceEquals(previous, view)) return;
            owned.Add(view);
        }

        internal IEnumerable<object> Read(object character)
        {
            List<object> owned;
            if (character != null && views.TryGetValue(character, out owned)) return new List<object>(owned);
            return new object[0];
        }

        internal void Clear() { views.Clear(); }
    }
}
