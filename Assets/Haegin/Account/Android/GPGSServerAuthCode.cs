using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPGSServerAuthCode : MonoBehaviour {
    public delegate void OnServerAuthCode(string authCode);
    private OnServerAuthCode onServerAuthCode = null;
    public static GPGSServerAuthCode GetInstance()
    {
        GameObject gameObject = GameObject.Find("HaeginGPGSServerAuthCode");
        if (gameObject == null)
        {
            gameObject = new GameObject("HaeginGPGSServerAuthCode");
            gameObject.AddComponent<GPGSServerAuthCode>();
        }
        return gameObject.GetComponent<GPGSServerAuthCode>();
    }

    private void Awake()
    {
        gameObject.name = "HaeginGPGSServerAuthCode";
    }

    public void GetServerAuthCode(string clientId, OnServerAuthCode callback)
    {
#if UNITY_ANDROID
        onServerAuthCode = callback;
        AndroidJNI.AttachCurrentThread();
        using (var pluginClass = new AndroidJavaClass("com.haegin.haeginmodule.googleplay.GooglePlayServicesManager"))
        {
            pluginClass.CallStatic("GetServerAuthCode", clientId);
        }
#else
        callback(null);
#endif
    }

    public void NativeOnServerAuthCode(string authCode)
    {
        if (onServerAuthCode != null)
            onServerAuthCode(authCode);
    }
}
