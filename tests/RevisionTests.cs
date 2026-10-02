using System;
using UnityEngine;
using PeakAdminToolkit.UI;
using PeakAdminToolkit.Items;

internal static class RevisionTests
{
    private static int checks;
    public static int Run()
    {
        Near(1f, WindowBounds.ScaleFor(1920,1080), "1080p layout scale");
        Near(4f / 3f, WindowBounds.ScaleFor(2560,1440), "1440p layout scale");
        Near(4f / 3f, WindowBounds.ScaleFor(3440,1440), "ultrawide scaling follows height");
        foreach (int[] resolution in new[] { new[] {1920,1080}, new[] {2560,1440}, new[] {1280,720}, new[] {1024,768}, new[] {3440,1440}, new[] {3840,2160} })
        {
            float w=resolution[0], h=resolution[1];
            float s=WindowBounds.ScaleFor(w,h);
            Rect rect=WindowBounds.Centered(w,h);
            Check(rect.x >= 0 && rect.y >= 0 && (rect.x+rect.width)*s <= w+0.01f && (rect.y+rect.height)*s <= h+0.01f, "responsive window remains fully visible");
            Near(w/2, (rect.x+rect.width/2)*s, "horizontal center after scaling");
            Near(h/2, (rect.y+rect.height/2)*s, "vertical center after scaling");
        }
        Type classifier = typeof(ItemClassification);
        Check(!Has(Resolve(classifier, "Kinds", "Sunscreen", "None"), "Recovery"), "sunscreen is not a recovery item");
        Check(Has(Resolve(classifier, "Kinds", "Sunscreen", "None"), "Buff"), "sunscreen remains findable under buffs");
        object effigy = Resolve(classifier, "Categories", "Scout Effigy", "Mystical");
        Check(Has(effigy, "Mystical") && Has(effigy, "Consumable"), "effigy appears in both relevant categories");
        Check(Has(Resolve(classifier, "Categories", "Portable Stove", "None"), "Deployable"), "stove is deployable");
        object beanCategory = Resolve(classifier, "Categories", "Magic Bean", "None");
        Check(Has(beanCategory, "Deployable") && !Has(beanCategory, "Consumable"), "magic bean is a deployable, not a consumable");
        object beanKind = Resolve(classifier, "Kinds", "Magic Bean", "None");
        Check(Convert.ToInt32(beanKind) == 0, "magic bean is not shown in food subfilters");
        object napberryKind = Resolve(classifier, "Kinds", "Napberry", "Berry");
        Check(Has(napberryKind, "Food") && Has(napberryKind, "Healing") && Has(napberryKind, "Recovery"), "Napberry is food, healing and debuff recovery");
        Check(Has(Resolve(classifier, "Kinds", "FrogLegs", "None"), "Food"), "frog legs are food");
        Check(Has(Resolve(classifier, "Categories", "backpack", "None"), "Equipment"), "classification uses stable names without case sensitivity");
        Check(Has(Resolve(classifier, "Categories", "New item", "Mystical, NonCloneable"), "Mystical"), "new mystical item uses PEAK tags");
        Check(Has(Resolve(classifier, "Categories", "Magic Bean", "Mystical"), "Mystical"), "native mystical tag supplements known consumable category");
        Check(Has(Resolve(classifier, "Categories", "New berry", "Berry"), "Consumable"), "new berries use PEAK tags");
        Check(Has(Resolve(classifier, "Categories", "Unknown", "None"), "Other"), "unknown items remain findable under other");
        Check(!Has(Resolve(classifier, "Categories", "Bounce Fungus", "Mushroom"), "Consumable"), "deployable fungi are not classified as edible");
        Check(Has(Resolve(classifier, "Kinds", "Medicinal Root", "None"), "Food"), "root is food");
        Check(Has(Resolve(classifier, "Kinds", "Medicinal Root", "None"), "Healing"), "root has healing use");
        Check(Has(Resolve(classifier, "Kinds", "Medicinal Root", "None"), "Recovery"), "root has recovery use");
        Check(!Has(Resolve(classifier, "Kinds", "Bandages", "None"), "Food"), "bandages are not food");
        Check(Has(Resolve(classifier, "Kinds", "Scout Effigy", "Mystical"), "Revive"), "effigy is a revival consumable");
        Check(Has(Resolve(classifier, "Kinds", "New berry", "Berry"), "Food"), "new berries join the food filter");
        Check(!Has(Resolve(classifier, "Kinds", "Bounce Fungus", "Mushroom"), "Food"), "deployable fungi stay out of food filter");
        return checks;
    }
    private static object Resolve(Type type, string method, string name, string tags)
    {
        return type.GetMethod(method).Invoke(null, new object[] {name, tags});
    }
    private static bool Has(object value, string flag)
    {
        int bit = Convert.ToInt32(Enum.Parse(value.GetType(), flag));
        return (Convert.ToInt32(value) & bit) != 0;
    }
    private static void Near(float expected, float actual, string name) { Check(Math.Abs(expected-actual)<0.01f, name); }
    private static void Check(bool condition, string name)
    {
        checks++;
        if (!condition) { Console.WriteLine("FAIL: " + name); Environment.Exit(1); }
    }
}
