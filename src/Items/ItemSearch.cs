using System;
using System.Text;

namespace PeakAdminToolkit.Items
{
    internal static class ItemSearch
    {
        public static bool Matches(string query, string currentName, string englishName, string simplifiedName, string traditionalName)
        {
            string normalizedQuery = Normalize(query);
            if (normalizedQuery.Length == 0) return true;
            string[] names =
            {
                currentName ?? string.Empty,
                englishName ?? string.Empty,
                simplifiedName ?? string.Empty,
                traditionalName ?? string.Empty
            };
            foreach (string name in names)
            {
                if (Contains(name, normalizedQuery)) return true;
            }
            bool isGlider = false;
            foreach (string name in names)
                if (Contains(name, "glider") || Contains(name, "滑翔翼")) { isGlider = true; break; }
            return isGlider && (Contains("傘 umbrella", normalizedQuery) || Contains("umbrella", normalizedQuery));
        }

        private static bool Contains(string value, string query)
        {
            return Normalize(value).IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string Normalize(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            StringBuilder result = new StringBuilder(value.Length);
            foreach (char character in value)
                if (!char.IsWhiteSpace(character))
                    result.Append(char.ToLowerInvariant(character));
            return ChineseScript.ToSimplified(result.ToString());
        }

    }
}
