using UnityEngine;

namespace Simva
{
    // Manager for "Simva.End"
    public class EndController : SimvaSceneController
    {
        public void Quit()
        {
            if (Application.isEditor) {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#endif
            } else {
                    Application.Quit();
            }
        }

        public override void Render()
        {
            Ready = true;
        }

        public override void Destroy()
        {
            GameObject.DestroyImmediate(this.gameObject);
        }
    }
}

