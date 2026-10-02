using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PeakAdminToolkit.Core
{
    internal static class EmbeddedJson
    {
        public static JObject Read(string name, Action<string> warning = null)
        {
            using (Stream stream = typeof(EmbeddedJson).Assembly.GetManifestResourceStream("PeakAdminToolkit." + name))
            {
                if (stream == null)
                {
                    if (warning != null) warning("Missing embedded data: " + name);
                    return new JObject();
                }
                try
                {
                    using (var reader = new StreamReader(stream)) return JObject.Parse(reader.ReadToEnd());
                }
                catch (JsonException ex)
                {
                    if (warning != null) warning("Invalid embedded data " + name + ": " + ex.Message);
                    return new JObject();
                }
            }
        }
    }
}
