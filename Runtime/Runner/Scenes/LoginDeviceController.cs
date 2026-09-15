using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Xasu.Auth.Protocols;
using Xasu.Auth.Protocols.OAuth2;

namespace Simva
{
    public class LoginDeviceController : SimvaSceneController
    {
        private const int SIMVA_DISCLAIMER_ACCEPTED_TRUE = 1;
        private const int SIMVA_DISCLAIMER_ACCEPTED_FALSE = 0;

        public GameObject disclaimer;
        public GameObject preview;
        public GameObject back;
        public GameObject adviceDemo;
        public GameObject QRCodeImage;
        public GameObject DeviceCompleteUrlValue;
        public GameObject loginDeviceCompleteUrlButton;
        public GameObject loginDeviceCompleteUrlCopyButton;
        public GameObject DeviceUrlValue;
        public GameObject loginDeviceUrlButton;
        public GameObject loginDeviceUrlCopyButton;
        public GameObject deviceCodeValue;
        public GameObject loginDeviceCodeCopyButton;
        public GameObject StatusText;
        public GameObject timerText;
        public GameObject login;

        private string deviceCode;
        private string deviceUrl;
        private string deviceCompleteUrl;
        private OAuth2DeviceAuthorization lastDeviceAuth;
        private Coroutine countdownCoroutine;

        public bool DisclaimerAccepted
        {
            get
            {
                return PlayerPrefs.HasKey(SimvaPlugin.SIMVA_DISCLAIMER_ACCEPTED) ? PlayerPrefs.GetInt(SimvaPlugin.SIMVA_DISCLAIMER_ACCEPTED) == SIMVA_DISCLAIMER_ACCEPTED_TRUE : false;
            }
            set
            {
                PlayerPrefs.SetInt(SimvaPlugin.SIMVA_DISCLAIMER_ACCEPTED, value ? SIMVA_DISCLAIMER_ACCEPTED_TRUE : SIMVA_DISCLAIMER_ACCEPTED_FALSE);
                PlayerPrefs.Save();
            }
        }

        private void OnEnable()
        {
            OAuth2DeviceProtocol.DeviceAuthorizationReceived += OnDeviceAuthorizationReceived;
        }

        private void OnDisable()
        {
            OAuth2DeviceProtocol.DeviceAuthorizationReceived -= OnDeviceAuthorizationReceived;
            StopCountdown();
        }

        private void OnDeviceAuthorizationReceived(OAuth2DeviceAuthorization info)
        {
            if (info == null) return;

            lastDeviceAuth = info;
            deviceCode = info.UserCode;
            deviceUrl = info.VerificationUri;
            deviceCompleteUrl = !string.IsNullOrEmpty(info.VerificationUriComplete)
                ? info.VerificationUriComplete
                : info.VerificationUri;

            if (DeviceCompleteUrlValue != null)
            {
                var text = DeviceCompleteUrlValue.GetComponent<Text>();
                if (text != null) text.text = deviceCompleteUrl;
            }
            if (DeviceUrlValue != null)
            {
                var text = DeviceUrlValue.GetComponent<Text>();
                if (text != null) text.text = deviceUrl;
            }
            if (deviceCodeValue != null)
            {
                var text = deviceCodeValue.GetComponent<Text>();
                if (text != null) text.text = deviceCode;
            }

            StartCountdown(info.ExpiresIn);
        }

        private void StartCountdown(int seconds)
        {
            StopCountdown();
            if (seconds <= 0) return;
            countdownCoroutine = StartCoroutine(CountdownCoroutine(seconds));
        }

        private void StopCountdown()
        {
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null;
            }
            if (timerText != null)
            {
                var text = timerText.GetComponent<Text>();
                if (text != null) text.text = "";
            }
        }

        private IEnumerator CountdownCoroutine(int totalSeconds)
        {
            int remaining = totalSeconds;
            while (remaining > 0)
            {
                int minutes = remaining / 60;
                int secs = remaining % 60;
                if (timerText != null)
                {
                    var text = timerText.GetComponent<Text>();
                    if (text != null) text.text = string.Format("{0:00}:{1:00}", minutes, secs);
                }
                yield return new WaitForSeconds(1f);
                remaining--;
            }
            if (timerText != null)
            {
                var text = timerText.GetComponent<Text>();
                if (text != null) text.text = "Expired";
            }
        }

        public void openSignInWithDeviceUrl()
        {
            if (deviceUrl != null) Application.OpenURL(deviceUrl);
        }

        public void openSignInWithDeviceCompleteUrl()
        {
            if (deviceCompleteUrl != null) Application.OpenURL(deviceCompleteUrl);
        }

        public void copySignInWithDeviceUrl()
        {
            if (deviceUrl != null)
            {
                GUIUtility.systemCopyBuffer = deviceUrl;
                if (loginDeviceUrlCopyButton != null)
                {
                    var text = loginDeviceUrlCopyButton.GetComponentInChildren<Text>();
                    if (text != null) text.text = "Copied!";
                }
            }
        }

        public void copySignInWithDeviceCompleteUrl()
        {
            if (deviceCompleteUrl != null)
            {
                GUIUtility.systemCopyBuffer = deviceCompleteUrl;
                if (loginDeviceCompleteUrlCopyButton != null)
                {
                    var text = loginDeviceCompleteUrlCopyButton.GetComponentInChildren<Text>();
                    if (text != null) text.text = "Copied!";
                }
            }
        }

        public void copySignInWithDeviceCode()
        {
            if (deviceCode != null)
            {
                GUIUtility.systemCopyBuffer = deviceCode;
                if (loginDeviceCodeCopyButton != null)
                {
                    var text = loginDeviceCodeCopyButton.GetComponentInChildren<Text>();
                    if (text != null) text.text = "Copied!";
                }
            }
        }

        public void Back()
        {
            PlayerPrefs.DeleteKey(SimvaPlugin.SIMVA_DISCLAIMER_ACCEPTED);
            if (SimvaPlugin.Instance.EnableLanguageScene && SimvaPlugin.Instance.SelectedLanguages.Count > 0)
            {
                SimvaPlugin.Instance.RunScene("Simva.Language");
                if (LanguageSelectorController.instance == null)
                {
                    SimvaPlugin.Instance.gameObject.AddComponent<LanguageSelectorController>();
                }
                LanguageSelectorController.instance.SetActive(true);
            }
            else if (!SimvaPlugin.Instance.AutoStart)
            {
                SimvaPlugin.Instance.RunScene("StartMenu");
            }
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public void LoginWithDevice()
        {
            SimvaManager.Instance.LoginAndScheduleDevice();
        }

        public void AcceptDisclaimer()
        {
            DisclaimerAccepted = true;
            disclaimer.SetActive(false);
            if (SimvaPlugin.Instance != null && SimvaPlugin.Instance.EnableLoginDemoButton)
            {
                preview.SetActive(true);
            }
        }

        public void Login()
        {
            if (deviceCompleteUrl != null)
            {
                Application.OpenURL(deviceCompleteUrl);
            }
            else
            {
                LoginWithDevice();
            }
        }

        public void Demo()
        {
            SimvaManager.Instance.Demo();
        }

        public override void Destroy()
        {
            OAuth2DeviceProtocol.DeviceAuthorizationReceived -= OnDeviceAuthorizationReceived;
            StopCountdown();
            GameObject.DestroyImmediate(this.gameObject);
        }

        public override void Render()
        {
            if (SimvaPlugin.Instance == null) return;
            SetActive(true);
            if (DisclaimerAccepted)
            {
                AcceptDisclaimer();
            }
            Ready = true;
            login.SetActive(true);
            if (lastDeviceAuth != null)
            {
                OnDeviceAuthorizationReceived(lastDeviceAuth);
            }
            if (SimvaPlugin.Instance.EnableLoginDemoButton)
            {
                if (adviceDemo)
                {
                    adviceDemo.SetActive(true);
                }
            }
            if (back)
            {
                if (SimvaPlugin.Instance.EnableLanguageScene && SimvaPlugin.Instance.SelectedLanguages.Count > 0)
                {
                    back.SetActive(true);
                }
                else
                {
                    if (!SimvaPlugin.Instance.AutoStart)
                    {
                        back.SetActive(true);
                    }
                }
            }
        }
    }
}
