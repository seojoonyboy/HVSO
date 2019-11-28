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
    public UnityEvent OnInvenLoadFinished = new UnityEvent();
    public UnityEvent OnTemplateLoadFinished = new UnityEvent();

    void Awake() {
        accountManager = AccountManager.Instance;

        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_HUMAN_TEMPLATES_UPDATED, OnHumanTemplateLoadFinished);
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_ORC_TEMPLATES_UPDATED, OnOrcTemplateLoadFinished);
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_INVENTORIES_UPDATED, OnInventoryLoadFinished);
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_DECKS_UPDATED, OnMyDecksLoadFinished);
    }

    void OnDestroy() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_HUMAN_TEMPLATES_UPDATED, OnHumanTemplateLoadFinished);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_ORC_TEMPLATES_UPDATED, OnOrcTemplateLoadFinished);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_INVENTORIES_UPDATED, OnInventoryLoadFinished);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_DECKS_UPDATED, OnMyDecksLoadFinished);
    }

    private void OnMyDecksLoadFinished(Enum Event_Type, Component Sender, object Param) {
        OnLoadFinished.Invoke();
    }

    private void OnHumanTemplateLoadFinished(Enum Event_Type, Component Sender, object Param) {
        HTTPResponse res = (HTTPResponse)Param;

        var result = JsonReader.Read<List<Templates>>(res.DataAsText);
        accountManager.humanTemplates = result;
    }

    private void OnInventoryLoadFinished(Enum Event_Type, Component Sender, object Param) {
        OnInvenLoadFinished.Invoke();
    }

    private void OnOrcTemplateLoadFinished(Enum Event_Type, Component Sender, object Param) {
        HTTPResponse res = (HTTPResponse)Param;

        var result = JsonReader.Read<List<Templates>>(res.DataAsText);
        accountManager.orcTemplates = result;
        OnTemplateLoadFinished.Invoke();
    }

    /// <summary>
    /// 내 덱 정보 불러오기
    /// </summary>
    /// <param name="humanDecks">불러온 휴먼 덱 정보를 저장할 타겟 변수</param>
    /// <param name="orcDecks">불러온 오크 덱 정보를 저장할 타겟 변수</param>
    public void Load() {
        accountManager.LoadAllCards();
        accountManager.LoadAllHeroes();
        accountManager.RequestMyDecks();
        accountManager.RequestHumanTemplates();
        accountManager.RequestOrcTemplates();
        accountManager.RequestInventories();
        accountManager.RequestClearedStoryList();
        accountManager.RequestShopItems();
    }
}
