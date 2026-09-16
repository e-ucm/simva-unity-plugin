using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
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

        public static event Action<string, string> LanguageSelected;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else if (instance != this) {
                GameObject.DestroyImmediate(gameObject);
                return;
            }
            SetActive(true);
            GetLanguages();
            RefreshLanguageList();
        }

        public string GetLanguageFromTitle(string title) {
            foreach(string languageCode in languages.Keys) {
                if(languages[languageCode] == title) {
                    return languageCode;
                }
            }
            return null;
        }


        public void FillDictionaryAndRunLoginScene(string language)
        {
            string langCode = SimvaLanguageLoader.ExtractLangCode(language);
            if (!string.IsNullOrEmpty(langCode))
            {
                jsonFiles = SimvaLanguageLoader.LoadLanguageJSON(langCode);
                SimvaPlugin.Instance.SetLanguageDictionary(SimvaLanguageLoader.LoadDictionary(jsonFiles), false);
            }
            defaultJsonFiles = SimvaLanguageLoader.LoadLanguageJSON(langCode);
            SimvaPlugin.Instance.SetLanguageDictionary(SimvaLanguageLoader.LoadDictionary(defaultJsonFiles), true);

            if (LanguageSelected != null)
            {
                string unityCode = langCode.Contains("_") ? langCode.Substring(0, langCode.IndexOf("_")) : langCode;
                LanguageSelected(langCode, unityCode);
            }

            if (SimvaManager.Instance != null && SimvaManager.Instance.IsEnabled)
            {
                SimvaPlugin.Instance.RunScene("Simva.Login");
            }
            else
            {
                SimvaPlugin.Instance.StartGameplay();
            }
        }


        public void SetActive(bool active)
        {
            this.gameObject.SetActive(active);
        }

        public void RefreshLanguageList()
        {
            if(languageGridLayout) {
                foreach (Transform child in languageGridLayout.transform)
                {
                    Destroy(child.gameObject);
                }
            }

            foreach (string lang in SimvaPlugin.Instance.SelectedLanguages)
            {
                GameObject item = Instantiate(languageItemPrefab, languageGridLayout.transform);
                foreach(string languageCode in languages.Keys) {
                    if(languages[languageCode] == lang) {
                        item.name = languageCode;
                        break;
                    }
                }
                item.SetActive(true);
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

        public override void Render()
        {
            SetActive(true);
            if(!SimvaPlugin.Instance.AutoStart) {
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