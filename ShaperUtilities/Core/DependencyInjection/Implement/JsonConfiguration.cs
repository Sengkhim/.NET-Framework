using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShaperUtilities.Core.DependencyInjection.Service;

namespace ShaperUtilities.Core.DependencyInjection.Implement
{
    /// <summary>
    /// Implements IConfiguration by loading and merging JSON files based on a runtime environment.
    /// It looks for a "NETFX_ENVIRONMENT" environment variable to determine the active environment.
    /// Defaults to "Development" if not specified.
    /// </summary>
     public class JsonConfiguration : IConfiguration
    {
        private readonly ConcurrentDictionary<string, string> _settingsCache = new();
        private readonly string _configBasePath;
        private readonly string _environmentName;

        /// <summary>
        /// Initializes a new instance of the JsonConfiguration class.
        /// </summary>
        /// <param name="configBasePath">The physical path to the directory containing appsettings.json files.</param>
        public JsonConfiguration(string configBasePath)
        {
            _configBasePath = configBasePath ?? throw new ArgumentNullException(nameof(configBasePath));
            
            if (!Directory.Exists(_configBasePath))
            {
                Console.WriteLine($"JsonConfiguration ERROR: Configuration base path not found: '{_configBasePath}'. Please ensure the folder exists and contains appsettings.json files.");
                throw new DirectoryNotFoundException($"Configuration base path not found: '{_configBasePath}'.");
            }

            _environmentName = GetEnvironmentName();
            LoadConfiguration();
        }

        /// <summary>
        /// Determines the current environment name by checking system/user environment variables
        /// and optionally Web.config appSettings.
        /// </summary>
        /// <returns>The detected environment name (e.g., "Development", "Production"), or "Development" by default.</returns>
        private static string GetEnvironmentName()
        {
            // System/User Environment Variable
            var env = Environment.GetEnvironmentVariable("NETFX_ENVIRONMENT");

            // Web.config appSettings (as a fallback or for development convenience)
            if (string.IsNullOrEmpty(env))
                env = System.Configuration.ConfigurationManager.AppSettings["NETFX_ENVIRONMENT"];
            
            return string.IsNullOrEmpty(env) ? "Development" : env;
        }

        /// <summary>
        /// Loads and merges configuration settings from JSON files based on the detected environment.
        /// </summary>
        private void LoadConfiguration()
        {
            // Clear cache for fresh load
            _settingsCache.Clear();

            // Load base appsettings.json (e.g., appsettings.json)
            var baseFilePath = Path.Combine(_configBasePath, "appsettings.json");
            LoadFile(baseFilePath);

            // Load environment-specific appsettings.{Environment}.json (e.g., appsettings.Development.json)
            if (string.IsNullOrEmpty(_environmentName)) return;
            
            var envFilePath = Path.Combine(_configBasePath, $"appsettings.{_environmentName}.json");
            // Overrides base settings if keys are duplicated
            LoadFile(envFilePath);
        }

        /// <summary>
        /// Reads a JSON file, flattens its structure, and adds/updates settings in the cache.
        /// </summary>
        /// <param name="filePath">The full path to the JSON file.</param>
        private void LoadFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    var json = File.ReadAllText(filePath);
                    var jsonObject = JObject.Parse(json);
                    FlattenAndAddSettings(jsonObject);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"JsonConfiguration ERROR: Failed to load or parse '{filePath}': {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"JsonConfiguration: File not found: {filePath}. Skipping.");
            }
        }

        /// <summary>
        /// Recursively flattens a JSON object into a single dictionary using dot/colon notation for keys.
        /// </summary>
        /// <param name="jsonObject">The JObject to flatten.</param>
        /// <param name="prefix">The prefix for nested keys (e.g., "Parent:Child").</param>
        private void FlattenAndAddSettings(JObject jsonObject, string prefix = "")
        {
            foreach (var property in jsonObject.Properties())
            {
                var key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}:{property.Name}";

                switch (property.Value.Type)
                {
                    case JTokenType.Object:
                        FlattenAndAddSettings((JObject)property.Value, key);
                        break;
                    case JTokenType.Array:
                    {
                        // HandleRetry simple arrays of primitive types by joining, or serialize for complex ones
                        var jsonArray = (JArray)property.Value;
                        _settingsCache[key] = jsonArray.ToString(Formatting.None); 
                        break;
                    }
                    default:
                        // Store the string representation of the value
                        _settingsCache[key] = property.Value.ToString();
                        break;
                }
            }
        }
        
        public string GetConnectionString(string name) => GetValue($"ConnectionStrings:{name}");

        public string GetValue(string key)
            => _settingsCache.TryGetValue(key, out var value) ? value : null;
        

        public T GetValue<T>(string key)
        {
            if (!_settingsCache.TryGetValue(key, out var stringValue))
                return default;
            
            try
            {
                return (T)CheckType<T>(stringValue);
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"JsonConfiguration ERROR: Cannot convert value for key '{key}' ('{stringValue}') to type '{typeof(T).Name}': {ex.Message}");
                return default;
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"JsonConfiguration ERROR: General error converting value for key '{key}' ('{stringValue}') to type '{typeof(T).Name}': {ex.Message}");
                return default;
            }
        }

        
        public bool TryGetValue(string key, out string value)
            => _settingsCache.TryGetValue(key, out value);
        
        
        public bool TryGetValue<T>(string key, out T value)
        {
            value = default;
            
            if (!_settingsCache.TryGetValue(key, out var stringValue)) return false;
            
            try

            {
                value = (T)CheckType<T>(stringValue);

                return true;
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"JsonConfiguration ERROR: Cannot convert value for key '{key}' ('{stringValue}') to type '{typeof(T).Name}' in TryGetValue: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JsonConfiguration ERROR: General error converting value for key '{key}' ('{stringValue}') to type '{typeof(T).Name}' in TryGetValue: {ex.Message}");
                return false;
            }
        }

      
        private static object CheckType<T>(string stringValue)
        {
            return typeof(T) switch
            {
                var t when t == typeof(string) => stringValue,
                var t when t == typeof(bool) => bool.Parse(stringValue),
                var t when t == typeof(int) => int.Parse(stringValue),
                var t when t == typeof(long) => long.Parse(stringValue),
                var t when t == typeof(double) => double.Parse(stringValue),
                var t when t == typeof(decimal) => decimal.Parse(stringValue),
                { IsEnum: true } => System.Enum.Parse(typeof(T), stringValue, true),
                _ => JsonConvert.DeserializeObject(stringValue, typeof(T)) 
            };
        }

        public IReadOnlyDictionary<string, string> Settings => _settingsCache;
    }
}