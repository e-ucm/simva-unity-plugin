using UnityEngine;
using UnityFx.Async.Promises;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Simva
{
    // Manager for "Simva.Survey"
    public class LoginController : SimvaSceneController
    {
        private const int SIMVA_DISCLAIMER_ACCEPTED_TRUE = 1;
        private const int SIMVA_DISCLAIMER_ACCEPTED_FALSE = 0;

        public GameObject disclaimer;
        public GameObject login;
        public GameObject preview;
        public GameObject back;
        public GameObject adviceDemo;

        public InputField token;

        public InputField password;
        public GameObject credentialFields;
        private bool credentialsMode;

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

        public void ToggleCredentialsMode()
        {
            credentialsMode = !credentialsMode;
            UpdateTokenLabels(credentialsMode);
            SetupCredentialFields();
            if (credentialFields != null)
            {
                credentialFields.SetActive(credentialsMode);
            }
        }

        private void UpdateTokenLabels(bool isCredentials)
        {
            if (login == null) return;
            var tokenLabelObj = login.transform.Find("TokenText");
            if (tokenLabelObj != null)
            {
                var tokenLabel = tokenLabelObj.GetComponent<Text>();
                var tokenLabelRt = tokenLabelObj.GetComponent<RectTransform>();
                if (tokenLabel != null && tokenLabelRt != null)
                {
                    tokenLabel.text = isCredentials ? SimvaPlugin.Instance.GetName("UsernameLabel") : SimvaPlugin.Instance.GetName("TokenText");
                    if (isCredentials)
                    {
                        var settings = tokenLabel.GetGenerationSettings(Vector2.zero);
                        float prefWidth = tokenLabel.cachedTextGeneratorForLayout.GetPreferredWidth(tokenLabel.text, settings);
                        tokenLabelRt.sizeDelta = new Vector2(prefWidth + 10f, 58.6f);
                    }
                    else
                    {
                        tokenLabelRt.sizeDelta = new Vector2(106.8f, 58.6f);
                    }
                }
            }
            if (token != null && token.placeholder != null)
            {
                var ph = token.placeholder as Text;
                if (ph != null)
                {
                    ph.text = isCredentials ? SimvaPlugin.Instance.GetName("UsernamePlaceholder") : SimvaPlugin.Instance.GetName("TokenInputPlaceHolder");
                }
            }
        }

        public void Login()
        {
            var simvaExtension = SimvaManager.Instance;
            if (credentialsMode)
            {
                if (token == null || string.IsNullOrEmpty(token.text))
                {
                    simvaExtension.NotifyManagers(SimvaPlugin.Instance.GetName("EmptyUsernameMsg"));
                    return;
                }
                if (password == null || string.IsNullOrEmpty(password.text))
                {
                    simvaExtension.NotifyManagers(SimvaPlugin.Instance.GetName("EmptyPasswordMsg"));
                    return;
                }
                simvaExtension.LoginAndSchedule(token.text, password.text);
                return;
            } else if(SimvaPlugin.Instance.EnableLoginDemoButton && token.text.ToLower() == "demo") {
                Demo();
            } else {
                if (token == null || string.IsNullOrEmpty(token.text)) {
                    simvaExtension.NotifyManagers(SimvaPlugin.Instance.GetName("EmptyUsernameMsg"));
                    return;
                } else {
                    simvaExtension.LoginAndSchedule(token.text);
                }
            }
        }

        public void LoginWithKeykloak()
        {
            SimvaManager.Instance.LoginAndSchedule();
        }

        public void LoginWithDevice()
        {
            SimvaManager.Instance.LoginAndScheduleDevice();
        }

        public void AcceptDisclaimer()
        {
            DisclaimerAccepted = true;
            disclaimer.SetActive(false);
            login.SetActive(true);
            if(SimvaPlugin.Instance != null && SimvaPlugin.Instance.EnableLoginDemoButton) {
                preview.SetActive(true);
            }
        }

        public void Demo()
        {
            SimvaManager.Instance.Demo();
        }

        public void SetupCredentialFields()
        {
            if (credentialFields != null && password != null) return;

            if (password == null)
            {
                var loginPanel = login ?? gameObject;
                credentialFields = new GameObject("CredentialFields");
                credentialFields.transform.SetParent(loginPanel.transform, false);

                var fieldsRt = credentialFields.AddComponent<RectTransform>();
                fieldsRt.anchorMin = Vector2.zero;
                fieldsRt.anchorMax = Vector2.one;
                fieldsRt.offsetMin = Vector2.zero;
                fieldsRt.offsetMax = Vector2.zero;

                var passLabel = new GameObject("PasswordLabel");
                passLabel.transform.SetParent(credentialFields.transform, false);
                var labelRt = passLabel.AddComponent<RectTransform>();
                labelRt.anchorMin = new Vector2(0.5f, 0.5f);
                labelRt.anchorMax = new Vector2(0.5f, 0.5f);
                labelRt.anchoredPosition = new Vector2(-132.4f, -83f);
                labelRt.sizeDelta = new Vector2(140f, 58.6f);
                var labelText = passLabel.AddComponent<Text>();
                var tokenLabel = loginPanel.transform.Find("TokenText")?.GetComponent<Text>();
                if (tokenLabel != null)
                {
                    labelText.font = tokenLabel.font;
                    labelText.fontSize = tokenLabel.fontSize;
                    labelText.fontStyle = tokenLabel.fontStyle;
                    labelText.color = tokenLabel.color;
                    labelText.lineSpacing = tokenLabel.lineSpacing;
                }
                else if (token != null && token.textComponent != null)
                {
                    labelText.font = token.textComponent.font;
                    labelText.fontSize = token.textComponent.fontSize;
                }
                else
                {
                    labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    labelText.fontSize = 26;
                }
                labelText.alignment = TextAnchor.MiddleLeft;
                labelText.text = SimvaPlugin.Instance.GetName("PasswordLabel");
                var labelSettings = labelText.GetGenerationSettings(Vector2.zero);
                float labelPrefWidth = labelText.cachedTextGeneratorForLayout.GetPreferredWidth(labelText.text, labelSettings);
                labelRt.sizeDelta = new Vector2(labelPrefWidth + 10f, 58.6f);

                var passInput = new GameObject("PasswordInput");
                passInput.transform.SetParent(credentialFields.transform, false);
                var inputRt = passInput.AddComponent<RectTransform>();
                inputRt.anchorMin = new Vector2(0.5f, 0.5f);
                inputRt.anchorMax = new Vector2(0.5f, 0.5f);
                inputRt.anchoredPosition = new Vector2(55.5f, -83f);
                inputRt.sizeDelta = new Vector2(274f, 51.4f);

                var bgImage = passInput.AddComponent<Image>();
                bgImage.type = Image.Type.Sliced;
                if (token != null)
                {
                    var tokenBg = token.GetComponent<Image>();
                    if (tokenBg != null)
                    {
                        bgImage.sprite = tokenBg.sprite;
                        bgImage.color = tokenBg.color;
                    }
                }

                var passInputField = passInput.AddComponent<InputField>();
                passInputField.contentType = InputField.ContentType.Password;

                var placeholder = new GameObject("Placeholder");
                placeholder.transform.SetParent(passInput.transform, false);
                var phRt = placeholder.AddComponent<RectTransform>();
                phRt.anchorMin = new Vector2(0, 0);
                phRt.anchorMax = new Vector2(1, 1);
                phRt.offsetMin = new Vector2(10, 0);
                phRt.offsetMax = new Vector2(-10, 0);
                var phText = placeholder.AddComponent<Text>();
                phText.font = labelText.font;
                phText.fontSize = 18;
                phText.fontStyle = FontStyle.Italic;
                phText.color = new Color(0.196f, 0.196f, 0.196f, 0.5f);
                phText.alignment = TextAnchor.MiddleLeft;
                phText.text = SimvaPlugin.Instance.GetName("PasswordPlaceholder");

                var textComp = new GameObject("Text");
                textComp.transform.SetParent(passInput.transform, false);
                var tRt = textComp.AddComponent<RectTransform>();
                tRt.anchorMin = new Vector2(0, 0);
                tRt.anchorMax = new Vector2(1, 1);
                tRt.offsetMin = new Vector2(10, 0);
                tRt.offsetMax = new Vector2(-10, 0);
                var txt = textComp.AddComponent<Text>();
                txt.font = labelText.font;
                txt.fontSize = labelText.fontSize;
                txt.color = new Color(0.196f, 0.196f, 0.196f, 1f);
                txt.alignment = TextAnchor.MiddleLeft;
                txt.supportRichText = false;

                passInputField.textComponent = txt;
                passInputField.placeholder = phText;

                password = passInputField;
            }

            credentialFields.SetActive(credentialsMode);
        }

        public void SetupDeviceLoginButton()
        {
            var loginPanel = login != null ? login.transform : transform;
            if (transform.Find("DeviceLoginButton") != null || loginPanel.Find("DeviceLoginButton") != null) return;

            var connectButton = loginPanel.Find("ConnectButton");

            var btnGo = new GameObject("DeviceLoginButton");
            btnGo.transform.SetParent(loginPanel, false);
            btnGo.transform.SetAsLastSibling();

            var rt = btnGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            if (connectButton != null)
            {
                var connectRt = connectButton.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(connectRt.anchoredPosition.x, connectRt.anchoredPosition.y - 80f);
                rt.sizeDelta = new Vector2(connectRt.sizeDelta.x, connectRt.sizeDelta.y);
            }
            else
            {
                rt.anchoredPosition = new Vector2(14f, -80f);
                rt.sizeDelta = new Vector2(403.9f, 80f);
            }

            var bgImage = btnGo.AddComponent<Image>();
            bgImage.type = Image.Type.Sliced;
            var tokenBg = token != null ? token.GetComponent<Image>() : null;
            if (tokenBg != null)
            {
                bgImage.sprite = tokenBg.sprite;
                bgImage.color = tokenBg.color;
            }
            else
            {
                bgImage.color = new Color(1, 0.69803923f, 0.058823533f, 1);
            }

            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = bgImage;
            btn.onClick.AddListener(LoginWithDevice);

            var textComp = new GameObject("Text");
            textComp.transform.SetParent(btnGo.transform, false);
            var tRt = textComp.AddComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.offsetMin = Vector2.zero;
            tRt.offsetMax = Vector2.zero;
            var txt = textComp.AddComponent<Text>();
            var refText = connectButton != null ? connectButton.GetComponentInChildren<Text>() : null;
            if (refText != null)
            {
                txt.font = refText.font;
                txt.fontSize = refText.fontSize;
                txt.fontStyle = refText.fontStyle;
                txt.color = refText.color;
            }
            else
            {
                txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                txt.fontSize = 26;
                txt.color = new Color(1, 1, 1, 1);
            }
            txt.alignment = TextAnchor.MiddleCenter;
            txt.supportRichText = false;
            txt.text = SimvaPlugin.Instance.GetName("DeviceLoginButton");
        }

        public void SetupHiddenToggleButton()
        {
            var existing = transform.Find("CredentialToggleBtn");
            if (existing != null) return;

            var simvaTitle = transform.Find("Title/SimvaTitle");
            if (simvaTitle == null) return;
            var simvaRt = simvaTitle.GetComponent<RectTransform>();

            var btnGo = new GameObject("CredentialToggleBtn");
            btnGo.transform.SetParent(transform, false);
            btnGo.transform.SetAsLastSibling();

            var rt = btnGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            float dotX = simvaRt.anchoredPosition.x - 45f;
            float dotY = simvaRt.anchoredPosition.y + simvaRt.rect.height * 0.4f;
            rt.anchoredPosition = new Vector2(dotX, dotY);
            rt.sizeDelta = new Vector2(60, 60);

            var img = btnGo.AddComponent<Image>();
            img.color = new Color(1, 1, 1, 0);
            img.raycastTarget = true;

            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.None;
            var nav = new Navigation();
            nav.mode = Navigation.Mode.None;
            btn.navigation = nav;
            btn.onClick.AddListener(ToggleCredentialsMode);
        }

        public override void Destroy()
        {
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
            //var background = GameObject.Find("background").GetComponent<Image>();
            /*var backgroundPath = 
            var backgroundSprite = Game.Instance.ResourceManager.getSprite();
            background.sprite = Game.Instance.ResourceManager.getSprite()*/
            Ready = true;
            SetupHiddenToggleButton();
            SetupDeviceLoginButton();
            if (credentialsMode)
            {
                SetupCredentialFields();
            }
            if(SimvaPlugin.Instance.EnableLoginDemoButton) {
                if(adviceDemo) {
                    adviceDemo.SetActive(true);
                }
            }
            if(back) {
                if(SimvaPlugin.Instance.EnableLanguageScene && SimvaPlugin.Instance.SelectedLanguages.Count > 0) {
                    back.SetActive(true);
                } else {
                    if(!SimvaPlugin.Instance.AutoStart) {
                        back.SetActive(true);
                    }
                }        
            }
        }
    }
}