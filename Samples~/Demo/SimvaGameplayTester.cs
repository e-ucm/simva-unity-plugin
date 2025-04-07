using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Xasu.HighLevel;

public class SimvaGameplayTester : MonoBehaviour
{
        public void SendTrace()
        {
            GameObjectTracker.Instance.Interacted("simple_button");
        }


        public void End()
        {
            var wants = Simva.SimvaPlugin.Instance.WantsToQuit();
            if (wants)
            {
                if (Application.isEditor)
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#endif
                } else {
                    Application.Quit();
                }
            }
        }
}