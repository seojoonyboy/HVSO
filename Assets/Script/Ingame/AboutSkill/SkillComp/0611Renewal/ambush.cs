using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ambush : MonoBehaviour {
    IngameEventHandler eventHandler;
    DebugManagement debugManagement;
    // Start is called before the first frame update
    void Start() {
        eventHandler = PlayMangement.instance.EventHandler;
        GetComponent<PlaceMonster>().unitSpine.HideUnit();

        AddListener();
    }

    void AddListener() {
        eventHandler.AddListener(IngameEventHandler.EVENT_TYPE.BEGIN_ORC_POST_TURN, OnOrcPostTurn);
    }

    private void OnOrcPostTurn(Enum Event_Type, Component Sender, object Param) {
        //Debug.Log("잠복 해제");

        //if (debugManagement != null)
        //    GetComponent<DebugUnit>().unitSpine.DetectUnit();
        //else
        GetComponent<PlaceMonster>().unitSpine.DetectUnit();
        Destroy(GetComponent<ambush>());
    }

    void RemoveListener() {
        eventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.BEGIN_ORC_POST_TURN, OnOrcPostTurn);
    }

    void OnDestroy() {
        RemoveListener();
    }
}