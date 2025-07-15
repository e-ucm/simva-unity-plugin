using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

namespace Simva
{
    public static class SimvaSceneManager
    {
        public static GameObject LoadPrefabScene(string name)
        {
            GameObject form = null;
            switch (name)
            {
                case "Simva.Language":
                    form = GameObject.Instantiate(Resources.Load<GameObject>("SimvaLanguage"));
                    break;
                case "Simva.Login":
                    form = GameObject.Instantiate(Resources.Load<GameObject>("SimvaLogin"));
                    break;
                case "Simva.Survey":
                    form = GameObject.Instantiate(Resources.Load<GameObject>("SimvaSurvey"));
                    break;
                case "Simva.Manual":
                    form = GameObject.Instantiate(Resources.Load<GameObject>("SimvaManual"));
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

        public static void LoadScene(string name)
        {
            CoroutineRunner.Instance.StartCoroutine(LoadSceneCoroutine(name));
        }

        private static IEnumerator LoadSceneCoroutine(string name)
        {
            Scene currentScene = SceneManager.GetActiveScene();

            // Load the new scene additively
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
            yield return new WaitUntil(() => loadOp.isDone);

            // Set the newly loaded scene as active
            Scene newScene = SceneManager.GetSceneByName(name);
            SceneManager.SetActiveScene(newScene);

            // Unload the previous scene
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(currentScene);
            yield return new WaitUntil(() => unloadOp.isDone);
        }
    }
}

