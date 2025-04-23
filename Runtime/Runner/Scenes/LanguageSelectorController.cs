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
    private static Dictionary<string, string> languages;
    private static Dictionary<string, string> myDictionary;
    private static string Language;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
        SetActive(true);
        GetLanguages();
        RefreshLanguageList();
        DontDestroyOnLoad(gameObject);
    }

    public void Back()
    {
        GameObject.DestroyImmediate(this.gameObject);
    }

    public void SetLanguageFromTitle(string title) {
        foreach(string languageCode in languages.Keys) {
            if(languages[languageCode] == title) {
                Language = languageCode;
            }
        }
        Debug.LogError("Language not found. (" + title + ")");
    }

    //Selects a language by flag button in Title scene
    public void fillDictionaryAndRunLoginScene()
    {
        SetUpJSONFiles();
        FillDictionary();
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
                var code="";
                var name="";
                var custom=false;
                foreach (var entry in jObject) {
                    if(entry.Key=="code") {
                        code=(string)entry.Value;
                    }
                    if(entry.Key=="displayName") {
                        name=(string)entry.Value;
                    }
                    if(entry.Key=="custom") {
                        custom=(bool)entry.Value;
                    }
                }
                var modifName=name + " [" + code + "]";
                if(!languages.ContainsKey(code)) {
                    languages.Add(code, modifName);
                } else {
                    //if(custom) {
                    //    languages.Add(code, modifName);
                    //}
                }
            }
        }
        Debug.Log("Languages : " + languages.Count);
    }

    //Creates an initializes Json Files array to be used by myDictionary
    void SetUpJSONFiles()
    {
        Object[] filler = Resources.LoadAll("Localization/" + Language + "/" + "Dictionaries", typeof(TextAsset));

        if (filler == null || filler.Length == 0)
        {
            Debug.LogError("No JSON Files in Dictionaries directory found (Localization/" + Language + "/" + "Dictionaries) !");
        }

        jsonFiles = new List<TextAsset>();
        foreach (Object file in filler)
        {
            jsonFiles.Add((TextAsset)file);
        }

#if UNITY_EDITOR
        foreach (var t in jsonFiles)
            Debug.Log("JSON File added for Language " + Language + " : " + t.name);
#endif

    }

    //Fills myDictionary with all JSON files
    void FillDictionary()
    {
        myDictionary = new Dictionary<string, string>();

        string fileContents;
        foreach (var jsonFile in jsonFiles)
        {
            SimvaPlugin.Instance.Log("JSON File added : " + jsonFile.name);
            fileContents = jsonFile.text;
            
            JObject jObject = JObject.Parse(fileContents);
            foreach (var entry in jObject) {
                if(!myDictionary.ContainsKey(entry.Key)) {
                    myDictionary.Add(entry.Key, (string)entry.Value);
                }
            }
        }
    }

    //Checks if the given key "objectName" is in myDictionary, if it's not, logs error; 
    //otherwise returns the string of the given key.
    public string GetName(string objectName)
    {
        if (!myDictionary.ContainsKey(objectName))
        {
            SimvaPlugin.Instance.LogError("The sequence with key " + objectName + " doesn't exit (Object " + this.gameObject.name + ")");
            return null;
        }

        string newWord = myDictionary[objectName];
        if (newWord.Contains("\\n"))
            newWord = myDictionary[objectName].Replace("\\n", "\n");

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

    public override void Destroy()
    {
        SetActive(false);
    }
}
}