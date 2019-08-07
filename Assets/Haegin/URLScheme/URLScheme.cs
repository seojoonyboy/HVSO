using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using Firebase.DynamicLinks;

namespace Haegin
{
    public class URLScheme : MonoBehaviour
    {
        public delegate void OpenWithURLScheme(string url, Dictionary<string, string> parameters);
        public event OpenWithURLScheme OnOpenWithURLScheme;

        public static URLScheme Instance()
        {
            GameObject gameObject = GameObject.Find("HaeginURLSchemeListener");
            if (gameObject == null)
            {
                gameObject = new GameObject("HaeginURLSchemeListener");
                DontDestroyOnLoad(gameObject);
                gameObject.AddComponent<URLScheme>();
            }
            return gameObject.GetComponent<URLScheme>();
        }

        public void NativeOpenURL(string url)
        {
#if MDEBUG
            Debug.Log("NativeOpenURL " + url);
#endif
            if(!string.IsNullOrEmpty(url) && url.StartsWith(ProjectSettings.urlScheme)) {
                Dictionary<string, string> dic = ParseQueryString(url);
                OnOpenWithURLScheme(url, dic);
            }
        }

        void Start()
        {
            DynamicLinks.DynamicLinkReceived += OnDynamicLink;
        }

		private string dynamicLink = null;

        // Display the dynamic link received by the application.
        void OnDynamicLink(object sender, EventArgs args)
        {
            var dynamicLinkEventArgs = args as ReceivedDynamicLinkEventArgs;
			dynamicLink = dynamicLinkEventArgs.ReceivedDynamicLink.Url.OriginalString;

#if MDEBUG
			Debug.Log("NativeOpenURL(dynamic links)" + dynamicLink + "  " + dynamicLinkEventArgs.ReceivedDynamicLink.MatchStrength);
#endif
			if (!string.IsNullOrEmpty(dynamicLink) && dynamicLink.StartsWith("https://dlink.haegin.kr") && dynamicLinkEventArgs.ReceivedDynamicLink.MatchStrength == LinkMatchStrength.PerfectMatch)
			{
				Dictionary<string, string> dic = ParseQueryString(dynamicLink);
				OnOpenWithURLScheme(dynamicLink, dic);
			}
			else
			{
				dynamicLink = null;
#if MDEBUG
				Debug.Log("       Ignored");
#endif
			}
		}

#if !UNITY_EDITOR && UNITY_IOS
        [DllImport("__Internal")]
        private static extern bool NativeCheckURLScheme();

        [DllImport("__Internal")]
        private static extern void NativeClearURLScheme();
#elif !UNITY_EDITOR && UNITY_ANDROID
        private static AndroidJavaObject _nativeURLScheme;
        static URLScheme() {
            AndroidJNI.AttachCurrentThread();
            using (var pluginClass = new AndroidJavaClass("com.haegin.haeginmodule.urlscheme.URLSchemePlugin"))
            {
                _nativeURLScheme = pluginClass.CallStatic<AndroidJavaObject>("instance");
            }
        }

        private bool NativeCheckURLScheme() {
            return _nativeURLScheme.Call<bool>("nativeCheckURLScheme");
        }
        private void NativeClearURLScheme() {
            _nativeURLScheme.Call("nativeClearURLScheme");
        }
#else
		private static bool NativeCheckURLScheme() { return false; }
        private static void NativeClearURLScheme() {}
#endif

        void OnApplicationPause(bool pause)
        {
            if(!pause) {
                CheckUrlScheme();
            }
        }

        public bool CheckUrlScheme()
        {
            if(!string.IsNullOrEmpty(dynamicLink))
			{
				ThreadSafeDispatcher.Instance.Invoke(() => {
					NativeOpenURL(dynamicLink);
				});
				return true;
			}
            else
			{
				return NativeCheckURLScheme();
			}
		}

        public void ClearUrlScheme()
        {
			dynamicLink = null;
            NativeClearURLScheme();
        }

        Dictionary<string, string> ParseQueryString(String query)
        {
            Dictionary<String, String> queryDict = new Dictionary<string, string>();

#if MDEBUG
            Debug.Log("Query [" + query + "]");
#endif
            query = query.Substring(query.IndexOf('?') + 1);
#if MDEBUG
            Debug.Log("Trim? [" + query + "]");
#endif
            foreach (String token in query.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
            {
#if MDEBUG
                Debug.Log("Token = " + token);
#endif
                string[] parts = token.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                    queryDict[parts[0].Trim()] = parts[1].Trim();
                else
                    queryDict[parts[0].Trim()] = "";
            }
            return queryDict;
        }
    }
}
