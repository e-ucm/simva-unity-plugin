using Newtonsoft.Json;
using System;
using System.Collections;
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
        public static SimvaPlugin Instance { get; private set; }
        public bool SaveAuthUntilCompleted = true;
        public bool ShowLoginOnStartup = true;
        public bool RunGameIfSimvaIsNotConfigured = true;
        public bool ContinueOnQuit = true;
        public bool AutoStart = true;
        public bool EnableLoginDemoButton = true;
        public string GamePlayScene;
        public string SimvaScene;
        public bool EnableDebugLogging = false;
        private SimvaSceneController previousController;
        private OAuth2Token lastAuth;
        

        
        public IEnumerator Start()
        {
            if(AutoStart)
                yield return ManualStart();
        }

        public IEnumerator ManualStart()
        {
            Instance = this;
            if(SimvaManager.Instance.Bridge != null)
            {
                DestroyImmediate(this.gameObject);
                yield break;
            }


            Log("[SIMVA] Starting...");
            if (SimvaConf.Local == null)
            {
                SimvaConf.Local = new SimvaConf();
                yield return StartCoroutine(SimvaConf.Local.LoadAsync());
                Log("[SIMVA] Conf Loaded...");
            }

            if (!SimvaManager.Instance.IsEnabled)
            {
                if (RunGameIfSimvaIsNotConfigured)
                {
                    Log("[SIMVA] Study is not set! Running the game without Simva...");
                    StartGameplay();
                }
                else
                {
                    Log("[SIMVA] Study is not set! Stopping...");
                    if (Application.isEditor)
                    {
#if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
#endif
                    } else {
                        Application.Quit();
                    }
                }
                yield break;
            }
            else if (SimvaManager.Instance.IsActive && !SimvaManager.Instance.Finalized)
            {
                Log("[SIMVA] Simva is already started...");
                // No need to restart
                yield break;
            }

            SimvaManager.Instance.Bridge = this;
            DontDestroyOnLoad(this.gameObject);

            if (ShowLoginOnStartup)
            {
                Log("[SIMVA] Setting current target to Simva.Login...");
                if(EnableLoginDemoButton) {
                    RunScene("Simva.Login.Demo");
                } else {
                    RunScene("Simva.Login");
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
            if (GamePlayScene != "")
            {
                SceneManager.LoadSceneAsync(GamePlayScene);
            }
        }

        public void OnAuthUpdated(OAuth2Token token)
        {
            lastAuth = token;
        }

        public void RunScene(string name)
        {
            if(SimvaScene == "") {
                DoRunScene(name);
            } else {
                if (SceneManager.GetActiveScene().name != SimvaScene)
                {
                    SceneManager.LoadSceneAsync(SimvaScene).completed += ev =>
                    {
                        DoRunScene(name);
                    };
                } else {
                    DoRunScene(name);
                }
            }
        }

        private void DoRunScene(string name)
        {
            Log(name);
            DestroyPreviousSimvaScene();
            var go = SimvaSceneManager.LoadPrefabScene(name);
            var controller = go.GetComponent<SimvaSceneController>();
            controller.Render();
            previousController = controller;
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
            if(GamePlayScene != "") {
                if (SceneManager.GetActiveScene().name != GamePlayScene)
                {
                    SceneManager.LoadScene(GamePlayScene);
                }
            } else {
                throw new Exception("Please define GamePlayScene.");
            }
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

