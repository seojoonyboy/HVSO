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
    public Button googleBtn;
    public Button couponBtn;
    
    public Sprite btnEnable;
    public Sprite btnDisable;
    public GameObject systemDialog;
    public GameObject eulaText;
    public GameObject hideModal;
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
        googleBtn.onClick.AddListener(OnGoogleLoginButtonClick);
        
        Text facebookText = facebookBtn.GetComponentInChildren<Text>();
        Text gameCenterText = gameCenterBtn.GetComponentInChildren<Text>();
        Text appleText = appleBtn.GetComponentInChildren<Text>();
        Text googleText = googleBtn.GetComponentInChildren<Text>();

        var fbl_translator = AccountManager.Instance.GetComponent<Fbl_Translator>();

        string _gameCenterText = fbl_translator.GetLocalizedText("MainUI", "ui_page_setting_gamecenter");
        string _googleText = fbl_translator.GetLocalizedText("MainUI", "ui_page_setting_googleplay");
        string _facebookText = fbl_translator.GetLocalizedText("MainUI", "ui_page_setting_facebook");
        string _appleIdText = fbl_translator.GetLocalizedText("MainUI", "ui_page_setting_appleid");

        facebookText.text = _facebookText;
#if UNITY_IOS
        googleBtn.gameObject.SetActive(false);
        gameCenterBtn.gameObject.SetActive(true);
        appleBtn.gameObject.SetActive(true);
        couponBtn.gameObject.SetActive(false);
        gameCenterText.text = _gameCenterText;
        appleText.text = _appleIdText;
#elif UNITY_ANDROID
        gameCenterBtn.gameObject.SetActive(false);
        appleBtn.gameObject.SetActive(false);
        googleBtn.gameObject.SetActive(true);
        couponBtn.gameObject.SetActive(true);
        googleText.text = _googleText;
#endif

        if (Account.IsLoggedInGameService() && Account.GameServiceAccountType != Account.HaeginAccountType.Guest) {
#if UNITY_IOS
            gameCenterText.text = _gameCenterText + "\n connected";
#elif UNITY_ANDROID
            googleText.text = _googleText + "\n connected";
            #endif
            gameCenterBtn.enabled = false;
            //gameCenterBtn.image.sprite = btnDisable;
        }

        if (Account.IsLoggedInFacebook()) {
            facebookText.text = _facebookText + "\n connected";
            facebookBtn.enabled = false;
            //facebookBtn.image.sprite = btnDisable;
        }
#if UNITY_IOS
        if(Account.IsSupportedAppleId() && Account.IsLoggedInAppleId()) {
            appleText.text = _appleIdText + "\n connected";
            appleBtn.enabled = false;
            //appleBtn.image.sprite = btnDisable;
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
        GameObject modal = MonoBehaviour.Instantiate(hideModal, canvas.transform);
		Account.LoginAccount(Account.HaeginAccountType.AppleGameCenter, accountDialog.OpenSelectDialog, (bool result, WebClient.AuthCode code, TimeSpan blockRemainTime, long blockSuid) => {
#if MDEBUG
            Debug.Log("LogintAccount  result=" + result + "    code=" + code + " blockSuid=" + blockSuid);
#endif
            MonoBehaviour.Destroy(modal);
            if (result && code == WebClient.AuthCode.SUCCESS)
                LoginComplete();
            else
                Modal.instantiate("로그인 과정에서 문제가 발생했습니다.", Modal.Type.CHECK);
        });
    }

    public void OnGoogleLoginButtonClick() {
        GameObject modal = MonoBehaviour.Instantiate(hideModal, canvas.transform);
        Account.LoginAccount(Account.HaeginAccountType.GooglePlayGameService, accountDialog.OpenSelectDialog, (bool result, WebClient.AuthCode code, TimeSpan blockRemainTime, long blockSuid) => {
#if MDEBUG
            Debug.Log("LogintAccount  result=" + result + "    code=" + code + " blockSuid=" + blockSuid);
#endif
            MonoBehaviour.Destroy(modal);
            if (result && code == WebClient.AuthCode.SUCCESS)
                LoginComplete();
            else
                Modal.instantiate("로그인 과정에서 문제가 발생했습니다.", Modal.Type.CHECK);
        });
    }

    public void OnSignInWithAppleButtonClick() {
        if(!Account.IsSupportedAppleId()) {
            Modal.instantiate("애플 로그인은 iOS 13 이상 버전에서만 지원합니다.", Modal.Type.CHECK);
            return;
        }
        GameObject modal = MonoBehaviour.Instantiate(hideModal, canvas.transform);
        Account.LoginAccount(Account.HaeginAccountType.AppleId, accountDialog.OpenSelectDialog, (bool result, WebClient.AuthCode code, TimeSpan blockRemainTime, long blockSuid) => {
#if MDEBUG
            Debug.Log("LogintAccount  result=" + result + "    code=" + code + "  blockSuid=" + blockSuid);
#endif
            MonoBehaviour.Destroy(modal);
            if (result && code == WebClient.AuthCode.SUCCESS)
                LoginComplete();
            else
                Modal.instantiate("로그인 과정에서 문제가 발생했습니다.", Modal.Type.CHECK);
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
            GameObject modal = MonoBehaviour.Instantiate(hideModal, canvas.transform);
            Account.LoginAccount(Account.HaeginAccountType.Facebook, accountDialog.OpenSelectDialog, (bool result, WebClient.AuthCode code, TimeSpan blockRemainTime, long blockSuid) =>
            {
#if MDEBUG
                Debug.Log("LogintAccount  result=" + result + "    code=" + code + "  blockSuid=" + blockSuid);
#endif
                MonoBehaviour.Destroy(modal);
                if (result && code == WebClient.AuthCode.SUCCESS)
                    LoginComplete();
                else
                    Modal.instantiate("로그인 과정에서 문제가 발생했습니다.", Modal.Type.CHECK);
            });
        }
    }

    public void LoginComplete() {
        webClient.Request(new IssueJWTReq());
    }
}