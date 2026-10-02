using System;
using System.Reflection;

namespace PeakAdminToolkit.Players
{
    internal sealed class PlayerRevive
    {
        private readonly PlayerDirectory directory;
        private readonly IPlayerLanding landing;
        internal bool Available { get; private set; }
        internal string FailureReason { get; private set; }
        internal PlayerRevive(PlayerDirectory directory, IPlayerLanding landing = null)
        {
            this.directory = directory;
            this.landing = landing;
            try
            {
                Type character = directory.CharacterType;
                if (character == null) throw new MissingMemberException("Character");
                PropertyInfo location = character.GetProperty("LastLivingPosition");
                if (location == null) throw new MissingMemberException("Character", "LastLivingPosition");
                Type position = location.PropertyType;
                MethodInfo method = character.GetMethod("RPCA_ReviveAtPosition", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                    null, new[] { position, typeof(bool), typeof(int) }, null);
                if (method == null || method.ReturnType != typeof(void)) throw new MissingMethodException("Character", "RPCA_ReviveAtPosition");
                Available = true;
            }
            catch (Exception ex) { Available = false; FailureReason = ex.GetType().Name + ": " + ex.Message; }
        }
        internal bool CanRequest(PlayerEntry entry)
        {
            try { return Available && directory.CanRequestPlayerRpc && directory.IsCurrent(entry) && directory.CanSend(entry.Character) && directory.Dead(entry.Character); }
            catch (Exception) { return false; }
        }
        internal bool Request(PlayerEntry entry)
        {
            if (!CanRequest(entry)) return false;
            try
            {
                object position;
                if (!TryPosition(entry, out position)) return false;
                return directory.Send(entry.Character, "RPCA_ReviveAtPosition", position, true, -1);
            }
            catch (Exception) { return false; }
        }

        internal bool TryPosition(PlayerEntry entry, out object position)
        {
            position = null;
            object anchor = ReferenceEquals(entry.Character, directory.Local())
                ? directory.SpectatedLivingTeammate() : directory.Local();
            if (anchor == null)
            {
                position = directory.Position(entry.Character, "LastLivingPosition");
                return position != null;
            }
            return landing != null && (landing.TryNear(anchor, out position) || landing.TryInFront(anchor, out position));
        }
    }
}
