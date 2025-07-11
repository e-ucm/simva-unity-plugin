using UnityEngine;
using UnityFx.Async.Promises;
using System;
using Newtonsoft.Json;

namespace Simva
{
    // Manager for "Simva.Manual"
    public class ManualController : SimvaSceneController
    {
        [SerializeField]
        private UnityEngine.UI.Text label;
        private bool manualOpened;

        public void OpenManual()
        {
            var simvaExtension = SimvaManager.Instance;
            simvaExtension.NotifyLoading(true);
            string activityId = simvaExtension.CurrentActivityId;
            string username = simvaExtension.API.Authorization.Agent.account.name;
            var url=SimvaManager.Instance.Schedule.Url;
            Application.OpenURL(url);
            simvaExtension.NotifyLoading(false);
            manualOpened = true;

        }

        protected void OnApplicationResume()
        {
            if (manualOpened)
            {
                CheckManual();
                manualOpened = false;
            }
        }

        public void CheckManual()
        {
            if (SimvaManager.Instance.Schedule.Activities[SimvaManager.Instance.CurrentActivityId].Details.Uri != null)
            {
                if (!manualOpened)
                {
                    SimvaManager.Instance.NotifyManagers(SimvaPlugin.Instance.GetName("NotOpenedManual"));
                }
                else
                {
                    SimvaManager.Instance.ContinueActivity();
                }
            }
            else
            {
                SimvaManager.Instance.ContinueActivity();
            }
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
        }

        public override void Destroy()
        {
            GameObject.DestroyImmediate(gameObject);
        }
    }
}

