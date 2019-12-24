using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomVibrate : MonoBehaviour {
#if UNITY_ANDROID && !UNITY_EDITOR
    public static AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    public static AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    public static AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
#else
    public static AndroidJavaClass unityPlayer;
    public static AndroidJavaObject currentActivity;
    public static AndroidJavaObject vibrator;
#endif

    public void Vibrate() {
        if (!isVibrateOn()) return;

        if (isAndroid())
            vibrator.Call("vibrate");
        else
            Handheld.Vibrate();
    }


    public void Vibrate(long milliseconds) {
        if (!isVibrateOn()) return;

        if (isAndroid())
            vibrator.Call("vibrate", milliseconds);
        else
            Handheld.Vibrate();
    }

    public void Vibrate(long[] pattern, int repeat) {
        if (!isVibrateOn()) return;

        if (isAndroid())
            vibrator.Call("vibrate", pattern, repeat);
        else
            Handheld.Vibrate();
    }

    public bool HasVibrator() {
        return isAndroid();
    }

    public void Cancel() {
        if (isAndroid())
            vibrator.Call("cancel");
    }

    private static bool isAndroid() {
#if UNITY_ANDROID && !UNITY_EDITOR
return true;
#else
        return false;
#endif
    }

    private bool isVibrateOn() {
        string isOn = PlayerPrefs.GetString("Vibrate");
        return isOn == "true";
    }
}