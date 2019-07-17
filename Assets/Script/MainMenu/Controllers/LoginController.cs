using System;
using BestHTTP;
using UnityEngine;

public class LoginController : MonoBehaviour {
    NetworkManager networkManager;
    GameObject loadingModal;
    // Start is called before the first frame update
    void Start() {
        networkManager = NetworkManager.Instance;
    }

    public void OnStartButton() {
        AccountManager.Instance.AuthUser(CheckTokenCallback);
        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
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
        if (response.StatusCode == 200 || response.StatusCode == 304) {
            AccountManager.Instance.SetSignInData(response);
            AccountManager.Instance.OnSignInResultModal();
        }
        else {
            AccountManager.Instance.OnSignUpModal();
        }
    }

    private void OccurErrorModal(long errorCode) {
        Modal.instantiate("네트워크 오류가 발생하였습니다. 다시 시도해 주세요.", Modal.Type.CHECK);
    }
}
