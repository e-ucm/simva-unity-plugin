using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Simva
{
    public static class SimvaSceneManager
    {
        public static GameObject LoadPrefabScene(string name)
        {
            GameObject form = null;
            switch (name)
            {
                case "Simva.Login.Demo":
                    form = GameObject.Instantiate(Resources.Load<GameObject>("SimvaLoginDemo"));
                    break;
                case "Simva.Login":
                    form = GameObject.Instantiate(Resources.Load<GameObject>("SimvaLogin"));
                    break;
                case "Simva.Survey":
                    form = GameObject.Instantiate(Resources.Load<GameObject>("SimvaSurvey"));
                    break;
                case "Simva.Finalize":
                    form = GameObject.Instantiate(Resources.Load<GameObject>("SimvaFinalize"));
                    break;
                case "Simva.End":
                    form = GameObject.Instantiate(Resources.Load<GameObject>("SimvaEnd"));
                    break;
            }
            return form;
        }

        public static Scene LoadScene(string name) {
            // Set Scene2 as the active Scene
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(name));

            // Ouput the name of the active Scene
            // See now that the name is updated
            SimvaPlugin.Instance.Log("Active Scene : " + SceneManager.GetActiveScene().name);

            return SceneManager.GetActiveScene();
        }
    }
}

