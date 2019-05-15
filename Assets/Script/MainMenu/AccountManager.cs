using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class AccountManager : Singleton<AccountManager> {
    protected AccountManager() { }

    public delegate void Callback();
    private Callback callback = null;

    public string DEVICEID { get; private set; }
    public UserClassInput userData { get; private set; }

    NetworkManager networkManager;

    private void Awake() {
        DEVICEID = SystemInfo.deviceUniqueIdentifier;
    }

    // Start is called before the first frame update
    void Start() {
        networkManager = NetworkManager.Instance;
    }

    // Update is called once per frame
    void Update() {

    }

    public void isUserExist() {
        RequestUserInfo();
    }

    public void SignUp(string inputText) {
        UserClassInput userInfo = new UserClassInput();
        userInfo.nickName = inputText;
        userInfo.deviceId = DEVICEID;

        string json = JsonUtility.ToJson(userInfo);
        StringBuilder url = new StringBuilder();

        url.Append(networkManager.baseUrl)
            .Append("api/users");
        networkManager.request("PUT", url.ToString(), json, CallbackSignUp, false);
    }

    private void RequestUserInfo() {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/users/")
            .Append(DEVICEID);

        networkManager.request("GET", url.ToString(), CallbackUserRequest, false);
    }

    private void CallbackUserRequest(HttpResponse response) {
        if (response.responseCode != 200) {
            Debug.Log(
                response.responseCode 
                + "에러\n"
                + response.errorMessage);
        }
        else {
            if(response.data == "null") {
                transform
                .GetChild(0)
                .GetComponent<LoginController>()
                .OnSignUpModal();
            }
            else {
                transform
                .GetChild(0)
                .GetComponent<LoginController>()
                .OnSignInModal();
            }
        }
    }

    private void CallbackSignUp(HttpResponse response) {
        if (response.responseCode != 200) {
            Debug.Log(
                response.responseCode
                + "에러\n"
                + response.errorMessage);
        }
        else {
            userData = dataModules.JsonReader.Read<UserClassInput>(response.data);

            Modal.instantiate("회원가입이 완료되었습니다.", Modal.Type.CHECK, () => {
                SceneManager.Instance.LoadScene(SceneManager.Scene.MAIN_SCENE);
            });
        }
    }

    public class UserClassInput {
        public string nickName;
        public string deviceId;
    }
}
