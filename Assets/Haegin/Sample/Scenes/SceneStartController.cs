using HaeginGame;
using UnityEngine;
using Haegin;
using System;
using G.Util;
using G.Network;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Facebook.Unity;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class SceneStartController : MonoBehaviour
{
    public Canvas EULACanvas, LoginTypeCanvas;
    public GameObject confirmDialog;
    public GameObject systemDialog;
    public GameObject eulaText;
    public GameObject updateMessage;
    public GameObject eulaBg;
    public GameObject eulaDetailL;
    public GameObject eulaDetailP;
    public GameObject promoEventFrame;
    public GameObject serviceCheckDialog;
    public AccountDialogController accountDialog;

    private WebClient webClient;

    public GameObject preloadSplash;
    public TextAsset preloadKorean;
    public TextAsset preloadEnglish;

    string urlscheme = null;

    void OnOpenURL(string url, Dictionary<string, string> parameters)
    {
        // parameters에 URL Scheme을 통해 전달된 파라메터 값들이 들어있다. 따로 url을 파싱할 필요없음.
#if MDEBUG
        Debug.Log("OnOpenURL " + url);
#endif

        // 여기서는 그냥 url 이 표시해 주는 정도만 처리한다. 
        GameObject urlschemeText = GameObject.Find("UrlScheme");
        if(urlschemeText != null) 
        {
            string matchVal;
            if(parameters.TryGetValue("match", out matchVal)) {
                // 파라메터에 match 값을 찾아서 보여준다. 이런식으로 값을 읽어올 수 있다.
                url = url + "    [" + matchVal + "]";
            }
            urlschemeText.GetComponent<Text>().text = url;
        }

        // 처리한 이후에는 URLScheme을 지운다.
        URLScheme.Instance().ClearUrlScheme();
    }

    private void Awake()
    {
        // URLScheme으로 실행된 경우 처리할 수 있는 함수 지정
        URLScheme.Instance().OnOpenWithURLScheme += OnOpenURL;         

        // WebView를 올바른 크기와 위치에 놓을 수 있도록, 초기 화면 사이즈를 저장해 둡니다. 
        //UGUICommon.SaveScreenSize();

        // Main Thread Dispatcher 초기화 : 최초 Scene에서 Awake 에서 반드시 콜
        //ThreadSafeDispatcher.Initialize();

        // 어떤 언어를 사용할 것인지 초기화 
        //TextManager.Initialize(Application.systemLanguage.ToString());

        // Account 관리를 위해 초기화
        Account.Initialize(ProjectSettings.webClientOAuth2ClientId);

        // 최초 씬에서 webClient를 생성
        MessagePack.Resolvers.CompositeResolver.RegisterAndSetAsDefault(Haegin.Resolvers.HaeginResolver.Instance, MessagePack.Unity.UnityResolver.Instance, MessagePack.Resolvers.BuiltinResolver.Instance, MessagePack.Resolvers.AttributeFormatterResolver.Instance, MessagePack.Resolvers.PrimitiveObjectResolver.Instance);

        ThreadSafeDispatcher.Instance.PushSystemBackKeyListener(OnSystemBackKey);

#if UNITY_ANDROID
        // AppID, Secret, Auth 할 것인지 여부 (서비스 빌드는 false로 반드시...)
        HockeyApp.Initialize("36b15dd0c5a240d4a98b086133f2ecf3", "9078c8bd7ca9af4866f38649ec520764", true);
#elif UNITY_IOS
        // AppID, Secret, Auth 할 것인지 여부 (서비스 빌드는 false로 반드시...)
        HockeyApp.Initialize("a340f4f488254145b766caec4bfdd037", "7e382ac0f1f816411393e13514c7939a", true);
#endif
    }

    void RetryOccurred(Protocol protocol, int retryCount)
    {
#if MDEBUG
        Debug.Log("Retry Occurred  " + retryCount);
#endif
    }

    void RetryFailed(Protocol protocol)
    {
        OnNetworkError();
    }

    void OnNetworkError()
    {
        UGUICommon.ShowMessageDialog(systemDialog, eulaText, EULACanvas, TextManager.GetString(TextManager.StringTag.NetworkError), TextManager.GetString(TextManager.StringTag.NetworkErrorMessage), (UGUICommon.ButtonType buttonType) =>
        {
            if (buttonType == UGUICommon.ButtonType.Ok)
            {
                ThreadSafeDispatcher.ApplicationQuit();
            }
        });
    }

    void OnSystemBackKey()
    {
        UGUICommon.ShowYesNoDialog(systemDialog, eulaText, EULACanvas, TextManager.GetString(TextManager.StringTag.Quit), TextManager.GetString(TextManager.StringTag.QuitConfirm), (UGUICommon.ButtonType buttonType) =>
        {
            if (buttonType == UGUICommon.ButtonType.Yes)
            {
                ThreadSafeDispatcher.ApplicationQuit();
            }
        });
    }

    void OnMaintenanceStarted()
    {
        // 메인터넌스가 시작되었다.
        UGUICommon.ShowMessageDialog(systemDialog, eulaText, EULACanvas, TextManager.GetString(TextManager.StringTag.ServerMaintenanceTitle), TextManager.GetString(TextManager.StringTag.ServerMaintenance), (UGUICommon.ButtonType buttonType) =>
        {
            if (buttonType == UGUICommon.ButtonType.Ok)
            {
                ThreadSafeDispatcher.ApplicationQuit();
            }
        });
    }

    void OnProcessing(ReqAndRes rar)
    {
        Protocol req = rar.Req.Protocol;
        ProtocolRes res = rar.Res;

        // 여기서 개별 게임용 패킷을 처리하나?
    }

    private void OnDestroy()
    {
        if (webClient != null)
        {
            webClient.ErrorOccurred -= OnErrorOccurred;
            webClient.Processing -= OnProcessing;
            webClient.RetryOccurred -= RetryOccurred;
            webClient.RetryFailed -= RetryFailed;
            webClient.MaintenanceStarted -= OnMaintenanceStarted;
        }
        ThreadSafeDispatcher.Instance.PopSystemBackKeyListener();
    }

    void Start()
    {
#if MDEBUG
        Debug.Log("SceneStartController Start1");
#endif
        URLScheme.Instance().CheckUrlScheme();
#if MDEBUG
        Debug.Log("SceneStartController Start2");
#endif
        HaeginSplash.ShowHaeginSplash(HaeginSplash.Orientations.Landscape, CheckServiceStatus);
#if MDEBUG
        Debug.Log("SceneStartController Start3");
#endif
    }

    // 
    //  서버가 점검 중인지 확인 한다.
    // 
    GameObject ServiceCheckDialogWin = null;

#if USE_MAINTENANCESERVER_V2
    void CheckServiceStatus()
    {
#if UNITY_EDITOR
        ServiceMaintenance.CheckStatusV2("http://dev-maintenance.fbl.kr/Gate", "DevForClient", ShowServerMaintenanceWin, OnServerMaintenanceAction, (string CommonUrl, string GameUrl, string PatchUrl) =>
#else
        ServiceMaintenance.CheckStatusV2("http://dev-maintenance.fbl.kr/Gate", "DevForTest", ShowServerMaintenanceWin, OnServerMaintenanceAction, (string CommonUrl, string GameUrl, string PatchUrl) =>
#endif
        {
            Debug.Log(CommonUrl);
            Debug.Log(GameUrl);
            Debug.Log(PatchUrl);

            NetworkManager.Instance.SetUrl(GameUrl + "/");

            //webClient = WebClient.GetInstance("http://10.0.2.3:80/Gate");
            webClient = WebClient.GetInstance(CommonUrl);

            webClient.ErrorOccurred += OnErrorOccurred;
            webClient.Processing += OnProcessing;
            webClient.RetryOccurred += RetryOccurred;
            webClient.RetryFailed += RetryFailed;
            webClient.MaintenanceStarted += OnMaintenanceStarted;
            webClient.Logged += (string log) =>
            {
#if MDEBUG
                Debug.Log("Unity   " + log);
#endif
            };

            // Patch용 url
            url = PatchUrl;

            if (ServiceCheckDialogWin != null)
                UGUICommon.CloseServiceCheckDialog(ServiceCheckDialogWin);

            GPrestoApi.Init(() =>
            {
                StartGame();
            });
        });
    }

    void ShowServerMaintenanceWin(bool isError, string contents, string time, ServiceMaintenance.OnRetry onRetry)
    {
        if (isError)
        {
            UGUICommon.ShowYesNoDialog(systemDialog, eulaText, EULACanvas, TextManager.GetString(TextManager.StringTag.MaintenanceNetworkError), TextManager.GetString(TextManager.StringTag.MaintenanceNetworkErrorMessage), (UGUICommon.ButtonType buttonType) =>
            {
                if (buttonType == UGUICommon.ButtonType.Yes)
                {
#if MDEBUG
                    Debug.Log("Retry");
#endif
                    onRetry();
                }
                else
                {
#if MDEBUG
                    Debug.Log("Quit.");
#endif
                    ThreadSafeDispatcher.ApplicationQuit();
                }
            });
        }
        else
        {
            ServiceCheckDialogWin = UGUICommon.ShowServiceCheckDialog(serviceCheckDialog, EULACanvas, contents, time, onRetry);
        }
    }

    void OnServerMaintenanceAction(ServiceMaintenance.Action op, string contents, string time)
    {
        if (ServiceCheckDialogWin != null)
        {
            if (op == ServiceMaintenance.Action.DisableInput)
            {
                GameObject.Find("ServerCheckWinButtonRetry").GetComponent<Button>().interactable = false;
            }
            else if (op == ServiceMaintenance.Action.EnableInput)
            {
                GameObject.Find("MaintenanceContents").GetComponent<Text>().text = contents;
                GameObject.Find("MaintenanceTime").GetComponent<Text>().text = time;
                GameObject.Find("ServerCheckWinButtonRetry").GetComponent<Button>().interactable = true;
            }
        }
    }
#else
    void CheckServiceStatus()
    {
#if MDEBUG
        Debug.Log("SceneStartController CheckServiceStatus 1");
#endif
        ServiceMaintenance.CheckStatus("http://dev-maintenance.fbl.kr/Gate", "DevForClient", ShowServerMaintenanceWin, OnServerMaintenanceAction, (string ServerUrl, string PatchUrl) =>
        {
#if MDEBUG
            Debug.Log("SceneStartController CheckServiceStatus 2");
            Debug.Log(ServerUrl);
            Debug.Log(PatchUrl);
#endif
            //webClient = WebClient.GetInstance("http://10.0.2.3:80/Gate");
            webClient = WebClient.GetInstance(ServerUrl);

            webClient.ErrorOccurred += OnErrorOccurred;
            webClient.Processing += OnProcessing;
            webClient.RetryOccurred += RetryOccurred;
            webClient.RetryFailed += RetryFailed;
            webClient.MaintenanceStarted += OnMaintenanceStarted;
            webClient.Logged += (string log) =>
            {
#if MDEBUG
                Debug.Log("Unity   " + log);
#endif
            };

            // Patch용 url
            url = PatchUrl;

            if (ServiceCheckDialogWin != null)
                UGUICommon.CloseServiceCheckDialog(ServiceCheckDialogWin);


#if MDEBUG
            Debug.Log("SceneStartController CheckServiceStatus 3");
#endif
            GPrestoApi.Init(() =>
            {
                StartGame();
            });
        });
    }

    void ShowServerMaintenanceWin(bool isError, string contents, string time, ServiceMaintenance.OnRetry onRetry)
    {
        if (isError)
        {
            UGUICommon.ShowYesNoDialog(systemDialog, eulaText, canvas, TextManager.GetString(TextManager.StringTag.MaintenanceNetworkError), TextManager.GetString(TextManager.StringTag.MaintenanceNetworkErrorMessage), (UGUICommon.ButtonType buttonType) =>
            {
                if (buttonType == UGUICommon.ButtonType.Yes)
                {
#if MDEBUG
                    Debug.Log("Retry");
#endif
                    onRetry();
                }
                else
                {
#if MDEBUG
                    Debug.Log("Quit.");
#endif
                    ThreadSafeDispatcher.ApplicationQuit();
                }
            });
        }
        else
        {
            ServiceCheckDialogWin = UGUICommon.ShowServiceCheckDialog(serviceCheckDialog, canvas, contents, time, onRetry);
        }
    }

    void OnServerMaintenanceAction(ServiceMaintenance.Action op, string contents, string time)
    {
        if (ServiceCheckDialogWin != null)
        {
            if (op == ServiceMaintenance.Action.DisableInput)
            {
                GameObject.Find("ServerCheckWinButtonRetry").GetComponent<Button>().interactable = false;
            }
            else if (op == ServiceMaintenance.Action.EnableInput)
            {
                GameObject.Find("MaintenanceContents").GetComponent<Text>().text = contents;
                GameObject.Find("MaintenanceTime").GetComponent<Text>().text = time;
                GameObject.Find("ServerCheckWinButtonRetry").GetComponent<Button>().interactable = true;
            }
        }
    }
#endif


        void StartGame()
    {
#if MDEBUG
        Debug.Log("ScencStartController StartGame");
#endif
        SecureInt i = 10;
        i++;
#if MDEBUG
        Debug.Log("Int : " + i);
#endif
        //        HockeyApp.TrackEvent("StartGame");
        //        HockeyApp.ShowFeedbackForm();

        Notification.SetAppIconBadgeCount(0);  // android 6.0 미만 버전들을 위해 아이콘 배지 리셋   


        // Handshake  &  Protocol Version Check
        ushort[] clientVersion = new ushort[] { 1, 0, 0, 1 };
#if UNITY_ANDROID
        string updateDownloadUrl = "https://play.google.com/store/apps/details?id=com.haegin.homerunclash";
#elif UNITY_IOS
        string updateDownloadUrl = "https://itunes.apple.com/app/id1345750763";
#else
        string updateDownloadUrl = "http://haegin.kr";
#endif
        string languageSetting = TextManager.GetLanguageSetting();
        webClient.RequestHandshake(clientVersion, languageSetting, (WebClient.ErrorCode error, WebClient.VersionCheckCode code, string versionInfo) =>
        {
#if MDEBUG
            Debug.Log("VersionInfo [" + code + "] " + versionInfo);
#endif
            if (error == WebClient.ErrorCode.SUCCESS)
            {
                switch (code)
                {
                    case WebClient.VersionCheckCode.LATEST:
                        OnFinishedVersionUp();
                        break;
                    case WebClient.VersionCheckCode.UPDATE_IF_YOU_WANT:
                        UGUICommon.ShowVersionUpWindow(updateMessage, eulaText, EULACanvas, OnFinishedVersionUp, true, versionInfo, updateDownloadUrl);
                        break;
                    case WebClient.VersionCheckCode.UPDATE_REQUIRED:
                        UGUICommon.ShowVersionUpWindow(updateMessage, eulaText, EULACanvas, OnFinishedVersionUp, false, versionInfo, updateDownloadUrl);
                        break;
                }
            }
            else
            {
                OnNetworkError();
            }
        });
    }

    public void OnErrorOccurred(int error)
    {
        OnNetworkError();
    }


    //
    //   VersionUp : Update Required
    //
    void OnFinishedVersionUp()
    {
        // 추가 다운로드 실시
        //        Patch();
        // 기본 사용자 인증 시작  
#if MDEBUG
        Debug.Log("OnFinishedVersionUp");
#endif
        Account.LoginAccount(Account.GameServiceAccountType, accountDialog.OpenSelectDialog, (bool result, WebClient.AuthCode code, TimeSpan blockRemainTime, long blockSuid) =>
        {
#if MDEBUG
            Debug.Log("LoginAccount Callback  " + result + ", " + code + ", " + blockRemainTime + ", " + blockSuid);
#endif
            switch (code)
            {
                case WebClient.AuthCode.SUCCESS:
                    EULA.CheckEULA(OpenEULADialog, (bool isSuccess) =>
                    {
                        if (isSuccess)
                        {
                            Notification.RequestPermissions((bool result2) =>
                            {
//                                Notification.ScheduleUserNotification("1", "Haegin3", "Content 3", -1, 90, new Color32(0xff, 0x44, 0x44, 0xff), (bool r, string id) =>
//                                {
//#if MDEBUG
//                                    Debug.Log("Scheduled " + r + "  id " + id);
//#endif
//                                    Notification.CancelUserNotification("1");
//                                    Notification.ScheduleUserNotification("2", "Haegin1", "Content 1", -1, 30, new Color32(0xff, 0x44, 0x44, 0xff), (bool r2, string id2) =>
//                                    {
//#if MDEBUG
//                                        Debug.Log("Scheduled " + r2 + "  id " + id2);
//#endif
//                                        Notification.ScheduleUserNotification("3", "Haegin2", "Content 2", -1, 60, new Color32(0xff, 0x44, 0x44, 0xff), (bool r3, string id3) =>
//                                        {
//#if MDEBUG
//                                            Debug.Log("Scheduled " + r3 + "  id " + id3);
//#endif
//                                        });
//                                    });
//                                });

                                // Push Notification 을 등록한다. 
                                Notification.RegisterPushNotificationDevice();
                            });

                            //Patch();
                            PromoEvents.CheckPromoEvents(OpenPromoEventWindow, () =>
                            {
                            #if MDEBUG
                                Debug.Log("LoadScene SceneLogin");
#endif
                                //SceneManager.LoadScene("SceneLogin", LoadSceneMode.Single);
                                EULACanvas.gameObject.SetActive(false);
                                NetworkManager.Instance.Auth();
                            });
                        }
                        else
                        {
#if MDEBUG
                            Debug.Log("EULA Confirm Failed");
#endif
                        }
                    });
                    break;
                case WebClient.AuthCode.FAILED_CrossCheckRetry:
#if MDEBUG
                    Debug.Log("=====================================    Login Fail (Retry)  =====================================   " + code + " " + blockRemainTime);
#endif
                    UGUICommon.ShowMessageDialog(systemDialog, eulaText, EULACanvas, TextManager.GetString(TextManager.StringTag.LoginErrorTitle), TextManager.GetString(TextManager.StringTag.LoginErrorMessage2), (UGUICommon.ButtonType buttonType) =>
                    {
                        if (buttonType == UGUICommon.ButtonType.Ok)
                        {
                            ThreadSafeDispatcher.ApplicationQuit();
                        }
                    });
                    break;
                case WebClient.AuthCode.FAILED_CrossCheckSignatureMismatched:
                case WebClient.AuthCode.FAILED_Blocked_Abusing:
                case WebClient.AuthCode.FAILED_Blocked_InvalidPurchase:
                case WebClient.AuthCode.FAILED_Blocked_UnauthorizedPrograms:
                case WebClient.AuthCode.FAILED_Blocked_InappropriateBehavior:
                case WebClient.AuthCode.FAILED_Blocked_MisusingSystemErrorsAndBugs:
#if MDEBUG
                    Debug.Log("=====================================    Login Fail (Block)  =====================================   " + code + " " + blockRemainTime);
#endif
                    // 이 경우는 블럭될 유저
                    UGUICommon.ShowMessageDialog(systemDialog, eulaText, EULACanvas, TextManager.GetString(TextManager.StringTag.LoginErrorTitle), TextManager.GetString(TextManager.StringTag.LoginErrorMessage2), (UGUICommon.ButtonType buttonType) =>
                    {
                        if (buttonType == UGUICommon.ButtonType.Ok)
                        {
                            ThreadSafeDispatcher.ApplicationQuit();
                        }
                    });
                    break;
                case WebClient.AuthCode.Cancel:
                case WebClient.AuthCode.FAILED:
                case WebClient.AuthCode.FAILED_AuthExpired:
                case WebClient.AuthCode.FAILED_CreationFailed:
                case WebClient.AuthCode.FAILED_NotExist:
#if MDEBUG
                    Debug.Log("=====================================    Login Fail   =====================================   " + code + " " + blockRemainTime);
#endif
                    UGUICommon.ShowMessageDialog(systemDialog, eulaText, EULACanvas, TextManager.GetString(TextManager.StringTag.LoginErrorTitle), TextManager.GetString(TextManager.StringTag.LoginErrorMessage1), (UGUICommon.ButtonType buttonType) =>
                    {
                        if (buttonType == UGUICommon.ButtonType.Ok)
                        {
                            ThreadSafeDispatcher.Instance.Invoke(() =>
                            {
                                OnFinishedVersionUp();
                            });
                        }
                    });
                    break;
            }
#if UNITY_STANDALONE && USE_STEAM
            if(!result)
            {
                // 테스트 편의성을 위해서 Steam에서는 초기화 해버리자.. 
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
#if MDEBUG
                Debug.Log("Delete PlayerPrefs");
#endif
            }
#endif
        });

    }

    private static string url = "http://10.0.2.1/HaeginPatch/ModuleSample/";
    private static string patchName = "base";
#if MERGED_PATCH_BIN
    private static string patchFileName = "Merged.bin";
    private static string subfolder = "base";
#endif
    private PatcherAsyncBG patcher;
    private string dataPath;
    private Progress progressbar;
    private Text progresstext;
    private uint[] key = ConvertEx.ToKey("5swdcsrBHoBYjDWi5VXN72");
    private int versionCode = -1;   // 현재 버전인 경우 -1로 세팅하면 됩니다.

    void Patch()
    {
        progressbar = GameObject.Find("ProgressBar").GetComponent<Progress>();
        progressbar.Value = 0;
        progresstext = GameObject.Find("ProgressBarText").GetComponent<Text>();
        progresstext.text = TextManager.GetString(TextManager.StringTag.CheckDownloadFiles);

        dataPath = AssetBundleUtility.GetAssetBundlesPath();
#if MDEBUG
        Debug.Log("dataPath = [" + dataPath + "]");
#endif
        patcher = new PatcherAsyncBG(url);
        patcher.Progressed += OnProgressed;
        patcher.FileCompleted += OnFileCompleted;
        patcher.AllCompleted += OnAllCompleted;
        patcher.ErrorOccurred += OnErrorOccurred;
        patcher.TotalProgressed += OnTotalProgressed;
        patcher.ReachabilityChanged += OnReachabilityChanged;

#if !MERGED_PATCH_BIN
        patcher.Patch(patchName, dataPath, key, OpenConfirmDialog, "Patch.bin", versionCode);
#else
        patcher.Patch("", dataPath, key, OpenConfirmDialog, patchFileName, versionCode);
#endif
    }

    public void OnReachabilityChanged(bool reachable)
    {
        if (!reachable)
            progresstext.text = "Check Your Internet Connection";
    }

    public void OpenConfirmDialog(long fileSize, PatcherAsyncBG.OnConfirm callback)
    {
        GameObject SelectDialog = (GameObject)Instantiate(confirmDialog);
        SelectDialog.transform.SetParent(EULACanvas.transform);
        SelectDialog.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        SelectDialog.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        GameObject.Find("WIFIText").GetComponent<Text>().text = String.Format(TextManager.GetString(TextManager.StringTag.DownloadAssetMessage), fileSize / (1024 * 1024));

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

    void OnAllCompleted(int totalFileCount)
    {
#if MDEBUG
        Debug.Log("--------------------------------------------\nOnAllCompleted  " + totalFileCount + "\n----------------------------------------------");
#endif

        if (totalFileCount == 0)
        {
            progressbar.Value = 0;
            progresstext.text = TextManager.GetString(TextManager.StringTag.CompleteDownloadFiles);
            patcher.ClearEvent();
            patcher = null;
            StartCoroutine(OnFinishPatch());
        }
        else
        {
            progressbar.Value = 0;
            progresstext.text = TextManager.GetString(TextManager.StringTag.CheckDownloadFiles);

            var key = ConvertEx.ToKey("5swdcsrBHoBYjDWi5VXN72");
#if !MERGED_PATCH_BIN
            patcher.Patch(patchName, dataPath, key, null, "Patch.bin", versionCode);
#else
            patcher.Patch("", dataPath, key, null, patchFileName, versionCode);
#endif
        }
    }

    void OnErrorOccurred(string fileName, string message)
    {
        Debug.Log("OnErrorOccurred " + fileName + ", " + message);

        progressbar.Value = 0;
        progresstext.text = TextManager.GetString(TextManager.StringTag.ErrorDownloadFiles);

#if !MERGED_PATCH_BIN
        patcher.Patch(patchName, dataPath, key, null, "Patch.bin", versionCode);
#else
        patcher.Patch("", dataPath, key, null, patchFileName, versionCode);
#endif
    }

    void OnTotalProgressed(long receivedBytes, long totalSize, int count, int totalCount)
    {
        progressbar.Value = (int)(receivedBytes * 100 / totalSize);
        progresstext.text = String.Format("{0}/{1}  [{2}/{3}]", receivedBytes, totalSize, count, totalCount);
    }

    IEnumerator OnFinishPatch()
    {
        AssetBundleManager.ActiveVariants = new string[] { "sd" };
        var initRequest = AssetBundleManager.Initialize(patchName);
        if (initRequest != null)
        {
            yield return StartCoroutine(initRequest);
        }
        AssetBundleLoadAssetOperation loadRequest = AssetBundleManager.LoadAssetAsync("textdata", TextManager.GetLanguageSetting(), typeof(TextAsset));
        if (loadRequest == null)
        {
            yield break;
        }
        yield return StartCoroutine(loadRequest);

        TextAsset textAsset = loadRequest.GetAsset<TextAsset>();
        if (textAsset == null)
        {
            AssetBundleManager.UnloadAssetBundle("textdata");
            loadRequest = AssetBundleManager.LoadAssetAsync("textdata", "English", typeof(TextAsset));
            if (loadRequest == null)
            {
                yield break;
            }
            yield return StartCoroutine(loadRequest);
            textAsset = loadRequest.GetAsset<TextAsset>();
            if (textAsset == null)
            {
                AssetBundleManager.UnloadAssetBundle("textdata");
                loadRequest = AssetBundleManager.LoadAssetAsync("textdata", "Korean", typeof(TextAsset));
                if (loadRequest == null)
                {
                    yield break;
                }
                yield return StartCoroutine(loadRequest);
                textAsset = loadRequest.GetAsset<TextAsset>();
            }
        }
        //        if (textAsset != null)
        //        {
        //            TextManager.Initialize(TextManager.GetLanguageSetting(), textAsset);
        //        }
        AssetBundleManager.UnloadAssetBundle("textdata");

        PromoEvents.CheckPromoEvents(OpenPromoEventWindow, () =>
        {
#if MDEBUG
            Debug.Log("LoadScene SceneLogin");
#endif
            SceneManager.LoadScene("SceneLogin", LoadSceneMode.Single);
        });
    }

    void OpenEULADialog(string[] titles, string[] contents, bool[] isChecked, EULA.OnConfirmEULA onConfirm)
    {
        UGUICommon.OpenEULADialog(eulaBg, EULACanvas, titles, contents, isChecked, OpenEULADetailDialog, onConfirm);
    }

    void OpenEULADetailDialog(string title, string content)
    {
        UGUICommon.ShowEULADetailWindow(eulaDetailL, eulaDetailP, eulaText, EULACanvas, title, content);
    }

    void OpenPromoEventWindow(string imageUrl, string destUrl, PromoEvents.OnCloseWindow onClose)
    {
        StartCoroutine(OpenPromoEventWindowSub(imageUrl, destUrl, onClose));
    }

    IEnumerator OpenPromoEventWindowSub(string imageUrl, string destUrl, PromoEvents.OnCloseWindow onClose)
    {
        using (WWW www = new WWW(imageUrl))
        {
            yield return www;
            Sprite sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0.5f, 0.5f));
            UGUICommon.OpenPromoEventWindow(promoEventFrame, EULACanvas, sprite, destUrl, onClose);
        }
    }

}
