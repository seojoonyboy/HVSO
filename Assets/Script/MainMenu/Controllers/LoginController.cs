using System;
using BestHTTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginController : MonoBehaviour {
    NetworkManager networkManager;
    GameObject loadingModal;
    // Start is called before the first frame update
    void Start() {
        networkManager = NetworkManager.Instance;
    }

    public void OnStartButton() {
        AccountManager.Instance.RequestUserInfo(OnRequestUserInfoCallback);
        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
    }

    public void OnGuestLoginButtonClick(string param) {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("Guest_Login");
        SceneManager.LoadScene("Login", LoadSceneMode.Single);
    }

    private void CheckTokenCallback(HTTPRequest originalRequest, HTTPResponse response) {
        if(response != null) {
            if (response.IsSuccess) {
                AccountManager.Instance.SetUserToken(response);
                AccountManager.Instance.RequestUserInfo(OnRequestUserInfoCallback);
            }
            else {
                if (response.DataAsText.Contains("no_user")) {
                    AccountManager.Instance.OnSignUpModal();
                }
                Logger.Log(response.DataAsText);
            }
        }
    }

    private void OnRequestUserInfoCallback(HTTPRequest originalRequest, HTTPResponse response) {
        var sceneStartController = GetComponent<SceneStartController>();
        if (response.StatusCode == 200 || response.StatusCode == 304) {
            AccountManager.Instance.SetSignInData(response);
            AccountManager.Instance.OnSignInResultModal();
        }
        else {
            //AccountManager.Instance.OnSignUpModal();
            sceneStartController
                .LoginTypeCanvas
                .gameObject
                .SetActive(true);
        }
    }

    public void OnSignUpModal() {
        AccountManager.Instance.OnSignUpModal();
    }
}
