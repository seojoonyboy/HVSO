using System;
using System.Collections.Generic;
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
        accountManager.LoadAllCards();
        accountManager.RequestMyDecks(OnDeckLoadFinished);
        accountManager.RequestHumanTemplates(OnOrcTemplateLoadFinished);
        accountManager.RequestOrcTemplates(OnHumanTemplateLoadFinished);
    }

    private void OnDeckLoadFinished(HTTPRequest originalRequest, HTTPResponse response) {
        if(response != null) {
            if(response.StatusCode == 200 || response.StatusCode == 304) {
                var result = JsonReader.Read<Decks>(response.DataAsText);
                accountManager.orcDecks = result.orc;
                accountManager.humanDecks = result.human;
                OnLoadFinished.Invoke();
            }
        }
        else {
            Logger.Log("Something is wrong");
        }
    }

    private void OnOrcTemplateLoadFinished(HTTPRequest originalRequest, HTTPResponse response) {
        if (response.IsSuccess) {
            if(response.StatusCode == 200 || response.StatusCode == 304) {
                var result = JsonReader.Read<List<Templates>>(response.DataAsText);
                accountManager.orcTemplates = result;
            }
        }
    }

    private void OnHumanTemplateLoadFinished(HTTPRequest originalRequest, HTTPResponse response) {
        if (response.IsSuccess) {
            if (response.StatusCode == 200 || response.StatusCode == 304) {
                var result = JsonReader.Read<List<Templates>>(response.DataAsText);
                accountManager.humanTemplates = result;
            }
        }
    }
}
