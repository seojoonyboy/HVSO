using System;
using System.Collections;
using BestHTTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginController : MonoBehaviour {
    NetworkManager networkManager;
    GameObject loadingModal;
    [SerializeField] GameObject logo, textImage;
    [SerializeField] Button loginBtn;
    // Start is called before the first frame update
    void Start() {
        networkManager = NetworkManager.Instance;
        StartCoroutine(LogoReveal());
    }

    IEnumerator LogoReveal() {
        yield return new WaitForSeconds(4.0f);
        logo.SetActive(true);
        Logger.Log("logo");
        yield return new WaitForSeconds(0.5f);
        Logger.Log("textImage");
        textImage.SetActive(true);
        loginBtn.enabled = true;
    }

    public void OnStartButton() {
        AccountManager.Instance.RequestUserInfo(OnRequestUserInfoCallback);
        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
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
    }

    private void ReqInTimer() {
        AccountManager.Instance.ReqInTimer(AccountManager.Instance.GetRemainSupplySec());
    }
}
