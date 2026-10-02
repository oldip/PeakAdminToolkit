using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using PeakAdminToolkit.Core;

namespace PeakAdminToolkit.Items
{
    internal sealed class ItemDescriptions
    {
        private readonly Dictionary<string, string> english;
        private readonly Dictionary<string, string> simplified;

        public ItemDescriptions(Action<string> warning = null)
        {
            english = Read("peak-item-tooltip.descriptions.json", warning);
            simplified = Read("peak-item-tooltip.descriptions.zh-CN.json", warning);
        }

        public string Get(string language, params string[] names)
        {
            string value = Find(language == "zh-CN" || language == "zh-TW" ? simplified : english, names);
            return language == "zh-TW" ? ChineseScript.ToTraditional(value) : value;
        }

        private static string Find(Dictionary<string, string> table, string[] names)
        {
            foreach (string name in names)
            {
                string value;
                if (!string.IsNullOrWhiteSpace(name) && table.TryGetValue(name.Trim(), out value)) return value;
            }
            return string.Empty;
        }

        private static Dictionary<string, string> Read(string fileName, Action<string> warning)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var items = EmbeddedJson.Read("Descriptions." + fileName, warning)["items"] as JObject;
            if (items == null) return result;
            foreach (JProperty property in items.Properties())
            {
                var item = property.Value as JObject;
                JToken token = item == null ? null : item["description"];
                if (token == null || token.Type != JTokenType.String) continue;
                string description = (string)token;
                if (!string.IsNullOrWhiteSpace(description)) result[property.Name.Trim()] = description.Trim();
            }
            return result;
        }
    }
}
