using Newtonsoft.Json;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityFx.Async;
using UnityFx.Async.Promises;

namespace Simva
{
    public static class RequestsUtil
    {

        public static IAsyncOperation<T> DoRequest<T>(UnityWebRequest webRequest)
        {
            var result = new AsyncCompletionSource<T>();

            DoRequest(webRequest)
                .Then(wr =>
                {
                    result.SetResult(JsonConvert.DeserializeObject<T>(wr.downloadHandler.text));
                })
                .Catch(result.SetException)
                .AddProgressCallback(result.SetProgress);

            return result;
        }

        public static IAsyncOperation<UnityWebRequest> DoRequest(UnityWebRequest webRequest)
        {
            var result = new AsyncCompletionSource<UnityWebRequest>();
            RunRoutine(DoRequest(result, webRequest, false));
            return result;
        }

        public static IAsyncOperation<UnityWebRequest> DoRequestInBackground(UnityWebRequest webRequest)
        {
            var result = new AsyncCompletionSource<UnityWebRequest>();
            RunRoutine(DoRequest(result, webRequest, false));
            return result;
        }

        private static void RunRoutine(IEnumerator routine)
        {

            CoroutineRunner.Instance.
                RunRoutine(routine);
        }

        private static IEnumerator DoRequest(IAsyncCompletionSource<UnityWebRequest> op, UnityWebRequest webRequest, bool inBackground)
        {
            UnityWebRequestAsyncOperation asyncRequest;
            string backgroundError = null;
            if (inBackground)
            {
                Application.runInBackground = true;
            }
            asyncRequest = webRequest.SendWebRequest();
            if (inBackground)
            {
                asyncRequest.completed += asyncOperation =>
                {
                    Application.runInBackground = false;
                };
            }

            if (webRequest.uploadHandler != null)
            {
                while (!webRequest.isDone)
                {
                    yield return new WaitForEndOfFrame();
                    op.SetProgress(asyncRequest.progress);
                }
            }
            else
            {
                yield return asyncRequest;
            }

            // Sometimes the webrequest is finished but the download is not
            bool webRequestResult=false;
            while (webRequestResult)
            {
                yield return new WaitForFixedUpdate();
                #if UNITY_2022_2_OR_NEWER
                    webRequestResult=(webRequest.result != UnityWebRequest.Result.ConnectionError && webRequest.result != UnityWebRequest.Result.ProtocolError && webRequest.downloadProgress != 1);
                #else
                    webRequestResult=(webRequest.isNetworkError && !webRequest.isHttpError && webRequest.downloadProgress != 1);
                #endif
            }

            bool webrequestError=false;
            #if UNITY_2022_2_OR_NEWER
                webrequestError=(webRequest.result != UnityWebRequest.Result.ConnectionError || webRequest.result != UnityWebRequest.Result.ProtocolError);
            #else
                webrequestError=(webRequest.isNetworkError || !webRequest.isHttpError);
            #endif
            if (!webrequestError)
            {
                Debug.Log(webRequest.error);
                op.SetException(new ApiException((int)webRequest.responseCode, webRequest.error, webRequest.downloadHandler.text));
            }
            else if (inBackground && !string.IsNullOrEmpty(backgroundError))
            {
                op.SetException(new ApiException((int)999, backgroundError));
            }
            else
            {
                op.SetResult(webRequest);
            }

            webRequest.Dispose();
        }

        public static IAsyncCompletionSource<T> Wrap<T>(this IAsyncOperation<T> me, IAsyncCompletionSource<T> other)
        {
            me.Then(other.SetResult).Catch(other.SetException).AddProgressCallback(other.SetProgress);
            return other;
        }
    }
}
