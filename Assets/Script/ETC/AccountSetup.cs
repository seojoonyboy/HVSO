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
    public GameObject hideModal;
    public Canvas canvas;
    public AccountDialogController accountDialog;

    public void Init() {
        ButtonInit();
        webClient = WebClient.GetInstance();
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
        Fbl_Translator translator = AccountManager.Instance.GetComponent<Fbl_Translator>();
        string connected = translator.GetLocalizedText("MainUI", "ui_page_setting_connected");
        if(string.IsNullOrEmpty(connected)) connected = "?????????";
        if (Account.IsLoggedInGameService() && Account.GameServiceAccountType != Account.HaeginAccountType.Guest) {
#if UNITY_IOS
            gameCenterText.text = connected;
            gameCenterBtn.enabled = false;
#elif UNITY_ANDROID
            googleText.text = connected;
            googleBtn.enabled = false;
#endif
            //gameCenterBtn.image.sprite = btnDisable;
        }

        if (Account.IsLoggedInFacebook()) {
            facebookText.text = connected;
            facebookBtn.enabled = false;
            //facebookBtn.image.sprite = btnDisable;
        }
#if UNITY_IOS
        if(Account.IsSupportedAppleId() && Account.IsLoggedInAppleId()) {
            appleText.text = connected;
            appleBtn.enabled = false;
            //appleBtn.image.sprite = btnDisable;
        }
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
                Modal.instantiate("????????? ???????????? ????????? ??????????????????.", Modal.Type.CHECK);
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
                Modal.instantiate("????????? ???????????? ????????? ??????????????????.", Modal.Type.CHECK);
        });
    }

    public void OnSignInWithAppleButtonClick() {
        if(!Account.IsSupportedAppleId()) {
            Modal.instantiate("?????? ???????????? iOS 13 ?????? ??????????????? ???????????????.", Modal.Type.CHECK);
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
                Modal.instantiate("????????? ???????????? ????????? ??????????????????.", Modal.Type.CHECK);
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
                    Modal.instantiate("????????? ???????????? ????????? ??????????????????.", Modal.Type.CHECK);
            });
        }
    }

    public void LoginComplete() {
        webClient.Request(new IssueJWTReq());
    }
}