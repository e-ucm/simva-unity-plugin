using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Simva
{
    public class DeviceCodeController : SimvaSceneController
    {
        public static DeviceCodeController Active { get; private set; }

        private enum View { Loading, Device }

        private View currentView = View.Loading;
        private DeviceAuthInfo lastInfo;
        private string lastError;

        private bool uiBuilt;
        private Canvas canvas;
        private Text titleText;
        private Text subtitleText;
        private Text waitingText;
        private Text expiryText;
        private Text errorText;
        private GameObject errorHolder;
        private GameObject deviceRoot;
        private GameObject loadingRoot;
        private RawImage qrImage;
        private Text qrFallbackText;
        private Text codeText;
        private Text manualUrlText;
        private Text completeUrlText;
        private Text optionATitle;
        private Text optionBTitle;
        private Text optionCTitle;
        private Text stepA1Text;
        private Text stepA2Text;
        private Text stepB1Text;
        private Text stepC1Text;
        private Text stepC2Text;
        private Text noTypingText;
        private Text copyLinkHintBText;
        private Text copyLinkHintCText;
        private Text loadingStatusText;
        private Text loadingSubtitleText;
        private Text backLabel;
        private Button backButton;
        private Text demoLabel;
        private Button demoButton;

        private Button openCompleteButton;
        private Button openManualButton;
        private Button copyCodeButton;
        private Button copyCompleteLinkButton;
        private Button copyManualLinkButton;
        private Text openCompleteLabel;
        private Text openManualLabel;
        private Text copyCodeLabel;
        private Text copyCompleteLinkLabel;
        private Text copyManualLinkLabel;

        private Coroutine countdownRoutine;
        private long countdownDeadlineTicks;

        private static readonly Color OverlayColor = new Color(0f, 0f, 0f, 0.7f);
        private static readonly Color CardColor = Color.white;
        private static readonly Color MutedColor = new Color(0.33f, 0.33f, 0.33f);
        private static readonly Color BadgeBg = new Color(0.89f, 0.94f, 1f);
        private static readonly Color BadgeText = new Color(0f, 0.29f, 0.6f);
        private static readonly Color PrimaryBg = new Color(0f, 0.4f, 0.8f);
        private static readonly Color PanelBg = new Color(0.98f, 0.99f, 1f);
        private static readonly Color CodeBg = new Color(0.94f, 0.94f, 0.96f);
        private static readonly Color LinkBlue = new Color(0f, 0.4f, 0.8f);
        private static readonly Color ExpiryBlue = new Color(0f, 0.29f, 0.6f);
        private static readonly Color ExpiredRed = new Color(0.48f, 0.12f, 0.12f);
        private static readonly Color ErrorBg = new Color(0.99f, 0.925f, 0.925f);
        private static readonly Color ErrorText = new Color(0.48f, 0.12f, 0.12f);
        private static readonly Color OkGreen = new Color(0.11f, 0.48f, 0.18f);

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public override void Render()
        {
            Active = this;
            Debug.Log("[Device] Render on instance " + GetInstanceID());
            SetActive(true);
            BuildUi();
            ShowLoading();
            RefreshTexts();
            Ready = true;
        }

        public override void Destroy()
        {
            StopAll();
            if (Active == this)
            {
                Active = null;
            }
            GameObject.DestroyImmediate(gameObject);
        }

        private void OnDestroy()
        {
            StopAll();
            if (Active == this)
            {
                Active = null;
            }
        }

        public void Back()
        {
            StopAll();
            SimvaPlugin.Instance.RunScene("Simva.Login");
        }

        public void Demo()
        {
            if (SimvaManager.Instance != null)
            {
                SimvaManager.Instance.Demo();
            }
        }

        public void ShowLoading()
        {
            EnsureUi();
            currentView = View.Loading;
            lastError = null;
            StopCountdown();
            if (loadingRoot != null) loadingRoot.SetActive(true);
            if (deviceRoot != null) deviceRoot.SetActive(false);
            HideError();
            RefreshTexts();
        }

        public void ShowDeviceInfo(DeviceAuthInfo info)
        {
            EnsureUi();
            lastInfo = info;
            lastError = null;
            currentView = View.Device;
            if (loadingRoot != null) loadingRoot.SetActive(false);
            if (deviceRoot != null) deviceRoot.SetActive(true);
            RefreshTexts();
            RenderQr();
            StartCountdown(info != null ? info.expires_in : 0);
        }

        public void ShowError(string message)
        {
            EnsureUi();
            lastError = string.IsNullOrEmpty(message) ? T("DeviceErrorFallback", "Something went wrong.") : message;
            if (loadingRoot != null && currentView == View.Loading) loadingRoot.SetActive(true);
            RefreshTexts();
            AppendError(lastError);
        }

        public void Dismiss()
        {
            StopAll();
            if (Active == this)
            {
                Active = null;
            }
            if (this != null && gameObject != null)
            {
                GameObject.Destroy(gameObject);
            }
        }

        public void RefreshLanguage()
        {
            if (!uiBuilt) return;
            RefreshTexts();
            if (currentView == View.Device && lastInfo != null) RenderQr();
            if (!string.IsNullOrEmpty(lastError)) AppendError(lastError);
        }

        private void EnsureUi()
        {
            if (!uiBuilt) BuildUi();
            if (canvas != null && !canvas.gameObject.activeInHierarchy) SetActive(true);
        }

        private void StopAll()
        {
            StopCountdown();
        }

        private void StopCountdown()
        {
            if (countdownRoutine != null)
            {
                try { StopCoroutine(countdownRoutine); } catch (Exception) { }
                countdownRoutine = null;
            }
            countdownDeadlineTicks = 0;
        }

        private string T(string key, string fallback)
        {
            if (SimvaPlugin.Instance != null)
            {
                try
                {
                    var v = SimvaPlugin.Instance.GetName(key);
                    if (!string.IsNullOrEmpty(v)) return ToUnityRichText(v);
                }
                catch (Exception) { }
            }
            return ToUnityRichText(fallback);
        }

        private static string ToUnityRichText(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return s.Replace("<strong>", "<b>").Replace("</strong>", "</b>");
        }

        private static string FormatCountdown(int totalSeconds)
        {
            int s = Math.Max(0, totalSeconds);
            return string.Format("{0:00}:{1:00}", s / 60, s % 60);
        }

        private void RefreshTexts()
        {
            if (!uiBuilt) return;
            SetText(titleText, T("DeviceTitle", "Sign in to play"));
            SetText(subtitleText, currentView == View.Loading
                ? T("DeviceLoadingSubtitle", "Hold on, getting your sign-in ready\u2026")
                : T("DeviceSubtitlePick", "Pick <b>A</b>, <b>B</b> or <b>C</b> \u2014 the game waits for you."));
            SetText(loadingSubtitleText, T("DeviceLoadingSubtitle", "Hold on, getting your sign-in ready\u2026"));
            SetText(loadingStatusText, T("DeviceLoadingStatus", "The game is paused. You don't need to click anything yet."));
            SetText(optionATitle, T("DeviceOptionA", "Option A \u00b7 Phone or tablet"));
            SetText(optionBTitle, T("DeviceOptionB", "Option B \u00b7 One click, no typing"));
            SetText(optionCTitle, T("DeviceOptionC", "Option C \u00b7 Type the code"));
            SetText(stepA1Text, T("DeviceStepA1", "<b>1.</b> Point your camera at the square above."));
            SetText(stepA2Text, T("DeviceStepA2", "<b>2.</b> Say yes on that screen, then come back here."));
            SetText(stepB1Text, T("DeviceStepB1", "<b>1.</b> Press the blue button:"));
            SetText(stepC1Text, T("DeviceStepC1", "<b>1.</b> Press this button:"));
            SetText(stepC2Text, T("DeviceStepC2", "<b>2.</b> Copy this code and type it there:"));
            SetText(noTypingText, T("DeviceNoTypingNote", "Your code is filled in already \u2014 no typing needed."));
            SetText(copyLinkHintBText, T("DeviceCopyLinkHint", "If the link does not open when you click the button, copy this link to another tab:"));
            SetText(copyLinkHintCText, T("DeviceCopyLinkHint", "If the link does not open when you click the button, copy this link to another tab:"));
            SetText(openCompleteLabel, T("DeviceOpenButton", "Open sign-in page"));
            SetText(openManualLabel, T("DeviceOpenButton", "Open sign-in page"));
            SetText(copyCodeLabel, T("DeviceCopyButton", "Copy"));
            SetText(copyCompleteLinkLabel, T("DeviceCopyLinkButton", "Copy link"));
            SetText(copyManualLinkLabel, T("DeviceCopyLinkButton", "Copy link"));
            SetText(waitingText, T("DeviceWaiting", "Waiting\u2026 finish on the other screen and the game starts by itself."));
            SetText(backLabel, T("BackButton", "Back"));
            SetText(demoLabel, T("DemoButton", "Demo"));
            SetButtonVisible(demoButton, SimvaPlugin.Instance != null && SimvaPlugin.Instance.EnableLoginDemoButton);

            if (lastInfo != null)
            {
                SetText(codeText, lastInfo.user_code ?? "");
                string complete = lastInfo.CompleteUrl ?? "";
                string manual = lastInfo.ManualUrl ?? "";
                SetText(completeUrlText, complete);
                SetText(manualUrlText, manual);
                SetButtonVisible(openCompleteButton, !string.IsNullOrEmpty(complete));
                SetButtonVisible(openManualButton, !string.IsNullOrEmpty(manual));
                SetButtonVisible(copyCompleteLinkButton, !string.IsNullOrEmpty(complete));
                SetButtonVisible(copyManualLinkButton, !string.IsNullOrEmpty(manual));
                SetButtonVisible(copyCodeButton, !string.IsNullOrEmpty(lastInfo.user_code));
            }
            else
            {
                SetText(codeText, "");
            }
            if (countdownDeadlineTicks == 0)
            {
                SetText(expiryText, "");
            }
            Canvas.ForceUpdateCanvases();
        }

        private void AppendError(string message)
        {
            if (errorText == null && errorHolder == null) return;
            string friendly = T("DeviceErrorFriendly", "Hmm, that did not work \u2014 keep this window open and try again.");
            SetText(errorText, friendly + "\n<size=12>" + message + "</size>");
            if (errorHolder != null)
            {
                errorHolder.SetActive(true);
            }
            else if (errorText != null)
            {
                errorText.gameObject.SetActive(true);
            }
        }

        private void HideError()
        {
            SetText(errorText, "");
            if (errorHolder != null)
            {
                errorHolder.SetActive(false);
            }
            else if (errorText != null)
            {
                errorText.gameObject.SetActive(false);
            }
        }

        private void StartCountdown(int expiresIn)
        {
            StopCountdown();
            if (expiryText == null) return;
            if (expiresIn > 0)
            {
                countdownDeadlineTicks = DateTime.UtcNow.AddSeconds(expiresIn).Ticks;
            }
            else
            {
                expiryText.text = "";
                return;
            }
            countdownRoutine = StartCoroutine(CountdownTick());
        }

        private IEnumerator CountdownTick()
        {
            while (true)
            {
                if (expiryText == null) yield break;
                int remaining = (int)Math.Max(0, Math.Ceiling((new DateTime(countdownDeadlineTicks, DateTimeKind.Utc) - DateTime.UtcNow).TotalSeconds));
                if (remaining <= 0)
                {
                    SetText(expiryText, T("DeviceCodeExpired", "This code ran out of time \u2014 getting a fresh one, keep this window open."));
                    if (expiryText != null) expiryText.color = ExpiredRed;
                    yield break;
                }
                string template = T("DeviceCodeValidFor", "This code works for {time}. Hurry!");
                SetText(expiryText, template.Replace("{time}", FormatCountdown(remaining)).Replace("{0}", FormatCountdown(remaining)));
                if (expiryText != null) expiryText.color = ExpiryBlue;
                yield return new WaitForSeconds(1f);
            }
        }

        private void RenderQr()
        {
            if (qrImage == null || lastInfo == null) return;
            string url = lastInfo.CompleteUrl;
            if (string.IsNullOrEmpty(url))
            {
                ShowQrFallback();
                return;
            }
            if (TryRenderQr(url, qrImage, 170))
            {
                if (qrImage.transform.parent != null) qrImage.transform.parent.gameObject.SetActive(true);
                qrImage.gameObject.SetActive(true);
                if (qrFallbackText != null) qrFallbackText.gameObject.SetActive(false);
            }
            else
            {
                ShowQrFallback();
            }
        }

        private void ShowQrFallback()
        {
            if (qrImage != null)
            {
                qrImage.gameObject.SetActive(false);
                if (qrImage.transform.parent != null) qrImage.transform.parent.gameObject.SetActive(false);
            }
            if (qrFallbackText != null)
            {
                qrFallbackText.text = T("DeviceQrFailedNote", "The square did not load \u2014 no problem, use Option B or C below.");
                qrFallbackText.gameObject.SetActive(true);
            }
        }

        private bool TryRenderQr(string content, RawImage target, int size)
        {
            try
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type writerType;
                    try { writerType = asm.GetType("ZXing.BarcodeWriterPixelData"); } catch (Exception) { continue; }
                    if (writerType == null) continue;
                    Type formatType = asm.GetType("ZXing.BarcodeFormat");
                    Type optionsType = asm.GetType("ZXing.Common.EncodingOptions");
                    if (formatType == null || optionsType == null) continue;
                    object writer = Activator.CreateInstance(writerType);
                    object format = Enum.Parse(formatType, "QR_CODE");
                    writerType.GetProperty("Format").SetValue(writer, format, null);
                    object options = Activator.CreateInstance(optionsType);
                    optionsType.GetProperty("Width").SetValue(options, size, null);
                    optionsType.GetProperty("Height").SetValue(options, size, null);
                    try { optionsType.GetProperty("Margin").SetValue(options, 1, null); } catch (Exception) { }
                    writerType.GetProperty("Options").SetValue(writer, options, null);
                    object pixelData = writerType.GetMethod("Write").Invoke(writer, new object[] { content });
                    if (pixelData == null) return false;
                    Type pixelType = pixelData.GetType();
                    byte[] pixels = (byte[])pixelType.GetProperty("Pixels").GetValue(pixelData, null);
                    int w = (int)pixelType.GetProperty("Width").GetValue(pixelData, null);
                    int h = (int)pixelType.GetProperty("Height").GetValue(pixelData, null);
                    var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                    tex.filterMode = FilterMode.Point;
                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            int i = (y * w + x) * 4;
                            tex.SetPixel(x, h - 1 - y, new Color32(pixels[i], pixels[i + 1], pixels[i + 2], 255));
                        }
                    }
                    tex.Apply();
                    target.texture = tex;
                    var rt = target.rectTransform;
                    rt.sizeDelta = new Vector2(size, size);
                    return true;
                }
            }
            catch (Exception e)
            {
                if (SimvaPlugin.Instance != null) SimvaPlugin.Instance.LogWarning("[Device] QR render failed: " + e.Message);
            }
            return false;
        }

        private void OnOpenComplete() { OpenUrl(lastInfo != null ? lastInfo.CompleteUrl : null); }
        private void OnOpenManual() { OpenUrl(lastInfo != null ? lastInfo.ManualUrl : null); }

        private void OpenUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            try { Application.OpenURL(url); } catch (Exception e) { ShowError(e.Message); }
        }

        private void OnCopyCode() { CopyText(lastInfo != null ? lastInfo.user_code : "", copyCodeButton, copyCodeLabel); }
        private void OnCopyCompleteLink() { CopyText(lastInfo != null ? lastInfo.CompleteUrl : "", copyCompleteLinkButton, copyCompleteLinkLabel); }
        private void OnCopyManualLink() { CopyText(lastInfo != null ? lastInfo.ManualUrl : "", copyManualLinkButton, copyManualLinkLabel); }

        private void CopyText(string text, Button button, Text label)
        {
            if (string.IsNullOrEmpty(text) || label == null) return;
            try { GUIUtility.systemCopyBuffer = text; } catch (Exception) { }
            StartCoroutine(CopyFeedback(label));
        }

        private IEnumerator CopyFeedback(Text label)
        {
            if (label == null) yield break;
            string original = label.text;
            SetText(label, T("DeviceCopiedFeedback", "Copied!"));
            yield return new WaitForSeconds(2f);
            SetText(label, original);
        }

        private Font BodyFont()
        {
            Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (f == null)
            {
                var all = Resources.FindObjectsOfTypeAll<Font>();
                if (all != null && all.Length > 0) f = all[0];
            }
            return f;
        }

        private void BuildUi()
        {
            if (uiBuilt) return;
            uiBuilt = true;
            Font font = BodyFont();
            Debug.Log("[Device] Building UI. Body font: " + (font != null ? font.name : "NULL"));

            canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[Device] FATAL: Canvas component could not be created. Aborting device UI.");
                uiBuilt = true;
                return;
            }
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            var scaler = gameObject.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(720, 1280);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            if (gameObject.GetComponent<GraphicRaycaster>() == null) gameObject.AddComponent<GraphicRaycaster>();

            var dim = new GameObject("Dim");
            dim.transform.SetParent(transform, false);
            var dimRt = dim.AddComponent<RectTransform>();
            Stretch(dimRt, 0, 0, 0, 0);
            var dimImg = dim.AddComponent<Image>();
            dimImg.color = OverlayColor;

            var scrollGo = new GameObject("Scroll");
            scrollGo.transform.SetParent(transform, false);
            var scrollRt = scrollGo.AddComponent<RectTransform>();
            Stretch(scrollRt, 24, 24, 24, 24);
            var scroll = scrollGo.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollGo.transform, false);
            var viewportRt = viewport.AddComponent<RectTransform>();
            Stretch(viewportRt, 0, 0, 0, 0);
            var viewportMask = viewport.AddComponent<RectMask2D>();
            if (viewportMask == null)
            {
                Debug.LogError("[Device] Failed to create RectMask2D for viewport.");
            }
            var viewportImg = viewport.AddComponent<Image>();
            if (viewportImg == null)
            {
                Debug.LogError("[Device] Failed to create Image for viewport.");
            }
            else
            {
                viewportImg.color = new Color(1, 1, 1, 0);
            }
            scroll.viewport = viewportRt;

            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            var contentRt = content.AddComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.offsetMin = Vector2.zero;
            contentRt.offsetMax = new Vector2(0, 0);
            var contentLayout = content.AddComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(8, 8, 8, 8);
            contentLayout.spacing = 8;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = false;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            var contentFitter = content.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = contentRt;

            var card = new GameObject("Card");
            card.transform.SetParent(content.transform, false);
            var cardLayoutEl = card.AddComponent<LayoutElement>();
            cardLayoutEl.preferredWidth = 672;
            cardLayoutEl.minHeight = 500;
            var cardImg = card.AddComponent<Image>();
            cardImg.color = CardColor;
            var cardLayout = card.AddComponent<VerticalLayoutGroup>();
            cardLayout.padding = new RectOffset(20, 20, 12, 20);
            cardLayout.spacing = 4;
            cardLayout.childControlWidth = true;
            cardLayout.childControlHeight = false;
            cardLayout.childForceExpandWidth = true;
            cardLayout.childForceExpandHeight = false;
            var cardFitter = card.AddComponent<ContentSizeFitter>();
            cardFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            cardFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var backRow = new GameObject("BackRow");
            backRow.transform.SetParent(card.transform, false);
            backRow.AddComponent<RectTransform>();
            var backH = backRow.AddComponent<HorizontalLayoutGroup>();
            backH.childAlignment = TextAnchor.MiddleLeft;
            backButton = MakeButton(backRow.transform, "< Back", font, 14, BadgeText, BadgeBg, out backLabel, "back");
            OnClick(backButton, Back);
            var backLayout = backButton != null ? backButton.GetComponent<LayoutElement>() : null;
            if (backLayout != null) backLayout.minWidth = 140;
            demoButton = MakeButton(backRow.transform, "", font, 14, Color.white, PrimaryBg, out demoLabel, "demo");
            OnClick(demoButton, Demo);

            titleText = MakeLabel(card.transform, "", font, 18, FontStyle.Bold, new Color(0.13f, 0.13f, 0.13f), TextAnchor.MiddleCenter, "title");
            subtitleText = MakeLabel(card.transform, "", font, 13, FontStyle.Normal, MutedColor, TextAnchor.MiddleCenter, "subtitle");

            loadingRoot = new GameObject("LoadingState");
            loadingRoot.transform.SetParent(card.transform, false);
            AddVertical(loadingRoot);
            loadingSubtitleText = MakeLabel(loadingRoot.transform, "", font, 14, FontStyle.Normal, MutedColor, TextAnchor.MiddleCenter);
            loadingStatusText = MakeLabel(loadingRoot.transform, "", font, 13, FontStyle.Normal, MutedColor, TextAnchor.MiddleCenter);

            deviceRoot = new GameObject("DeviceState");
            deviceRoot.transform.SetParent(card.transform, false);
            AddVertical(deviceRoot);

            var optionsRow = new GameObject("OptionsRow");
            optionsRow.transform.SetParent(deviceRoot.transform, false);
            optionsRow.AddComponent<RectTransform>();
            var responsive = optionsRow.AddComponent<ResponsiveOptionsLayout>();
            if (responsive == null)
            {
                Debug.LogError("[Device] Failed to create ResponsiveOptionsLayout for options row.");
            }

            var optA = MakeOptionPanel(optionsRow.transform, font, out optionATitle, BadgeText, BadgeBg);
            qrImage = MakeQr(optA.transform);
            qrFallbackText = MakeLabel(optA.transform, "", font, 13, FontStyle.Normal, MutedColor, TextAnchor.MiddleCenter);
            qrFallbackText.gameObject.SetActive(false);
            stepA1Text = MakeLabel(optA.transform, "", font, 14, FontStyle.Normal, new Color(0.2f, 0.2f, 0.2f), TextAnchor.MiddleCenter);
            stepA2Text = MakeLabel(optA.transform, "", font, 14, FontStyle.Normal, new Color(0.2f, 0.2f, 0.2f), TextAnchor.MiddleCenter);

            var optB = MakeOptionPanel(optionsRow.transform, font, out optionBTitle, BadgeText, BadgeBg);
            stepB1Text = MakeLabel(optB.transform, "", font, 14, FontStyle.Normal, new Color(0.2f, 0.2f, 0.2f), TextAnchor.MiddleCenter);
            openCompleteButton = MakeButton(optB.transform, "", font, 16, Color.white, PrimaryBg, out openCompleteLabel);
            OnClick(openCompleteButton, OnOpenComplete);
            noTypingText = MakeLabel(optB.transform, "", font, 13, FontStyle.Bold, OkGreen, TextAnchor.MiddleCenter);
            copyLinkHintBText = MakeLabel(optB.transform, "", font, 13, FontStyle.Normal, MutedColor, TextAnchor.MiddleCenter);
            var urlRowB = MakeUrlRow(optB.transform, font, out completeUrlText, out copyCompleteLinkButton, out copyCompleteLinkLabel);
            OnClick(copyCompleteLinkButton, OnCopyCompleteLink);

            var optC = MakeOptionPanel(optionsRow.transform, font, out optionCTitle, BadgeText, BadgeBg);
            stepC1Text = MakeLabel(optC.transform, "", font, 14, FontStyle.Normal, new Color(0.2f, 0.2f, 0.2f), TextAnchor.MiddleCenter);
            openManualButton = MakeButton(optC.transform, "", font, 16, BadgeText, Color.white, out openManualLabel, true);
            OnClick(openManualButton, OnOpenManual);
            stepC2Text = MakeLabel(optC.transform, "", font, 14, FontStyle.Normal, new Color(0.2f, 0.2f, 0.2f), TextAnchor.MiddleCenter);
            var codeRow = new GameObject("CodeRow");
            codeRow.transform.SetParent(optC.transform, false);
            codeRow.AddComponent<RectTransform>();
            var codeH = codeRow.AddComponent<HorizontalLayoutGroup>();
            codeH.childAlignment = TextAnchor.MiddleCenter;
            codeH.spacing = 8;
            codeH.childControlWidth = true;
            codeH.childForceExpandWidth = false;
            var codeBox = MakeBox(codeRow.transform, CodeBg, 8, true, "code");
            codeText = MakeLabel(codeBox != null ? codeBox.transform : codeRow.transform, "", font, 26, FontStyle.Bold, new Color(0.1f, 0.1f, 0.18f), TextAnchor.MiddleCenter, "code");
            copyCodeButton = MakeButton(codeRow.transform, "", font, 14, BadgeText, BadgeBg, out copyCodeLabel);
            OnClick(copyCodeButton, OnCopyCode);
            copyLinkHintCText = MakeLabel(optC.transform, "", font, 13, FontStyle.Normal, MutedColor, TextAnchor.MiddleCenter);
            var urlRowC = MakeUrlRow(optC.transform, font, out manualUrlText, out copyManualLinkButton, out copyManualLinkLabel);
            OnClick(copyManualLinkButton, OnCopyManualLink);

            var statusRow = new GameObject("StatusRow");
            statusRow.transform.SetParent(deviceRoot.transform, false);
            statusRow.AddComponent<RectTransform>();
            var statusH = statusRow.AddComponent<HorizontalLayoutGroup>();
            statusH.childAlignment = TextAnchor.MiddleCenter;
            statusH.spacing = 10;
            waitingText = MakeLabel(statusRow.transform, "", font, 13, FontStyle.Normal, MutedColor, TextAnchor.MiddleCenter);
            expiryText = MakeLabel(deviceRoot.transform, "", font, 13, FontStyle.Bold, ExpiryBlue, TextAnchor.MiddleCenter);

            errorHolder = MakeBox(card.transform, ErrorBg, 10, false, "error");
            var errorParent = errorHolder != null ? errorHolder.transform : card.transform;
            errorText = MakeLabel(errorParent, "", font, 13, FontStyle.Normal, ErrorText, TextAnchor.MiddleCenter, "error");
            if (errorHolder != null)
            {
                errorHolder.SetActive(false);
            }
            else if (errorText != null)
            {
                errorText.gameObject.SetActive(false);
            }

            int totalObjects = GetComponentsInChildren<Transform>(true).Length;
            Debug.Log("[Device] UI built. Total objects: " + totalObjects + ", canvas sorting: " + canvas.sortingOrder);
            Canvas.ForceUpdateCanvases();
            LogGeometry(contentRt, viewportRt, card, font);
            uiBuilt = true;
        }

        private GameObject MakeOptionPanel(Transform parent, Font font, out Text badge, Color badgeFg, Color badgeBg)
        {
            var panel = new GameObject("Option");
            panel.transform.SetParent(parent, false);
            AddVertical(panel);
            var flex = panel.AddComponent<LayoutElement>();
            if (flex == null)
            {
                Debug.LogError("[Device] Failed to create LayoutElement for option panel.");
            }
            else
            {
                flex.flexibleWidth = 1;
            }
            var img = panel.AddComponent<Image>();
            img.color = PanelBg;
            var badgeRow = new GameObject("BadgeRow");
            badgeRow.transform.SetParent(panel.transform, false);
            badgeRow.AddComponent<RectTransform>();
            var badgeH = badgeRow.AddComponent<HorizontalLayoutGroup>();
            badgeH.childAlignment = TextAnchor.MiddleCenter;
            var badgeBox = MakeBox(badgeRow.transform, badgeBg, 3, true, "badge");
            badge = MakeLabel(badgeBox != null ? badgeBox.transform : badgeRow.transform, "", font, 11, FontStyle.Bold, badgeFg, TextAnchor.MiddleCenter, "badge");
            return panel;
        }

        private GameObject MakeUrlRow(Transform parent, Font font, out Text urlText, out Button copyButton, out Text copyLabel)
        {
            urlText = null;
            copyButton = null;
            copyLabel = null;
            if (parent == null)
            {
                Debug.LogError("[Device] MakeUrlRow called with null parent.");
                return null;
            }
            var row = new GameObject("UrlRow");
            row.transform.SetParent(parent, false);
            row.AddComponent<RectTransform>();
            var h = row.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 6;
            h.childAlignment = TextAnchor.UpperCenter;
            h.childControlWidth = true;
            h.childForceExpandWidth = true;
            urlText = MakeLabel(row.transform, "", font, 12, FontStyle.Normal, LinkBlue, TextAnchor.UpperLeft);
            copyButton = MakeSmallButton(row.transform, "", font, out copyLabel);
            return row;
        }

        private void AddVertical(GameObject go)
        {
            if (go.GetComponent<RectTransform>() == null) go.AddComponent<RectTransform>();
            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.spacing = 4;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            var fitter = go.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private Text MakeLabel(Transform parent, string text, Font font, int size, FontStyle style, Color color, TextAnchor align, string purpose = "text")
        {
            if (parent == null)
            {
                Debug.LogError("[Device] MakeLabel '" + purpose + "' called with null parent.");
                return null;
            }
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            if (t == null)
            {
                Debug.LogError("[Device] Failed to create Text component for '" + purpose + "'.");
                return null;
            }
            t.font = font;
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            t.alignment = align;
            t.supportRichText = true;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.text = text;
            return t;
        }

        private GameObject MakeBox(Transform parent, Color bg, int padding, bool fitWidth, string purpose)
        {
            if (parent == null)
            {
                Debug.LogError("[Device] MakeBox '" + purpose + "' called with null parent.");
                return null;
            }
            var holder = new GameObject("Box");
            holder.transform.SetParent(parent, false);
            holder.AddComponent<RectTransform>();
            var img = holder.AddComponent<Image>();
            if (img == null)
            {
                Debug.LogError("[Device] Failed to create background Image for '" + purpose + "'.");
                GameObject.Destroy(holder);
                return null;
            }
            img.color = bg;
            var layout = holder.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(padding, padding, padding, padding);
            layout.spacing = 0;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            var fitter = holder.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = fitWidth ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return holder;
        }

        private static void SetText(Text label, string value)
        {
            if (label != null)
            {
                label.text = value;
            }
        }

        private static void SetButtonVisible(Button button, bool visible)
        {
            if (button != null)
            {
                button.gameObject.SetActive(visible);
            }
        }

        private static void OnClick(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.AddListener(action);
            }
            else
            {
                Debug.LogError("[Device] Cannot wire button handler: button is null.");
            }
        }

        private RawImage MakeQr(Transform parent)
        {
            if (parent == null)
            {
                Debug.LogError("[Device] MakeQr called with null parent.");
                return null;
            }
            var holder = new GameObject("QRHolder");
            holder.transform.SetParent(parent, false);
            var holderRt = holder.AddComponent<RectTransform>();
            holderRt.sizeDelta = new Vector2(186, 186);
            var holderH = holder.AddComponent<HorizontalLayoutGroup>();
            holderH.childAlignment = TextAnchor.MiddleCenter;
            var holderImg = holder.AddComponent<Image>();
            holderImg.color = Color.white;
            var go = new GameObject("QR");
            go.transform.SetParent(holder.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(170, 170);
            var img = go.AddComponent<RawImage>();
            if (img == null)
            {
                Debug.LogError("[Device] Failed to create RawImage for QR.");
                return null;
            }
            img.color = Color.white;
            var le = go.AddComponent<LayoutElement>();
            le.minWidth = 170;
            le.minHeight = 170;
            le.preferredWidth = 170;
            le.preferredHeight = 170;
            return img;
        }

        private Button MakeButton(Transform parent, string label, Font font, int size, Color fg, Color bg, out Text labelOut, bool outlined)
        {
            var btn = MakeButton(parent, label, font, size, fg, bg, out labelOut);
            if (btn == null)
            {
                return null;
            }
            if (outlined)
            {
                var outline = btn.gameObject.AddComponent<Outline>();
                outline.effectColor = PrimaryBg;
                outline.effectDistance = new Vector2(2, 2);
            }
            return btn;
        }

        private Button MakeButton(Transform parent, string label, Font font, int size, Color fg, Color bg, out Text labelOut, string purpose = "button")
        {
            labelOut = null;
            if (parent == null)
            {
                Debug.LogError("[Device] MakeButton '" + purpose + "' called with null parent.");
                return null;
            }
            var go = new GameObject("Button");
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var img = go.AddComponent<Image>();
            if (img == null)
            {
                Debug.LogError("[Device] Failed to create Image for button '" + purpose + "'.");
                return null;
            }
            img.color = bg;
            var btn = go.AddComponent<Button>();
            if (btn == null)
            {
                Debug.LogError("[Device] Failed to create Button for '" + purpose + "'.");
                return null;
            }
            btn.targetGraphic = img;
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 44;
            le.preferredHeight = 44;
            var t = new GameObject("Label");
            t.transform.SetParent(go.transform, false);
            var trt = t.AddComponent<RectTransform>();
            Stretch(trt, 12, 6, 12, 6);
            var txt = t.AddComponent<Text>();
            if (txt == null)
            {
                Debug.LogError("[Device] Failed to create Text for button '" + purpose + "' label.");
                labelOut = null;
                return null;
            }
            txt.font = font;
            txt.fontSize = size;
            txt.fontStyle = FontStyle.Bold;
            txt.color = fg;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.supportRichText = true;
            txt.text = label;
            labelOut = txt;
            return btn;
        }

        private Button MakeSmallButton(Transform parent, string label, Font font, out Text labelOut)
        {
            var btn = MakeButton(parent, label, font, 12, BadgeText, BadgeBg, out labelOut, "small-button");
            if (btn == null)
            {
                return null;
            }
            var le = btn.gameObject.GetComponent<LayoutElement>();
            le.minHeight = 32;
            le.preferredHeight = 32;
            le.minWidth = 90;
            le.preferredWidth = 90;
            le.flexibleWidth = 0;
            return btn;
        }

        private static void LogGeometry(RectTransform contentRt, RectTransform viewportRt, GameObject cardObj, Font font)
        {
            var cardRt = cardObj != null ? cardObj.GetComponent<RectTransform>() : null;
            string cardInfo = cardRt != null ? ("pos=" + cardRt.anchoredPosition + " size=" + cardRt.rect.size) : "NULL";
            string contentInfo = contentRt != null ? ("size=" + contentRt.rect.size + " pos=" + contentRt.anchoredPosition) : "NULL";
            string viewportInfo = viewportRt != null ? ("size=" + viewportRt.rect.size) : "NULL";
            Debug.Log("[Device] Geometry: card[" + cardInfo + "] content[" + contentInfo + "] viewport[" + viewportInfo + "] font=" + (font != null ? font.name : "NULL"));
        }

        private static void Stretch(RectTransform rt, float left, float top, float right, float bottom)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }
    }
}
