using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankTableViewController : MonoBehaviour {
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] Transform content;
    [SerializeField] GameObject rankObject, myRankObject;
    [SerializeField] HUDController hudController;
    [SerializeField] MyLeagueInfoCanvasController myLeagueInfoCanvasController;

    AccountManager accountManager;
    NoneIngameSceneEventHandler eventHandler;
    void Awake() {
        accountManager = AccountManager.Instance;
        eventHandler = NoneIngameSceneEventHandler.Instance;
    }

    void Start() {
        //eventHandler.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_RANK_TABLE_RECEIVED, OnTableLoaded);
    }

    private void OnTableLoaded(Enum Event_Type, Component Sender, object Param) {

    }

    void OnEnable() {
        EscapeKeyController.escapeKeyCtrl.AddEscape(OnBackButton);
        hudController.SetHeader(HUDController.Type.ONLY_BAKCK_BUTTON);
        hudController.SetBackButton(() => {
            OnBackButton();
        });

        ClearList();
        MakeList();
    }

    void OnDisable() {
        eventHandler.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_RANK_TABLE_RECEIVED, OnTableLoaded);
    }

    void OnBackButton() {
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(OnBackButton);
        gameObject.SetActive(false);
    }

    void ClearList() {
        foreach(Transform child in content) {
            Destroy(child.gameObject);
        }
    }

    void MakeList() {
        string myRankName = accountManager.scriptable_leagueData.leagueInfo.rankDetail.minorRankName;
        var tableData = accountManager.rankTable;

        int index = 0;
        foreach(AccountManager.RankTableRow row in tableData) {
            if(row.minorRankName == myRankName) {
                GameObject rankObj = Instantiate(rankObject);
                rankObj.AddComponent<dataModules.RankTableRow>();
                rankObj.transform.SetParent(content, true);
                rankObj.name = "item_" + index;
            }
            else {
                GameObject myRankObj = Instantiate(myRankObject);
                myRankObj.AddComponent<dataModules.RankTableRow>();
                myRankObj.transform.SetParent(content, true);
                myRankObj.name = "item_" + index;
            }
            index++;
        }
    }
}
