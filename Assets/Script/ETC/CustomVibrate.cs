using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CustomVibrate {
#if UNITY_ANDROID && !UNITY_EDITOR
    public static AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    public static AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    public static AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
#else
    public static AndroidJavaClass unityPlayer;
    public static AndroidJavaObject currentActivity;
    public static AndroidJavaObject vibrator;
#endif

    public static void Vibrate() {
        if (!isVibrateOn()) return;

        if (isAndroid())
            vibrator.Call("vibrate");
        else
            Handheld.Vibrate();
    }


    public static void Vibrate(long milliseconds) {
        if (!isVibrateOn()) return;

        Logger.Log("Vibrate " + milliseconds);

        if (isAndroid())
            vibrator.Call("vibrate", milliseconds);
        else
            Handheld.Vibrate();
    }

    /// <summary>
    /// 패턴 예시 : long[] { 0, 200, 1000 } => delay : 0 , vibrate : 200 milliseconds , sleep : 1000 milliseconds
    /// </summary>
    /// <param name="pattern">진동 패턴</param>
    /// <param name="repeat"></param>
    public static void Vibrate(long[] pattern, int repeat) {
        if (!isVibrateOn()) return;

        if (isAndroid())
            vibrator.Call("vibrate", pattern, repeat);
        else
            Handheld.Vibrate();
    }

    public static bool HasVibrator() {
        return isAndroid();
    }

    public static void Cancel() {
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

    private static bool isVibrateOn() {
        string isOn = PlayerPrefs.GetString("Vibrate");
        return isOn == "On";
    }
}