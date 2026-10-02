using System;
using UnityEngine;
using PeakAdminToolkit.UI;

internal static class HoverTests
{
    private static int checks;
    private static readonly Rect Viewport = new Rect(20, 180, 1168, 300);
    public static int Run()
    {
        Equal(0, ItemGridLayout.HitTest(Viewport, new Vector2(0, 0), new Vector2(30, 190), 20), "first visible card is hovered");
        Equal(-1, ItemGridLayout.HitTest(Viewport, new Vector2(0, 0), new Vector2(30, 170), 20), "header is not a card");
        Equal(-1, ItemGridLayout.HitTest(Viewport, new Vector2(0, 0), new Vector2(1170, 190), 20), "scrollbar is not a card");
        Equal(-1, ItemGridLayout.HitTest(Viewport, new Vector2(0, 0), new Vector2(30, 480), 20), "clipped card below viewport cannot hover");
        Equal(-1, ItemGridLayout.HitTest(Viewport, new Vector2(0, 0), new Vector2(180, 190), 20), "space between columns clears hover");
        Equal(-1, ItemGridLayout.HitTest(Viewport, new Vector2(0, 0), new Vector2(30, 325), 20), "space between rows clears hover");
        Equal(-1, ItemGridLayout.HitTest(Viewport, new Vector2(0, 0), new Vector2(200, 340), 8), "empty final-row cells are not targets");
        Equal(7, ItemGridLayout.HitTest(Viewport, new Vector2(0, 150), new Vector2(30, 190), 20), "scroll offset selects the newly visible card");
        Equal(-1, ItemGridLayout.HitTest(Viewport, new Vector2(0, 0), new Vector2(30, 190), 0), "empty search results cannot retain a target");

        var tip = new Tooltip();
        Equal(null, ObserveAt(tip, new Vector2(30, 190), 0), "card begins one-second delay");
        Equal("Description 0", ObserveAt(tip, new Vector2(30, 190), 1), "continuous hover reveals details");
        Equal(null, ObserveAt(tip, new Vector2(30, 170), 1.1), "moving onto header immediately clears old details");
        Equal(null, ObserveAt(tip, new Vector2(30, 170), 3), "old details do not reappear after waiting on header");
        Equal(null, ObserveAt(tip, new Vector2(30, 190), 4), "returning to card starts a new delay");
        Equal(null, tip.Observe(null, null, 4.5), "overview page with no card clears target");
        Equal(null, ObserveAt(tip, new Vector2(30, 190), 5), "returning from overview starts a new delay");
        Equal("Description 0", ObserveAt(tip, new Vector2(30, 190), 6), "details appear after the new delay");
        tip.Clear();
        Equal(null, tip.Observe(null, null, 8), "reopening away from a card does not reuse old target");
        Equal(null, tip.Observe("item-a", "Same description", 9), "new item starts delay");
        Equal("Same description", tip.Observe("item-a", "Same description", 10), "first identity reaches delay");
        Equal(null, tip.Observe("item-b", "Same description", 10.1), "different item with same text restarts delay");
        tip.Clear();
        Equal(null, tip.Observe("item-b", "Same description", 12), "scroll or filter reset restarts delay on same identity");

        Rect bounds = Tooltip.Bounds(new Vector2(1180, 780), 380, 180, 1200, 800);
        Equal(true, bounds.x >= 0 && bounds.y >= 0 && bounds.x + bounds.width <= 1200 && bounds.y + bounds.height <= 800,
            "details box stays inside the window near bottom-right corner");
        Equal(true, bounds.height == 180 && bounds.x + bounds.width < 1180, "details height is preserved and box flips away from pointer");
        Rect tall = Tooltip.Bounds(new Vector2(30, 30), 380, 2000, 1200, 800);
        Equal(true, tall.height <= 776 && tall.y + tall.height <= 800, "oversized details stay within window bounds");
        return checks;
    }
    private static string ObserveAt(Tooltip tip, Vector2 pointer, double time)
    {
        int index = ItemGridLayout.HitTest(Viewport, new Vector2(0, 0), pointer, 20);
        return tip.Observe(index < 0 ? null : "item-" + index, index < 0 ? null : "Description " + index, time);
    }
    private static void Equal<T>(T expected, T actual, string name)
    {
        checks++;
        if (!object.Equals(expected, actual)) { Console.WriteLine("FAIL: " + name); Environment.Exit(1); }
    }
}
