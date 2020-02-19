using System;
using UnityEngine;

public class ambush : UnitAttribute {
    IngameEventHandler eventHandler;
    // Start is called before the first frame update
    void Start() {
        eventHandler = PlayMangement.instance.EventHandler;
        GetComponent<PlaceMonster>().SetHiding();
        GetComponent<PlaceMonster>().HideUnit();

        AddListener();
    }

    void AddListener() {
        eventHandler.AddListener(IngameEventHandler.EVENT_TYPE.BEGIN_ORC_POST_TURN, OnOrcPostTurn);
    }

    private void OnOrcPostTurn(Enum Event_Type, Component Sender, object Param) {
        GetComponent<PlaceMonster>().DetectUnit();
    }

    void RemoveListener() {
        eventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.BEGIN_ORC_POST_TURN, OnOrcPostTurn);
    }

    void OnDestroy() {
        RemoveListener();
    }
}