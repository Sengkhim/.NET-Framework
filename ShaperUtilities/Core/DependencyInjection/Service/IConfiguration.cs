using System.Collections.Generic;

namespace ShaperUtilities.Core.DependencyInjection.Service
{
    /// <summary>
    /// Contract for a configuration provider.
    /// </summary>
    public interface IConfiguration
    {
        /// <summary>
        /// Shorthand for GetSection("ConnectionStrings")[name]
        /// </summary>
        /// <param name="name">The connection string key</param>
        /// <returns>The connection string.</returns>
        string GetConnectionString(string name);
        /// <summary>
        /// Gets a configuration value as a string. Returns null if key not found.
        /// Keys are dot-notation or colon-notation (e.g., "AppSettings:SiteName" or "ConnectionStrings:DefaultConnection").
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <returns>The string value, or null if the key is not found.</returns>
        string GetValue(string key);

        /// <summary>
        /// Gets a configuration value and attempts to convert it to the specified type.
        /// Returns default(T) if key not found or conversion fails.
        /// </summary>
        /// <typeparam name="T">The type to convert the value to.</typeparam>
        /// <param name="key">The configuration key.</param>
        /// <returns>The value converted to type T, or default(T).</returns>
        T GetValue<T>(string key);

        /// <summary>
        /// Tries to get a configuration value as a string.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <param name="value">When this method returns, contains the string value associated with the specified key, if the key is found; otherwise, null.</param>
        /// <returns>true if the key is found; otherwise, false.</returns>
        bool TryGetValue(string key, out string value);

        /// <summary>
        /// Tries to get a configuration value and convert it to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to convert the value to.</typeparam>
        /// <param name="key">The configuration key.</param>
        /// <param name="value">When this method returns, contains the value converted to type T, if the key is found and conversion succeeds; otherwise, default(T).</param>
        /// <returns>true if the key is found and conversion succeeds; otherwise, false.</returns>
        bool TryGetValue<T>(string key, out T value);

        /// <summary>
        /// Gets all loaded configuration settings as a read-only dictionary.
        /// </summary>
        /// <returns>A dictionary of all configuration settings.</returns>
        IReadOnlyDictionary<string, string> Settings { get; }
    }
}

   