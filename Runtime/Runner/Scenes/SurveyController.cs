using UnityEngine;
using UnityFx.Async.Promises;
using System;

namespace Simva
{
    // Manager for "Simva.Survey"
    public class SurveyController : SimvaSceneController
    {
        [SerializeField]
        private UnityEngine.UI.Text label;
        private bool surveyOpened;

        public void OpenSurveyPreviousVersion()
        {
            var simvaExtension = SimvaManager.Instance;
            simvaExtension.NotifyLoading(true);
            SimvaPlugin.Instance.Log("[SIMVA]" + simvaExtension.CurrentActivityId);
            string activityId = simvaExtension.CurrentActivityId;
            string username = simvaExtension.API.Authorization.Agent.account.name;
            simvaExtension.API.Api.GetActivityTarget(activityId)
                .Then(result =>
                {
                    simvaExtension.NotifyLoading(false);
                    surveyOpened = true;
                    Application.OpenURL(result[username]);
                })
                .Catch(error =>
                {
                    SimvaPlugin.Instance.Log("[SIMVA]" + error.Message);
                    simvaExtension.NotifyManagers(error.Message);
                    simvaExtension.NotifyLoading(false);
                });
        }

        public void OpenSurvey()
        {
            var simvaExtension = SimvaManager.Instance;
            simvaExtension.NotifyLoading(true);
            string activityId = simvaExtension.CurrentActivityId;
            string username = simvaExtension.API.Authorization.Agent.account.name;
            var url=SimvaManager.Instance.Schedule.Url;
            if (url != "")
            {
                Application.OpenURL(url);
                simvaExtension.NotifyLoading(false);
                surveyOpened = true;
            }
            else
            {
                OpenSurveyPreviousVersion();
            }
        }

        protected void OnApplicationResume()
        {
            if (surveyOpened)
            {
                surveyOpened = false;
                CheckSurvey();
            }
        }

        public void CheckSurvey()
        {
            SimvaManager.Instance.ContinueSurvey();
        }

        public void Back()
        {
            PlayerPrefs.DeleteKey("simva_auth");
            SimvaPlugin.Instance.RunScene("Simva.Login");
        }

        public override void Render()
        {
            Ready = true;
            label.text = SimvaManager.Instance.Schedule.Activities[SimvaManager.Instance.CurrentActivityId].Name;
            //var background = GameObject.Find("background").GetComponent<Image>();
            /*var backgroundPath = 
            var backgroundSprite = Game.Instance.ResourceManager.getSprite();
            background.sprite = Game.Instance.ResourceManager.getSprite()*/
        }

        public override void Destroy()
        {
            GameObject.DestroyImmediate(gameObject);
        }
    }
}

