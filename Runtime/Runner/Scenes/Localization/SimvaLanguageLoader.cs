using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Simva
{
    public static class SimvaLanguageLoader
    {
        public static string ExtractLangCode(string language)
        {
            if (string.IsNullOrEmpty(language))
            {
                return null;
            }

            if (language.Contains("["))
            {
                int start = language.IndexOf("[") + 1;
                int end = language.IndexOf("]", start);
                if (start > 0 && end > start)
                {
                    return language.Substring(start, end - start);
                }
            }
            return language;
        }

        public static List<TextAsset> LoadLanguageJSON(string language)
        {
            SimvaPlugin.Instance.Log("Loading Dictionaries directory (Localization/" + language + "/" + "Dictionaries)...");
            Object[] filler = Resources.LoadAll("Localization/" + language + "/" + "Dictionaries", typeof(TextAsset));
            if (filler == null || filler.Length == 0)
            {
                SimvaPlugin.Instance.LogError("No JSON Files in Dictionaries directory found (Localization/" + language + "/" + "Dictionaries) !");
            }

            List<TextAsset> json = new List<TextAsset>();
            foreach (Object file in filler)
            {
                json.Add((TextAsset)file);
            }
#if UNITY_EDITOR
            foreach (var t in json)
                SimvaPlugin.Instance.Log("JSON File added for Language " + language + " : " + t.name);
#endif
            return json;
        }

        public static Dictionary<string, string> LoadDictionary(List<TextAsset> json)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (var jsonFile in json)
            {
                SimvaPlugin.Instance.Log("JSON File added : " + jsonFile.name);
                JObject jObject = JObject.Parse(jsonFile.text);
                foreach (var entry in jObject)
                {
                    if (!dictionary.ContainsKey(entry.Key))
                    {
                        dictionary.Add(entry.Key, (string)entry.Value);
                    }
                }
            }
            return dictionary;
        }
    }
}
