using dataModules;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MyDecksLoader : MonoBehaviour {
    GameObject loadingModal;
    public UnityEvent OnLoadFinished = new UnityEvent();

    /// <summary>
    /// 내 덱 정보 불러오기
    /// </summary>
    /// <param name="humanDecks">불러온 휴먼 덱 정보를 저장할 타겟 변수</param>
    /// <param name="orcDecks">불러온 오크 덱 정보를 저장할 타겟 변수</param>
    public void Load() {
        AccountManager.Instance.RequestHumanDecks(OnReqHumanDecks, OnRetryReq);
        loadingModal = LoadingModal.instantiate();
        loadingModal.transform.Find("Panel/AdditionalMessage").GetComponent<Text>().text = "덱 정보를 불러오는중...Human Decks";
    }

    private void OnRetryReq(string msg) {
        loadingModal.transform.GetChild(0).GetComponent<UIModule.LoadingTextEffect>().AddAdditionalMsg(msg);
    }

    private void OnReqHumanDecks(HttpResponse response) {
        AccountManager accountManager = AccountManager.Instance;
        if (response.responseCode == 200) {
            accountManager.humanDecks = JsonReader.Read<HumanDecks>(response.data);

            accountManager.RequestOrcDecks(OnReqOrcDecks, OnRetryReq);
            loadingModal.transform.Find("Panel/AdditionalMessage").GetComponent<Text>().text = "덱 정보를 불러오는중...Orc Decks";
        }
        else {
            Modal.instantiate("데이터를 정상적으로 불러오지 못했습니다.\n다시 요청합니까?", Modal.Type.YESNO, () => {
                Load();
            });
        }
    }

    private void OnReqOrcDecks(HttpResponse response) {
        AccountManager accountManager = AccountManager.Instance;
        Destroy(loadingModal);

        if (response.responseCode == 200) {
            accountManager.orcDecks = JsonReader.Read<OrcDecks>(response.data);
            OnLoadFinished.Invoke();
        }
        else {
            Modal.instantiate("데이터를 정상적으로 불러오지 못했습니다.\n다시 요청합니까?", Modal.Type.YESNO, () => {
                Load();
            });
        }
    }
}
