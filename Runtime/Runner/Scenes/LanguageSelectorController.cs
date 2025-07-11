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
    private static Dictionary<string, string> myDictionary;
    private static Dictionary<string, string> defaultDictionary;
    private static string Language;

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
        SetUpJSONFiles(true);
        FillDictionary(true);
        DontDestroyOnLoad(gameObject);
    }

    public string GetLanguageFromTitle(string title) {
        foreach(string languageCode in languages.Keys) {
            if(languages[languageCode] == title) {
                return languageCode;
            }
        }
        return null;
    }

    public void SetLanguageFromTitle(string title) {
        string  lang = GetLanguageFromTitle(title);
        if (lang == null) {
            SimvaPlugin.Instance.LogError("Language not found. (" + title + ")");
        } else {
            Language = title;
        }
    }

    //Selects a language by flag button in Title scene
    public void FillDictionaryAndRunLoginScene()
    {
        SetUpJSONFiles(false);
        FillDictionary(false);
        SetActive(false);
        SimvaPlugin.Instance.RunScene("Simva.Login");
    }

      //Selects a language by flag button in Title scene
    public void SetLanguageCode(string code)
    {
        Language = code;
    }

    public void SetActive(bool active)
    {
        this.gameObject.SetActive(active);
    }

    public string GetCurrentLanguage() { return Language; }

    public void RefreshLanguageList()
    {
        // Clear existing children
        if(languageGridLayout) {
            foreach (Transform child in languageGridLayout.transform)
            {
                Destroy(child.gameObject);
            }
        }

        // Spawn one UI item per selected language
        foreach (string lang in SimvaPlugin.Instance.SelectedLanguages)
        {
            GameObject item = Instantiate(languageItemPrefab, languageGridLayout.transform);
            foreach(string languageCode in languages.Keys) {
                if(languages[languageCode] == lang) {
                    item.name = languageCode;
                    item.SetActive(true);
                    continue;
                }
            }
        }
    }

    void GetLanguages()
    {
        languages = new Dictionary<string, string>();

        TextAsset[] filler = Resources.LoadAll<TextAsset>("Localization");
        foreach (TextAsset obj in filler) {
            if (obj.name == "lang") {
                JObject jObject = JObject.Parse(obj.text);
                var code = (string)jObject["code"];
                var name = (string)jObject["displayName"];
                var modifName=name + " [" + code + "]";
                if(!languages.ContainsKey(code)) {
                    languages.Add(code, modifName);
                }
            }
        }
        SimvaPlugin.Instance.Log("Languages : " + languages.Count);
    }

    //Creates an initializes Json Files array to be used by myDictionary
    void SetUpJSONFiles(bool useDefault)
    {
        var lang=Language;
        if (useDefault) {
            lang = GetLanguageFromTitle(SimvaPlugin.Instance.LanguageByDefault);
        }

        Object[] filler = Resources.LoadAll("Localization/" + lang + "/" + "Dictionaries", typeof(TextAsset));

        if (filler == null || filler.Length == 0)
        {
            SimvaPlugin.Instance.LogError("No JSON Files in Dictionaries directory found (Localization/" + lang + "/" + "Dictionaries) !");
        }
        
        List<TextAsset> json = new List<TextAsset>();
        foreach (Object file in filler)
        {
            json.Add((TextAsset)file);
        }

#if UNITY_EDITOR
        foreach (var t in json)
            SimvaPlugin.Instance.Log("JSON File added for Language " + lang + " : " + t.name);
#endif
        if(useDefault) {
            defaultJsonFiles = json;
        } else {
            jsonFiles = json;
        }
    }

    //Fills myDictionary with all JSON files
    void FillDictionary(bool useDefault)
    {
        List<TextAsset> json;
        Dictionary<string, string> dictionary = new Dictionary<string, string>();
        if(useDefault) {
            json=defaultJsonFiles;
        } else {
            json=jsonFiles;
        }
        string fileContents;
        foreach (var jsonFile in json)
        {
            SimvaPlugin.Instance.Log("JSON File added : " + jsonFile.name + " (Default : " + useDefault + ")");
            fileContents = jsonFile.text;
            
            JObject jObject = JObject.Parse(fileContents);
            foreach (var entry in jObject) {
                if(!dictionary.ContainsKey(entry.Key)) {
                    dictionary.Add(entry.Key, (string)entry.Value);
                }
            }
        }
        if(useDefault) {
            defaultDictionary =  dictionary;
        } else {
            myDictionary = dictionary;
        }
    }

    //Checks if the given key "objectName" is in myDictionary, if it's not, logs error; 
    //otherwise returns the string of the given key.
    public string GetName(string objectName)
    {
        bool useDefault=false;
        if (!myDictionary.ContainsKey(objectName))
        {
            if(defaultDictionary.ContainsKey(objectName)) {
                useDefault=true;
            } else {
                SimvaPlugin.Instance.LogError("The sequence with key " + objectName + " doesn't exit (Object " + this.gameObject.name + ")");
                return null;
            }
        }
        Dictionary<string, string> dictionary;
        if(useDefault) {
            dictionary = defaultDictionary;
        } else {
            dictionary = myDictionary;
        }
        string newWord = dictionary[objectName];
        if (newWord.Contains("\\n"))
            newWord = dictionary[objectName].Replace("\\n", "\n");

        SimvaPlugin.Instance.Log(objectName + " : " + newWord);
        return newWord;
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
        GameObject.DestroyImmediate(this.gameObject);
    }
    
    public override void Destroy()
    {
        SetActive(false);
    }
}
}