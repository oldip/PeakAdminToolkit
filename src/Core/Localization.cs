using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace PeakAdminToolkit.Core
{
    internal sealed class Localization
    {
        private static readonly Dictionary<string, string> English = Read("en");
        private static readonly Dictionary<string, string> Simplified = Read("zh-CN");
        private static readonly Dictionary<string, string> Traditional = Read("zh-TW");

        public string Language { get; private set; }
        public static IEnumerable<string> Keys { get { return English.Keys; } }

        public Localization() { Language = "en"; }

        public static string Resolve(string preference, string gameLanguage)
        {
            if (preference == "en" || preference == "zh-CN" || preference == "zh-TW") return preference;
            if (gameLanguage == "SimplifiedChinese") return "zh-CN";
            if (gameLanguage == "TraditionalChinese") return "zh-TW";
            return "en";
        }

        public void Update(string preference, string gameLanguage) { Language = Resolve(preference, gameLanguage); }

        public string Text(string key)
        {
            string value;
            var table = Language == "zh-CN" ? Simplified : Language == "zh-TW" ? Traditional : English;
            if (table.TryGetValue(key, out value) || English.TryGetValue(key, out value)) return value;
            return key;
        }

        public string Format(string key, params object[] values)
        {
            try { return string.Format(Text(key), values); }
            catch (FormatException)
            {
                string fallback;
                if (English.TryGetValue(key, out fallback))
                {
                    try { return string.Format(fallback, values); }
                    catch (FormatException) { }
                }
                return key;
            }
        }

        private static Dictionary<string, string> Read(string language)
        {
            var result = new Dictionary<string, string>();
            foreach (JProperty property in EmbeddedJson.Read("Locales." + language + ".json").Properties())
            {
                if (property.Value.Type != JTokenType.String) continue;
                string value = (string)property.Value;
                if (!string.IsNullOrWhiteSpace(value)) result[property.Name] = value;
            }
            return result;
        }
    }
}
