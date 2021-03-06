using UnityEngine;
using System.Collections.Generic;

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
            TermsOfService,
            AcquirePossibility,
            ZendeskMain,
            ZendeskPrivacyPolicy,
            ZendeskTermsOfService,
            ZendeskAcquirePossibility,
            ZendeskDirectPage
        }; 
        public static WebViewObject webViewObject = null;

        private string LanugageToLocale(string lang)
        {
            try
            {
                if (lang.Equals("Korean")) return "ko";
                else if (lang.Equals("Japanese")) return "ja";
            }
            catch { }
            return "en_us";
        }

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
                        string body = "UserID : " + userId + "\\nNick Name : " + nickname + "\\nApplication Version : " + appversion + "\\nDevice Model : " + SystemInfo.deviceModel + "\\nDevice OS Version : " + SystemInfo.operatingSystem;
                        string href = "mailto:support@homerunclash.zendesk.com?subject=" + System.Uri.EscapeUriString("[" + Application.productName + " Inquiry]") + "&body=" + System.Uri.EscapeUriString("UserID : " + userId + "\nNick Name : " + nickname + "\nApplication Version : " + appversion + "\nDevice Model : " + SystemInfo.deviceModel + "\nDevice OS Version : " + SystemInfo.operatingSystem + "\n\n--- " + TextManager.GetString(TextManager.StringTag.EmailContent1) + " ---\n");
                        webViewObject.EvaluateJS(@"
                            if (document.getElementById('haeginsupport') != null) { document.getElementById('haeginsupport').href='" + href + @"'; }
                            if (document.getElementById('request_custom_fields_360024238834') != null) { document.getElementById('request_custom_fields_360024238834').value='" + body + @"'; document.getElementById('request_custom_fields_360024238834').readOnly = 'true'; }
                        ");
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

        public static void OpenHelpWebView(HelpItem item, string baseUrl, int left, int top, int right, int bottom, string userId, string nickname, string appversion, OnLoaded callback = null, string[] supportedLanguages = null, string zendeskDirectPageCode = null)
        {
            if(item == HelpItem.None)
            {
                OpenWebView(baseUrl, left, top, right, bottom, null, null, null, callback);
            }
            else 
            {
                string prefix = "/English";
                Dictionary<string, string> lang2locale = new Dictionary<string, string>()
                {
                    { "/English", "/en-us" },
                    { "/Korean", "/ko" },
                    { "/ChineseTraditional", "/zh-tw" },
                    { "/Japanese", "/ja" },
                    { "/Spanish", "/es" },
                    { "/German", "/de" },
                    { "/French", "/fr" },
                    { "/Indonesian", "/id" },
                    { "/ChineseSimplified", "/zh-cn" },
                    { "/Portuguese", "/pt" },
                    { "/Italian", "/it" },
                    { "/Thai", "/th" },
                    { "/Vietnamese", "/vi" },
                };
                string ZendeskBaseURL = "https://help-homerunclash.haegin.kr/hc";
                switch (item)
                {
                    case HelpItem.Main:
                    case HelpItem.PrivacyPolicy:
                    case HelpItem.TermsOfService:
                    case HelpItem.AcquirePossibility:
                        if (supportedLanguages != null)
                        {
                            for (int i = 0; i < supportedLanguages.Length; i++)
                            {
                                if (TextManager.GetLanguageSetting().Equals(supportedLanguages[i]))
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
                        break;
                    case HelpItem.ZendeskMain:
                    case HelpItem.ZendeskPrivacyPolicy:
                    case HelpItem.ZendeskTermsOfService:
                    case HelpItem.ZendeskAcquirePossibility:
                    case HelpItem.ZendeskDirectPage:
                        prefix = "/" + TextManager.GetLanguageSetting();
                        if (lang2locale.ContainsKey(prefix))
                        {
                            prefix = lang2locale[prefix];
                        }
                        else
                        {
                            prefix = "/en-us";
                        }
                        break;
                }

                switch (item)
                {
                    case HelpItem.Main:
                        OpenWebView(baseUrl + prefix + ".html?reload=" + System.DateTime.Now.Ticks, left, top, right, bottom, userId, nickname, appversion, callback);
                        break;
                    case HelpItem.PrivacyPolicy:
                        OpenWebView(baseUrl + prefix + "_PP.html?reload=" + System.DateTime.Now.Ticks, left, top, right, bottom, userId, nickname, appversion, callback);
                        break;
                    case HelpItem.TermsOfService:
                        OpenWebView(baseUrl + prefix + "_ToS.html?reload=" + System.DateTime.Now.Ticks, left, top, right, bottom, userId, nickname, appversion, callback);
                        break;
                    case HelpItem.AcquirePossibility:
                        OpenWebView(baseUrl + prefix + "_AP.html?reload=" + System.DateTime.Now.Ticks, left, top, right, bottom, userId, nickname, appversion, callback);
                        break;
                    case HelpItem.ZendeskMain:
                        OpenWebView(ZendeskBaseURL + prefix, left, top, right, bottom, userId, nickname, appversion, callback);
                        break;
                    case HelpItem.ZendeskPrivacyPolicy:
                        OpenWebView("http://haegin.kr/cs/v2" + prefix + "/PP.html?game=hvso&reload=" + System.DateTime.Now.Ticks, left, top, right, bottom, userId, nickname, appversion, callback);
                        break;
                    case HelpItem.ZendeskTermsOfService:
                        OpenWebView("http://haegin.kr/cs/v2" + prefix + "/ToS.html?game=hvso&reload=" + System.DateTime.Now.Ticks, left, top, right, bottom, userId, nickname, appversion, callback);
                        break;
                    case HelpItem.ZendeskAcquirePossibility:
                        OpenWebView(ZendeskBaseURL + prefix + "/articles/" + ProjectSettings.ZendeskHelpAPPageID, left, top, right, bottom, userId, nickname, appversion, callback);
                        break;
                    case HelpItem.ZendeskDirectPage:
                        OpenWebView(ZendeskBaseURL + prefix + "/articles/" + zendeskDirectPageCode, left, top, right, bottom, userId, nickname, appversion, callback);
                        break;
                    default:
                        OpenWebView(baseUrl, left, top, right, bottom, null, null, null, callback);
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
            string href = "mailto:support@homerunclash.zendesk.com?subject=" + System.Uri.EscapeUriString("[" + Application.productName + " Inquiry]") + "&body=" + System.Uri.EscapeUriString("UserID : " + userId + "\nNick Name : " + nickname + "\nApplication Version : " + appversion + "\nDevice Model : " + SystemInfo.deviceModel + "\nDevice OS Version : " + SystemInfo.operatingSystem + "\n\n--- " + TextManager.GetString(TextManager.StringTag.EmailContent1) + " ---\n");
            Application.OpenURL(href);
        }
    }
}
