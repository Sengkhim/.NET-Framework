using System.Collections.Generic;
using System.Web;
using System;
using System.IO;
using System.Threading;
using Newtonsoft.Json; 

namespace ServerTest.Extension
{
    public static class JsonLocalizer
    {
        private static readonly Dictionary<string, Dictionary<string, string>> LocalizedStringsCache =
            new Dictionary<string, Dictionary<string, string>>();
        
        private static readonly object Lock = new object();
        
        //Or wherever you store your JSON files
        private static readonly string ResourcesFolderPath = HttpContext.Current.Server.MapPath("~/App_Data/Localization");
        
        public static string GetString(string key, params object[] args)
        {
            var cultureName = Thread.CurrentThread.CurrentUICulture.Name;
            Dictionary<string, string> currentCultureStrings;

            lock (Lock)
            {
                if (!LocalizedStringsCache.TryGetValue(cultureName, out currentCultureStrings))
                {
                    currentCultureStrings = LoadStringsForCulture(cultureName);
                    LocalizedStringsCache[cultureName] = currentCultureStrings;
                }
            }

            if (currentCultureStrings != null && currentCultureStrings.TryGetValue(key, out var s))
            {
                return string.Format(s, args);
            }

            // Fallback to default culture (e.g., "en") if key not found in current culture
            if (cultureName == "en") return $"[{key}]"; // Assuming "en" is your default
            
            lock (Lock)
            {
                if (!LocalizedStringsCache.TryGetValue("en", out currentCultureStrings))
                {
                    currentCultureStrings = LoadStringsForCulture("en");
                    LocalizedStringsCache["en"] = currentCultureStrings;
                }
            }

            if (currentCultureStrings == null || !currentCultureStrings.TryGetValue(key, out var s1))
                return $"[{key}]";
            
            return string.Format(s1, args);
        }

        private static Dictionary<string, string> LoadStringsForCulture(string cultureName)
        {
            var filePath = Path.Combine(ResourcesFolderPath, $"{cultureName}.json");

            if (!File.Exists(filePath)) return null;
            
            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(filePath));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading localization file for {cultureName}: {ex.Message}");
                return null;
            }
        }
    }
}