using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Haegin;
using HaeginGame;

public class SceneGameServiceController : MonoBehaviour
{
    public Canvas canvas;
    public GameObject friendPanel;
    public GameObject systemDialog;
    public GameObject eulaText;
    public Texture2D image;

    private WebClient webClient;

    private void Awake()
    {
        UGUICommon.ResetCanvasReferenceSize(canvas);

        webClient = WebClient.GetInstance();

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

        ThreadSafeDispatcher.Instance.PushSystemBackKeyListener(OnSystemBackKey);
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
        UGUICommon.ShowMessageDialog(systemDialog, eulaText, canvas, TextManager.GetString(TextManager.StringTag.NetworkError), TextManager.GetString(TextManager.StringTag.NetworkErrorMessage), (UGUICommon.ButtonType buttonType) =>
        {
            if (buttonType == UGUICommon.ButtonType.Ok)
            {
                ThreadSafeDispatcher.ApplicationQuit();
            }
        });
    }

    void OnMaintenanceStarted()
    {
        // 메인터넌스가 시작되었다.
        UGUICommon.ShowMessageDialog(systemDialog, eulaText, canvas, TextManager.GetString(TextManager.StringTag.ServerMaintenanceTitle), TextManager.GetString(TextManager.StringTag.ServerMaintenance), (UGUICommon.ButtonType buttonType) =>
        {
            if (buttonType == UGUICommon.ButtonType.Ok)
            {
                ThreadSafeDispatcher.ApplicationQuit();
            }
        });
    }

    void OnProcessing(ReqAndRes rar)
    {
    }

    public void OnErrorOccurred(int error)
    {
        OnNetworkError();
    }

    public void OnSystemBackKey()
    {
        UGUICommon.ShowYesNoDialog(systemDialog, eulaText, canvas, TextManager.GetString(TextManager.StringTag.Quit), TextManager.GetString(TextManager.StringTag.QuitConfirm), (UGUICommon.ButtonType buttonType) =>
        {
            if (buttonType == UGUICommon.ButtonType.Yes)
            {
                ThreadSafeDispatcher.ApplicationQuit();
            }
        });
    }

    void OnDestroy()
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
        GameServiceSocial.LoadMyInfo(PhotoLoad.HiRes, (GameServiceResult result, SocialPlayerInfo playerInfo) =>
        {
            if (playerInfo != null)
            {
                GameObject.Find("UserName").GetComponent<Text>().text = playerInfo.name;
                GameObject.Find("UserPhoto").GetComponent<RawImage>().texture = playerInfo.photo;
            }
        });

        GameServiceSocial.LoadFriendsList((GameServiceResult result, List<SocialPlayerInfo> friends) =>
        {
            if (friends != null && friends.Count > 0)
            {
                GameObject contentObject = GameObject.Find("FriendsContent");
                for (int i = 0; i < friends.Count; i++)
                {
                    GameServiceSocial.LoadPlayerInfo(friends[i], PhotoLoad.Normal, (GameServiceResult result2, SocialPlayerInfo friend) =>
                    {
                        GameObject textBlock = (GameObject)Object.Instantiate(friendPanel);
                        textBlock.transform.SetParent(contentObject.transform);
                        textBlock.transform.localRotation = Quaternion.identity;
                        textBlock.transform.localPosition = Vector3.zero;
                        textBlock.transform.localScale = Vector3.one;
                        textBlock.transform.GetComponentsInChildren<Text>()[0].text = friend.name + "(" + friend.id + ")";
                        textBlock.name = i.ToString();
                        textBlock.GetComponent<LayoutElement>().preferredWidth = 860;
                        textBlock.GetComponent<LayoutElement>().preferredHeight = 128;
                        if (friend.photo != null)
                        {
                            textBlock.transform.GetComponentsInChildren<RawImage>()[0].texture = friend.photo;
                        }
                    });
                }
            }
            else
            {
                //Debug.Log("no friends");
            }
        });
    }

    public void OnCloseButton()
    {
        SceneManager.LoadScene("SceneStore", LoadSceneMode.Single);
    }

    public void OnAchievementButton()
    {
        GameServiceAchievements.ProgressAchievement("CgkIuNCNwIUQEAIQAQ", 5, 10, () =>
        {
#if MDEBUG
            Debug.Log("Submit Achievement");
#endif
            GameServiceAchievements.ShowAchievementsUI();
        });
    }

    public void OnLeaderboardButton()
    {
        GameServiceLeaderboards.SubmitScore("CgkIuNCNwIUQEAIQAw", 100.0f, () =>
        {
#if MDEBUG
            Debug.Log("Submit Score");
#endif
//            GameServiceLeaderboards.ShowLeaderboardUI("CgkIuNCNwIUQEAIQAw");
            GameServiceLeaderboards.ShowLeaderboardUI();
        });
    }

    public void OnInvite()
    {
        /*
        #if FB_CANVAS_APP_EXIST
                SceneManager.LoadScene("SceneFBInvite", LoadSceneMode.Single);        
        #else
                GameServiceAppInvite.FirebaseInvite("http://10.0.1.2/HaeginSample.html", "Title", "Message", "Free Download", (GameServiceAppInvite.AppInviteResult result) =>
                {
        #if MDEBUG
                    Debug.Log("invite callback");
        #endif
                });
        #endif   
        */
        Share.OpenShareDialog("ModuleSample Share", "모듈 샘플을 같이 할래?\nhttp://haegin.kr/Download/modulesample.php\n나하고 한 게임 어때?", image);
    }
}
