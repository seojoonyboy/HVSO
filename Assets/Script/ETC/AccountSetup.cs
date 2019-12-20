using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Haegin;
using HaeginGame;
using Facebook.Unity;

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
    }

    public void Destory() {

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

    void OnProcessing(ReqAndRes rar){ }

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

    public void OnGameCenterLoginButtonClick(string param) {
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
                ActivateIssueJWT();
        });
#endif
    }

    public void OnSignInWithAppleButtonClick(string param) {
        Account.LoginAccount(Account.HaeginAccountType.AppleId, accountDialog.OpenSelectDialog, (bool result, WebClient.AuthCode code, TimeSpan blockRemainTime, long blockSuid) => {
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

    }
}