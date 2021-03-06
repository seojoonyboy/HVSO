using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

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

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport ( "__Internal" )]
    private static extern bool _HasVibrator ();

    [DllImport ( "__Internal" )]
    private static extern void _Vibrate ();

    [DllImport ( "__Internal" )]
    private static extern void _VibratePop ();

    [DllImport ( "__Internal" )]
    private static extern void _VibratePeek ();

    [DllImport ( "__Internal" )]
    private static extern void _VibrateNope ();

    ///<summary>
    ///Only on iOS
    ///</summary>
    public static void VibratePop ()
    {
        if (!isVibrateOn()) return;
        _VibratePop ();
    }

    ///<summary>
    ///Only on iOS
    ///</summary>
    public static void VibratePeek ()
    {
        if (!isVibrateOn()) return;
        _VibratePeek ();
    }

    ///<summary>
    ///Only on iOS
    ///</summary>
    public static void VibrateNope ()
    {
        if (!isVibrateOn()) return;
        _VibrateNope ();
    }
#endif

    private static bool isVibrateOn() {
        string isOn = PlayerPrefs.GetString("Vibrate");
        return isOn == "On";
    }
}