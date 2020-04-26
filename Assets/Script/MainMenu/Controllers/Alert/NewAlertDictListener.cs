using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewAlertDictListener : NewAlertListenerBase {
    protected override void Awake() {
        base.Awake();
        alertManager = NewAlertManager.Instance;
    }

    // Start is called before the first frame update
    protected override void Start() {
        base.Start();
    }

    protected override void OnDestroy() {
        base.OnDestroy();
    }

    public override void AddListener() {
        eventHandler.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_INVENTORIES_UPDATED, CheckCondition);
        eventHandler.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_ADREWARD_CHEST, CheckCondition);
    }

    public override void RemoveListener() {
        eventHandler.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_INVENTORIES_UPDATED, CheckCondition);
        eventHandler.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_ADREWARD_CHEST, CheckCondition);
    }

    private void CheckCondition(Enum Event_Type, Component Sender, object Param) {
        CheckCondition();
    }

    private void CheckCondition() {
        return;
        AccountManager accountManager = AccountManager.Instance;
        var myHeroInventories = accountManager.myHeroInventories;
        int myCrystal = accountManager.userResource.crystal;

        foreach (KeyValuePair<string, dataModules.HeroInventory> keyValuePair in myHeroInventories) {
            if (myCrystal >= keyValuePair.Value.next_level.crystal) {

            }
        }
    }
}
