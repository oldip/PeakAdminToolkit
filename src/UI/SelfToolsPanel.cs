using System;
using UnityEngine;
using PeakAdminToolkit.Core;
using PeakAdminToolkit.Self;
using PeakAdminToolkit.Players;

namespace PeakAdminToolkit.UI
{
    internal sealed class SelfToolsPanel
    {
        private Vector2 scroll;
        private bool lastEnableFailed;
        private readonly PlayerDirectory directory = new PlayerDirectory();
        private readonly PlayerCleanse cleanse;

        internal SelfToolsPanel() { cleanse = new PlayerCleanse(directory); }

        internal void Draw(SelfTools tools, Theme theme, Localization text, Toast toast)
        {
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.ExpandHeight(true));
            GUILayout.Label(text.Text("SelfScope"), theme.Body);
            GUILayout.Space(12);
            Row(tools.God, "GodMode", "GodModeHelp", theme, text);
            Row(tools.Stamina, "InfiniteStamina", "InfiniteStaminaHelp", theme, text);
            Row(tools.Fall, "NoFallDamage", "NoFallDamageHelp", theme, text);
            Row(tools.Flight, "Flight", "FlightHelp", theme, text);
            if (tools.Flight.Available && !tools.Fall.Available) GUILayout.Label(text.Text("FlightFallUnavailable"), theme.Small);
            bool speedEnabled = GUI.enabled;
            GUI.enabled = speedEnabled && tools.Flight.Available;
            GUILayout.Label(text.Format("FlightSpeed", tools.Flight.Speed.ToString("0")), theme.Body);
            tools.Flight.Speed = GUILayout.HorizontalSlider(tools.Flight.Speed, 2, 20);
            GUI.enabled = speedEnabled;
            GUILayout.Label(text.Text("FlightControls"), theme.Small);
            GUILayout.Space(14);
            object local = directory.Local();
            var own = local == null ? null : new PlayerEntry(local, string.Empty);
            bool previous = GUI.enabled;
            GUI.enabled = previous && cleanse.CanRequest(own);
            if (GUILayout.Button(text.Text("PlayerCleanse"), theme.Button, GUILayout.Width(230)))
                toast.Show(text.Text(cleanse.Request(own) ? "PlayerCleanseDone" : "PlayerCleanseFailed"),
                    Time.unscaledTimeAsDouble, 3.0);
            GUI.enabled = previous;
            GUILayout.Label(text.Text("PlayerCleanseScope"), theme.Small);
            if (!cleanse.Available) GUILayout.Label(text.Format("SelfUnavailableReason", cleanse.FailureReason), theme.Small);
            GUILayout.Space(14);
            if (GUILayout.Button(text.Text("SelfReset"), theme.Button)) { tools.Reset(); lastEnableFailed = false; }
            if (lastEnableFailed) GUILayout.Label(text.Text("SelfCannotEnable"), theme.Body);
            GUILayout.Label(text.Text("SelfResetHelp"), theme.Small);
            GUILayout.EndScrollView();
        }

        private void Row(SelfFeature feature, string label, string help, Theme theme, Localization text)
        {
            GUILayout.BeginHorizontal();
            bool previous = GUI.enabled;
            GUI.enabled = previous && feature.Available;
            bool next = GUILayout.Toggle(feature.Enabled, text.Text(label), theme.Button, GUILayout.Width(230));
            GUI.enabled = previous;
            if (next != feature.Enabled) lastEnableFailed = !feature.SetEnabled(next);
            string details = feature.Available ? text.Text(help) :
                (string.IsNullOrEmpty(feature.FailureReason) ? text.Text("SelfUnavailable") : text.Format("SelfUnavailableReason", feature.FailureReason));
            GUILayout.Label(details, theme.Body);
            GUILayout.EndHorizontal();
            GUILayout.Space(14);
        }
    }
}
