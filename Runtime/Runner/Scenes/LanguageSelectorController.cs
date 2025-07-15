using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
//using UnityEngine.Localization.Settings;

namespace Simva
{
    public class LanguageSelectorController : SimvaSceneController
    {
        public static LanguageSelectorController instance;
        public GameObject back;
        public GameObject languageGridLayout;
        public GameObject languageItemPrefab;
        private static List<TextAsset> jsonFiles;
        private static List<TextAsset> defaultJsonFiles;
        private static Dictionary<string, string> languages;
        private bool active = false;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
            {
                GameObject.DestroyImmediate(gameObject);
                return;
            }
            SetActive(true);
            GetLanguages();
            RefreshLanguageList();
        }

        public void ChangeLocale(string locale)
        {
            if (active)
            {
                return;
            }
            //StartCoroutine(SetLocale(locale));
            return;
        }
//IEnumerator<object> SetLocale(string _locale)

        void SetLocale(string _locale)
        {
            active = true;
            //yield return LocalizationSettings.InitializationOperation;
            //
            //if (LocalizationSettings.AvailableLocale.Locales.Contains(_locale))
            //{
            //    LocalizationSettings.SelectedLocate = _locale;
            //}
            //PlayerPrefs.setInt("Locale", _locale);
            active = false;
         }

        public string GetLanguageFromTitle(string title)
        {
            foreach (string languageCode in languages.Keys)
            {
                if (languages[languageCode] == title)
                {
                    return languageCode;
                }
            }
            return null;
        }


        //Selects a language by flag button in Title scene
        public void FillDictionaryAndRunLoginScene(string language)
        {
            //if (!String.IsNullOrEmpty(language))
            //{
            //    jsonFiles = LoadLanguageJSON(language);
            //    SimvaPlugin.Instance.SetLanguageDictionary(LoadDictionary(jsonFiles), false);
            //}
            //defaultJsonFiles = LoadLanguageJSON(language);
            //SimvaPlugin.Instance.SetLanguageDictionary(LoadDictionary(defaultJsonFiles), true);
            ChangeLocale(language);
            SimvaPlugin.Instance.RunScene("Simva.Login");
        }


        public void SetActive(bool active)
        {
            this.gameObject.SetActive(active);
        }

        public void RefreshLanguageList()
        {
            // Clear existing children
            if (languageGridLayout)
            {
                foreach (Transform child in languageGridLayout.transform)
                {
                    Destroy(child.gameObject);
                }
            }

            // Spawn one UI item per selected language
            foreach (string lang in SimvaPlugin.Instance.SelectedLanguages)
            {
                GameObject item = Instantiate(languageItemPrefab, languageGridLayout.transform);
                foreach (string languageCode in languages.Keys)
                {
                    if (languages[languageCode] == lang)
                    {
                        item.name = languageCode;
                        item.SetActive(true);
                        continue;
                    }
                }
            }
        }

        Dictionary<string, string> GetLanguages()
        {
            languages = new Dictionary<string, string>();

            TextAsset[] filler = Resources.LoadAll<TextAsset>("Localization");
            foreach (TextAsset obj in filler)
            {
                if (obj.name == "lang")
                {
                    JObject jObject = JObject.Parse(obj.text);
                    var code = (string)jObject["code"];
                    var name = (string)jObject["displayName"];
                    var modifName = name + " [" + code + "]";
                    if (!languages.ContainsKey(code))
                    {
                        languages.Add(code, modifName);
                    }
                }
            }
            SimvaPlugin.Instance.Log("Languages : " + languages.Count);
            return languages;
        }

        List<TextAsset> LoadLanguageJSON(string language)
        {
            SimvaPlugin.Instance.Log("Loading Dictionaries directory (Localization/" + language + "/" + "Dictionaries)...");
            UnityEngine.Object[] filler = Resources.LoadAll("Localization/" + language + "/" + "Dictionaries", typeof(TextAsset));
            if (filler == null || filler.Length == 0)
            {
                SimvaPlugin.Instance.LogError("No JSON Files in Dictionaries directory found (Localization/" + language + "/" + "Dictionaries) !");
            }

            List<TextAsset> json = new List<TextAsset>();
            foreach (UnityEngine.Object file in filler)
            {
                json.Add((TextAsset)file);
            }
#if UNITY_EDITOR
            foreach (var t in json)
                SimvaPlugin.Instance.Log("JSON File added for Language " + language + " : " + t.name);
#endif
            return json;
        }

        Dictionary<string, string> LoadDictionary(List<TextAsset> json)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            string fileContents;
            foreach (var jsonFile in json)
            {
                SimvaPlugin.Instance.Log("JSON File added : " + jsonFile.name);
                fileContents = jsonFile.text;

                JObject jObject = JObject.Parse(fileContents);
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

        public override void Render()
        {
            SetActive(true);
            if (!SimvaPlugin.Instance.AutoStart)
            {
                back.SetActive(true);
            }
            Ready = true;
        }

        public void Back()
        {
            SimvaPlugin.Instance.RunScene("StartMenu");
        }

        public override void Destroy()
        {
            GameObject.DestroyImmediate(this.gameObject);
        }
    }
}