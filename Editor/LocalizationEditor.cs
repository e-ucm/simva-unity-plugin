using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

[CustomEditor(typeof(Simva.SimvaPlugin))]
public class LocalizationEditor : Editor
{
    private string[] languageOptions;
    private int selectedIndex = 0;
    private bool AutoStart;
    private bool EnableLanguageScene;

    public override void OnInspectorGUI()
    {
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
                enableProp=true;
            }  else if (prop.name == "LanguageByDefault") {
                enableProp=false;
                if(!EnableLanguageScene && AutoStart) {
                    // Lazy-load language options
                    if (languageOptions == null || languageOptions.Length == 0)
                    {
                        LoadLanguageOptions();
                    }
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

    languageOptions = allLanguageMarkers
        .Select(asset =>
        {
            if(asset.name == "lang") {
                string path = AssetDatabase.GetAssetPath(asset);
                string folderName = Path.GetFileName(Path.GetDirectoryName(path));
                return folderName;
            } else {
                return "";
            }
        })
        .Distinct()
        .OrderBy(name => name)
        .ToArray();
    }

}
