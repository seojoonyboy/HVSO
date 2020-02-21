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

    }

    public override void RemoveListener() {

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
