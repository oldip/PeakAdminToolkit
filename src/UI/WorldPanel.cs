using System;
using UnityEngine;
using PeakAdminToolkit.Core;
using PeakAdminToolkit.World;

namespace PeakAdminToolkit.UI
{
    internal sealed class WorldPanel
    {
        private readonly WorldTime time = new WorldTime();
        private readonly WorldAdvance advance = new WorldAdvance();

        internal void Draw(Theme theme, Localization text, Toast toast)
        {
            GUILayout.Label(text.Text("WorldTimeHelp"), theme.Body);
            int day; float hour;
            if (time.TryRead(out day, out hour))
            {
                int wholeHour = Mathf.FloorToInt(hour);
                int minute = Mathf.FloorToInt((hour - wholeHour) * 60f);
                GUILayout.Label(text.Format("WorldCurrentTime", wholeHour.ToString("00") + ":" + minute.ToString("00")), theme.Body);
                GUILayout.Label(text.Format("WorldCurrentDay", day), theme.Small);
            }
            GUILayout.BeginHorizontal();
            TimeButton(TimePreset.Morning, "WorldMorning", theme, text, toast);
            TimeButton(TimePreset.Noon, "WorldNoon", theme, text, toast);
            TimeButton(TimePreset.Evening, "WorldEvening", theme, text, toast);
            TimeButton(TimePreset.Midnight, "WorldMidnight", theme, text, toast);
            GUILayout.EndHorizontal();
            string timeReason = time.UnavailableReason();
            if (timeReason != null) GUILayout.Label(text.Text(timeReason), theme.Small);

            GUILayout.Space(22);
            GUILayout.Label(text.Text("WorldAdvanceHelp"), theme.Body);
            string current, next;
            if (advance.TryRead(out current, out next))
            {
                GUILayout.Label(text.Format("WorldCurrentSegment", text.Text("WorldSegment" + current)), theme.Body);
                if (current == "Void") GUILayout.Label(text.Text("WorldEndpointDestination"), theme.Body);
                else if (next != null) GUILayout.Label(text.Format("WorldNextSegment", text.Text("WorldSegment" + next)), theme.Body);
            }
            string advanceReason = advance.UnavailableReason();
            bool previous = GUI.enabled;
            GUI.enabled = previous && advanceReason == null;
            if (GUILayout.Button(text.Text(current == "Void" ? "WorldEndpointAdvance" : "WorldAdvance"), theme.Button, GUILayout.Width(340)))
            {
                toast.Show(text.Text(advance.Advance() ? (current == "Void" ? "WorldEndpointSubmitted" : next == "Peak" ? "WorldPeakSubmitted" : "WorldAdvanceSubmitted") :
                    (current == "Void" ? "WorldEndpointUnavailable" : next == "Peak" ? "WorldPeakLandingUnavailable" : "WorldAdvanceFailed")), Time.unscaledTimeAsDouble, 4.0);
            }
            GUI.enabled = previous;
            if (advanceReason != null) GUILayout.Label(text.Text(advanceReason), theme.Small);
            GUILayout.FlexibleSpace();
        }

        private void TimeButton(TimePreset preset, string label, Theme theme, Localization text, Toast toast)
        {
            bool previous = GUI.enabled;
            GUI.enabled = previous && time.CanSet();
            if (GUILayout.Button(text.Text(label), theme.Button))
                toast.Show(text.Text(time.Set(preset) ? "WorldTimeChanged" : "WorldTimeFailed"), Time.unscaledTimeAsDouble, 3.0);
            GUI.enabled = previous;
        }
    }
}
