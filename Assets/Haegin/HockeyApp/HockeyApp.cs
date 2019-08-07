using UnityEngine;
using System.Collections.Generic;

namespace Haegin
{
    public class HockeyApp
    {
        public static void Initialize(string appId, string secret, bool auth)
        {
            GameObject obj = new GameObject("HockeyApp");
            obj.SetActive(false);
#if (UNITY_ANDROID && !UNITY_EDITOR)
#if MDEBUG
            if(appId.Equals("36b15dd0c5a240d4a98b086133f2ecf3")) {
                auth = false;
            }
#endif
        HockeyAppAndroid hockeyApp = obj.AddComponent<HockeyAppAndroid>();
        hockeyApp.packageID = Application.identifier;
        hockeyApp.appID = appId;
        hockeyApp.secret = secret;
        if(auth)
        {
            hockeyApp.authenticatorType = HockeyAppAndroid.AuthenticatorType.HockeyAppUser;
            hockeyApp.updateAlert = true;
        }
        else 
        {
            hockeyApp.authenticatorType = HockeyAppAndroid.AuthenticatorType.Anonymous;
            hockeyApp.updateAlert = false;
        }
#elif (UNITY_IOS && !UNITY_EDITOR)
#if MDEBUG
            if(appId.Equals("a340f4f488254145b766caec4bfdd037")) {
                auth = false;
            }
#endif
        HockeyAppIOS hockeyApp = obj.AddComponent<HockeyAppIOS>();
        hockeyApp.appID = appId;
        hockeyApp.secret = secret;
        if(auth)
        {
            hockeyApp.authenticatorType = HockeyAppIOS.AuthenticatorType.HockeyAppUser;
            hockeyApp.updateAlert = true;
        }
        else 
        {
            hockeyApp.authenticatorType = HockeyAppIOS.AuthenticatorType.Anonymous;
            hockeyApp.updateAlert = false;
        }
#endif
            obj.SetActive(true);
        }

        public static void TrackEvent(string eventName)
        {
#if (UNITY_ANDROID && !UNITY_EDITOR)
        HockeyAppAndroid.TrackEvent(eventName);
#elif (UNITY_IOS && !UNITY_EDITOR)
        HockeyAppIOS.TrackEvent(eventName);
#endif
        }

        public static void TrackEvent(string eventName, IDictionary<string, string> properties)
        {
#if (UNITY_ANDROID && !UNITY_EDITOR)
        HockeyAppAndroid.TrackEvent(eventName, properties);
#elif (UNITY_IOS && !UNITY_EDITOR)
        HockeyAppIOS.TrackEvent(eventName, properties, null);
#endif
        }

        public static void TrackEvent(string eventName, IDictionary<string, string> properties, IDictionary<string, double> measurements)
        {
#if (UNITY_ANDROID && !UNITY_EDITOR)
        HockeyAppAndroid.TrackEvent(eventName, properties, measurements);
#elif (UNITY_IOS && !UNITY_EDITOR)
        HockeyAppIOS.TrackEvent(eventName, properties, measurements);
#endif
        }

        public static void ShowFeedbackForm()
        {
#if (UNITY_ANDROID && !UNITY_EDITOR)
        HockeyAppAndroid.ShowFeedbackForm();
#elif (UNITY_IOS && !UNITY_EDITOR)
        HockeyAppIOS.ShowFeedbackForm();
#endif
        }
    }
}
