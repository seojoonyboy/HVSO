using System;
using System.Collections;
using System.Collections.Generic;
using dataModules;
using Tutorial;
using UnityEngine;

public class NewAlertStoryListener : NewAlertListenerBase {
    public override void AddListener() {
        // eventHandler.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_CLEARED_STAGE_UPDATED, CheckCondition);
    }

    public override void RemoveListener() {
        // eventHandler.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_CLEARED_STAGE_UPDATED, CheckCondition);
    }
    
    private void CheckCondition(Enum event_type, Component sender, object param) { }
}