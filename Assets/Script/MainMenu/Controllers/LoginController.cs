using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class LoginController : MonoBehaviour {
    NetworkManager networkManager;
    // Start is called before the first frame update
    void Start() {
        networkManager = NetworkManager.Instance;
    }

    // Update is called once per frame
    void Update() {

    }

    public void OnStartButton() {
        AccountManager.Instance.isUserExist();
    }

    public void OnSignInModal() {
        Modal.instantiate("로그인 되었습니다.", Modal.Type.CHECK);
    }

    public void OnSignUpModal() {
        Modal.instantiate(
            "새로운 계정을 등록합니다.", 
            "닉네임을 입력하세요.",
            null,
            Modal.Type.INSERT, 
            SetUserReqData);
    }

    private void SetUserReqData(string inputText) {
        AccountManager.Instance.SignUp(inputText);
    }
}
