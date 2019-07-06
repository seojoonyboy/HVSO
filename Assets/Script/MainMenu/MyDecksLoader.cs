using System;
using BestHTTP;
using dataModules;
using UnityEngine;
using UnityEngine.Events;

public class MyDecksLoader : MonoBehaviour {
    GameObject loadingModal;
    AccountManager accountManager;
    public UnityEvent OnLoadFinished = new UnityEvent();
    void Awake() {
        accountManager = AccountManager.Instance;    
    }
    /// <summary>
    /// 내 덱 정보 불러오기
    /// </summary>
    /// <param name="humanDecks">불러온 휴먼 덱 정보를 저장할 타겟 변수</param>
    /// <param name="orcDecks">불러온 오크 덱 정보를 저장할 타겟 변수</param>
    public void Load() {
        accountManager.RequestHumanDecks(OnHumanDeckLoadFinished);
        accountManager.RequestOrcDecks(OnOrcDeckLoadFinished);
    }

    private void OnHumanDeckLoadFinished(HTTPRequest originalRequest, HTTPResponse response) {
        accountManager.humanDecks = JsonReader.Read<HumanDecks>(response.DataAsText);
    }

    private void OnOrcDeckLoadFinished(HTTPRequest originalRequest, HTTPResponse response) {
        accountManager.orcDecks = JsonReader.Read<OrcDecks>(response.DataAsText);
        OnLoadFinished.Invoke();

        //accountManager.AddDummyCustomDeck();
    }
}
