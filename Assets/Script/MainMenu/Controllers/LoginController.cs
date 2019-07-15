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

    public void OnStartButton() {
        AccountManager.Instance.RequestUserInfo();
        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
    }

    private void OccurErrorModal(long errorCode) {
        Modal.instantiate("네트워크 오류가 발생하였습니다. 다시 시도해 주세요.", Modal.Type.CHECK);
    }
}
