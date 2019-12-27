using UnityEngine;

public static class CustomVibrate {
    public static void Vibrate() {
        if (isAndroid()) {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
                using (AndroidJavaObject obj = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity")) {
                    var vibrator = obj.Call<AndroidJavaObject>("getSystemService", "vibrator");
                    vibrator.Call("vibrate");
                }
            }
        }
        else
            Handheld.Vibrate();
    }


    public static void Vibrate(long milliseconds) {
        if (isAndroid()) {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
                using (AndroidJavaObject obj = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity")) {
                    var vibrator = obj.Call<AndroidJavaObject>("getSystemService", "vibrator");
                    vibrator.Call("vibrate", milliseconds);
                }
            }
        }
        else
            Handheld.Vibrate();
    }

    /// <summary>
    /// 패턴 예시 : long[] { 0, 200, 1000 } => delay : 0 , vibrate : 200 milliseconds , sleep : 1000 milliseconds
    /// </summary>
    /// <param name="pattern">진동 패턴</param>
    /// <param name="repeat"></param>
    public static void Vibrate(long[] pattern, int repeat) {
        if (isAndroid()) {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
                using (AndroidJavaObject obj = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity")) {
                    var vibrator = obj.Call<AndroidJavaObject>("getSystemService", "vibrator");
                    vibrator.Call("vibrate", pattern, repeat);
                }
            }
        }
        else
            Handheld.Vibrate();
    }

    public static bool HasVibrator() {
        return isAndroid();
    }

    public static void Cancel() {
        if (isAndroid()) {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
                using (AndroidJavaObject obj = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity")) {
                    var vibrator = obj.Call<AndroidJavaObject>("getSystemService", "vibrator");
                    vibrator.Call("Cancel");
                }
            }
        }
    }

    private static bool isAndroid() {
#if UNITY_ANDROID && !UNITY_EDITOR
        return true;
#else
        return false;
#endif
    }
}