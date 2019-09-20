using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class NetworkInfo {
#if UNITY_EDITOR
    public static string GetNetworkOperatorName()
    {
        return "";
    }
#elif UNITY_IOS
    #region NativeMethods
        [DllImport("__Internal")]
        public static extern string GetNetworkOperatorName();
    #endregion
#elif UNITY_ANDROID
        public static string GetNetworkOperatorName() {
            AndroidJNI.AttachCurrentThread();
            using (var pluginClass = new AndroidJavaClass("com.haegin.haeginmodule.NetworkInfo"))
            {
                return pluginClass.CallStatic<string>("getNetworkOperatorName");
            }
        }
#endif
}
