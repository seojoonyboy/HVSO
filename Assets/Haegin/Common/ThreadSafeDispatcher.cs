using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;

namespace Haegin
{
    public class ThreadSafeDispatcher : MonoBehaviour
    {
        public delegate void OnSystemBackKey();

        private static ThreadSafeDispatcher instance = null;
        private List<Action> pending = new List<Action>();

        private Stack<OnSystemBackKey> systemBackKeyListeners = new Stack<OnSystemBackKey>();

        public static ThreadSafeDispatcher Instance
        {
            get
            {
                return instance;
            }
        }

        public void Invoke(Action fn)
        {
            lock (pending)
            {
                pending.Add(fn);
            }
        }

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        public static void Initialize()
        {
            if (instance == null)
            {
                GameObject gameObject = new GameObject("ThreadSafeDispatcher");
                gameObject.AddComponent<ThreadSafeDispatcher>();
            }
        }

        void Update()
        {
            lock (pending)
            {
                foreach (var action in pending)
                {
                    action();
                }
                pending.Clear();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                lock (systemBackKeyListeners)
                {
                    if (systemBackKeyListeners.Count > 0)
                    {
                        OnSystemBackKey top = systemBackKeyListeners.Peek();
                        if (top != null)
                        {
                            top();
                        }
                    }
                }
            }
        }

        public void PushSystemBackKeyListener(OnSystemBackKey listener)
        {
            lock (systemBackKeyListeners)
            {
#if MDEBUG
                Debug.Log("Push " + listener.ToString());
#endif
                systemBackKeyListeners.Push(listener);
            }
        }

        public void PopSystemBackKeyListener()
        {
            lock (systemBackKeyListeners)
            {
#if MDEBUG
                Debug.Log("Pop " + systemBackKeyListeners.Peek().ToString());
#endif
                systemBackKeyListeners.Pop();
            }
        }


        public static void ApplicationQuit()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaClass JavaSystemClass = new AndroidJavaClass("java.lang.System");
            JavaSystemClass.CallStatic("exit", 0);
#else
            Application.Quit();
#endif
        }


        public static void CheckHaeginCertificate()
        {
#if !UNITY_EDITOR
            instance.StartCoroutine(instance.CheckHaeginCertificateSub());
#endif
        }

        IEnumerator CheckHaeginCertificateSub()
        {
#if MDEBUG
            Debug.Log("-CheckCertificate--------------------------------------begin");
#endif
            UnityWebRequest req = UnityWebRequest.Get("https://office.haegin.kr:50443");
            yield return req.SendWebRequest();
            if (req != null)
            {
#if MDEBUG
                Debug.Log("-CheckCertificate--    isNetworkError [" + req.isNetworkError + "]");
                Debug.Log("-CheckCertificate--    isDone [" + req.isDone + "]");
                Debug.Log("-CheckCertificate--    isHttpError [" + req.isHttpError + "]");
                Debug.Log("-CheckCertificate--    error [" + req.error + "]");
                Debug.Log("-CheckCertificate--    responseCode [" + req.responseCode + "]");
#endif
                if(req.isNetworkError)
                {
#if UNITY_ANDROID
                    var message = new SA.Android.App.AN_AlertDialog(SA.Android.App.AN_DialogTheme.Material);
                    message.Title = "인증 실패";
                    message.Message = "해긴 인증에 실패했습니다. 게임을 종료합니다.";
                    message.SetPositiveButton("확인", () => {
                        ApplicationQuit();
                    });
                    message.Show();
#elif UNITY_IOS
                    SA.iOS.UIKit.ISN_UIAlertController alert = new SA.iOS.UIKit.ISN_UIAlertController("인증 실패", "해긴 인증에 실패했습니다. 게임을 종료합니다.", SA.iOS.UIKit.ISN_UIAlertControllerStyle.Alert);
                    SA.iOS.UIKit.ISN_UIAlertAction defaultAction = new SA.iOS.UIKit.ISN_UIAlertAction("확인", SA.iOS.UIKit.ISN_UIAlertActionStyle.Default, () => {
                        ApplicationQuit();
                    });
                    alert.AddAction(defaultAction);
                    alert.Present();
#endif
                }
                else
                {
#if UNITY_ANDROID
                    var message = new SA.Android.App.AN_AlertDialog(SA.Android.App.AN_DialogTheme.Material);
                    message.Title = "인증 성공";
                    message.Message = "해긴 인증에 성공했습니다. 릴리즈용 버전에서는 이 창이 뜨면 안됩니다.";
                    message.SetPositiveButton("확인", () => {
                    });
                    message.Show();
#elif UNITY_IOS
                    SA.iOS.UIKit.ISN_UIAlertController alert = new SA.iOS.UIKit.ISN_UIAlertController("인증 성공", "해긴 인증에 성공했습니다. 릴리즈용 버전에서는 이 창이 뜨면 안됩니다.", SA.iOS.UIKit.ISN_UIAlertControllerStyle.Alert);
                    SA.iOS.UIKit.ISN_UIAlertAction defaultAction = new SA.iOS.UIKit.ISN_UIAlertAction("확인", SA.iOS.UIKit.ISN_UIAlertActionStyle.Default, () => {
                    });
                    alert.AddAction(defaultAction);
                    alert.Present();
#endif
                }
            }
#if MDEBUG
            Debug.Log("-CheckCertificate--------------------------------------end");
#endif
        }
    }
}
