using System;
using UnityEngine;
using PeakAdminToolkit.UI;

internal static class CardLayoutTests
{
    private static int checks;
    public static int Run()
    {
        Check(ItemGridLayout.Columns(1144) == 7, "normal panel fits seven cards across");
        Rect first = ItemGridLayout.CardRect(0, 1144);
        Rect second = ItemGridLayout.CardRect(1, 1144);
        Rect nextRow = ItemGridLayout.CardRect(7, 1144);
        Check(second.x > first.x + first.width && second.y == first.y, "cards occupy separate columns in one row");
        Check(nextRow.x == first.x && nextRow.y > first.y + first.height, "eighth card wraps to a new row");
        Check(ItemGridLayout.ContentHeight(0, 1144) == 0, "empty results reserve no card rows");
        Check(ItemGridLayout.ContentHeight(7, 1144) == first.height, "full row has no trailing empty row");
        Check(ItemGridLayout.ContentHeight(8, 1144) == nextRow.y + nextRow.height, "partial final row is scrollable in full");
        Check(ItemGridLayout.Columns(100) == 1 && ItemGridLayout.CardRect(0, 100).width <= 100,
            "narrow panel falls back to one fitting column");
        foreach (int[] resolution in new[] { new[] {1920,1080}, new[] {2560,1440}, new[] {1280,720}, new[] {1024,768}, new[] {3440,1440}, new[] {3840,2160} })
        {
            Rect window = WindowBounds.Centered(resolution[0], resolution[1]);
            float width = window.width - 56;
            bool inside = true;
            for (int i = 0; i < 22; i++)
            {
                Rect card = ItemGridLayout.CardRect(i, width);
                inside &= card.x >= 0 && card.width > 0 && card.x + card.width <= width + 0.01f
                    && card.y + card.height <= ItemGridLayout.ContentHeight(22, width) + 0.01f;
            }
            Check(inside, "all cards fit the scroll content at " + resolution[0] + "x" + resolution[1]);
        }
        // The screenshot reproduces clipping in the old fixed 20-unit status rect.
        foreach (float measuredHeight in new[] { 24f, 32f, 40f })
        {
            Rect status = ItemGridLayout.StatusRect(first, measuredHeight);
            Check(status.height >= measuredHeight + 4, "status region includes measured font height plus vertical padding");
            Check(status.x >= first.x && status.x + status.width <= first.x + first.width,
                "status text remains inside card width");
            Check(status.y >= first.y && status.y + status.height <= first.y + 92,
                "status never overlaps the name region");
            Rect icon = ItemGridLayout.IconRect(first, status.height);
            Check(icon.width > 0 && icon.y + icon.height + 4 <= status.y,
                "icon fits above the measured status region with a gap");
        }
        Check(ItemGridLayout.StatusRect(first, 0).height == 0 && ItemGridLayout.IconRect(first, 0).height == 80,
            "ordinary cards retain the full-size icon without a status row");
        return checks;
    }
    private static void Check(bool value, string name)
    {
        checks++;
        if (!value) { Console.WriteLine("FAIL: " + name); Environment.Exit(1); }
    }
}
