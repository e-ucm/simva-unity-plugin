using System;
using UnityEngine;
using UnityFx.Async;
using UnityFx.Async.Promises;
using SimvaPlugin;
using Simva.Api;
using Simva.Model;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Newtonsoft.Json;
using Xasu.Auth.Protocols;
using Xasu.Auth.Protocols.OAuth2;
using Xasu.HighLevel;

namespace Simva
{
    public class SimvaManager : MonoBehaviour
    {
        private static SimvaManager instance;
        private bool replayableProcessed;
        private string replayableActivityId;

        public static SimvaManager Instance 
        { 
            get 
            { 
                if(instance == null)
                {
                    instance = new GameObject("SimvaManager", typeof(SimvaManager)).GetComponent<SimvaManager>();
                    DontDestroyOnLoad(instance.gameObject);
                }
                return instance; 
            } 
        }

        public delegate void LoadingDelegate(bool loading);
        public delegate void ResponseDelegate(string message);

        private LoadingDelegate loadingListeners;
        private ResponseDelegate responseListeners;
        private OAuth2Token auth;
        public ISimvaBridge Bridge { get; set; }

        public bool Finalized { get; protected set; }
        public bool HasStartedGameplay { get; set; }

        private Schedule schedule;
        public Schedule Schedule
        {
            get => schedule;
            set
            {
                schedule = value;
            }
        }

        public SimvaApi<IStudentsApi> API { get; set; }

        public bool IsActive
        {
            get
            {
                return this.API != null && this.auth != null && Schedule != null && Bridge != null;
            }
        }

        public string CurrentActivityId
        {
            get
            {
                return currentActivity?.Id;
            }
        }

        public Activity currentActivity;

        private string attemptId;

        private string activityUrl="";

        private string homePage;

        public bool IsEnabled
        {
            get
            {
                return !string.IsNullOrEmpty(SimvaConf.Local.Simlet);
            }
        }

        protected SimvaManager()
        {

        }

        public void AfterFinalize()
        {
            Finalized = true;
        }


        public IAsyncOperation InitUser()
        {
            return LoginAndSchedule();
        }

        public void Demo()
        {
            Bridge.Demo();
        }

        public IAsyncOperation LoginWithRefreshToken(string refreshToken)
        {
            NotifyLoading(true);
            return SimvaApi<IStudentsApi>.Login(refreshToken)
                    .Then(simvaController =>
                    {
                        this.API = simvaController;
                        RegisterAuthInfoUpdate();
                        return UpdateSchedule();
                    })
                    .Then(schedule =>
                    {
                        return LaunchActivityById(schedule.Next);
                    })
                    .Catch(error =>
                    {
                        NotifyLoading(false);
                        NotifyManagers(error.Message);
                    });
        }

        public IAsyncOperation LoginAndSchedule()
        {
            NotifyLoading(true);
            return SimvaApi<IStudentsApi>.Login()
                .Then(simvaController =>
                {
                    this.API = simvaController;
                    RegisterAuthInfoUpdate();
                    return UpdateSchedule();
                })
                .Then(schedule =>
                {
                    return LaunchActivityById(schedule.Next);
                })
                .Catch(error =>
                {
                    NotifyLoading(false);
                    var msg = SimvaPlugin.Instance.GetName("InvalidLoginMsg");
                    SimvaPlugin.Instance.LogError(msg + ": " + error.ToString());
                    NotifyManagers(msg);
                });
        }

        public IAsyncOperation LoginAndScheduleDevice()
        {
            NotifyLoading(true);
            return SimvaApi<IStudentsApi>.LoginDevice()
                .Then(simvaController =>
                {
                    this.API = simvaController;
                    RegisterAuthInfoUpdate();
                    return UpdateSchedule();
                })
                .Then(schedule =>
                {
                    return LaunchActivityById(schedule.Next);
                })
                .Catch(error =>
                {
                    NotifyLoading(false);
                    var msg = SimvaPlugin.Instance.GetName("InvalidLoginMsg");
                    SimvaPlugin.Instance.LogError(msg + ": " + error.ToString());
                    NotifyManagers(msg);
                });
        }

        public IAsyncOperation LoginAndSchedule(string token)
        {
            NotifyLoading(true);
            return SimvaApi<IStudentsApi>.LoginWithToken(token)
                .Then(simvaController =>
                {
                    this.API = simvaController;
                    RegisterAuthInfoUpdate();
                    PlayerPrefs.SetString("simva_auth", JsonConvert.SerializeObject(auth));
                    PlayerPrefs.Save();
                    return UpdateSchedule();
                })
                .Then(schedule =>
                {
                    return LaunchActivityById(schedule.Next);
                })
                .Catch(error =>
                {
                    NotifyLoading(false);
                    var msg = SimvaPlugin.Instance.GetName("InvalidLoginMsg");
                    SimvaPlugin.Instance.LogError(msg + ": " + error.ToString());
                    NotifyManagers(msg);
                });
        }

        public IAsyncOperation LoginAndSchedule(string username, string password)
        {
            NotifyLoading(true);
            return SimvaApi<IStudentsApi>.LoginWithCredentials(username, password)
                .Then(simvaController =>
                {
                    this.API = simvaController;
                    RegisterAuthInfoUpdate();
                    PlayerPrefs.SetString("simva_auth", JsonConvert.SerializeObject(auth));
                    PlayerPrefs.Save();
                    return UpdateSchedule();
                })
                .Then(schedule =>
                {
                    return LaunchActivityById(schedule.Next);
                })
                .Catch(error =>
                {
                    NotifyLoading(false);
                    var msg = SimvaPlugin.Instance.GetName("InvalidLoginMsg");
                    SimvaPlugin.Instance.LogError(msg + ": " + error.ToString());
                    NotifyManagers(msg);
                });
        }

        public IAsyncOperation ContinueLoginAndSchedule()
        {
            NotifyLoading(true);
            return SimvaApi<IStudentsApi>.ContinueLogin()
                .Then(simvaController =>
                {
                    this.API = simvaController;
                    RegisterAuthInfoUpdate();
                    return UpdateSchedule();
                })
                    .Then(schedule =>
                    {
                        return LaunchActivityById(schedule.Next);
                    })
                    .Catch(error =>
                    {
                        NotifyLoading(false);
                        NotifyManagers(error.Message);
                    });
            }


            public IAsyncOperation<Schedule> UpdateSchedule()
        {
            var result = new AsyncCompletionSource<Schedule>();

            API.Api.GetSchedule(API.SimvaConf.Simlet)
                .Then(schedule =>
                {
                    this.Schedule = schedule;
                    foreach (var a in schedule.Activities)
                    {
                        schedule.Activities[a.Key].Id = a.Key;
                    }
                    SimvaPlugin.Instance.Log("[SIMVA] Schedule: " + JsonConvert.SerializeObject(schedule));
                    result.SetResult(schedule);
                })
                .Catch(result.SetException);
            return result;
        }
        public IAsyncOperation SaveActivity(string activityId, string traces, bool completed)
        {
            NotifyLoading(true);

            var body = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(traces))
            {
                body.Add("tofile", true);
                body.Add("result", traces);
            }

            var result = new AsyncCompletionSource();

            var response = (AsyncCompletionSource)API.Api.SetResult(activityId, API.Authorization.Agent.account.name, body);
            response.AddProgressCallback((p) =>
            {
                SimvaPlugin.Instance.UnityEngineLog("SaveActivityAndContinue progress: " + p);
                if (!result.IsCompleted && !result.IsCanceled)
                {
                    result.SetProgress(p);
                }
            });

            response
                .Then(() =>
                {
                    NotifyLoading(false);
                    result.SetCompleted();
                })
                .Catch(e => {
                    result.SetException(e);
                });

            return result;
        }

        public IAsyncOperation Continue(string activityId, bool completed)
        {
            if (string.IsNullOrEmpty(activityId))
            {
                return UpdateSchedule();
            }
            NotifyLoading(true);
            return API.Api.SetCompletion(activityId, API.Authorization.Agent.account.name, completed)
                .Then(() =>
                {
                    return UpdateSchedule();
                })
                .Then(schedule =>
                {
                    return LaunchActivityById(schedule.Next);
                })
                .Finally(() =>
                {
                    NotifyLoading(false);
                })
                .Catch(error =>
                {
                    NotifyLoading(false);
                    NotifyManagers(error.Message);
                });
        }

        public IAsyncOperation ContinueActivity()
        {
            if (string.IsNullOrEmpty(CurrentActivityId))
            {
                return UpdateSchedule().Then(_ =>
                {
                    if (Schedule != null)
                    {
                        return LaunchActivityById(Schedule.Next);
                    }
                    else
                    {
                        var result = new AsyncCompletionSource();
                        result.SetException(new Exception(SimvaPlugin.Instance.GetName("NoScheduleMsg")));
                        return result;
                    }
                });
            }

            NotifyLoading(true);
            return API.Api.GetCompletion(CurrentActivityId, API.Authorization.Agent.account.name)
                .Then(result =>
                {
                    Schedule.Activities.TryGetValue(CurrentActivityId, out var activity);
                    if (result.TryGetValue(API.Authorization.Agent.account.name, out var completed) && completed && activity != null)
                    {
                        NotifyLoading(false);
                        if (CurrentActivityId == replayableActivityId)
                        {
                            replayableProcessed = true;
                            replayableActivityId = null;
                            return UpdateSchedule();
                        }
                        if (!replayableProcessed && Schedule.ReplayableActivities?.Count > 0)
                        {
                            var replayable = Schedule.ReplayableActivities
                                .FirstOrDefault(id => Schedule.Activities.TryGetValue(id, out var a) && a.Type == "gameplay");
                            if (replayable != null)
                            {
                                Schedule.ReplayableActivities.Remove(replayable);
                                replayableActivityId = replayable;
                                var restartRes = new AsyncCompletionSource<Schedule>();
                                StartCoroutine(AsyncCoroutine(LaunchActivity(replayable), (IAsyncCompletionSource)restartRes));
                                return restartRes;
                            }
                        }
                        replayableProcessed = true;
                        return UpdateSchedule();
                    }
                    else
                    {
                        NotifyLoading(false);
                        var res = new AsyncCompletionSource<Schedule>();
                        if (!Schedule.Activities.TryGetValue(CurrentActivityId, out var act))
                        {
                            res.SetException(new Exception(SimvaPlugin.Instance.GetName("NoScheduleMsg")));
                            return res;
                        }
                        switch (act.Type)
                        {
                            case "manual":
                                res.SetException(new Exception(SimvaPlugin.Instance.GetName("NotCompletedManualMsg")));
                                break;
                            case "survey":
                                NotifyManagers(SimvaPlugin.Instance.GetName("NotCompletedSurveyMsg"));
                                return res;
                    default:
                        res.SetException(new Exception(SimvaPlugin.Instance.GetName("NotCompletedMsg")));
                        break;
                        }
                        return res;
                    }
                })
                .Then(schedule =>
                {
                    if (schedule != null)
                    {
                        return LaunchActivityById(schedule.Next);
                    }
                    else
                    {
                        var result = new AsyncCompletionSource();
                        result.SetException(new Exception(SimvaPlugin.Instance.GetName("NoScheduleMsg")));
                        return result;
                    }
                })
                .Catch(error =>
                {
                    SimvaPlugin.Instance.Log("[SIMVA] GLOBAL : " + error.Message);
                    NotifyManagers(error.Message);
                    NotifyLoading(false);
                });
        }

        public IAsyncOperation SetCompletionAndUpdateSchedule()
        {
            NotifyLoading(true);
            if (string.IsNullOrEmpty(CurrentActivityId))
            {
                return UpdateSchedule();
            }
            return API.Api.SetCompletion(CurrentActivityId, API.Authorization.Agent.account.name, true)
                .Then(() =>
                {
                    return UpdateSchedule();
                })
                .Then(schedule =>
                {
                    if (schedule != null)
                    {
                        return LaunchActivityById(schedule.Next);
                    }
                    else
                    {
                        var result = new AsyncCompletionSource();
                        result.SetException(new Exception(SimvaPlugin.Instance.GetName("NoScheduleMsg")));
                        return result;
                    }
                })
                .Catch(error =>
                {
                    SimvaPlugin.Instance.Log("[SIMVA] GLOBAL : " + error.Message);
                    NotifyManagers(error.Message);
                    NotifyLoading(false);
                });
        }

        public Activity GetActivity(string activityId)
        {
            if (Schedule != null && !string.IsNullOrEmpty(activityId))
            {
                Schedule.Activities.TryGetValue(activityId, out var activity);
                return activity;
            }
            return null;
        }


        private void OnAuthInfoUpdate(OAuth2Token token)
        {
            this.auth = token;
            SimvaPlugin.Instance.Log("[SIMVA] Retrieved JWT: " + token.AccessToken + ". Username found: " + token.Username);
            Bridge.OnAuthUpdated(token);
        }

        private void RegisterAuthInfoUpdate()
        {
            if (API.Authorization is OAuth2Protocol oauth)
            {
                oauth.RegisterAuthInfoUpdate(OnAuthInfoUpdate);
            }
            else if (API.Authorization is OAuth2DeviceProtocol device)
            {
                device.RegisterAuthInfoUpdate(OnAuthInfoUpdate);
            }
        }


        private IEnumerator LaunchActivity(string activityId)
        {
            if (string.IsNullOrEmpty(activityId))
            {
                Bridge.RunScene("Simva.End");
                PlayerPrefs.DeleteKey("simva_auth");
                Schedule = null;
            }
            else
            {
                Activity activity = GetActivity(activityId);

                if (activity != null)
                {
                    currentActivity = activity;
                    SimvaPlugin.Instance.Log("[SIMVA] Schedule: " + activity.Type + ". Name: " + activity.Name + " activityId " + activityId);
                    switch (activity.Type)
                    {
                        case "manual":
                            SimvaPlugin.Instance.Log("[SIMVA] Starting Manual activity...");
                            Bridge.RunScene("Simva.Manual");
                            break;
                        case "limesurvey":
                            SimvaPlugin.Instance.Log("[SIMVA] Starting Survey...");
                            Bridge.RunScene("Simva.Survey");
                            break;
                        case "gameplay":
                        default:
                            SimvaPlugin.Instance.Log("[SIMVA] Getting Xasu tracker Config...");
                            var trackerStarted = false;
                            var xasuTrackerConfig = new Xasu.Config.TrackerConfig {
                                Offline = true,
                                TraceFormat = Xasu.Config.TraceFormats.XAPI,
                                FlushInterval = 3,
                                BatchSize = 256
                            };
                            if (API.SimvaConf.HomePage != null){
                                xasuTrackerConfig.HomePage = API.SimvaConf.HomePage;
                            }
                            if (activity.Details.TraceStorage)
                            {
                                SimvaPlugin.Instance.Log("[SIMVA] Starting trace storage tracker...");
                                xasuTrackerConfig.Online = true;
                                xasuTrackerConfig.Fallback = true;
                                var lrsPath = ApiClient.HasLrsPrefix ? string.Format("/activities/{0}/lrs", activityId) : string.Format("/activities/{0}", activityId);
                                SimvaPlugin.Instance.Log("[SIMVA] LRS endpoint: " + lrsPath + " (features: HasLrsPrefix=" + ApiClient.HasLrsPrefix + ")");
                                xasuTrackerConfig.LRSEndpoint = API.SimvaConf.URL + lrsPath;
                            }

                            if (activity.Details.Backup)
                            {
                                // Backup
                                SimvaPlugin.Instance.Log("[SIMVA] Starting backup tracker...");
                                xasuTrackerConfig.Backup = true;
                                xasuTrackerConfig.BackupEndpoint = API.SimvaConf.URL + string.Format("/activities/{0}/result", activityId);
                                xasuTrackerConfig.BackupFileName = auth.Username + "_" + activityId + "_backup.log";
                                xasuTrackerConfig.BackupTraceFormat = Xasu.Config.TraceFormats.XAPI;
                            }
                            SimvaPlugin.Instance.Log("[SIMVA] Xasu tracker Config : " + JsonConvert.SerializeObject(xasuTrackerConfig));
                            
                            if (activity.Details.TraceStorage || activity.Details.Backup)
                            {
                                homePage = xasuTrackerConfig.HomePage;
                                activityUrl=xasuTrackerConfig.HomePage + "/simlet/" + Schedule.Study + "/activity/" + activityId;
                                Bridge.StartTracker(xasuTrackerConfig, API.Authorization, API.Authorization)
                                    .Then(() => trackerStarted = true);
                                if (SimvaPlugin.Instance.BasicScormXAPIDataManagementByGame) {
                                    attemptId = new Guid().ToString();
                                    ScormTracker.Instance.Initialized(activityUrl).CreateAndAddContextGroupingActivity(
                                        xasuTrackerConfig.HomePage + "/simlets/" + Schedule.Study,
                                        Schedule.StudyName,
                                        "The activity representing the study" + Schedule.StudyName,
                                        "http://adlnet.gov/expapi/activities/course").CreateAndAddContextGroupingActivity(
                                        xasuTrackerConfig.HomePage + "/simlets/" + Schedule.Study + "/activity/" + activityId + "?id=" + attemptId,
                                        "Attempt of activity" + currentActivity.Name,
                                        "The activity representing an attempt of activity" + currentActivity.Name + " in study " + Schedule.StudyName,
                                        "http://adlnet.gov/expapi/activities/attempt");
                                }
                                yield return new WaitUntil(() => trackerStarted);
                            }

                            SimvaPlugin.Instance.Log("[SIMVA] Starting Gameplay...");
                            Bridge.StartGameplay();
                            break;
                    }
                }
            }
        }

        public void OnApplicationFocus(bool hasFocus)
        {
            if (!string.IsNullOrEmpty(CurrentActivityId) && Schedule.Activities.TryGetValue(CurrentActivityId, out var activity) && SimvaPlugin.Instance.BasicScormXAPIDataManagementByGame)
            {
                SimvaPlugin.Instance.Log("[SIMVA] " + activityUrl);
                if (hasFocus)
                {
                    attemptId = new Guid().ToString();
                    Debug.Log("Application is in focus.");
                    ScormTracker.Instance.Resumed(activityUrl).CreateAndAddContextGroupingActivity(
                                    homePage + "/simlets/" + Schedule.Study,
                                    Schedule.StudyName,
                                    "The activity representing the study" + Schedule.StudyName,
                                    "http://adlnet.gov/expapi/activities/course").CreateAndAddContextGroupingActivity(
                                    homePage + "/simlets/" + Schedule.Study + "/activity/" + currentActivity.Id + "?id=" + attemptId,
                                    "Attempt of activity" + currentActivity.Name,
                                    "The activity representing an attempt of activity" + currentActivity.Name + " in study " + Schedule.StudyName,
                                    "http://adlnet.gov/expapi/activities/attempt");
                }
                else
                {
                    Debug.Log("Application lost focus.");
                    ScormTracker.Instance.Suspended(activityUrl).CreateAndAddContextGroupingActivity(
                                    homePage + "/simlets/" + Schedule.Study,
                                    Schedule.StudyName,
                                    "The activity representing the study" + Schedule.StudyName,
                                    "http://adlnet.gov/expapi/activities/course").CreateAndAddContextGroupingActivity(
                                    homePage + "/simlets/" + Schedule.Study + "/activity/" + currentActivity.Id + "?id=" + attemptId,
                                    "Attempt of activity" + currentActivity.Name,
                                    "The activity representing an attempt of activity" + currentActivity.Name + " in study " + Schedule.StudyName,
                                    "http://adlnet.gov/expapi/activities/attempt");
                }
            }
        }

        public IAsyncOperation OnGameFinished()
        {
            Debug.Log("GamePlay terminated.");
            if (!string.IsNullOrEmpty(CurrentActivityId) && Schedule.Activities.TryGetValue(CurrentActivityId, out var activity) && SimvaPlugin.Instance.BasicScormXAPIDataManagementByGame)
            {
                SimvaPlugin.Instance.Log("[SIMVA] " + activityUrl);
                ScormTracker.Instance.Terminated(activityUrl).CreateAndAddContextGroupingActivity(
                                    homePage + "/simlets/" + Schedule.Study,
                                    Schedule.StudyName,
                                    "The activity representing the study" + Schedule.StudyName,
                                   "http://adlnet.gov/expapi/activities/course").CreateAndAddContextGroupingActivity(
                                    homePage + "/simlets/" + Schedule.Study + "/activity/" + currentActivity.Id + "?id=" + attemptId,
                                    "Attempt of activity" + currentActivity.Name,
                                    "The activity representing an attempt of activity" + currentActivity.Name + " in study " + Schedule.StudyName,
                                    "http://adlnet.gov/expapi/activities/attempt");
                activityUrl="";
            }
            var result = new AsyncCompletionSource();
            try
            {
                if (IsActive)
                {
                    Bridge.RunScene("Simva.Finalize");
                    StartCoroutine(OnGameFinishedRoutine(() => result.SetCompleted()));
                }
                else
                {
                    result.SetCompleted();
                }
                SimvaPlugin.Instance.StopTracker();
            }
            catch(Exception ex)
            {
                result.SetException(ex);
            }
            return result;
        }

        private IEnumerator OnGameFinishedRoutine(System.Action done)
        {
            var readyToClose = false;
            SimvaManager.Instance.Finalized = false;
            yield return new WaitUntil(() => SimvaManager.Instance.Finalized);
            Continue(SimvaManager.Instance.CurrentActivityId, true)
                .Then(() => readyToClose = true);
            yield return new WaitUntil(() => readyToClose);
            done();
        }

        #region Private

        private bool ActivityHasDetails(Activity activity, params string[] details)
        {
            if (activity.Details == null)
            {
                return false;
            }

            return details.Any(d => IsTrue(activity.Details, d));
        }

        private static bool IsTrue(ActivityDetails details, string key)
        {
            var propertyInfo = details.GetType().GetProperty(key, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (propertyInfo == null)
            {
                return false;
            }

            var value = propertyInfo.GetValue(details);
            return value is bool v && v;
        }

        private IAsyncOperation LaunchActivityById(string activityId)
        {
            if (string.IsNullOrEmpty(activityId) && !replayableProcessed && Schedule.ReplayableActivities?.Count > 0)
            {
                var replayable = Schedule.ReplayableActivities
                    .FirstOrDefault(id => Schedule.Activities.TryGetValue(id, out var a) && a.Type == "gameplay");
                if (replayable != null)
                {
                    Schedule.ReplayableActivities.Remove(replayable);
                    replayableProcessed = true;
                    activityId = replayable;
                    SimvaPlugin.Instance.Log("[SIMVA] LaunchActivityById used replayable instead of null next: " + activityId);
                }
            }

            if (string.IsNullOrEmpty(activityId))
            {
                SimvaPlugin.Instance.Log("[SIMVA] LaunchActivityById — no next activity, showing End scene");
                Bridge.RunScene("Simva.End");
                PlayerPrefs.DeleteKey("simva_auth");
                Schedule = null;
                var result = new AsyncCompletionSource();
                result.SetCompleted();
                return result;
            }

            if (Schedule.Activities.TryGetValue(activityId, out var activity) &&
                activity.Type == "gameplay" &&
                activity.Details != null &&
                activity.Details.ActivityCanBeRestarted)
            {
                SimvaPlugin.Instance.Log("[SIMVA] LaunchActivityById saving replayableActivityId: " + activityId + " (ActivityCanBeRestarted=true, gameplay)");
                replayableActivityId = activityId;
            }
            else
            {
                SimvaPlugin.Instance.Log("[SIMVA] LaunchActivityById NOT saving replayable (" + activityId + "): type=" + (activity?.Type ?? "null") + ", canRestart=" + (activity?.Details?.ActivityCanBeRestarted ?? false));
            }
            var launchResult = new AsyncCompletionSource();
            StartCoroutine(AsyncCoroutine(LaunchActivity(activityId), launchResult));
            return launchResult;
        }

        internal IEnumerator AsyncCoroutine(IEnumerator coroutine, IAsyncCompletionSource op)
        {
            yield return coroutine;
            op.SetCompleted();
        }

        #endregion

        #region Notifiers
        // NOTIFIERS

        public void AddResponseManager(SimvaResponseManager manager)
        {
            if (manager)
            {
                // To make sure we only have one instance of a notify per manager
                // We first remove (as it is ignored if not present)
                responseListeners -= manager.Notify;
                // Then we add it
                responseListeners += manager.Notify;
            }
        }

        public void RemoveResponseManager(SimvaResponseManager manager)
        {
            if (manager)
            {
                // If a delegate is not present the method gets ignored
                responseListeners -= manager.Notify;
            }
        }

        public void AddLoadingManager(SimvaLoadingManager manager)
        {
            if (manager)
            {
                // To make sure we only have one instance of a notify per manager
                // We first remove (as it is ignored if not present)
                loadingListeners -= manager.IsLoading;
                // Then we add it
                loadingListeners += manager.IsLoading;
            }
        }

        public void RemoveLoadingManager(SimvaLoadingManager manager)
        {
            if (manager)
            {
                // If a delegate is not present the method gets ignored
                loadingListeners -= manager.IsLoading;
            }
        }

        public void NotifyManagers(string message)
        {
            if (responseListeners == null)
            {
                Debug.LogError("[Simva] No response listeners — error: " + message);
                return;
            }
            foreach (var d in responseListeners.GetInvocationList())
            {
                try
                {
                    ((ResponseDelegate)d)(message);
                }
                catch (Exception ex)
                {
                    Debug.LogError("[Simva] Response delegate error: " + ex.Message);
                }
            }
        }

        public void NotifyLoading(bool state)
        {
            if (loadingListeners == null)
            {
                Debug.LogError("[Simva] No loading listeners — state: " + state);
                return;
            }
            foreach (var d in loadingListeners.GetInvocationList())
            {
                try
                {
                    ((LoadingDelegate)d)(state);
                }
                catch (Exception ex)
                {
                    Debug.LogError("[Simva] Loading delegate error: " + ex.Message);
                }
            }
        }

        #endregion
    }
}
