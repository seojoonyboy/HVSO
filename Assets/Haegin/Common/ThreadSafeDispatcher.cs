using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
    }
}
