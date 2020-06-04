using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MyLeagueInfoCanvasController : MonoBehaviour {
    [SerializeField] HUDController hudController;
    [SerializeField] BattleReadySceneController battleReadySceneController;

    NoneIngameSceneEventHandler eventHandler;
    AccountManager accountManager;
    // Start is called before the first frame update
    void Awake() {
        accountManager = AccountManager.Instance;
        eventHandler = NoneIngameSceneEventHandler.Instance;
    }

    void Start() {

    }

    void OnPanel() {
        gameObject.SetActive(true);
        if (BattleReadySceneController.instance != null && BattleReadySceneController.instance.gameObject.activeSelf) {
            EscapeKeyController.escapeKeyCtrl.AddEscape(OffPanelByBattleReady);
        }
        else {
            EscapeKeyController.escapeKeyCtrl.AddEscape(OffPanelByMain);
        }
    }

    public void OnPanelByMain() {
        hudController.SetHeader(HUDController.Type.RESOURCE_ONLY_WITH_BACKBUTTON);
        hudController.SetBackButton(() => {
            OffPanelByMain();
        });

        OnPanel();
    }

    public void OnPanelByBattleReady() {
        hudController.SetHeader(HUDController.Type.RESOURCE_ONLY_WITH_BACKBUTTON);
        hudController.SetBackButton(() => {
            OffPanelByBattleReady();
        });

        OnPanel();
    }

    public void OnPanelByUserInfo() {
        hudController.SetHeader(HUDController.Type.RESOURCE_ONLY_WITH_BACKBUTTON);
        hudController.SetBackButton(() => {
            OffPanelByUserInfo();
        });
        gameObject.SetActive(true);
        EscapeKeyController.escapeKeyCtrl.AddEscape(OffPanelByUserInfo);
    }

    void OffPanel() {
        gameObject.SetActive(false);
        AccountManager.Instance.RequestMailBoxNum();
        if (BattleReadySceneController.instance != null && BattleReadySceneController.instance.gameObject.activeSelf) {
            EscapeKeyController.escapeKeyCtrl.RemoveEscape(OffPanelByBattleReady);
        }
        else {
            EscapeKeyController.escapeKeyCtrl.RemoveEscape(OffPanelByMain);
        }
    }

    public void OffPanelByBattleReady() {
        hudController.SetBackButton(() => battleReadySceneController.OnBackButton());
        hudController.SetHeader(HUDController.Type.BATTLE_READY_CANVAS);
        OffPanel();
    }

    public void OffPanelByMain() {
        hudController.SetHeader(HUDController.Type.SHOW_USER_INFO);
        OffPanel();
    }

    public void OffPanelByUserInfo() {
        AccountManager.Instance.RequestMailBoxNum();
        hudController.SetHeader(HUDController.Type.RESOURCE_ONLY_WITH_BACKBUTTON);
        hudController.SetBackButton(() => hudController.CloseUserInfo());
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(OffPanelByUserInfo);
        gameObject.SetActive(false);
    }
}
