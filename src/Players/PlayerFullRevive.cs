using System;
using System.Reflection;

namespace PeakAdminToolkit.Players
{
    internal sealed class PlayerFullRevive
    {
        private readonly PlayerDirectory directory;
        private readonly PlayerNoPenaltyRevive revive;
        private readonly PlayerCleanse cleanse;
        private readonly MethodInfo maximum;
        private readonly MethodInfo refill;
        private readonly MethodInfo wake;
        internal string FailureReason { get; private set; }
        internal bool Available { get { return cleanse.Available && maximum != null && refill != null; } }

        internal PlayerFullRevive(PlayerDirectory directory, IPlayerLanding landing = null, Func<string, Type> resolve = null)
        {
            this.directory = directory;
            revive = new PlayerNoPenaltyRevive(directory, landing);
            cleanse = new PlayerCleanse(directory, resolve);
            try
            {
                wake = directory.CharacterType.GetMethod("RPCA_UnPassOut", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null, Type.EmptyTypes, null);
                if (wake != null && wake.ReturnType != typeof(void)) wake = null;
                maximum = directory.CharacterType.GetMethod("GetMaxStamina", Type.EmptyTypes);
                refill = directory.CharacterType.GetMethod("AddStamina", new[] { typeof(float) });
                if (maximum == null || maximum.ReturnType != typeof(float)) throw new MissingMethodException("Character", "GetMaxStamina");
                if (refill == null || refill.ReturnType != typeof(void)) throw new MissingMethodException("Character", "AddStamina");
                FailureReason = cleanse.FailureReason;
            }
            catch (Exception ex) { maximum = null; refill = null; FailureReason = ex.GetType().Name + ": " + ex.Message; }
        }

        internal bool CanRequest(PlayerEntry entry)
        {
            try
            {
                if (!directory.CanRequestPlayerRpc || !directory.IsCurrent(entry) || !directory.CanSend(entry.Character)) return false;
                if (directory.Dead(entry.Character))
                {
                    if (!revive.CanRequest(entry)) return false;
                }
                else if (directory.PassedOut(entry.Character) && wake == null) return false;
                if (!ReferenceEquals(entry.Character, directory.Local()))
                {
                    float[] deltas;
                    return NativeReviveStatusRequest.TryPrepare(entry.Character, out deltas);
                }
                return Available && (bool)PlayerDirectory.ReadMember(PlayerDirectory.ReadMember(entry.Character, "photonView"), "IsMine");
            }
            catch (Exception) { return false; }
        }

        internal bool Request(PlayerEntry entry)
        {
            if (!CanRequest(entry)) return false;
            try
            {
                if (!ReferenceEquals(entry.Character, directory.Local()))
                {
                    float[] deltas;
                    if (!NativeReviveStatusRequest.TryPrepare(entry.Character, out deltas) || !PrepareState(entry)) return false;
                    return directory.Send(entry.Character, "RPC_ApplyStatusesFromFloatArray", (object)deltas);
                }
                if (!PrepareState(entry) || directory.Dead(entry.Character) || !cleanse.Request(entry)) return false;
                float amount = (float)maximum.Invoke(entry.Character, null);
                if (amount <= 0 || float.IsNaN(amount) || float.IsInfinity(amount)) return false;
                refill.Invoke(entry.Character, new object[] { amount });
                return true;
            }
            catch (Exception) { return false; }
        }

        private bool PrepareState(PlayerEntry entry)
        {
            if (directory.Dead(entry.Character)) return revive.Request(entry);
            return !directory.PassedOut(entry.Character) || directory.Send(entry.Character, "RPCA_UnPassOut");
        }
    }
}
