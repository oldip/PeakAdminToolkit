using System;
using System.Collections.Generic;
using UnityEngine;
using PeakAdminToolkit.Core;
using PeakAdminToolkit.Players;

namespace PeakAdminToolkit.UI
{
    internal sealed class PlayerPanel
    {
        private readonly PlayerDirectory directory = new PlayerDirectory();
        private readonly PlayerRevive revive;
        private readonly PlayerNoPenaltyRevive cleanRevive;
        private readonly PlayerTeleport teleport;
        private readonly DroppedItemRecovery recovery;
        private object recoveryOwner;
        private readonly HashSet<int> recoverySelection = new HashSet<int>();
        private Vector2 scroll;

        internal PlayerPanel(DroppedItemHistory history, Action<string> log)
        {
            var landing = new UnityPlayerLanding(log);
            revive = new PlayerRevive(directory, landing);
            cleanRevive = new PlayerNoPenaltyRevive(directory, landing);
            teleport = new PlayerTeleport(directory, landing);
            recovery = new DroppedItemRecovery(directory, null, history);
        }

        internal void Draw(Theme theme, Localization text, Toast toast)
        {
            GUILayout.Label(text.Text("PlayerScope"), theme.Body);
            if (!revive.Available) GUILayout.Label(text.Text("PlayerRevive") + " / " + text.Text("PlayerNoPenaltyRevive") + " · " +
                text.Format("SelfUnavailableReason", revive.FailureReason), theme.Small);
            if (!teleport.Available) GUILayout.Label(text.Text("PlayerTeleportTo") + " / " + text.Text("PlayerBringHere") + " · " +
                text.Format("SelfUnavailableReason", teleport.FailureReason), theme.Small);
            if (!directory.CanRequestPlayerRpc) GUILayout.Label(text.Text("PlayerRoomOnly"), theme.Small);
            GUILayout.Label(text.Text("PlayerNoPenaltyScope"), theme.Small);
            GUILayout.Label(text.Text("RecoveryScope"), theme.Small);
            List<PlayerEntry> players = directory.Read();
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.ExpandHeight(true));
            if (players.Count == 0) GUILayout.Label(text.Text("PlayerNoPlayers"), theme.Body);
            foreach (PlayerEntry entry in players)
            {
                string status;
                try { status = text.Text(directory.Dead(entry.Character) ? "PlayerDead" :
                    directory.PassedOut(entry.Character) ? "PlayerPassedOut" : "PlayerAlive"); }
                catch (Exception) { continue; }
                bool self = ReferenceEquals(entry.Character, directory.Local());
                GUILayout.BeginVertical(theme.Card);
                GUILayout.Label(entry.Name + (self ? " (" + text.Text("PlayerSelf") + ")" : "") + " · " + status, theme.Body);
                GUILayout.BeginHorizontal();
                ActionButton(revive.CanRequest(entry), "PlayerRevive", () => revive.Request(entry), entry.Name, theme, text, toast);
                ActionButton(cleanRevive.CanRequest(entry), "PlayerNoPenaltyRevive", () => cleanRevive.Request(entry), entry.Name, theme, text, toast);
                GUILayout.EndHorizontal();
                if (!self)
                {
                    GUILayout.BeginHorizontal();
                    ActionButton(teleport.CanRequestTo(entry), "PlayerTeleportTo", () => teleport.RequestTo(entry), entry.Name, theme, text, toast);
                    ActionButton(teleport.CanRequestBring(entry), "PlayerBringHere", () => teleport.RequestBring(entry), entry.Name, theme, text, toast);
                    GUILayout.EndHorizontal();
                }
                if (GUILayout.Button(text.Text("RecoveryCandidates"), theme.Button))
                {
                    recoveryOwner = ReferenceEquals(recoveryOwner, entry.Character) ? null : entry.Character;
                    recoverySelection.Clear();
                }
                if (ReferenceEquals(recoveryOwner, entry.Character)) DrawRecovery(entry, theme, text, toast);
                GUILayout.EndVertical();
                GUILayout.Space(8);
            }
            GUILayout.EndScrollView();
        }

        private void DrawRecovery(PlayerEntry owner, Theme theme, Localization text, Toast toast)
        {
            List<DroppedItemEntry> candidates = recovery.Read(owner);
            if (candidates.Count == 0)
            {
                GUILayout.Label(text.Text(recovery.Available && directory.CanManage ? "RecoveryEmpty" : "RecoveryUnavailable"), theme.Small);
                return;
            }
            foreach (DroppedItemEntry candidate in candidates)
            {
                bool chosen = recoverySelection.Contains(candidate.ViewId);
                bool next = GUILayout.Toggle(chosen, candidate.Name, theme.Toggle);
                if (next) recoverySelection.Add(candidate.ViewId);
                else recoverySelection.Remove(candidate.ViewId);
            }
            bool previous = GUI.enabled;
            try
            {
                GUI.enabled = previous && recoverySelection.Count > 0;
                if (GUILayout.Button(text.Format("RecoveryMove", recoverySelection.Count), theme.Button))
                {
                    int submitted = recovery.MoveSelected(owner, recoverySelection);
                    recoverySelection.Clear();
                    toast.Show(text.Format("RecoverySubmitted", submitted), Time.unscaledTimeAsDouble, 3.0);
                }
            }
            finally { GUI.enabled = previous; }
        }

        private static void ActionButton(bool allowed, string label, Func<bool> request, string name,
            Theme theme, Localization text, Toast toast, string successKey = null, string failureKey = null)
        {
            bool previous = GUI.enabled;
            try
            {
                GUI.enabled = previous && allowed;
                if (GUILayout.Button(text.Text(label), theme.Button, GUILayout.Width(165)))
                    toast.Show(request() ? (successKey == null ? text.Format("PlayerRequestSubmitted", name) : text.Text(successKey))
                        : text.Text(failureKey ?? "PlayerRequestFailed"),
                        Time.unscaledTimeAsDouble, 3.0);
            }
            finally { GUI.enabled = previous; }
        }
    }
}
