using System;
using System.Runtime.InteropServices;
using System.Text;

namespace PeakAdminToolkit.Items
{
    internal static class ChineseScript
    {
        public static string ToSimplified(string value) { return Convert(value, "zh-CN", 0x02000000); }
        public static string ToTraditional(string value) { return Convert(value, "zh-TW", 0x04000000); }

        // Windows supplies both mappings; no character dictionary is embedded.
        private static string Convert(string value, string locale, uint flags)
        {
            if (string.IsNullOrEmpty(value) || Environment.OSVersion.Platform != PlatformID.Win32NT) return value;
            try
            {
                int size = LCMapStringEx(locale, flags, value, -1, null, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                if (size == 0) return value;
                var mapped = new StringBuilder(size);
                return LCMapStringEx(locale, flags, value, -1, mapped, size, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero) == 0
                    ? value : mapped.ToString();
            }
            catch (DllNotFoundException) { return value; }
            catch (EntryPointNotFoundException) { return value; }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int LCMapStringEx(string locale, uint flags, string source, int sourceLength,
            StringBuilder destination, int destinationLength, IntPtr version, IntPtr reserved, IntPtr sortHandle);
    }
}
