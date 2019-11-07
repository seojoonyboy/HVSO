#define SEND_ERROR_CODE
#define DONT_UNLOAD_BGWEBCLIENT
using UnityEngine;
using System.Runtime.InteropServices;
#if SEND_ERROR_CODE
using System;
#endif
using System.IO;

namespace Haegin
{
    public class BGWebClient : MonoBehaviour
    {
        public enum ResultCode { Completed, Cancelled, Error };
        public delegate void OnAsyncCompleted(string url, ResultCode code);
        public delegate void OnDownloadProgressChanged(string url, int writtenBytes, int expectedWrittenBytes);
        public delegate void OnInternetReachabilityChanged(bool reachable);
        public delegate void OnOBBInfo(int count, string[] filename, int[] filesize, string[] url);
        public event OnAsyncCompleted DownloadFileCompleted;
        public event OnDownloadProgressChanged DownloadProgressChanged;
        public event OnInternetReachabilityChanged InternaetReachabilityChanged;

        private OnOBBInfo OBBInfoCallback = null;

#if DONT_UNLOAD_BGWEBCLIENT
        public int RefCount = 0;
#endif
        public static BGWebClient GetInstance()
        {
            GameObject gameObject = GameObject.Find("BGWebClient");
            if (gameObject == null)
            {
#if MDEBUG
                Debug.Log("HAEGINHAEGIN     new BGWebClient");
#endif
                gameObject = new GameObject("BGWebClient");
#if DONT_UNLOAD_BGWEBCLIENT
                DontDestroyOnLoad(gameObject);
#endif
                gameObject.AddComponent<BGWebClient>();
            }
            return gameObject.GetComponent<BGWebClient>();
        }

#if DONT_UNLOAD_BGWEBCLIENT
        public bool CheckRef()
        {
            return (RefCount > 0);
        }
#endif

        public void ClearEvent()
        {
            DownloadFileCompleted = delegate { };
            DownloadProgressChanged = delegate { };
            InternaetReachabilityChanged = delegate { };
        }

        private void Awake()
        {
            gameObject.name = "BGWebClient";
        }

        bool InternetReachable = true;
        private void Update()
        {
            if (InternetReachable && Application.internetReachability == NetworkReachability.NotReachable)
            {
                InternetReachable = false;
                InternaetReachabilityChanged(InternetReachable);
            }
            else if (!InternetReachable && Application.internetReachability != NetworkReachability.NotReachable)
            {
                InternetReachable = true;
                InternaetReachabilityChanged(InternetReachable);
            }
        }

#if UNITY_EDITOR
        public void DownloadFileAsync(System.Uri uri, string fileName, object userToken)
        {
        }
#elif UNITY_IOS
        #region NativeMethods
        [DllImport("__Internal")]
        private static extern void StartDownload(string url, string dest);

        [DllImport("__Internal")]
        private static extern void StopDownload(string url);
        #endregion

        public string GetOBBFilePath(int versionCode = -1)
        {
            return Path.Combine(Application.streamingAssetsPath, "main.obb");
        }

        public void DownloadFileAsync(System.Uri uri, string fileName, object userToken)
        {
            StartDownload(uri.ToString(), fileName);
        }
#elif UNITY_ANDROID
        private static AndroidJavaObject _nativeBGDownload;
        static BGWebClient() {
            AndroidJNI.AttachCurrentThread();
            using (var pluginClass = new AndroidJavaClass("com.haegin.haeginmodule.download.BGDownloadPlugin"))
            {
                _nativeBGDownload = pluginClass.CallStatic<AndroidJavaObject>("instance");
            }
        }

        public void DownloadFileAsync(System.Uri uri, string fileName, object userToken) {
            _nativeBGDownload.Call<long>("downloads", uri.ToString(), fileName, "Update Data");
        }

        public string GetOBBFilePath(int versionCode = -1) {
            return _nativeBGDownload.Call<string>("getObbFilePath", versionCode);
        }

        public string GetWritableOBBDir() {
            return _nativeBGDownload.Call<string>("getWritableObbDir");
        }

        public void GetOBBDownloadInfo(string publicKey, string obburl, OnOBBInfo callback) {
            if(obburl == null)
            {            
                OBBInfoCallback = callback;
                _nativeBGDownload.Call("getOBBDownloadInfo", publicKey);
            }
            else
            {
                callback(0, null, null, null);
            }
        }

        public string GetUnityBuildId() {
            return _nativeBGDownload.Call<string>("getUnityBuildId");
        }

        public string GetUnitySplitApplicationBinarySize() {
            return _nativeBGDownload.Call<string>("getUnitySplitApplicationBinarySize");
        }
#endif

#if SEND_ERROR_CODE
        DateTime lastSendTime = DateTime.MinValue;
#endif
        public void NativeAsyncCompleted(string param)
        {
            char[] delemiterChars = { ',' };
            string[] parameters = param.Split(delemiterChars);
            int nCode = System.Int32.Parse(parameters[1]);
            switch (nCode)
            {
                case 0:
                    DownloadFileCompleted(parameters[0], ResultCode.Completed);
                    break;
                case 1:
                    DownloadFileCompleted(parameters[0], ResultCode.Cancelled);
                    break;
                case 2:
                    DownloadFileCompleted(parameters[0], ResultCode.Error);
#if SEND_ERROR_CODE
                    try {
#if MDEBUG
                        Debug.Log("BGDownload Error : " + parameters[2]);
#endif
                        if (parameters[2].StartsWith("CopyFailed")) parameters[2] = "CopyFailed";
                        if((DateTime.UtcNow - lastSendTime).TotalHours >= 1) {
                            lastSendTime = DateTime.UtcNow;
                            WebClient.GetInstance().RequestKeyCount("BGD_E_" + parameters[2]);
#if MDEBUG
                            Debug.Log("BGDownload Send : BGD_E_" + parameters[2]);
#endif
                        }
                    } catch { }            
#endif
                    break;
            }
            InternetReachable = true;
        }

        public void NativeDownloadProgressChanged(string param)
        {
            char[] delemiterChars = { ',' };
            string[] parameters = param.Split(delemiterChars);
            DownloadProgressChanged(parameters[0], System.Int32.Parse(parameters[1]), System.Int32.Parse(parameters[2]));
            InternetReachable = true;
        }

        public void OBBInfo(string param)
        {
#if MDEBUG
            Debug.Log("OBBInfo " + param);
#endif
            char[] delemiterChars = { ',' };
            string[] parameters = param.Split(delemiterChars);

            foreach(string s in parameters)
            {
                Debug.Log("[" + s + "]");
            }


            int count = System.Int32.Parse(parameters[0]);

            if(count > 0)
            {
                string[] filenames = new string[count];
                string[] urls = new string[count];
                int[] filesizes = new int[count];
                for(int i = 0; i < count; i++)
                {
                    filenames[i] = parameters[i * 3 + 1];
                    filesizes[i] = System.Int32.Parse(parameters[i * 3 + 2]);
                    urls[i] = parameters[i * 3 + 3];
                }
                OBBInfoCallback(count, filenames, filesizes, urls);
            }
            else
            {
                OBBInfoCallback(0, null, null, null);
            }
        }
    }
}
