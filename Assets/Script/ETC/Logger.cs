#define USE_LOG
using UnityEngine;

public sealed class Logger {
#if USE_LOG
    public const string ENABLE_LOGS = "ENABLE_LOG";
#endif

    [System.Diagnostics.Conditional(ENABLE_LOGS)]
    public static void Log(object message) {
        Debug.Log(message);
    }

    [System.Diagnostics.Conditional(ENABLE_LOGS)]
    public static void Log(object message, Object context) {
        Debug.Log(message, context);
    }

    [System.Diagnostics.Conditional(ENABLE_LOGS)]
    public static void LogFormat(string message, params object[] args) {
        Debug.LogFormat(message, args);
    }

    [System.Diagnostics.Conditional(ENABLE_LOGS)]
    public static void LogFormat(Object context, string message, params object[] args) {
        Debug.LogFormat(context, message, args);
    }

    [System.Diagnostics.Conditional(ENABLE_LOGS)]
    public static void LogWarning(object message) {
        Debug.LogWarning(message);
    }

    [System.Diagnostics.Conditional(ENABLE_LOGS)]
    public static void LogWarning(object message, Object context) {
        Debug.LogWarning(message, context);
    }

    [System.Diagnostics.Conditional(ENABLE_LOGS)]
    public static void LogWarningFormat(string message, params object[] args) {
        Debug.LogWarningFormat(message, args);
    }

    [System.Diagnostics.Conditional(ENABLE_LOGS)]
    public static void LogWarningFormat(Object context, string message, params object[] args) {
        Debug.LogWarningFormat(context, message, args);
    }

    [System.Diagnostics.Conditional(ENABLE_LOGS)]
    public static void LogError(object message) {
        Debug.LogError(message);
    }

    [System.Diagnostics.Conditional(ENABLE_LOGS)]
    public static void LogError(object message, Object context) {
        Debug.LogError(message, context);
    }

    [System.Diagnostics.Conditional(ENABLE_LOGS)]
    public static void LogErrorFormat(string message, params object[] args) {
        Debug.LogErrorFormat(message, args);
    }

    [System.Diagnostics.Conditional(ENABLE_LOGS)]
    public static void LogErrorFormat(Object context, string message, params object[] args) {
        Debug.LogErrorFormat(context, message, args);
    }

    [System.Diagnostics.Conditional(ENABLE_LOGS)]
    public static void LogException(System.Exception exception) {
        Debug.LogException(exception);
    }

    [System.Diagnostics.Conditional(ENABLE_LOGS)]
    public static void LogException(System.Exception exception, Object context) {
        Debug.LogException(exception, context);
    }
}