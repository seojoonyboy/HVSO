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
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_INVENTORIES_UPDATED, OnInventoryLoadFinished);
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_DECKS_UPDATED, OnMyDecksLoadFinished);
    }

    void OnDestroy() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_INVENTORIES_UPDATED, OnInventoryLoadFinished);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_DECKS_UPDATED, OnMyDecksLoadFinished);
    }

    private void OnMyDecksLoadFinished(Enum Event_Type, Component Sender, object Param) {
        OnLoadFinished.Invoke();
    }


    private void OnInventoryLoadFinished(Enum Event_Type, Component Sender, object Param) {
        OnInvenLoadFinished.Invoke();
    }

    /// <summary>
    /// 내 덱 정보 불러오기
    /// </summary>
    /// <param name="humanDecks">불러온 휴먼 덱 정보를 저장할 타겟 변수</param>
    /// <param name="orcDecks">불러온 오크 덱 정보를 저장할 타겟 변수</param>
    public void Load() {
        accountManager.RequestInventories();
        accountManager.RequestMyDecks();
        accountManager.RequestHumanTemplates();
        accountManager.RequestOrcTemplates();
        accountManager.RequestClearedStoryList();
        accountManager.RequestShopItems();
        accountManager.RequestLeagueInfo();
    }
}
