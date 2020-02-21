using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewAlertDictListener : NewAlertListenerBase {
    AccountManager accountManager;
    protected override void Awake() {
        base.Awake();
        accountManager = AccountManager.Instance;
    }

    protected override void Start() {
        base.Start();
    }

    protected override void OnDestroy() {
        base.OnDestroy();
    }

    public override void AddListener() {
        eventHandler.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_OPENBOX, OnBoxOpenRequest);
        eventHandler.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_ADREWARD_CHEST, SetAdReward);
        eventHandler.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_TIERUP_HERO, OnHeroAdded);
    }

    public override void RemoveListener() {
        eventHandler.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_OPENBOX, OnBoxOpenRequest);
        eventHandler.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_ADREWARD_CHEST, SetAdReward);
        eventHandler.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_TIERUP_HERO, OnHeroAdded);
    }

    private void SetAdReward(Enum Event_Type, Component Sender, object Param) {
        var adReward = accountManager.boxAdReward;
        if (adReward.type == "card" || adReward.type == "hero") SetAlert(adReward.item);
    }

    private void OnBoxOpenRequest(Enum Event_Type, Component Sender, object Param) {
        RewardClass[] rewardList = accountManager.rewardList;
        foreach(RewardClass reward in rewardList) {
            if(reward.type == "card" || reward.type == "hero") {
                SetAlert(reward.item);
                break;
            }
        }
    }

    private void SetAlert(string id) {
        alertManager
            .SetUpButtonToAlert(
                alertManager.referenceToInit[NewAlertManager.ButtonName.DICTIONARY],
                NewAlertManager.ButtonName.DICTIONARY,
                false
            );
        alertManager
            .SetUpButtonToUnlockCondition(
                NewAlertManager.ButtonName.DICTIONARY, 
                id
            );
    }

    private void OnHeroAdded(Enum Event_Type, Component Sender, object Param) {
        //SetAlert();
    }
}
