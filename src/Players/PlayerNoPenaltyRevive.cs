using System;

namespace PeakAdminToolkit.Players
{
    internal sealed class PlayerNoPenaltyRevive
    {
        private readonly PlayerDirectory directory;
        private readonly PlayerRevive revive;
        internal bool Available { get { return revive.Available; } }
        internal string FailureReason { get { return revive.FailureReason; } }

        internal PlayerNoPenaltyRevive(PlayerDirectory directory, IPlayerLanding landing = null)
        {
            this.directory = directory;
            revive = new PlayerRevive(directory, landing);
        }

        internal bool CanRequest(PlayerEntry entry) { return revive.CanRequest(entry); }

        internal bool Request(PlayerEntry entry)
        {
            if (!CanRequest(entry)) return false;
            try
            {
                object position;
                return revive.TryPosition(entry, out position) && directory.Send(entry.Character, "RPCA_ReviveAtPosition", position, false, -1);
            }
            catch (Exception) { return false; }
        }
    }
}
