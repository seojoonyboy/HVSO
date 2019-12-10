using UnityEngine;
#if !DO_NOT_USE_GPRESTO
using GPresto.Protector.Variables;
#endif

namespace Haegin
{
    public class GPrestoApi
    {
        public delegate void OnGPrestoInitialized();
        public static void Init(OnGPrestoInitialized callback)
        {
#if !DO_NOT_USE_GPRESTO && !UNITY_EDITOR
#if UNITY_IOS
            //G-Presto iOS 버전 시작
            //앱이 시작되는 지점에서 해당 함수를 호출
            if(GPresto.Protector.Common.GPiOS.GPiOS_init()) {
#if MDEBUG
                Debug.Log("G-Presto Init Failed : Quit....");
#endif
                ThreadSafeDispatcher.ApplicationQuit();
            }
#endif
#if USE_GPRESTO_CRASH_REPORT
            GPresto.Protector.Common.GPCrash crash = new GPresto.Protector.Common.GPCrash();
            crash.CrashReport();
#endif
#endif
            callback();
        }

        public static string GetSData()
        {
#if !DO_NOT_USE_GPRESTO && !UNITY_EDITOR
#if UNITY_IOS
            return GPresto.Protector.Common.GPiOS.GPiOS_COS();
#elif UNITY_ANDROID
            AndroidJavaClass cls = new AndroidJavaClass ("com.bishopsoft.Presto.SDK.Presto"); 
            string sData = cls.CallStatic<string>("getsData");
            return sData;
#endif
#endif
            return "";
        }
    }
}
