using System;
using System.Collections.Generic;

namespace Cprima.RpaPub.EdgedChisel
{
    public static class ConfigHelper
    {
        public static string GetString(Dictionary<string, object> config, string section, string key)
        {
            var value = GetValue(config, section, key);
            return value as string ?? throw new InvalidCastException($"Value for '{section}.{key}' is not a string.");
        }

        public static int GetInt32(Dictionary<string, object> config, string section, string key)
        {
            if (!config.TryGetValue(section, out var sectionObj) ||
                !(sectionObj is Dictionary<string, object> sectionDict))
                throw new ArgumentException($"Missing or invalid section: {section}");
        
            if (!sectionDict.TryGetValue(key, out var value))
                throw new ArgumentException($"Missing key: {key} in section: {section}");
        
            return value switch
            {
                int i => i,
                long l => Convert.ToInt32(l),
                double d => Convert.ToInt32(d),
                string s when int.TryParse(s, out var parsed) => parsed,
                _ => throw new InvalidCastException($"Cannot convert value of type {value.GetType()} to Int32")
            };
        }
        
        public static bool GetBool(Dictionary<string, object> config, string section, string key)
        {
            if (!config.TryGetValue(section, out var sectionObj) ||
                !(sectionObj is Dictionary<string, object> sectionDict))
                throw new ArgumentException($"Missing or invalid section: {section}");
        
            if (!sectionDict.TryGetValue(key, out var value))
                throw new ArgumentException($"Missing key: {key} in section: {section}");
        
            return value switch
            {
                bool b => b,
                int i => i != 0,
                double d => d != 0,
                string s => s.Trim().ToLowerInvariant() switch
                {
                    "true" => true,
                    "1" => true,
                    "false" => false,
                    "0" => false,
                    _ => throw new InvalidCastException($"Cannot convert string '{s}' to bool")
                },
                _ => throw new InvalidCastException($"Cannot convert value of type {value.GetType()} to bool")
            };
        }

        public static TimeSpan GetTimeSpanFromSeconds(
            Dictionary<string, object> config,
            string section,
            string key,
            double defaultSeconds = 5)
        {
            if (!config.TryGetValue(section, out var sectionObj) ||
                sectionObj is not Dictionary<string, object> sectionDict)
                throw new ArgumentException($"Missing or invalid section: {section}");

            if (!sectionDict.TryGetValue(key, out var value))
                return TimeSpan.FromSeconds(defaultSeconds);

            double seconds = value switch
            {
                double d => d,
                float f => f,
                int i => i,
                long l => l,
                string s when double.TryParse(s, out var parsed) => parsed,
                _ => throw new InvalidCastException($"Cannot convert value of type {value.GetType()} to double")
            };

            return TimeSpan.FromSeconds(seconds);
        }

        private static object GetValue(Dictionary<string, object> config, string section, string key)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (!config.TryGetValue(section, out var sectionObj))
                throw new KeyNotFoundException($"Section '{section}' not found.");

            if (sectionObj is not Dictionary<string, object> sectionDict)
                throw new InvalidCastException($"Section '{section}' is not a Dictionary<string, object>.");

            if (!sectionDict.TryGetValue(key, out var value))
                throw new KeyNotFoundException($"Key '{key}' not found in section '{section}'.");

            return value;
        }
    }
}