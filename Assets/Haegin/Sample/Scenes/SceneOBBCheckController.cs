//#define QA
#define DONT_UNLOAD_BGWEBCLIENT
using UnityEngine;
using Haegin;
using System;
using G.Util;
using G.Network;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneOBBCheckController : MonoBehaviour {
    public Canvas canvas;
    public GameObject confirmDialog;
    public GameObject systemDialog;
    public GameObject eulaText;
    public GameObject requestPermissionDialog;

    public TextAsset[] preloadTextAssets;

    private void Awake()
    {
#if MDEBUG
#if DONT_UNLOAD_BGWEBCLIENT
        Debug.Log("HAEGINHAEGIN -------------------------------- Started ----  DONT_UNLOAD_BGWEBCLIENT  -----");
#else
        Debug.Log("HAEGINHAEGIN -------------------------------- Started ------------------------------------");
#endif
#endif
        // WebView를 올바른 크기와 위치에 놓을 수 있도록, 초기 화면 사이즈를 저장해 둡니다. 
        UGUICommon.SaveScreenSize();

        // Main Thread Dispatcher 초기화 : 최초 Scene에서 Awake 에서 반드시 콜
        ThreadSafeDispatcher.Initialize();

        // 어떤 언어를 사용할 것인지 초기화 
        TextManager.Initialize(Application.systemLanguage.ToString());

        ThreadSafeDispatcher.Instance.PushSystemBackKeyListener(OnSystemBackKey);
    }

    void OnSystemBackKey()
    {
        UGUICommon.ShowYesNoDialog(systemDialog, eulaText, canvas, TextManager.GetString(TextManager.StringTag.Quit), TextManager.GetString(TextManager.StringTag.QuitConfirm), (UGUICommon.ButtonType buttonType) =>
        {
            if (buttonType == UGUICommon.ButtonType.Yes)
            {
                ThreadSafeDispatcher.ApplicationQuit();
            }
        });
    }

    private void OnDestroy()
    {
        ThreadSafeDispatcher.Instance.PopSystemBackKeyListener();
    }

    private PatcherAsyncBG patcher;
    private string dataPath;
    private Progress progressbar;
    private Text progresstext;
    private uint[] key = ConvertEx.ToKey("5swdcsrBHoBYjDWi5VXN72");
    private int versionCode = -1;   // 현재 버전인 경우 -1로 세팅하면 됩니다.

#if UNITY_ANDROID
    // 비필수 부터 나열하, 필수를 나중에 하자.

    private string[] permissions = { 
        "android.permission.VIBRATE", 
        "android.permission.GET_TASKS", 
        "android.permission.RECEIVE_BOOT_COMPLETED", 
        "android.permission.ACCESS_WIFI_STATE",
        "com.android.vending.BILLING",
        "com.android.vending.CHECK_LICENSE",
        "android.permission.WAKE_LOCK",
        "android.permission.ACCESS_NETWORK_STATE",
        "android.permission.ACCESS_DOWNLOAD_MANAGER",
        "android.permission.READ_EXTERNAL_STORAGE",
        "android.permission.WRITE_EXTERNAL_STORAGE" 
    };
    private bool[] mustHavePermissions = { false, false, false, false, false, false, false, false, false, true, false };
    private bool toBeRestart = false;
#endif

    void Start()
    {
#if MDEBUG
        Debug.Log("---------------------------------------------------\nNetwork Operator Name : " + NetworkInfo.GetNetworkOperatorName() + "\n---------------------------------------------------");
#endif

#if UNITY_ANDROID
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
#if MDEBUG
        Debug.Log("Checking GoogleApiAvailability");
#endif
        int res = SA.Android.GMS.Common.AN_GoogleApiAvailability.IsGooglePlayServicesAvailable();
        if (res == SA.Android.GMS.Common.AN_ConnectionResult.SUCCESS)
        {
            bool permissionRequestRequired = false;
            for (int i = 0; i < permissions.Length; i++)
            {
                if (!AndroidPermissionsManager.IsPermissionGranted(permissions[i]) && mustHavePermissions[i])
                {   
                    permissionRequestRequired = true;
                    break;
                }
            }
            if (permissionRequestRequired)
            {
                StartCoroutine(ShowRequestPermission());
            }
            else
            {
                StartOBBDownload();
            }
        }
        else
        {
#if MDEBUG
            Debug.Log("Google Api not avaliable on current device, trying to resolve");
#endif
            SA.Android.GMS.Common.AN_GoogleApiAvailability.MakeGooglePlayServicesAvailable((result) => {
                RestartApp();
            });
        }
#else
        StartOBBDownload();
#endif
    }

#if UNITY_ANDROID
    IEnumerator ShowRequestPermission()
    {
        yield return null;

        UGUICommon.ShowRequestPermission(requestPermissionDialog, canvas, TextManager.GetString(TextManager.StringTag.AppPermissionsTitle1), TextManager.GetString(TextManager.StringTag.AppPermissionsInfo1), (UGUICommon.ButtonType buttonType) =>
        {
            //RequestAndroidPermission(0);
            SceneManager.LoadScene("Login", LoadSceneMode.Single);
        });
    }

    void OpenAndroidSettings()
    {
        try
        {
#if UNITY_ANDROID
            using (var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivityObject = unityClass.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                string packageName = currentActivityObject.Call<string>("getPackageName");

                using (var uriClass = new AndroidJavaClass("android.net.Uri"))
                using (AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("fromParts", "package", packageName, null))
                using (var intentObject = new AndroidJavaObject("android.content.Intent", "android.settings.APPLICATION_DETAILS_SETTINGS", uriObject))
                {
                    intentObject.Call<AndroidJavaObject>("addCategory", "android.intent.category.DEFAULT");
                    intentObject.Call<AndroidJavaObject>("setFlags", 0x10000000);
                    currentActivityObject.Call("startActivity", intentObject);
                }
            }
#endif
        }
        catch (Exception ex)
        {
#if MDEBUG
            Debug.LogException(ex);
#endif
        }
    }

    static int getSDKInt()
    {
        try
        {
            using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                return version.GetStatic<int>("SDK_INT");
            }
        }
        catch
        {
            return 0;
        }
    }

    void RequestAndroidPermission(int permissionIndex)
    {
        if(permissionIndex >= permissions.Length)
        {

            if (toBeRestart && getSDKInt() <= 23)
            {
                RestartApp();
            }
            else 
            {
                StartOBBDownload();
            }
        }
        else
        {
            if (AndroidPermissionsManager.IsPermissionGranted(permissions[permissionIndex]))
            {
                RequestAndroidPermission(permissionIndex + 1);
            }
            else
            {
                if(mustHavePermissions[permissionIndex])
                {
                    toBeRestart = true;
                }
                AndroidPermissionsManager.RequestPermission(permissions[permissionIndex], new AndroidPermissionCallback(
                    grantedPermission =>
                    {
                        RequestAndroidPermission(permissionIndex + 1);
                    },
                    deniedPermission =>
                    {
                        if (mustHavePermissions[permissionIndex])
                        {
                            StartCoroutine(ShowRequestPermission());
                        }
                        else
                        {
                            RequestAndroidPermission(permissionIndex + 1);
                        }
                    },
                    deniedPermissionAndDontAskAgain =>
                    {
                        if (mustHavePermissions[permissionIndex])
                        {
                            UGUICommon.ShowRequestPermission(requestPermissionDialog, canvas, TextManager.GetString(TextManager.StringTag.AppPermissionsTitle2), TextManager.GetString(TextManager.StringTag.AppPermissionsInfo2), (UGUICommon.ButtonType buttonType) =>
                            {
                                OpenAndroidSettings();
                                StartCoroutine(ShowRequestPermission());
                            });
                        }
                        else
                        {
                            RequestAndroidPermission(permissionIndex + 1);
                        }
                    }
                ));
            }
        }
    }
#endif

    void StartOBBDownload()
    { 
        progressbar = GameObject.Find("ProgressBar").GetComponent<Progress>();
        progressbar.Value = 0;
        progresstext = GameObject.Find("ProgressBarText").GetComponent<Text>();
        progresstext.text = TextManager.GetString(TextManager.StringTag.CheckDownloadFiles);

        dataPath = AssetBundleUtility.GetAssetBundlesPath();
#if MDEBUG
        Debug.Log("dataPath = [" + dataPath + "]");
#endif
        patcher = new PatcherAsyncBG("");

        patcher.Progressed += OnProgressed;
        patcher.FileCompleted += OnFileCompleted;
        patcher.AllCompleted += OnAllCompleted;
        patcher.ErrorOccurred += OnErrorOccurred;
        patcher.TotalProgressed += OnTotalProgressed;
        patcher.ReachabilityChanged += OnReachabilityChanged;

#if QA
        patcher.DownloadOBB(OpenConfirmDialog, versionCode, "http://10.0.2.1/HaeginPatch/ModuleSample/Android/main.1.com.haegin.modulesample.obb");
#elif USE_ONESTORE_IAP
        patcher.DownloadOBB(OpenConfirmDialog, versionCode, "http://10.0.2.1/HaeginPatch/ModuleSample/Android/main.1.com.haegin.modulesample.obb");
#else
        patcher.DownloadOBB(OpenConfirmDialog, versionCode);
#endif
    }

    public void OnReachabilityChanged(bool reachable)
    {
        if (!reachable)
            progresstext.text = "Check Your Internet Connection";
    }

    public void OpenConfirmDialog(long fileSize, PatcherAsyncBG.OnConfirm callback)
    {
        GameObject.Find("Cover").transform.SetParent(null);

        GameObject SelectDialog = (GameObject)Instantiate(confirmDialog);
        SelectDialog.transform.SetParent(canvas.transform);
        SelectDialog.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        SelectDialog.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        GameObject.Find("Title").GetComponent<Text>().text = TextManager.GetString(TextManager.StringTag.OBBFileError);
        long size = fileSize / (1024 * 1024);
        if (size <= 0) size = 1;
        GameObject.Find("WIFIText").GetComponent<Text>().text = String.Format(TextManager.GetString(TextManager.StringTag.OBBFileDownloadMessage), size);

        ThreadSafeDispatcher.OnSystemBackKey onSystemBack = () =>
        {
            ThreadSafeDispatcher.Instance.PopSystemBackKeyListener();
            callback();
            Destroy(SelectDialog);
        };
        ThreadSafeDispatcher.Instance.PushSystemBackKeyListener(onSystemBack);

        GameObject.Find("Okay").GetComponent<Button>().onClick.AddListener(() =>
        {
            onSystemBack();
        });
    }

    void OnProgressed(string fileName, long fileDownloadedByte, long fileLength, int currentFileCount, int totalFileCount)
    {
    }

    void OnFileCompleted(string fileName, int currentFileCount, int totalFileCount)
    {
    }

    void RestartApp()
    {
        // Restart App
        AndroidJavaObject AOSUnityActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject baseContext = AOSUnityActivity.Call<AndroidJavaObject>("getBaseContext");
        AndroidJavaObject intentObj = baseContext.Call<AndroidJavaObject>("getPackageManager").Call<AndroidJavaObject>("getLaunchIntentForPackage", baseContext.Call<string>("getPackageName"));
        AndroidJavaObject componentName = intentObj.Call<AndroidJavaObject>("getComponent");
        AndroidJavaObject mainIntent = intentObj.CallStatic<AndroidJavaObject>("makeMainActivity", componentName);
        AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
        mainIntent = mainIntent.Call<AndroidJavaObject>("addFlags", intentClass.GetStatic<int>("FLAG_ACTIVITY_NEW_TASK"));
        mainIntent = mainIntent.Call<AndroidJavaObject>("addFlags", intentClass.GetStatic<int>("FLAG_ACTIVITY_CLEAR_TASK"));
        baseContext.Call("startActivity", mainIntent);
        AndroidJavaClass JavaSystemClass = new AndroidJavaClass("java.lang.System");
        JavaSystemClass.CallStatic("exit", 0);
    }

    void OnAllCompleted(int totalFileCount)
    {
#if MDEBUG
        Debug.Log("OnAllCompleted");
#endif
        if(totalFileCount == 0)
        {
            progressbar.Value = 0;
            progresstext.text = TextManager.GetString(TextManager.StringTag.CompleteDownloadFiles);
            patcher.ClearEvent();
            patcher = null;
            //SceneManager.LoadScene(1, LoadSceneMode.Single);
            SceneManager.LoadScene("Login", LoadSceneMode.Single);
        }
        else
        {
            RestartApp();
        }
    }

    void OnErrorOccurred(string fileName, string message)
    {
        // OBB 파일을 다운로드하지 못한 경우인데...
#if MDEBUG
        Debug.Log("OnErrorOccurred " + fileName + ", " + message);
        Debug.Log("ShowMessageDialog");
#endif
        GameObject.Find("Cover").transform.SetParent(null);

        GameObject SelectDialog = (GameObject)Instantiate(confirmDialog);
        SelectDialog.transform.SetParent(canvas.transform);
        SelectDialog.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        SelectDialog.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        GameObject.Find("Title").GetComponent<Text>().text = TextManager.GetString(TextManager.StringTag.OBBFileError);
        GameObject.Find("WIFIText").GetComponent<Text>().text = TextManager.GetString(TextManager.StringTag.OBBFileErrorMessage);

        ThreadSafeDispatcher.OnSystemBackKey onSystemBack = () =>
        {
            ThreadSafeDispatcher.ApplicationQuit();
        };
        ThreadSafeDispatcher.Instance.PushSystemBackKeyListener(onSystemBack);

        GameObject.Find("Okay").GetComponent<Button>().onClick.AddListener(() =>
        {
            onSystemBack();
        });
    }

    void OnTotalProgressed(long receivedBytes, long totalSize, int count, int totalCount)
    {
        progressbar.Value = (int)(receivedBytes * 100 / totalSize);
        progresstext.text = String.Format("{0}/{1}  [{2}/{3}]", receivedBytes, totalSize, count, totalCount);
    }
}
