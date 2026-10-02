using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using PeakAdminToolkit.Core;

internal static class TranslationTests
{
    private static int checks;
    private static void Main(string[] args)
    {
        try { Run(args[0]); }
        catch (Exception ex) { Console.WriteLine("FAIL: " + ex.Message); Environment.ExitCode = 1; }
    }

    private static void Run(string root)
    {
        string directory = Path.Combine(root, "locales");
        JObject english = Read(Path.Combine(directory, "en.json"));
        var loc = new Localization();
        foreach (string language in new[] { "en", "zh-CN", "zh-TW" })
        {
            JObject messages = Read(Path.Combine(directory, language + ".json"));
            Check(english.Properties().Select(p => p.Name).OrderBy(k => k).SequenceEqual(messages.Properties().Select(p => p.Name).OrderBy(k => k)),
                "translation keys match English: " + language);
            bool valid = true;
            bool loaded = true;
            loc.Update(language, null);
            foreach (JProperty property in english.Properties())
            {
                JToken token = messages[property.Name];
                string value = token.Type == JTokenType.String ? (string)token : null;
                valid &= !string.IsNullOrWhiteSpace(value);
                if (value == null) continue;
                valid &= Placeholders((string)property.Value) == Placeholders(value);
                // Also rejects unescaped braces and other invalid composite formats.
                string.Format(value, "first", "second", "third", "fourth", "fifth");
                loaded &= loc.Text(property.Name) == value;
            }
            Check(valid, "translation text and format parameters remain valid: " + language);
            Check(loaded, "runtime uses the contributor language file: " + language);
        }
        Check((string)Read(Path.Combine(directory, "en.json"))["SoloManualSpawn"] == "Does not spawn naturally in solo",
            "English solo card explains natural spawn only");
        Check((string)Read(Path.Combine(directory, "zh-CN.json"))["SoloManualSpawn"] == "单人游戏不会自然生成",
            "Simplified solo card explains natural spawn only");
        Check((string)Read(Path.Combine(directory, "zh-TW.json"))["SoloManualSpawn"] == "單人遊戲不會自然生成",
            "Traditional solo card explains natural spawn only");
        Check((string)Read(Path.Combine(directory, "en.json"))["PlayerNoPenaltyScope"] == "Skips new revival curse and hunger; does not refill stamina or cleanse existing effects.",
            "English no-penalty text distinguishes revival from cleanse");
        Check((string)Read(Path.Combine(directory, "zh-TW.json"))["PlayerNoPenaltyScope"] == "略過新增的復活詛咒與飢餓；不補滿體力，也不清除原有狀態。",
            "Traditional no-penalty text distinguishes revival from cleanse");
        Console.WriteLine("PASS: " + checks + " translation source/resource checks.");
    }

    private static JObject Read(string path)
    {
        return JObject.Parse(File.ReadAllText(path), new JsonLoadSettings { DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error });
    }

    private static string Placeholders(string value)
    {
        return string.Join(",", Regex.Matches(value, @"(?<!\{)\{(\d+)(?:[^{}]*)\}").Cast<Match>().Select(m => m.Groups[1].Value).Distinct().OrderBy(k => k));
    }

    private static void Check(bool value, string name)
    {
        checks++;
        if (!value) throw new InvalidOperationException(name);
    }
}
