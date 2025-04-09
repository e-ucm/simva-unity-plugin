using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Xasu.HighLevel;

namespace Simva
{
    public class SimvaGameplayManualTester : MonoBehaviour
    {
        public GameObject canvasStart;
        public GameObject canvasGamePlay;
        public async void StartGame()
        {
            while (SimvaPlugin.Instance == null)
                await Task.Yield();   
            canvasStart.SetActive(false);
            canvasGamePlay.SetActive(true);
            StartCoroutine(SimvaPlugin.Instance.ManualStart("en_UK"));
        }

        public void SendTrace()
        {
            GameObjectTracker.Instance.Interacted("simple_button");
        }

        public void End()
        {
            if (Application.isEditor)
            {
#if UNITY_EDITOR
                var wants = ((SimvaPlugin)SimvaManager.Instance.Bridge).WantsToQuit();
                if (wants)
                {
                    UnityEditor.EditorApplication.isPlaying = false;
                }
#endif
            }
            else
            {
                Application.Quit();
            }
        }
    }
}