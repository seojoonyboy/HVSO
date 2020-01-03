using System;
using System.Collections;
using BestHTTP;
using Spine.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginController : MonoBehaviour {
    NetworkManager networkManager;
    GameObject loadingModal;
    [SerializeField] GameObject logo, textImage;
    [SerializeField] Button loginBtn;
    [SerializeField] GameObject skipbuttons, mmrchange;
    [SerializeField] TMPro.TMP_InputField mmrInputField, rankIdInputField;

    public GameObject obbCanvas, sceneStartCanvas, sceneLoginCanvas, LoginTypeSelCanvas, EULACanvas, fbl_loginCanvas;

    bool isClicked = false;

    private void Awake() {
        AccountManager.Instance.tokenSetFinished += OnTokenSetFinished;
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_UPDATED, OnRequestUserInfoCallback);
//#if UNITY_EDITOR
        skipbuttons.SetActive(true);
//#endif
        mmrchange.SetActive(true);
    }

    private void OnTokenSetFinished() {
        loginBtn.enabled = true;
    }

    public void Login() {
        AccountManager.Instance.prevSceneName = "Login";

        networkManager = NetworkManager.Instance;
        StartCoroutine(LogoReveal());
    }

    private void OnRequestUserInfoCallback(Enum Event_Type, Component Sender, object Param) {
        AccountManager accountManager = AccountManager.Instance;

        HTTPResponse res = (HTTPResponse)Param;

        if (res.IsSuccess) {
            if (accountManager.userData.preSupply < 200 && accountManager.userData.supplyTimeRemain > 0) {
                Invoke("ReqInTimer", (float)accountManager.userData.supplyTimeRemain);
            }
            else {
                Logger.Log("Pre Supply가 가득찼습니다. Timer를 호출하지 않습니다.");
            }

            if (PlayerPrefs.GetInt("isFirst", 2) == 2) {
                LoginTypeSelCanvas.SetActive(true);
            }
            else {
                accountManager.OnSignInResultModal();
            }
        }
        else {
            isClicked = false;
        }
        
    }

    IEnumerator LogoReveal() {
        yield return new WaitForSeconds(0.8f);
        logo.SetActive(true);
        //Logger.Log("logo");
        yield return new WaitForSeconds(0.5f);
        //Logger.Log("textImage");
        textImage.SetActive(true);
        //loginBtn.enabled = true;
        SkeletonGraphic skeletonGraphic = logo.GetComponent<SkeletonGraphic>();
        Spine.AnimationState state = skeletonGraphic.AnimationState;
        state.SetAnimation(0, "loop", true);
        isClicked = false;
        IAPSetup.Instance.Init();
        //CustomVibrate.Vibrate(1000);
    }

    public void OnStartButton() {
        if (!isClicked) {
            AccountManager.Instance.RequestUserInfo();
            SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
        }
        isClicked = true;
    }

    public void OnGuestLoginButtonClick(string param) {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("Guest_Login");
        FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);
    }

    private void OnRequestUserInfoCallback(HTTPRequest originalRequest, HTTPResponse response) {
        var sceneStartController = GetComponent<SceneStartController>();
        AccountManager accountManager = AccountManager.Instance;
        if (response.StatusCode == 200 || response.StatusCode == 304) {
            accountManager.SetSignInData(response);
            if (accountManager.userData.preSupply < 200 && accountManager.userData.supplyTimeRemain > 0) {
                Invoke("ReqInTimer", (float)accountManager.userData.supplyTimeRemain);
            }
            else {
                Logger.Log("Pre Supply가 가득찼습니다. Timer를 호출하지 않습니다.");
            }

            if (PlayerPrefs.GetInt("isFirst") == 1) {
                sceneStartController
                    .LoginTypeCanvas
                    .gameObject
                    .SetActive(true);
            }
            else {
                accountManager.OnSignInResultModal();
            }
        }
        else {
            isClicked = false;
        }
    }

    private void ReqInTimer() {
        AccountManager.Instance.ReqInTimer(AccountManager.Instance.GetRemainSupplySec());
    }

    public void SkipStory(int type) {
        AccountManager accountManager = AccountManager.Instance;
        if(type == 0) {
            accountManager.SkipStoryRequest("human", 1);
        }
        else if(type == 1) {
            accountManager.SkipStoryRequest("human", 1);
            accountManager.SkipStoryRequest("orc", 1);
        }
        else if(type == 2) {
            accountManager.SkipStoryRequest("human", 1);
            accountManager.SkipStoryRequest("orc", 1);
            accountManager.SkipStoryRequest("human", 2);
            accountManager.SkipStoryRequest("orc", 2);
        }
    }

    public void ChangeMMR() {
        //Modal.instantiate(mmrInputField.text, Modal.Type.CHECK);
        int value = 0;
        int value2 = 0;
        int.TryParse(mmrInputField.text, out value);
        int.TryParse(rankIdInputField.text, out value2);

        AccountManager.Instance.RequestChangeMMRForTest(value, 16 - value2);
    }

    public void ChangeDefaultRank() {
        int value = 0;
        int.TryParse(mmrInputField.text, out value);

        if (value >= 0 && value <= 149) rankIdInputField.text = "15";
        else if (value >= 150 && value <= 299) rankIdInputField.text = "14";
        else if (value >= 300 && value <= 449) rankIdInputField.text = "13";
        else if (value >= 450 && value <= 599) rankIdInputField.text = "12";
        else if(value >= 600 && value <= 799) rankIdInputField.text = "11";
        else if (value >= 800 && value <= 999) rankIdInputField.text = "10";
        else if (value >= 1000 && value <= 1199) rankIdInputField.text = "9";
        else if (value >= 1200 && value <= 1399) rankIdInputField.text = "8";
        else if (value >= 1400 && value <= 1699) rankIdInputField.text = "7";
        else if (value >= 1700 && value <= 1999) rankIdInputField.text = "6";
        else if (value >= 2000 && value <= 2299) rankIdInputField.text = "5";
        else if (value >= 2300 && value <= 2599) rankIdInputField.text = "4";
        else if (value >= 2600 && value <= 2999) rankIdInputField.text = "3";
        else rankIdInputField.text = "2";
    }
}
