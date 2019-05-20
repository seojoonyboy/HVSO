using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class LoginController : MonoBehaviour {
    NetworkManager networkManager;
    GameObject loadingModal;
    // Start is called before the first frame update
    void Start() {
        networkManager = NetworkManager.Instance;
    }

    // Update is called once per frame
    void Update() {

    }

    public void OnStartButton() {
        AccountManager.Instance.RequestUserInfo(OnResult, OnRetry);
        loadingModal = LoadingModal.instantiate();
        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
    }

    private void OnRetry(string msg) {
        loadingModal.transform.GetChild(0).GetComponent<UIModule.LoadingTextEffect>().AddAdditionalMsg(msg);
    }

    private void OnResult(HttpResponse response) {
        //Debug.Log("요청 최종 응답");
        if(response.responseCode != 200) {
            if (!response.request.isNetworkError) {
                OnSignUpModal();
            }
            else {
                OccurErrorModal(response.responseCode);
            }
        }
        else {
            OnSignInModal();
        }

        Destroy(loadingModal);
    }

    public void OnSignInModal() {
        Destroy(loadingModal);
        Modal.instantiate("로그인이 되었습니다.", Modal.Type.CHECK, ()=> {
            //SceneManager.Instance.LoadScene(SceneManager.Scene.MAIN_SCENE);
            AccountManager.Instance.RequestMyCardInventory();
        });
    }

    public void OnSignUpModal() {
        Destroy(loadingModal);
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

    private void OccurErrorModal(long errorCode) {
        Modal.instantiate("네트워크 오류가 발생하였습니다. 다시 시도해 주세요.", Modal.Type.CHECK);
    }
}
