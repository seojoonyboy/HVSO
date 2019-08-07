using UnityEngine;

namespace Haegin
{
    public class Help
    {
        public delegate void OnLoaded();
        public enum HelpItem
        {
            None,
            Main,
            PrivacyPolicy,
            TermsOfService
        }; 
        static WebViewObject webViewObject = null;

        public static void CloseWebView()
        {
            if (webViewObject != null)
            {
                webViewObject.transform.SetParent(null);
                Object.Destroy(webViewObject.gameObject);
            }
            webViewObject = null;
        }

        public static void OpenWebView(string Url, int left, int top, int right, int bottom, string userId, string nickname, string appversion, OnLoaded callback = null) 
        {
            if(webViewObject != null) 
            {
                CloseWebView();
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
                    webViewObject.LoadHTML("<html><body><br><br><p style=\" font-size: 3.0em; text-align: center; \">Server Not Responding</p></body></html>", "http://localhost");
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
                    if(userId != null && nickname != null && appversion != null)
                    {
                        string href = "mailto:support@haegin.kr?subject=" + System.Uri.EscapeUriString("[" + Application.productName + " Inquiry]") + "&body=" + System.Uri.EscapeUriString("UserID : " + userId + "\nNick Name : " + nickname + "\nApplication Version : " + appversion + "\nDevice Model : " + SystemInfo.deviceModel + "\nDevice OS Version : " + SystemInfo.operatingSystem + "\n\n--- " + TextManager.GetString(TextManager.StringTag.EmailContent1) + " ---\n");
                        webViewObject.EvaluateJS(@"
                            if (document.getElementById('haeginsupport') != null) { document.getElementById('haeginsupport').href='" + href + @"';
                        }");
                    }
                    webViewObject.SetVisibility(true);
                    if(callback != null) 
                        callback();
                },
                //ua: "custom user agent string",
            enableWKWebView: true);
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            webViewObject.bitmapRefreshCycle = 1;
#endif
            webViewObject.SetMargins(left, top, right, bottom);

            webViewObject.LoadURL(Url.Replace(" ", "%20"));
        }

        public static void OpenHelpWebView(HelpItem item, string baseUrl, int left, int top, int right, int bottom, string userId, string nickname, string appversion, OnLoaded callback = null, string[] supportedLanguages = null)
        {
            if(item == HelpItem.None)
            {
                OpenWebView(baseUrl, left, top, right, bottom, null, null, null, callback);
            }
            else 
            {
                string prefix = "/English";
                if (supportedLanguages != null)
                {
                    for(int i = 0; i < supportedLanguages.Length; i++)
                    {
                        if(TextManager.GetLanguageSetting().Equals(supportedLanguages[i]))
                        {
                            prefix = "/" + supportedLanguages[i];
                            break;
                        }
                    }
                }
                else if (TextManager.GetLanguageSetting().Equals("Korean"))
                {
                    prefix = "/Korean";
                }

                switch (item)
                {
                    case HelpItem.PrivacyPolicy:
                        OpenWebView(baseUrl + prefix + "_PP.html?reload=" + Random.value, left, top, right, bottom, userId, nickname, appversion, callback);
                        break;
                    case HelpItem.TermsOfService:
                        OpenWebView(baseUrl + prefix + "_ToS.html?reload=" + Random.value, left, top, right, bottom, userId, nickname, appversion, callback);
                        break;
                    default:
                        OpenWebView(baseUrl + prefix + ".html?reload=" + Random.value, left, top, right, bottom, userId, nickname, appversion, callback);
                        break;
                }
            }
        }

        public static void CloseHelpWebView()
        {
            CloseWebView();
        }

        public static void SendSupportMail(string userId, string nickname, string appversion)
        {
            string href = "mailto:support@haegin.kr?subject=" + System.Uri.EscapeUriString("[" + Application.productName + " Inquiry]") + "&body=" + System.Uri.EscapeUriString("UserID : " + userId + "\nNick Name : " + nickname + "\nApplication Version : " + appversion + "\nDevice Model : " + SystemInfo.deviceModel + "\nDevice OS Version : " + SystemInfo.operatingSystem + "\n\n--- " + TextManager.GetString(TextManager.StringTag.EmailContent1) + " ---\n");
            Application.OpenURL(href);
        }
    }
}
