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
        //UGUICommon.ResetCanvasReferenceSize(canvas);

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
#if UNITY_IOS
        GameObject googleLogin = GameObject.Find("GoogleLogin");
        if (googleLogin != null) googleLogin.gameObject.SetActive(false);
#elif UNTIY_ANDROID
        GameObject gameCenterLogin = GameObject.Find("GameCenterLogin");
        if (gameCenterLogin != null) gameCenterLogin.gameObject.SetActive(false);
        GameObject appleSign = GameObject.Find("SignInWithApple");
        if(appleSign != null) appleSign.gameObject.SetActive(false);
#endif
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
        LoginComplete();
    }

    public void OnGameCenterLoginButtonClick(string param)
    {
		Account.LoginAccount(Account.HaeginAccountType.AppleGameCenter, accountDialog.OpenSelectDialog, (bool result, WebClient.AuthCode code, TimeSpan blockRemainTime, long blockSuid) => {
#if MDEBUG
            Debug.Log("LogintAccount  result=" + result + "    code=" + code + " blockSuid=" + blockSuid);
#endif
            if (result && code == WebClient.AuthCode.SUCCESS)
                LoginComplete();
        });
    }

    public void OnGooglePlayLoginButtonClick(string param) {
        Account.LoginAccount(Account.HaeginAccountType.GooglePlayGameService, accountDialog.OpenSelectDialog, (bool result, WebClient.AuthCode code, TimeSpan blockRemainTime, long blockSuid) => {
            Debug.Log("LogintAccount  result=" + result + "    code=" + code + " blockSuid=" + blockSuid);
            if (result && code == WebClient.AuthCode.SUCCESS)
                LoginComplete();
        });
    }

    public void OnSignInWithAppleButtonClick(string param)
    {
        if(!Account.IsSupportedAppleId()) {
            Modal.instantiate("애플 로그인은 iOS 13 이상 버전에서만 지원합니다.", Modal.Type.CHECK);
            return;
        }
        Account.LoginAccount(Account.HaeginAccountType.AppleId, accountDialog.OpenSelectDialog, (bool result, WebClient.AuthCode code, TimeSpan blockRemainTime, long blockSuid) =>
        {
#if MDEBUG
            Debug.Log("LogintAccount  result=" + result + "    code=" + code + "  blockSuid=" + blockSuid);
#endif
            if (result && code == WebClient.AuthCode.SUCCESS)
                LoginComplete();
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
                    LoginComplete();
            });
        }
    }

    public void LoginComplete() {
        if(PlayerPrefs.GetInt("isFirst", 2) == 2) {
            PlayerPrefs.SetInt("isFirst", 1);
            PlayerPrefs.Save();
        }
        gameObject.SetActive(false);
        webClient.Request(new IssueJWTReq());
        AccountManager.Instance.OnSignInResultModal();
    }
}

