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
        FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);
    }

    private void OnRequestUserInfoCallback(HTTPRequest originalRequest, HTTPResponse response) {
        var sceneStartController = GetComponent<SceneStartController>();
        if (response.StatusCode == 200 || response.StatusCode == 304) {
            AccountManager.Instance.SetSignInData(response);
            //AccountManager.Instance.OnSignInResultModal();

            if (PlayerPrefs.GetInt("isFirst") == 1) {
                sceneStartController
                    .LoginTypeCanvas
                    .gameObject
                    .SetActive(true);
            }
            else {
                AccountManager.Instance.OnSignInResultModal();
            }
        }
    }
}
