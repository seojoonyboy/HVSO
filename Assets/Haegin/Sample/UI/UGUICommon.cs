using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Haegin
{
    public static class UGUICommon
    {
        public enum HelpDialogAction
        {
            Open,
            Close
        }
        public delegate void OnOpenEULADetailDialog(string title, string content);
        public delegate void OnHelpDialog(HelpDialogAction action);

        public static void OpenEULADialog(GameObject eulabg, Canvas canvasEULA, string[] titles, string[] contents, bool[] isChecked, OnOpenEULADetailDialog onOpenDetail, EULA.OnConfirmEULA onConfirm)
        {
            GameObject EULAWin = null;
            Toggle toggleEULA = null;
            Toggle togglePersonal = null;
            Toggle toggleNightSMS = null;
            Button startButton = null;

            EULAWin = (GameObject)Object.Instantiate(eulabg);

            EULAWin.transform.SetParent(canvasEULA.transform);
            EULAWin.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
            EULAWin.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

            startButton = GameObject.Find("ButtonStart").GetComponent<Button>();
            toggleEULA = GameObject.Find("EULAToggle1").GetComponent<Toggle>();
            toggleEULA.onValueChanged.AddListener((bool value) =>
            {
                startButton.interactable = toggleEULA.isOn && togglePersonal.isOn;
            });
            toggleEULA.isOn = isChecked[0];
            togglePersonal = GameObject.Find("EULAToggle2").GetComponent<Toggle>();
            togglePersonal.onValueChanged.AddListener((bool value) =>
            {
                startButton.interactable = toggleEULA.isOn && togglePersonal.isOn;
            });
            togglePersonal.isOn = isChecked[1];
            if (isChecked.Length > 2)
            {
                toggleNightSMS = GameObject.Find("EULAToggle3").GetComponent<Toggle>();
                toggleNightSMS.isOn = isChecked[2];
            }
            else
            {
                toggleNightSMS = null;
            }

            startButton.interactable = toggleEULA.isOn && togglePersonal.isOn;

            ThreadSafeDispatcher.Instance.PushSystemBackKeyListener(() =>
            {
                if (toggleEULA.isOn && togglePersonal.isOn)
                {
                    ThreadSafeDispatcher.Instance.PopSystemBackKeyListener();
                    onConfirm(toggleEULA.isOn, togglePersonal.isOn, toggleNightSMS.isOn);
                    EULAWin.transform.SetParent(null);
                }
            });

            GameObject.Find("EULATitle1").GetComponent<Text>().text = titles[0];
            GameObject.Find("EULATitle2").GetComponent<Text>().text = titles[1];
            if (isChecked.Length > 2)
            {
                GameObject.Find("EULATitle3").GetComponent<Text>().text = titles[2];
            }
            GameObject.Find("ButtonAllAgree").GetComponent<Button>().onClick.AddListener(() =>
            {
                ThreadSafeDispatcher.Instance.PopSystemBackKeyListener();

                toggleEULA.isOn = true;
                togglePersonal.isOn = true;
                if (toggleNightSMS == null)
                {
                    onConfirm(toggleEULA.isOn, togglePersonal.isOn, false);
                }
                else
                {
                    toggleNightSMS.isOn = true;
                    onConfirm(toggleEULA.isOn, togglePersonal.isOn, toggleNightSMS.isOn);
                }
                EULAWin.transform.SetParent(null);
                Object.Destroy(EULAWin);
            });
            GameObject.Find("ButtonStart").GetComponent<Button>().onClick.AddListener(() =>
            {
                if (toggleEULA.isOn && togglePersonal.isOn)
                {
                    ThreadSafeDispatcher.Instance.PopSystemBackKeyListener();
                    if (toggleNightSMS == null)
                    {
                        onConfirm(toggleEULA.isOn, togglePersonal.isOn, false);
                    }
                    else
                    {
                        onConfirm(toggleEULA.isOn, togglePersonal.isOn, toggleNightSMS.isOn);
                    }
                    EULAWin.transform.SetParent(null);
                    Object.Destroy(EULAWin);
                }
            });
            GameObject.Find("ButtonEULADetail1").GetComponent<Button>().onClick.AddListener(() =>
            {
                onOpenDetail(titles[0], contents[0]);
            });
            GameObject.Find("ButtonEULADetail2").GetComponent<Button>().onClick.AddListener(() =>
            {
                onOpenDetail(titles[1], contents[1]);
            });
            if (isChecked.Length > 2)
            {
                GameObject.Find("ButtonEULADetail3").GetComponent<Button>().onClick.AddListener(() =>
                {
                    onOpenDetail(titles[2], contents[2]);
                });
            }
            else
            {
                GameObject.Find("List3").transform.SetParent(null);
                GameObject.Find("List1").transform.position = new Vector3(GameObject.Find("List1").transform.position.x, GameObject.Find("List1").transform.position.y - 40, GameObject.Find("List1").transform.position.z);
                GameObject.Find("List2").transform.position = new Vector3(GameObject.Find("List2").transform.position.x, GameObject.Find("List2").transform.position.y - 60, GameObject.Find("List2").transform.position.z);
            }
        }

        static bool[] backupValues = new bool[8];
        public static void SetEULAInteractable(bool value)
        {
            if(value == false) 
            {
                backupValues[0] = GameObject.Find("ButtonStart").GetComponent<Button>().interactable;
                backupValues[1] = GameObject.Find("EULAToggle1").GetComponent<Toggle>().interactable;
                backupValues[2] = GameObject.Find("EULAToggle2").GetComponent<Toggle>().interactable;
                if (GameObject.Find("EULAToggle3") != null) backupValues[3] = GameObject.Find("EULAToggle3").GetComponent<Toggle>().interactable;
                backupValues[4] = GameObject.Find("ButtonAllAgree").GetComponent<Button>().interactable;
                backupValues[5] = GameObject.Find("ButtonEULADetail1").GetComponent<Button>().interactable;
                backupValues[6] = GameObject.Find("ButtonEULADetail2").GetComponent<Button>().interactable;
                if (GameObject.Find("ButtonEULADetail3") != null) backupValues[7] = GameObject.Find("ButtonEULADetail3").GetComponent<Button>().interactable;

                GameObject.Find("ButtonStart").GetComponent<Button>().interactable = value;
                GameObject.Find("EULAToggle1").GetComponent<Toggle>().interactable = value;
                GameObject.Find("EULAToggle2").GetComponent<Toggle>().interactable = value;
                if (GameObject.Find("EULAToggle3") != null) GameObject.Find("EULAToggle3").GetComponent<Toggle>().interactable = value;
                GameObject.Find("ButtonAllAgree").GetComponent<Button>().interactable = value;
                GameObject.Find("ButtonEULADetail1").GetComponent<Button>().interactable = value;
                GameObject.Find("ButtonEULADetail2").GetComponent<Button>().interactable = value;
                if (GameObject.Find("ButtonEULADetail3") != null) GameObject.Find("ButtonEULADetail3").GetComponent<Button>().interactable = value;
            }
            else 
            {
                GameObject.Find("ButtonStart").GetComponent<Button>().interactable = backupValues[0];
                GameObject.Find("EULAToggle1").GetComponent<Toggle>().interactable = backupValues[1];
                GameObject.Find("EULAToggle2").GetComponent<Toggle>().interactable = backupValues[2];
                if (GameObject.Find("EULAToggle3") != null) GameObject.Find("EULAToggle3").GetComponent<Toggle>().interactable = backupValues[3];
                GameObject.Find("ButtonAllAgree").GetComponent<Button>().interactable = backupValues[4];
                GameObject.Find("ButtonEULADetail1").GetComponent<Button>().interactable = backupValues[5];
                GameObject.Find("ButtonEULADetail2").GetComponent<Button>().interactable = backupValues[6];
                if (GameObject.Find("ButtonEULADetail3") != null) GameObject.Find("ButtonEULADetail3").GetComponent<Button>().interactable = backupValues[7];
            }
        }

        public static void ShowEULADetailWindow(GameObject euladetail_l, GameObject euladetail_p, GameObject eulatext, Canvas canvasEULA, string title, string content)
        {
            SetEULAInteractable(false);

            GameObject EULADetailWin = null;
            bool isLandscape = canvasEULA.GetComponent<CanvasScaler>().referenceResolution.x > canvasEULA.GetComponent<CanvasScaler>().referenceResolution.y;
            if (isLandscape)
            {
                EULADetailWin = (GameObject)Object.Instantiate(euladetail_l);
            }
            else
            {
                EULADetailWin = (GameObject)Object.Instantiate(euladetail_p);
            }

            EULADetailWin.transform.SetParent(canvasEULA.transform);
            EULADetailWin.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
            EULADetailWin.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

            ThreadSafeDispatcher.Instance.PushSystemBackKeyListener(() =>
            {
                ThreadSafeDispatcher.Instance.PopSystemBackKeyListener();
                EULADetailWin.transform.SetParent(null);
                Object.Destroy(EULADetailWin);
                GameObject gameObject = GameObject.Find("WebViewObject");
                gameObject.transform.SetParent(null);
                Object.Destroy(gameObject);
                SetEULAInteractable(true);
            });
            GameObject.Find("EULATitle").GetComponent<Text>().text = title;
            GameObject contentObject = GameObject.Find("bg");

            Rect rect = contentObject.GetComponent<RectTransform>().rect;
            Rect canvasRect = canvasEULA.GetComponent<RectTransform>().rect;
            EULADetailWin.transform.SetParent(null);
            int left = 0, top = 0, bottom = 0;
            float scaleFactor = canvasEULA.scaleFactor * originWidth / Screen.width;

            switch (canvasEULA.renderMode)
            {
                case RenderMode.ScreenSpaceOverlay:
                case RenderMode.ScreenSpaceCamera:
                    left = (int)(scaleFactor * (canvasRect.width - rect.width) * 0.5f);
                    top = (int)(scaleFactor * ((canvasRect.height - rect.height) * 0.5f + 45));
                    bottom = (int)(scaleFactor * ((canvasRect.height - rect.height) * 0.5f - 45));
                    break;
                case RenderMode.WorldSpace:
                default:
                    left = top = bottom = 100;
                    break;
            }
#if MDEBUG
            Debug.Log(string.Format("Display Size {0}, {1}\nScreen Size {2}, {3}\nWin Rect  {4}, {5}, {6}, {7}\nCanvas Rect  {8}, {9}, {10}, {11}\nscale = {12}, left = {13}, top = {14}, bottom = {15}", Display.main.systemWidth, Display.main.systemHeight, Screen.width, Screen.height, rect.x, rect.y, rect.width, rect.height, canvasRect.x, canvasRect.y, canvasRect.width, canvasRect.height, scaleFactor, left, top, bottom));
#endif
            WebViewObject webViewObject = null;
            if (webViewObject != null)
            {
                GameObject gameObject = GameObject.Find("WebViewObject");
                gameObject.transform.SetParent(null);
                Object.Destroy(gameObject);
                webViewObject = null;
                return;
            }
            webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
            webViewObject.SetVisibility(false);
            webViewObject.Init(
                cb: (msg) =>
                {
#if MDEBUG                
                    Debug.Log(string.Format("CallFromJS[{0}]", msg));
#endif
                },
                err: (msg) =>
                {
#if MDEBUG
                    Debug.Log(string.Format("CallOnError[{0}]", msg));
#endif
                },
                started: (msg) =>
                {
#if MDEBUG                
                    Debug.Log(string.Format("1CallOnStarted[{0}]", msg));
#endif
                },
                ld: (msg) =>
                {
#if MDEBUG                
                    Debug.Log(string.Format("CallOnLoaded[{0}]", msg));
#endif
#if !UNITY_ANDROID
                    // NOTE: depending on the situation, you might prefer
                    // the 'iframe' approach.
                    // cf. https://github.com/gree/unity-webview/issues/189

                    webViewObject.EvaluateJS(@"
                        if (window && window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.unityControl) {
                            window.Unity = {
                                call: function(msg) {
                                    window.webkit.messageHandlers.unityControl.postMessage(msg);
                                }
                            }
                        } else {
                            window.Unity = {
                                call: function(msg) {
                                    window.location = 'unity:' + msg;
                                }
                            }
                        }
                    ");
#endif
                    webViewObject.SetVisibility(true);
                    EULADetailWin.transform.SetParent(canvasEULA.transform);
                    EULADetailWin.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
                    EULADetailWin.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                },
            //ua: "custom user agent string",
            enableWKWebView: true);
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            webViewObject.bitmapRefreshCycle = 1;
#endif
            webViewObject.SetMargins(left, top, left, bottom);

            webViewObject.LoadHTML("<html><body style=\"background-color: #ffffff; font-size: 150%; padding: 10px; color: #484b4f; \">" + content.Replace("\n", "<br>") + "</body></html>", "http://localhost");

            contentObject.transform.SetParent(null);
            Object.Destroy(contentObject);

            GameObject.Find("ButtonClose").GetComponent<Button>().onClick.AddListener(() =>
            {
                ThreadSafeDispatcher.Instance.PopSystemBackKeyListener();
                EULADetailWin.transform.SetParent(null);
                Object.Destroy(EULADetailWin);
                GameObject gameObject = GameObject.Find("WebViewObject");
                gameObject.transform.SetParent(null);
                Object.Destroy(gameObject);
                SetEULAInteractable(true);
            });
        }

        public static void OpenPromoEventWindow(GameObject promoeventframe, Canvas canvasEvents, Sprite sprite, string destUrl, PromoEvents.OnCloseWindow onClose)
        {
            GameObject EventsWin = (GameObject)Object.Instantiate(promoeventframe);

            EventsWin.transform.SetParent(canvasEvents.transform);
            EventsWin.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
            EventsWin.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

            ThreadSafeDispatcher.Instance.PushSystemBackKeyListener(() =>
            {
                ThreadSafeDispatcher.Instance.PopSystemBackKeyListener();
                bool doNotShowAgainToday = GameObject.Find("Toggle").GetComponent<Toggle>().isOn;
                EventsWin.transform.SetParent(null);
                Object.Destroy(EventsWin);
                onClose(doNotShowAgainToday);
            });

            Image image = (Image)GameObject.Find("EventImage").GetComponent<Image>();
            image.sprite = sprite;

            GameObject.Find("Close").GetComponent<Button>().onClick.AddListener(() =>
            {
                ThreadSafeDispatcher.Instance.PopSystemBackKeyListener();
                bool doNotShowAgainToday = GameObject.Find("Toggle").GetComponent<Toggle>().isOn;
                EventsWin.transform.SetParent(null);
                Object.Destroy(EventsWin);
                onClose(doNotShowAgainToday);
            });
            GameObject.Find("EventImage").GetComponent<Button>().onClick.AddListener(() =>
            {
                Application.OpenURL(destUrl);
            });
        }


        public delegate void OnFinishedDelegate();
        public static void ShowVersionUpWindow(GameObject updatemessage, GameObject eulatext, Canvas canvasVersionUp, OnFinishedDelegate callback, bool isOption, string versionInfo, string urlString)
        {
            GameObject VersionUpWin = (GameObject)Object.Instantiate(updatemessage);

            VersionUpWin.transform.SetParent(canvasVersionUp.transform);
            VersionUpWin.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
            VersionUpWin.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

            ThreadSafeDispatcher.Instance.PushSystemBackKeyListener(() =>
            {
                if (isOption)
                {
                    ThreadSafeDispatcher.Instance.PopSystemBackKeyListener();
                    VersionUpWin.transform.SetParent(null);
                    Object.Destroy(VersionUpWin);
                    callback();
                }
            });

            if (string.IsNullOrEmpty(versionInfo)) versionInfo = "";

            string[] substrings = versionInfo.Split('|');
            GameObject contentObject = GameObject.Find("UpdateContent");
            for (int i = 0; i < substrings.Length; i++)
            {
                GameObject textBlock = (GameObject)Object.Instantiate(eulatext);
                textBlock.transform.SetParent(contentObject.transform);
                textBlock.transform.localRotation = Quaternion.identity;
                textBlock.transform.localPosition = Vector3.zero;
                textBlock.transform.localScale = Vector3.one;
                textBlock.GetComponent<Text>().text = substrings[i];
                textBlock.name = i.ToString();
            }

            if (!isOption)
            {
                GameObject.Find("Cancel").transform.SetParent(null);
            }
            else
            {
                GameObject.Find("Cancel").GetComponent<Button>().onClick.AddListener(() =>
                {
                    ThreadSafeDispatcher.Instance.PopSystemBackKeyListener();
                    VersionUpWin.transform.SetParent(null);
                    Object.Destroy(VersionUpWin);
                    callback();
                });
            }
            GameObject.Find("GoToUpdate").GetComponent<Button>().onClick.AddListener(() =>
            {
                // URL로 연결 
                Application.OpenURL(urlString);
            });
        }


        public enum ButtonType
        {
            Yes,
            No,
            Ok
        };
        public delegate void OnButtonDelegate(ButtonType button);

        public static void ShowYesNoDialog(GameObject systemdialog, GameObject eulatext, Canvas canvasVersionUp, string title, string message, OnButtonDelegate callback)
        {
            ButtonType backKeyButton = ButtonType.No;

            GameObject DialogWin = (GameObject)Object.Instantiate(systemdialog);

            DialogWin.transform.SetParent(canvasVersionUp.transform);
            DialogWin.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
            DialogWin.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

            ThreadSafeDispatcher.Instance.PushSystemBackKeyListener(() =>
            {
                ThreadSafeDispatcher.Instance.PopSystemBackKeyListener();
                DialogWin.transform.SetParent(null);
                Object.Destroy(DialogWin);
                callback(backKeyButton);
            });

            GameObject.Find("DialogTitle").GetComponent<Text>().text = title;

            string[] substrings = message.Split('|');
            GameObject contentObject = GameObject.Find("DialogContent");
            for (int i = 0; i < substrings.Length; i++)
            {
                GameObject textBlock = (GameObject)Object.Instantiate(eulatext);
                textBlock.transform.SetParent(contentObject.transform);
                textBlock.transform.localRotation = Quaternion.identity;
                textBlock.transform.localPosition = Vector3.zero;
                textBlock.transform.localScale = Vector3.one;

                textBlock.GetComponent<Text>().text = substrings[i];
                textBlock.name = i.ToString();
            }

            GameObject.Find("DialogYes").GetComponent<Button>().onClick.AddListener(() =>
            {
                ThreadSafeDispatcher.Instance.PopSystemBackKeyListener();
                DialogWin.transform.SetParent(null);
                Object.Destroy(DialogWin);
                callback(ButtonType.Yes);
            });
            GameObject.Find("DialogNo").GetComponent<Button>().onClick.AddListener(() =>
            {
                ThreadSafeDispatcher.Instance.PopSystemBackKeyListener();
                DialogWin.transform.SetParent(null);
                Object.Destroy(DialogWin);
                callback(ButtonType.No);
            });
            Object.Destroy(GameObject.Find("DialogOk"));
        }

        public static void ShowMessageDialog(GameObject systemdialog, GameObject eulatext, Canvas canvasVersionUp, string title, string message, OnButtonDelegate callback)
        {
            ButtonType backKeyButton = ButtonType.No;

            GameObject DialogWin = (GameObject)Object.Instantiate(systemdialog);

            DialogWin.transform.SetParent(canvasVersionUp.transform);
            DialogWin.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
            DialogWin.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

            ThreadSafeDispatcher.Instance.PushSystemBackKeyListener(() =>
            {
                ThreadSafeDispatcher.Instance.PopSystemBackKeyListener();
                DialogWin.transform.SetParent(null);
                Object.Destroy(DialogWin);
                callback(backKeyButton);
            });

            GameObject.Find("DialogTitle").GetComponent<Text>().text = title;

            string[] substrings = message.Split('|');
            GameObject contentObject = GameObject.Find("DialogContent");
            for (int i = 0; i < substrings.Length; i++)
            {
                GameObject textBlock = (GameObject)Object.Instantiate(eulatext);
                textBlock.transform.SetParent(contentObject.transform);
                textBlock.transform.localRotation = Quaternion.identity;
                textBlock.transform.localPosition = Vector3.zero;
                textBlock.transform.localScale = Vector3.one;

                textBlock.GetComponent<Text>().text = substrings[i];
                textBlock.name = i.ToString();
            }

            GameObject.Find("DialogOk").GetComponent<Button>().onClick.AddListener(() =>
            {
                ThreadSafeDispatcher.Instance.PopSystemBackKeyListener();
                DialogWin.transform.SetParent(null);
                Object.Destroy(DialogWin);
                callback(ButtonType.Ok);
            });
            Object.Destroy(GameObject.Find("DialogYes"));
            Object.Destroy(GameObject.Find("DialogNo"));
        }

        static float originWidth = 0;

        public static void SaveScreenSize()
        {
            originWidth = Screen.width;
        }

        public static void ShowHelpWindow(GameObject helpDialog, Canvas canvas, Help.HelpItem item, string baseUrl, string userId, string nickname, string appversion, OnHelpDialog callback)
        {
            GameObject DialogWin = (GameObject)Object.Instantiate(helpDialog);

            DialogWin.transform.SetParent(canvas.transform);
            DialogWin.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
            DialogWin.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            DialogWin.transform.SetParent(null);
            Rect rect = DialogWin.GetComponent<RectTransform>().rect;
            Rect canvasRect = canvas.GetComponent<RectTransform>().rect;
            int left = 0, top = 0, bottom = 0;
            float scaleFactor = canvas.scaleFactor * originWidth / Screen.width;

            switch(canvas.renderMode)
            {
                case RenderMode.ScreenSpaceOverlay:
                case RenderMode.ScreenSpaceCamera:
                    left = (int)(scaleFactor * (canvasRect.width - rect.width + 0) * 0.5f);
                    top = (int)(scaleFactor * (canvasRect.height - rect.height + 0) * 0.5f);
                    bottom = (int)(scaleFactor * ((canvasRect.height - rect.height + 0) * 0.5f + 154));
                    break;
                case RenderMode.WorldSpace:
                default:
                    left = top = bottom = 100;
                    break;
            }
#if MDEBUG
            Debug.Log(string.Format("Display Size {0}, {1}\nScreen Size {2}, {3}\nWin Rect  {4}, {5}, {6}, {7}\nCanvas Rect  {8}, {9}, {10}, {11}\nscale = {12}, left = {13}, top = {14}, bottom = {15}", Display.main.systemWidth, Display.main.systemHeight, Screen.width, Screen.height, rect.x, rect.y, rect.width, rect.height, canvasRect.x, canvasRect.y, canvasRect.width, canvasRect.height, scaleFactor, left, top, bottom));
#endif
            string[] supportedLanguages = { "English", "Korean" };
            Help.OpenHelpWebView(item, baseUrl, left, top, left, bottom, userId, nickname, appversion, () => {
                DialogWin.transform.SetParent(canvas.transform);
                DialogWin.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
                DialogWin.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                if (callback != null)
                    callback(HelpDialogAction.Open);
            }, supportedLanguages);

            ThreadSafeDispatcher.Instance.PushSystemBackKeyListener(() =>
            {
                ThreadSafeDispatcher.Instance.PopSystemBackKeyListener();
                DialogWin.transform.SetParent(null);
                Object.Destroy(DialogWin);
                Help.CloseHelpWebView();
                if (callback != null)
                    callback(HelpDialogAction.Close);
            });

            GameObject.Find("HelpWinButtonClose").GetComponent<Button>().onClick.AddListener(() =>
            {
                ThreadSafeDispatcher.Instance.PopSystemBackKeyListener();
                DialogWin.transform.SetParent(null);
                Object.Destroy(DialogWin);
                Help.CloseHelpWebView();
                if (callback != null)
                    callback(HelpDialogAction.Close);
            });
        }

        public static GameObject ShowServiceCheckDialog(GameObject serviceDialog, Canvas canvas, string contents, string time, ServiceMaintenance.OnRetry callback)
        {
            GameObject DialogWin = (GameObject)Object.Instantiate(serviceDialog);

            DialogWin.transform.SetParent(canvas.transform);
            DialogWin.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
            DialogWin.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

            GameObject.Find("MaintenanceTime").GetComponent<Text>().text = time;
            GameObject.Find("MaintenanceContents").GetComponent<Text>().text = contents;

            GameObject.Find("ServerCheckWinButtonRetry").GetComponent<Button>().onClick.AddListener(() =>
            {
                callback();
            });

            return DialogWin;
        }

        public static void CloseServiceCheckDialog(GameObject DialogWin)
        {
            DialogWin.transform.SetParent(null);
            Object.Destroy(DialogWin);
            Help.CloseHelpWebView();
        }

        public static void ShowRequestPermission(GameObject systemdialog, Canvas canvas, string title, string message, OnButtonDelegate callback)
        {
            GameObject DialogWin = (GameObject)Object.Instantiate(systemdialog);

            DialogWin.transform.SetParent(canvas.transform);
            DialogWin.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
            DialogWin.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

            ThreadSafeDispatcher.Instance.PushSystemBackKeyListener(() =>
            {
                ThreadSafeDispatcher.Instance.PopSystemBackKeyListener();
                DialogWin.transform.SetParent(null);
                Object.Destroy(DialogWin);
                callback(ButtonType.Ok);
            });

            GameObject.Find("ReqTitle").GetComponent<Text>().text = title;
            GameObject.Find("ReqMessage").GetComponent<Text>().text = message;
            GameObject.Find("Okay").GetComponent<Button>().onClick.AddListener(() =>
            {
                ThreadSafeDispatcher.Instance.PopSystemBackKeyListener();
                DialogWin.transform.SetParent(null);
                Object.Destroy(DialogWin);
                callback(ButtonType.Ok);
            });
        }
    }
}
