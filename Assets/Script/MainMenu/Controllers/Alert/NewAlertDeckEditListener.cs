using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewAlertDeckEditListener : NewAlertListenerBase {
    protected override void Awake() {
        base.Awake();
        alertManager = NewAlertManager.Instance;
    }
    
    public override void AddListener() {
        //eventHandler.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_TIERUP_HERO, CheckCondition);
    }

    public override void RemoveListener() {
        //eventHandler.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_TIERUP_HERO, CheckCondition);
    }

    private void CheckCondition(Enum Event_Type, Component Sender, object Param) {
        object[] parms = (object[])Param;
        string tierUpHeroId = (string)parms[1];

        NewAlertManager alertManager = NewAlertManager.Instance;

        alertManager.SetUpButtonToAlert(
            gameObject,
            NewAlertManager.ButtonName.DECK_EDIT, 
            false
        );

        alertManager.SetUpButtonToUnlockCondition(
            NewAlertManager.ButtonName.DECK_EDIT, 
            tierUpHeroId
        );
    }
}
