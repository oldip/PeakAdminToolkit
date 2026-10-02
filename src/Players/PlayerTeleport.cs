using System;
using System.Reflection;

namespace PeakAdminToolkit.Players
{
    internal sealed class PlayerTeleport
    {
        private readonly PlayerDirectory directory;
        private readonly IPlayerLanding landing;
        internal bool Available { get; private set; }
        internal string FailureReason { get; private set; }
        internal PlayerTeleport(PlayerDirectory directory, IPlayerLanding landing = null)
        {
            this.directory = directory;
            this.landing = landing;
            try
            {
                Type character = directory.CharacterType;
                if (character == null) throw new MissingMemberException("Character");
                PropertyInfo location = character.GetProperty("Center");
                if (location == null) throw new MissingMemberException("Character", "Center");
                Type position = location.PropertyType;
                MethodInfo method = character.GetMethod("WarpPlayerRPC", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null, new[] { position, typeof(bool) }, null);
                if (method == null || method.ReturnType != typeof(void)) throw new MissingMethodException("Character", "WarpPlayerRPC");
                Available = true;
            }
            catch (Exception ex) { Available = false; FailureReason = ex.GetType().Name + ": " + ex.Message; }
        }
        internal bool CanRequestTo(PlayerEntry target) { return CanMove(target, true); }
        internal bool CanRequestBring(PlayerEntry target) { return CanMove(target, false); }
        private bool CanMove(PlayerEntry target, bool moveLocal)
        {
            try
            {
                object local = directory.Local();
                return Available && directory.CanRequestPlayerRpc && directory.IsCurrent(target) && !ReferenceEquals(target.Character, local) &&
                    directory.CanSend(moveLocal ? local : target.Character) &&
                    !directory.Dead(local) && !directory.PassedOut(local) &&
                    !directory.Dead(target.Character) && !directory.PassedOut(target.Character);
            }
            catch (Exception) { return false; }
        }
        internal bool RequestTo(PlayerEntry target)
        {
            if (!CanRequestTo(target)) return false;
            try { object position; return landing != null && landing.TryNear(target.Character, out position) && directory.Send(directory.Local(), "WarpPlayerRPC", position, false); }
            catch (Exception) { return false; }
        }
        internal bool RequestBring(PlayerEntry target)
        {
            if (!CanRequestBring(target)) return false;
            try { object position; return landing != null && landing.TryNear(directory.Local(), out position) && directory.Send(target.Character, "WarpPlayerRPC", position, false); }
            catch (Exception) { return false; }
        }
    }
}
