using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Xasu.HighLevel;

namespace Simva
{
    public class SimvaGameplayTester : MonoBehaviour
    {
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