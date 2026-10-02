using System;
using System.Collections.Generic;

namespace PeakAdminToolkit.World
{
    internal static class WorldWarpBatch
    {
        internal static bool TryWarp<T>(IList<T> players, Func<T, bool> canSend, Func<int, object> landing,
            Func<T, object, bool> send)
        {
            if (players == null || players.Count == 0) return false;
            object[] positions = new object[players.Count];
            for (int i = 0; i < players.Count; i++)
            {
                if (!canSend(players[i])) return false;
                positions[i] = landing(i);
                if (positions[i] == null) return false;
            }
            for (int i = 0; i < players.Count; i++)
                if (!send(players[i], positions[i])) return false;
            return true;
        }
    }
}
