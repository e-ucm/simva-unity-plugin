using Newtonsoft.Json;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityFx.Async;
using Xasu;
using Xasu.Auth.Protocols;
using Xasu.Auth.Protocols.OAuth2;
using Xasu.Config;

namespace Simva
{

    public class SimvaPlugin : MonoBehaviour, ISimvaBridge
    {
        public bool SaveAuthUntilCompleted = true;
        public bool ShowLoginOnStartup = true;
        public bool ContinueOnQuit = true;
        private SimvaSceneController previousController;
        private OAuth2Token lastAuth;

        public IEnumerator Start()
        {
            SimvaManager.Instance.Bridge = this;

            Debug.Log("[SIMVA] Starting...");
            if (SimvaConf.Local == null)
            {
                SimvaConf.Local = new SimvaConf();
                yield return StartCoroutine(SimvaConf.Local.LoadAsync());
                Debug.Log("[SIMVA] Conf Loaded...");
            }

            if (!SimvaManager.Instance.IsEnabled)
            {
                Debug.Log("[SIMVA] Study is not set! Stopping...");
                yield break;
            }
            else if (SimvaManager.Instance.IsActive)
            {
                Debug.Log("[SIMVA] Simva is already started...");
                // No need to restart
                yield break;
            }

            if (ShowLoginOnStartup)
            {
                Debug.Log("[SIMVA] Setting current target to Simva.Login...");
                RunScene("Simva.Login");

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
            Debug.Log("Demo!");
        }

        public void OnAuthUpdated(OAuth2Token token)
        {
            lastAuth = token;
        }

        public void RunScene(string name)
        {
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
            Debug.Log("Starting Gameplay");
        }

        public IAsyncOperation StartTracker(TrackerConfig config, IAuthProtocol onlineProtocol, IAuthProtocol backupProtocol)
        {
            Debug.Log("Starting Tracker");
            var result = new AsyncCompletionSource();
            XasuTracker.Instance.Init(config, onlineProtocol, backupProtocol)
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
    }
}

