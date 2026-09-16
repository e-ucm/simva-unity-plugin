using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
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
        private GameObject qrFallbackNoteGo;

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

            SetStatus("DeviceWaiting", "Waiting… finish on the other screen and the game starts by itself.");
            UpdateQrDisplay(deviceCompleteUrl);
            StartCountdown(info.ExpiresIn);
        }

        private string L(string key, string hardcodedFallback)
        {
            string value = SimvaPlugin.Instance != null ? SimvaPlugin.Instance.GetName(key) : null;
            return string.IsNullOrEmpty(value) ? hardcodedFallback : value;
        }

        private void SetStatus(string key, string hardcodedFallback)
        {
            if (StatusText == null) return;
            var text = StatusText.GetComponent<Text>();
            if (text == null) return;
            text.text = L(key, hardcodedFallback);
        }

        private void ShowDeviceError(string technical = null)
        {
            if (StatusText == null) return;
            var text = StatusText.GetComponent<Text>();
            if (text == null) return;
            string friendly = SimvaPlugin.Instance != null ? SimvaPlugin.Instance.GetName("DeviceErrorFriendly") : null;
            if (!string.IsNullOrEmpty(friendly))
            {
                text.text = friendly;
            }
            else
            {
                text.text = L("DeviceErrorFallback", "Something went wrong.");
            }
            if (!string.IsNullOrEmpty(technical) && SimvaPlugin.Instance != null)
            {
                SimvaPlugin.Instance.LogError(technical);
            }
        }

        private bool HasQrTexture()
        {
            if (QRCodeImage == null) return false;
            var raw = QRCodeImage.GetComponent<RawImage>();
            if (raw != null && raw.texture != null) return true;
            var image = QRCodeImage.GetComponent<Image>();
            if (image != null && image.sprite != null) return true;
            var childRaw = QRCodeImage.GetComponentInChildren<RawImage>();
            if (childRaw != null && childRaw.texture != null) return true;
            var childImage = QRCodeImage.GetComponentInChildren<Image>();
            if (childImage != null && childImage.sprite != null && (qrFallbackNoteGo == null || childImage.gameObject != qrFallbackNoteGo)) return true;
            return false;
        }

        private void UpdateQrDisplay(string url)
        {
            if (QRCodeImage == null) return;
            var parent = QRCodeImage.transform.parent;
            if (parent != null) parent.gameObject.SetActive(true);
            QRCodeImage.SetActive(true);
            if (string.IsNullOrEmpty(url) || !HasQrTexture())
            {
                ShowQrFallbackNote();
            }
            else
            {
                ClearQrFallbackNote();
            }
        }

        private void ShowQrFallbackNote()
        {
            if (QRCodeImage == null) return;
            var parent = QRCodeImage.transform.parent;
            if (parent != null) parent.gameObject.SetActive(true);
            QRCodeImage.SetActive(true);
            if (qrFallbackNoteGo == null)
            {
                var existing = QRCodeImage.transform.Find("QrFallbackNote");
                if (existing != null) qrFallbackNoteGo = existing.gameObject;
            }
            if (qrFallbackNoteGo == null)
            {
                qrFallbackNoteGo = new GameObject("QrFallbackNote");
                qrFallbackNoteGo.transform.SetParent(QRCodeImage.transform, false);
                var rt = qrFallbackNoteGo.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                var note = qrFallbackNoteGo.AddComponent<Text>();
                Font font = null;
                if (StatusText != null)
                {
                    var statusText = StatusText.GetComponent<Text>();
                    if (statusText != null) font = statusText.font;
                }
                note.font = font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                note.fontSize = 16;
                note.alignment = TextAnchor.MiddleCenter;
                note.horizontalOverflow = HorizontalWrapMode.Wrap;
                note.verticalOverflow = VerticalWrapMode.Truncate;
            }
            qrFallbackNoteGo.SetActive(true);
            var noteText = qrFallbackNoteGo.GetComponent<Text>();
            if (noteText != null)
            {
                noteText.text = L("DeviceQrFailedNote", "The square did not load — no problem, use Option B or C below.");
            }
        }

        private void ClearQrFallbackNote()
        {
            if (qrFallbackNoteGo != null)
            {
                qrFallbackNoteGo.SetActive(false);
            }
            else if (QRCodeImage != null)
            {
                var existing = QRCodeImage.transform.Find("QrFallbackNote");
                if (existing != null) existing.gameObject.SetActive(false);
            }
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
                UpdateTimerText(remaining);
                yield return new WaitForSeconds(1f);
                remaining--;
            }
            // Code expired, request a new one
            LoginWithDevice();
        }

        private void UpdateTimerText(int remaining)
        {
            if (timerText == null) return;
            var text = timerText.GetComponent<Text>();
            if (text == null) return;
            int minutes = remaining / 60;
            int secs = remaining % 60;
            string time = string.Format("{0:00}:{1:00}", minutes, secs);
            string template = SimvaPlugin.Instance != null ? SimvaPlugin.Instance.GetName("DeviceCodeValidFor") : null;
            text.text = string.IsNullOrEmpty(template) ? time : template.Replace("{time}", time);
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
            if (SimvaConf.Local != null)
            {
                if (SimvaConf.Local.AuthParameters == null)
                {
                    SimvaConf.Local.AuthParameters = new Dictionary<string, string>();
                }
                SimvaConf.Local.AuthParameters["auto_open_device_url"] = "false";
            }
            SetStatus("DeviceLoadingSubtitle", "Hold on, getting your sign-in ready…");
            try
            {
                SimvaManager.Instance.LoginAndScheduleDevice();
            }
            catch (Exception ex)
            {
                ShowDeviceError(ex.ToString());
            }
        }

        public void AcceptDisclaimer()
        {
            DisclaimerAccepted = true;
            disclaimer.SetActive(false);
            if (SimvaPlugin.Instance != null && SimvaPlugin.Instance.EnableLoginDemoButton)
            {
                preview.SetActive(true);
            }
            login.SetActive(true);
            LoginWithDevice();
        }

        public void Login()
        {
            if (deviceCompleteUrl != null)
            {
                Application.OpenURL(deviceCompleteUrl);
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
