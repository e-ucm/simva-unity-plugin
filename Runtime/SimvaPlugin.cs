using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityFx.Async;
using Xasu.Auth.Protocols;
using Xasu.Auth.Protocols.OAuth2;
using Xasu.Config;

namespace Simva
{

    public class SimvaPlugin : MonoBehaviour, ISimvaBridge
    {
        private SimvaSceneController previousController;

        public void Start()
        {
            RunScene("Simva.Login");
        }

        public void Demo()
        {
            throw new System.NotImplementedException();
        }

        public void OnAuthUpdated(OAuth2Token token)
        {
            throw new System.NotImplementedException();
        }

        public void RunScene(string name)
        {
            DestroyPreviousSimvaScene();
            var go = SimvaSceneManager.LoadPrefabScene(name);
            var controller = go.GetComponent<SimvaSceneController>();
            controller.Render();
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
            throw new System.NotImplementedException();
        }

        public IAsyncOperation StartTracker(TrackerConfig config, IAuthProtocol onlineProtocol, IAuthProtocol backupProtocol)
        {
            throw new System.NotImplementedException();
        }
    }
}

