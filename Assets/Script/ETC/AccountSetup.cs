using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using Haegin;
using HaeginGame;
using Facebook.Unity;
using TMPro;

[Serializable] public class AccountSetup {
    private WebClient webClient;
    public Button facebookBtn;
    public Button gameCenterBtn;
    public Button appleBtn;
    public Sprite btnEnable;
    public Sprite btnDisable;
    public GameObject systemDialog;
    public GameObject eulaText;
    public Canvas canvas;
    public AccountDialogController accountDialog;

    public void Init() {
        WebClientInit();
        ButtonInit();
        AccountManager.Instance.tokenSetFinished += delegate{};
    }

    public void Destory() {
        DestoryWebClient();
        AccountManager.Instance.tokenSetFinished = null;
    }

    private void WebClientInit() {
        webClient = WebClient.GetInstance();

        webClient.ErrorOccurred += OnErrorOccurred;
        webClient.Processing += OnProcessing;
        webClient.RetryOccurred += RetryOccurred;
        webClient.RetryFailed += RetryFailed;
        webClient.MaintenanceStarted += OnMaintenanceStarted;
        webClient.Logged += (string log) => {
#if MDEBUG
            Debug.Log("Unity   " + log);
#endif
        };
    }

    public void ButtonInit() {
        facebookBtn.onClick.AddListener(OnFacebookLoginButtonClick);
        gameCenterBtn.onClick.AddListener(OnGameCenterLoginButtonClick);
        appleBtn.onClick.AddListener(OnSignInWithAppleButtonClick);
        TextMeshProUGUI facebookText = facebookBtn.GetComponentInChildren<TextMeshProUGUI>();
        TextMeshProUGUI gameCenterText = gameCenterBtn.GetComponentInChildren<TextMeshProUGUI>();
        TextMeshProUGUI appleText = appleBtn.GetComponentInChildren<TextMeshProUGUI>();
        

        facebookText.text = "페이스북 로그인";
        #if UNITY_IOS
        gameCenterText.text = "게임센터 로그인";
        appleText.text = "애플 로그인";
        #elif UNITY_ANDROID
        gameCenterText.text = "구글 로그인";
        appleBtn.gameObject.SetActive(false);
        #endif
        
        

        if (Account.IsLoggedInGameService() && Account.GameServiceAccountType != Account.HaeginAccountType.Guest) {
            #if UNITY_IOS
            gameCenterText.text = "게임센터 연동됨";
            #elif UNITY_ANDROID
            gameCenterText.text = "구글 연동됨";
            #endif
            gameCenterBtn.enabled = false;
            gameCenterBtn.image.sprite = btnDisable;
        }

        if (Account.IsLoggedInFacebook()) {
            facebookText.text = "페이스북 연동됨";
            facebookBtn.enabled = false;
            facebookBtn.image.sprite = btnDisable;
        }
        #if UNITY_IOS
        if(Account.IsSupportedAppleId() && Account.IsLoggedInAppleId()) {
            appleText.text = "애플 연동됨";
            appleBtn.enabled = false;
            appleBtn.image.sprite = btnDisable;
        }
        #endif
    }

    private void DestoryWebClient() {
        if (webClient != null) {
            webClient.ErrorOccurred -= OnErrorOccurred;
            webClient.Processing -= OnProcessing;
            webClient.RetryOccurred -= RetryOccurred;
            webClient.RetryFailed -= RetryFailed;
            webClient.MaintenanceStarted -= OnMaintenanceStarted;
        }
    }

    void OnProcessing(ReqAndRes rar){
        if(ProtocolId.IssueJWT == rar.Res.ProtocolId) {
            IssueJWTRes result = (IssueJWTRes)rar.Res;
            AccountManager.Instance.TokenId = result.Token;
            Modal.instantiate("계정 연동이 완료 되었습니다.", Modal.Type.CHECK, () => FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE));
        }
    }

    public void OnErrorOccurred(int error) {
        OnNetworkError();
    }

    void OnNetworkError() {
        UGUICommon.ShowMessageDialog(systemDialog, eulaText, canvas, TextManager.GetString(TextManager.StringTag.NetworkError), TextManager.GetString(TextManager.StringTag.NetworkErrorMessage), (UGUICommon.ButtonType buttonType) => {
            if (buttonType == UGUICommon.ButtonType.Ok) {
                ThreadSafeDispatcher.ApplicationQuit();
            }
        });
    }

    void OnMaintenanceStarted() {
        // 메인터넌스가 시작되었다.
        UGUICommon.ShowMessageDialog(systemDialog, eulaText, canvas, TextManager.GetString(TextManager.StringTag.ServerMaintenanceTitle), TextManager.GetString(TextManager.StringTag.ServerMaintenance), (UGUICommon.ButtonType buttonType) =>
        {
            if (buttonType == UGUICommon.ButtonType.Ok)
            {
                ThreadSafeDispatcher.ApplicationQuit();
            }
        });
    }
    
    void RetryFailed(Protocol protocol) {
        OnNetworkError();
    }

    void RetryOccurred(Protocol protocol, int retryCount) {
#if MDEBUG
        Debug.Log("Retry Occurred  " + retryCount);
#endif
    }

    public void OnGameCenterLoginButtonClick() {
#if UNITY_IOS
		Account.LoginAccount(Account.HaeginAccountType.AppleGameCenter, accountDialog.OpenSelectDialog, (bool result, WebClient.AuthCode code, TimeSpan blockRemainTime, long blockSuid) => {
#if MDEBUG
            Debug.Log("LogintAccount  result=" + result + "    code=" + code + " blockSuid=" + blockSuid);
#endif
            if (result && code == WebClient.AuthCode.SUCCESS)
                LoginComplete();
        });
#elif UNITY_ANDROID
        Account.LoginAccount(Account.HaeginAccountType.GooglePlayGameService, accountDialog.OpenSelectDialog, (bool result, WebClient.AuthCode code, TimeSpan blockRemainTime, long blockSuid) => {
#if MDEBUG
            Debug.Log("LogintAccount  result=" + result + "    code=" + code + " blockSuid=" + blockSuid);
#endif
            if (result && code == WebClient.AuthCode.SUCCESS)
                LoginComplete();
        });
#elif UNITY_STANDALONE && USE_STEAM
        Account.LoginAccount(Account.HaeginAccountType.Steam, accountDialog.OpenSelectDialog, (bool result, WebClient.AuthCode code, TimeSpan blockRemainTime, long blockSuid) => {
#if MDEBUG
            Debug.Log("LogintAccount  result=" + result + "    code=" + code + " blockSuid=" + blockSuid);
#endif
            if (result && code == WebClient.AuthCode.SUCCESS)
                LoginComplete();
        });
#endif
    }

    public void OnSignInWithAppleButtonClick() {
        Account.LoginAccount(Account.HaeginAccountType.AppleId, accountDialog.OpenSelectDialog, (bool result, WebClient.AuthCode code, TimeSpan blockRemainTime, long blockSuid) => {
#if MDEBUG
            Debug.Log("LogintAccount  result=" + result + "    code=" + code + "  blockSuid=" + blockSuid);
#endif
            if (result && code == WebClient.AuthCode.SUCCESS)
                LoginComplete();
        });
    }

    public void OnFacebookLoginButtonClick()
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
        webClient.Request(new IssueJWTReq());
    }
}