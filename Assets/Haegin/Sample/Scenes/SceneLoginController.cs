using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Haegin;
using Facebook.Unity;
using System;
using HaeginGame;

public class SceneLoginController : MonoBehaviour
{
    public Canvas canvas;
    public GameObject systemDialog;
    public GameObject eulaText;
    public AccountDialogController accountDialog;

    private WebClient webClient;

    void Awake()
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


#if UNITY_IOS
        GameObject.Find("GameCenterText").GetComponent<Text>().text = "GameCenter Login";
#elif UNITY_ANDROID
        GameObject.Find("GameCenterText").GetComponent<Text>().text = "Google Login";
#elif UNITY_STANDALONE && USE_STEAM
        GameObject.Find("GameCenterText").GetComponent<Text>().text = "Steam Login";
#endif

        if (Account.IsLoggedInGameService() && Account.GameServiceAccountType != Account.HaeginAccountType.Guest)
        {
            //GameObject.Find("GameCenterLogin").GetComponent<Button>().enabled = false;
            //GameObject.Find("GameCenterLogin").GetComponent<Image>().color = Color.grey;
        }
        if (Account.IsLoggedInFacebook())
        {
            //GameObject.Find("FacebookLogin").GetComponent<Button>().enabled = false;
            //GameObject.Find("FacebookLogin").GetComponent<Image>().color = Color.grey;
            GameObject.Find("FacebookLogin").GetComponent<Text>().text = "Facebook Logout";
        }

        if(!Account.IsSupportedAppleId())
        {
#if MDEBUG
            Debug.Log("Sign in with Apple is not supported.....");
#endif
            GameObject.Find("SignInWithApple").GetComponent<Button>().interactable = false;
        }


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

    public void OnGuestLoginButtonClick(string param)
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("Guest_Login");
        SceneManager.LoadScene("SceneGameService", LoadSceneMode.Single);
    }


    public void OnGameCenterLoginButtonClick(string param)
    {
#if UNITY_IOS
		Account.LoginAccount(Account.HaeginAccountType.AppleGameCenter, accountDialog.OpenSelectDialog, (bool result, WebClient.AuthCode code, TimeSpan blockRemainTime, long blockSuid) =>
        {
#if MDEBUG
            Debug.Log("LogintAccount  result=" + result + "    code=" + code + " blockSuid=" + blockSuid);
#endif
            if (result && code == WebClient.AuthCode.SUCCESS)
                SceneManager.LoadScene("SceneGameService", LoadSceneMode.Single);
        });
#elif UNITY_ANDROID
        Account.LoginAccount(Account.HaeginAccountType.GooglePlayGameService, accountDialog.OpenSelectDialog, (bool result, WebClient.AuthCode code, TimeSpan blockRemainTime, long blockSuid) =>
        {
#if MDEBUG
            Debug.Log("LogintAccount  result=" + result + "    code=" + code + " blockSuid=" + blockSuid);
#endif
            if (result && code == WebClient.AuthCode.SUCCESS)
                SceneManager.LoadScene("SceneGameService", LoadSceneMode.Single);
        });
#elif UNITY_STANDALONE && USE_STEAM
        Account.LoginAccount(Account.HaeginAccountType.Steam, accountDialog.OpenSelectDialog, (bool result, WebClient.AuthCode code, TimeSpan blockRemainTime, long blockSuid) =>
        {
#if MDEBUG
            Debug.Log("LogintAccount  result=" + result + "    code=" + code + " blockSuid=" + blockSuid);
#endif
            if (result && code == WebClient.AuthCode.SUCCESS)
                SceneManager.LoadScene("SceneGameService", LoadSceneMode.Single);
        });
#endif
    }

    public void OnSignInWithAppleButtonClick(string param)
    {
        Account.LoginAccount(Account.HaeginAccountType.AppleId, accountDialog.OpenSelectDialog, (bool result, WebClient.AuthCode code, TimeSpan blockRemainTime, long blockSuid) =>
        {
#if MDEBUG
            Debug.Log("LogintAccount  result=" + result + "    code=" + code + "  blockSuid=" + blockSuid);
#endif
            if (result && code == WebClient.AuthCode.SUCCESS)
                SceneManager.LoadScene("SceneGameService", LoadSceneMode.Single);
        });
    }

    public void OnFacebookLoginButtonClick(string param)
    {
        if (FB.IsLoggedIn)
        {
            FB.LogOut();
        }
        else
        {
            Account.LoginAccount(Account.HaeginAccountType.Facebook, accountDialog.OpenSelectDialog, (bool result, WebClient.AuthCode code, TimeSpan blockRemainTime, long blockSuid) =>
            {
#if MDEBUG
                Debug.Log("LogintAccount  result=" + result + "    code=" + code + "  blockSuid=" + blockSuid);
#endif
                if (result && code == WebClient.AuthCode.SUCCESS)
                    SceneManager.LoadScene("SceneGameService", LoadSceneMode.Single);
            });
        }
    }

}

