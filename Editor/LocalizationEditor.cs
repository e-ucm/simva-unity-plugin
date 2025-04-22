using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

[CustomEditor(typeof(Simva.SimvaPlugin))]
public class LocalizationEditor : Editor
{
    private string[] languageOptions;
    private int selectedIndex = 0;
    private bool AutoStart;
    private bool EnableLanguageScene;

    public override void OnInspectorGUI()
    {
        // Lazy-load language options
        LoadLanguageOptions();
        var settings = (Simva.SimvaPlugin)target;
        // Draw all other fields normally, except LanguageByDefault
        serializedObject.Update();
        SerializedProperty prop = serializedObject.GetIterator();
        bool enterChildren = true;
        while (prop.NextVisible(enterChildren))
        {
            bool enableProp=true;
            if (prop.name == "AutoStart") {
                AutoStart=prop.boolValue;
            }  else if (prop.name == "StartScene") {
                if(AutoStart) {
                    enableProp=false;
                }
            } else if (prop.name == "EnableLanguageScene") {
                EnableLanguageScene=prop.boolValue;
            } else if(prop.name == "SelectedLanguages") {
                enableProp=false;
                if(EnableLanguageScene) {
                    foreach (var lang in languageOptions)
                    {
                        bool isSelected = settings.SelectedLanguages.Contains(lang);
                        bool newSelected = EditorGUILayout.ToggleLeft(lang, isSelected);
                        if (newSelected && !isSelected)
                            settings.SelectedLanguages.Add(lang);
                        else if (!newSelected && isSelected)
                            settings.SelectedLanguages.Remove(lang);
                    }
                }
            } else if (prop.name == "LanguageByDefault") {
                enableProp=false;
                if(!EnableLanguageScene && AutoStart) {
                    var actual = settings.LanguageByDefault;
                    // Set current selection
                    selectedIndex = System.Array.IndexOf(languageOptions, actual);
                    if (selectedIndex < 0) selectedIndex = 0;

                    // Draw dropdown
                    selectedIndex = EditorGUILayout.Popup("Language By Default", selectedIndex, languageOptions);
                    settings.LanguageByDefault = languageOptions[selectedIndex];
                }
            }
            if(enableProp) {
                EditorGUILayout.PropertyField(prop, true);
                enterChildren = false;
            }
        }
        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(settings);
        }
    }

    private void LoadLanguageOptions()
    {
        TextAsset[] allLanguageMarkers = Resources.LoadAll<TextAsset>("Localization");

        Dictionary<string, string> languages = new Dictionary<string, string>();
        foreach (TextAsset asset in allLanguageMarkers) {
            if(asset.name == "lang") {
                JObject jObject = JObject.Parse(asset.text);
                var code="";
                var name="";
                foreach (var entry in jObject) {
                    if(entry.Key=="code") {
                        code=(string)entry.Value;
                    }
                    if(entry.Key=="displayName") {
                        name=(string)entry.Value;
                    }
                }
                var modifName=name + " [" + code + "]";
                if(!languages.ContainsKey(code)) {
                    languages.Add(code, modifName);
                }
            }
        }
        languageOptions=languages.Values
            .Distinct()
            .OrderBy(name => name)
            .ToArray();
    }
}
