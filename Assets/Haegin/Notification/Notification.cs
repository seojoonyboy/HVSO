#define USE_FCM

using System;
using System.IO;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_IOS
using SA.iOS.UIKit;
using SA.iOS.UserNotifications;
#endif

#if USE_FCM && UNITY_ANDROID
using System.Threading.Tasks;
#endif

namespace Haegin
{
    public class Notification
    {
        public delegate void PermissionResult(bool result);
        public delegate void ScheduleResult(bool result, string id);

#if USE_FCM && UNITY_ANDROID
        static Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;
#endif

#if UNITY_ANDROID
    private static string fullClassName = "com.haegin.haeginmodule.notification.UnityNotificationManager";
    private static string actionClassName = "com.haegin.haeginmodule.notification.NotificationAction";

#if UNITY_5_6_OR_NEWER
        private static string bundleIdentifier { get { return Application.identifier; } }
#else
    private static string bundleIdentifier { get { return Application.bundleIdentifier; } }
#endif
        public static int SendNotification(TimeSpan delay, string title, string message, int badge, Color32 bgColor, bool sound = true, bool vibrate = true, bool lights = true, string bigIcon = "", String soundName = null, string channel = "default", params Action[] actions)
        {
            int id = new System.Random().Next();
            return SendNotification(id, (long)delay.TotalSeconds * 1000, title, message, badge, bgColor, sound, vibrate, lights, bigIcon, soundName, channel, actions);
        }

        public static int SendNotification(int id, TimeSpan delay, string title, string message, int badge, Color32 bgColor, bool sound = true, bool vibrate = true, bool lights = true, string bigIcon = "", String soundName = null, string channel = "default", params Action[] actions)
        {
            return SendNotification(id, (long)delay.TotalSeconds * 1000, title, message, badge, bgColor, sound, vibrate, lights, bigIcon, soundName, channel, actions);
        }

        public static int SendNotification(int id, long delayMs, string title, string message, int badge, Color32 bgColor, bool sound = true, bool vibrate = true, bool lights = true, string bigIcon = "", String soundName = null, string channel = "default", params Action[] actions)
        {
            AndroidJavaClass pluginClass = new AndroidJavaClass(fullClassName);
            if (pluginClass != null)
            {
                pluginClass.CallStatic("SetNotification", id, delayMs, title, message, message, badge,
                sound ? 1 : 0, soundName, vibrate ? 1 : 0, lights ? 1 : 0, bigIcon, "notify_icon_small",
                ToInt(bgColor), bundleIdentifier, channel, PopulateActions(actions));
            }
            return id;
        }

        public static int SendRepeatingNotification(TimeSpan delay, TimeSpan timeout, string title, string message, int badge, Color32 bgColor, bool sound = true, bool vibrate = true, bool lights = true, string bigIcon = "", String soundName = null, string channel = "default", params Action[] actions)
        {
            int id = new System.Random().Next();
            return SendRepeatingNotification(id, (long)delay.TotalSeconds * 1000, (int)timeout.TotalSeconds, title, message, badge, bgColor, sound, vibrate, lights, bigIcon, soundName, channel, actions);
        }

        public static int SendRepeatingNotification(int id, TimeSpan delay, TimeSpan timeout, string title, string message, int badge, Color32 bgColor, bool sound = true, bool vibrate = true, bool lights = true, string bigIcon = "", String soundName = null, string channel = "default", params Action[] actions)
        {
            return SendRepeatingNotification(id, (long)delay.TotalSeconds * 1000, (int)timeout.TotalSeconds, title, message, badge, bgColor, sound, vibrate, lights, bigIcon, soundName, channel, actions);
        }

        public static int SendRepeatingNotification(int id, long delayMs, long timeoutMs, string title, string message, int badge, Color32 bgColor, bool sound = true, bool vibrate = true, bool lights = true, string bigIcon = "", String soundName = null, string channel = "default", params Action[] actions)
        {
            AndroidJavaClass pluginClass = new AndroidJavaClass(fullClassName);
            if (pluginClass != null)
            {
                pluginClass.CallStatic("SetRepeatingNotification", id, delayMs, title, message, message, badge, timeoutMs,
                    sound ? 1 : 0, soundName, vibrate ? 1 : 0, lights ? 1 : 0, bigIcon, "notify_icon_small",
                    ToInt(bgColor), bundleIdentifier, channel, PopulateActions(actions));
            }
            return id;
        }

        public static void CancelNotification(int id)
        {
            AndroidJavaClass pluginClass = new AndroidJavaClass(fullClassName);
            if (pluginClass != null)
            {
                pluginClass.CallStatic("CancelPendingNotification", id);
            }
        }

        public static void ClearNotifications()
        {
            AndroidJavaClass pluginClass = new AndroidJavaClass(fullClassName);
            if (pluginClass != null)
            {
                pluginClass.CallStatic("ClearShowingNotifications");
            }
        }

        /// This allows you to create a custom channel for different kinds of notifications.
        /// Channels are required on Android Oreo and above. If you don't call this method, a channel will be created for you with the configuration you give to SendNotification.
        public static void CreateChannel(string identifier, string name, string description, Color32 lightColor, bool enableLights = true, string soundName = null, Importance importance = Importance.Default, bool vibrate = true, long[] vibrationPattern = null)
        {
            AndroidJavaClass pluginClass = new AndroidJavaClass(fullClassName);
            if (pluginClass != null)
            {
                pluginClass.CallStatic("CreateChannel", identifier, name, description, (int)importance, soundName, enableLights ? 1 : 0, ToInt(lightColor), vibrate ? 1 : 0, vibrationPattern, bundleIdentifier, null);
            }
        }

        public enum Importance
        {
            /// Default notification importance: shows everywhere, makes noise, but does not visually intrude.
            Default = 3,

            /// Higher notification importance: shows everywhere, makes noise and peeks. May use full screen intents.
            High = 4,

            /// Low notification importance: shows everywhere, but is not intrusive.
            Low = 2,

            /// Unused.
            Max = 5,

            /// Min notification importance: only shows in the shade, below the fold. This should not be used with Service.startForeground since a foreground service is supposed to be something the user cares about so it does not make semantic sense to mark its notification as minimum importance. If you do this as of Android version O, the system will show a higher-priority notification about your app running in the background.
            Min = 1,

            /// A notification with no importance: does not show in the shade.
            None = 0
        }

        public class Action
        {
            public Action(string identifier, string title, MonoBehaviour handler)
            {
                this.Identifier = identifier;
                this.Title = title;
                if (handler != null)
                {
                    this.GameObject = handler.gameObject.name;
                    this.HandlerMethod = "OnAction";
                }
            }

            public string Identifier;
            public string Title;
            public string Icon;
            public bool Foreground = true;
            public string GameObject;
            public string HandlerMethod;
        }

        private static int ToInt(Color32 color)
        {
            return color.r * 65536 + color.g * 256 + color.b;
        }

        private static AndroidJavaObject PopulateActions(Action[] actions)
        {
            AndroidJavaObject actionList = null;
            if (actions.Length > 0)
            {
                actionList = new AndroidJavaObject("java.util.ArrayList");
                for (int i = 0; i < actions.Length; i++)
                {
                    var action = actions[i];
                    using (AndroidJavaObject notificationObject = new AndroidJavaObject(actionClassName))
                    {
                        notificationObject.Call("setIdentifier", action.Identifier);
                        notificationObject.Call("setTitle", action.Title);
                        notificationObject.Call("setIcon", action.Icon);
                        notificationObject.Call("setForeground", action.Foreground);
                        notificationObject.Call("setGameObject", action.GameObject);
                        notificationObject.Call("setHandlerMethod", action.HandlerMethod);
                        actionList.Call<bool>("add", notificationObject);
                    }
                }
            }
            return actionList;
        }
#endif

        public static void RegisterPushNotificationDevice()
        {
#if UNITY_ANDROID
#if USE_FCM
            ThreadSafeDispatcher.Instance.Invoke(() =>
            {
                Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
                {
                    dependencyStatus = task.Result;
                    if (dependencyStatus == Firebase.DependencyStatus.Available)
                    {
                        Firebase.Messaging.FirebaseMessaging.TokenReceived += (object sender, Firebase.Messaging.TokenReceivedEventArgs token) =>
                        {
#if USE_APPSFLYER
                            AppsFlyer.updateServerUninstallToken (token.Token);
#endif
                            WebClient.GetInstance().RequestFcmRegister(token.Token);
                        };
                        Firebase.Messaging.FirebaseMessaging.MessageReceived += (object sender, Firebase.Messaging.MessageReceivedEventArgs e) => 
                        {
#if MDEBUG
                            Debug.Log("FCM Received [" + e.Message.From + "]");
                            foreach(var k in e.Message.Data.Keys) {
                                Debug.Log("RawData [" + k + "]");
                            }
                            if(e.Message.Notification != null) {
                                Debug.Log("Badge [" + e.Message.Notification.Badge + "]");
                                Debug.Log("Body [" + e.Message.Notification.Body + "]");
                            }
                            else {
                                Debug.Log("Badge [null]");
                            }
#endif
                        };
                    }
                    else
                    {
#if MDEBUG
                        Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
#endif
                    }
                });
            });
#else
            GoogleCloudMessageService.ActionCMDRegistrationResult += (GP_GCM_RegistrationResult obj) =>
            {
                if (obj.IsSucceeded)
                {
                    WebClient.GetInstance().RequestFcmRegister(GoogleCloudMessageService.Instance.registrationId);
                }
                else
                {
#if MDEBUG
                    Debug.Log("Failed to GCM Register");
#endif
                }
            };
            GoogleCloudMessageService.Instance.RgisterDevice();
#endif
#elif UNITY_IOS
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    // Set a flag here indiciating that Firebase is ready to use by your
                    // application.
#if MDEBUG
                    UnityEngine.Debug.LogError("Firebase init..... ok");
#endif
                }
                else
                {
#if MDEBUG
                    UnityEngine.Debug.LogError(System.String.Format("Could not resolve all Firebase dependencies: {0}", dependencyStatus));
#endif
                    // Firebase Unity SDK is not safe to use here.
                }
            });

            ISN_UIApplication.RegisterForRemoteNotifications();
            ISN_UIApplication.ApplicationDelegate.DidRegisterForRemoteNotifications.AddListener((result) => {
                if (result.IsSucceeded)
                {
#if USE_APPSFLYER
                    AppsFlyer.registerUninstall (result.DeviceToken);
#endif
                    WebClient webClient = WebClient.GetInstance();
                    webClient.RequestApnsRegister(result.DeviceTokenUTF8);
                }
                else
                {
#if MDEBUG
                    Debug.Log("Failed to APNS Register");
#endif
                }
            });
#endif
        }

        public static void SetAppIconBadgeCount(int count)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaClass pluginClass = new AndroidJavaClass(fullClassName);
            if (pluginClass != null)
            {
                pluginClass.CallStatic("SetIconBadgeCount", count);
            }
#elif UNITY_IOS && !UNITY_EDITOR
            ISN_UIApplication.ApplicationBagesNumber = count;
#endif
        }

        public static void RequestPermissions(PermissionResult callback)
        {
#if UNITY_ANDROID
            callback(true);
#elif UNITY_IOS && !UNITY_EDITOR
            int options = ISN_UNAuthorizationOptions.Alert | ISN_UNAuthorizationOptions.Sound | ISN_UNAuthorizationOptions.Badge;
            ISN_UNUserNotificationCenter.RequestAuthorization(options, (result) => {
                callback(result.IsSucceeded);
            });
#endif
        }

        public static void ScheduleUserNotification(string id, string title, string body, int badge, int seconds, Color32 color, ScheduleResult callback)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                int intId = int.Parse(id);
                SendNotification(intId, seconds * 1000, title, body, badge, color);
                callback(true, id);
            }
            catch
            {
                callback(false, id);
            }
#elif UNITY_IOS && !UNITY_EDITOR
            var content = new ISN_UNNotificationContent();
            content.Title = title;
            content.Body = body;
            content.Badge = badge;
            var trigger = new ISN_UNTimeIntervalNotificationTrigger(seconds, false);
            var request = new ISN_UNNotificationRequest(id, content, trigger);
            ISN_UNUserNotificationCenter.AddNotificationRequest(request, (result) => {
#if MDEBUG
                Debug.Log("AddNotificationRequest: " + result.IsSucceeded);
#endif
                callback(result.IsSucceeded, id);
            });
#endif
        }

        public static void CancelUserNotification(string id)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            CancelNotification(int.Parse(id));
#elif UNITY_IOS
            string[] reqs = new string[1];
            reqs[0] = id;
            ISN_UNUserNotificationCenter.RemovePendingNotificationRequests(reqs);
#endif
        }
    }
}
