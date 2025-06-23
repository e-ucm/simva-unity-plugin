using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityFx.Async;
using Xasu;
using Xasu.Auth.Protocols;
using Xasu.Auth.Protocols.OAuth2;
using Xasu.Config;
using UnityEngine.SceneManagement;

namespace Simva
{

    public class SimvaPlugin : MonoBehaviour, ISimvaBridge
    {
        public const string SIMVA_DISCLAIMER_ACCEPTED = "simva_disclaimer_accepted";
        public static SimvaPlugin Instance { get; private set; }
        public bool SaveAuthUntilCompleted = true;
        public bool ShowLoginOnStartup = true;
        public bool RunGameIfSimvaIsNotConfigured = true;
        public bool ContinueOnQuit = true;
        public bool EnableLoginDemoButton = true;
        public bool EnableLanguageScene=true;
        public List<string> SelectedLanguages = new List<string>();
        public string LanguageByDefault;
        public bool AutoStart = true;
        public string StartScene;
        public string GamePlayScene;
        public bool SaveDisclaimerAccepted=false;
        public bool EnableDebugLogging = false;
        private SimvaSceneController previousController;
        private LanguageSelectorController languageSelector;
        private OAuth2Token lastAuth;
        private static Dictionary<string, string> myDictionary;
        private static Dictionary<string, string> defaultDictionary;
        
        void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(Instance.gameObject);
        }

        public IEnumerator Start()
        {
            foreach(var lang in SelectedLanguages)
                Log(lang);
            if(SimvaManager.Instance.Bridge != null)
            {
                DestroyImmediate(this.gameObject);
                yield break;
            }
            if (string.IsNullOrEmpty(GamePlayScene))
            {
                throw new Exception("Please provide your GamePlay Scene name.");
            }
            if (EnableLanguageScene && SelectedLanguages.Count == 0)
            {
                throw new Exception("Please select one or more languages for language scene name.");
            }
            if(AutoStart) {
                yield return ManualStart();
            } else {
                if (string.IsNullOrEmpty(StartScene))
                {
                    throw new Exception("Please provide your StartScene Scene name if not Autostart.");
                }
                RunScene("StartMenu");
            }
        }

        public IEnumerator ManualStart(string selectedLanguage = "")
        {
            Log("[SIMVA] Starting...");
            if (SimvaConf.Local == null)
            {
                SimvaConf.Local = new SimvaConf();
                yield return StartCoroutine(SimvaConf.Local.LoadAsync());
                Log("Conf Loaded...");
            }

            if (PlayerPrefs.HasKey(SIMVA_DISCLAIMER_ACCEPTED) && !SaveDisclaimerAccepted)
            {
                PlayerPrefs.DeleteKey(SIMVA_DISCLAIMER_ACCEPTED);
            }

            if (!SimvaManager.Instance.IsEnabled)
            {
                if (RunGameIfSimvaIsNotConfigured)
                {
                    Log("Study is not set! Running the game without Simva...");
                    StartGameplay();
                }
                else
                {
                    Log("Study is not set! Stopping...");
                    if (Application.isEditor)
                    {
#if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
#endif
                    }
                    else
                    {
                        Application.Quit();
                    }
                }
                yield break;
            }
            else if (SimvaManager.Instance.IsActive && !SimvaManager.Instance.Finalized)
            {
                Log("[SIMVA] Simva is already started...");
                yield return null;
            }
            SimvaManager.Instance.Bridge = this;
            DontDestroyOnLoad(this.gameObject);

            if (ShowLoginOnStartup)
            {
                Log("[SIMVA] Setting current target to Simva.Language and to Simva.Login...");
                if (!EnableLanguageScene)
                {
                    if (LanguageSelectorController.instance == null)
                    {
                        Instance.gameObject.AddComponent<LanguageSelectorController>();
                    }
                    Log("Set language to " + LanguageByDefault);
                    LanguageSelectorController.instance.SetActive(false);
                    if (string.IsNullOrEmpty(selectedLanguage))
                    {
                        LanguageSelectorController.instance.FillDictionaryAndRunLoginScene(LanguageByDefault);
                    }
                    else
                    {
                        LanguageSelectorController.instance.FillDictionaryAndRunLoginScene(selectedLanguage);
                    }
                }
                else
                {
                    RunScene("Simva.Language");
                }

                if (PlayerPrefs.HasKey("simva_auth") && SaveAuthUntilCompleted)
                {
                    var auth = JsonConvert.DeserializeObject<OAuth2Token>(PlayerPrefs.GetString("simva_auth"));
                    yield return new WaitForFixedUpdate();
                    SimvaManager.Instance.LoginWithRefreshToken(auth.RefreshToken);
                }
            }

            if (ContinueOnQuit)
            {
                Application.wantsToQuit += WantsToQuit;
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused && lastAuth != null)
            {
                PlayerPrefs.SetString("simva_auth", JsonConvert.SerializeObject(lastAuth));
            }
        }
        
        public bool WantsToQuit()
        {
            if (SimvaManager.Instance.IsActive)
            {
                SimvaManager.Instance.OnGameFinished();
                return false;
            }
            else
            {
                PlayerPrefs.DeleteKey("simva_auth");
            }
            return true;
        }

        public void Demo()
        {
            Log("Starting Demo Gameplay");
            StartGameplay();
        }

        public void OnAuthUpdated(OAuth2Token token)
        {
            lastAuth = token;
        }

        public void RunScene(string sceneName)
        {
            var name="";
            switch (sceneName)
            {
                case "Simva.Login":
                    name = EnableLoginDemoButton ? "Simva.Login.Demo" : "Simva.Login";
                    break;

                case "Gameplay":
                    if (!string.IsNullOrEmpty(GamePlayScene))
                    {
                        name = GamePlayScene;
                    }
                    else
                    {
                        throw new Exception("Please provide your GamePlay Scene name.");
                    }
                    break;
                case "StartMenu":
                    name=StartScene;
                    break;
                default:
                    name = sceneName;
                    break;
            }
            if (SceneManager.GetActiveScene().name != name)
            {
                DoRunScene(name);
            }
        }

        private void DoRunScene(string name)
        {
            Log("Running scene: " + name);
            DestroyPreviousSimvaScene();
            var go = SimvaSceneManager.LoadPrefabScene(name);
            if (go == null) {
                SimvaSceneManager.LoadScene(name);
            } else {
                var controller = go.GetComponent<SimvaSceneController>();
                controller.Render();
                previousController = controller;
            }
        }

        private void DestroyPreviousSimvaScene()
        {
            if (previousController)
            {
                previousController.Destroy();
                previousController = null;
            }
        }

        public void StartGameplay()
        {
            DestroyPreviousSimvaScene();
            Log("Starting Gameplay");
            RunScene("Gameplay");
        }

        public IAsyncOperation StartTracker(TrackerConfig config, IAuthProtocol onlineProtocol, IAuthProtocol backupProtocol)
        {
            Log("Starting Tracker");
            var result = new AsyncCompletionSource();
            XasuTracker.Instance.Init(config, onlineProtocol, backupProtocol, EnableDebugLogging)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        result.SetException(t.Exception);
                        return;
                    }
                    result.SetCompleted();
                }, TaskScheduler.FromCurrentSynchronizationContext());
            return result;
        }

        public void SetLanguageDictionary(Dictionary<string, string> dictionary, bool defaultDict)
        {
            if (defaultDict)
            {
                defaultDictionary = dictionary;
            }
            else
            {
                myDictionary = dictionary;
            }
        }

        //Checks if the given key "objectName" is in myDictionary, if it's not, logs error; 
        //otherwise returns the string of the given key.
        public string GetName(string objectName)
        {
            bool useDefault = false;
            if (!myDictionary.ContainsKey(objectName))
            {
                if (defaultDictionary.ContainsKey(objectName))
                {
                    useDefault = true;
                }
                else
                {
                    LogError("The sequence with key " + objectName + " doesn't exit (Object " + ")");
                    return null;
                }
            }
            Dictionary<string, string> dictionary;
            if (useDefault)
            {
                dictionary = defaultDictionary;
            }
            else
            {
                dictionary = myDictionary;
            }
            string newWord = dictionary[objectName];
            if (newWord.Contains("\\n"))
                newWord = dictionary[objectName].Replace("\\n", "\n");

            Log(objectName + " : " + newWord);
            return newWord;
        }


        public IAsyncOperation StopTracker()
        {
            Log("Stopping Tracker");
            var progress = new Progress<float>();
            progress.ProgressChanged += (_, p) =>
            {
                Debug.Log("Finalization progress: " + p);
            };
            Xasu.XasuTracker.Instance.Finalize(progress)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        return;
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
            return null;
        }

        internal void Log(string message)
        {
            if (EnableDebugLogging)
            {
                Debug.Log("[SimvaUnityPlugin] " + message);
            }
        }

        internal void UnityEngineLog(string message)
        {
            if (EnableDebugLogging)
            {
                UnityEngine.Debug.Log("[SimvaUnityPlugin] " + message);
            }
        }

        internal void LogWarning(string message)
        {
            if (EnableDebugLogging)
            {
                Debug.LogWarning("[SimvaUnityPlugin] " + message);
            }
        }

        internal void LogError(string message)
        {
            if (EnableDebugLogging)
            {
                Debug.LogError("[SimvaUnityPlugin] " + message);
            }
        }
    }
}