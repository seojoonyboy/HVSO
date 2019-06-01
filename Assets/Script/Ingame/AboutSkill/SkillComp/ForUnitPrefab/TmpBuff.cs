using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 방패병을 위한 컴포넌트
/// </summary>
public class TmpBuff : MonoBehaviour {
    IngameEventHandler eventHandler;
    private void Start() {
        eventHandler = PlayMangement.instance.EventHandler;

        RemoveListener();
        AddListener();
    }

    void AddListener() {
        eventHandler.AddListener(IngameEventHandler.EVENT_TYPE.END_CARD_PLAY, OnTriggerEvent);
    }

    private void OnTriggerEvent(Enum Event_Type, Component Sender, object Param) {
        //뒤쪽 라인 아군 검사
        bool isPlayer = GetComponent<PlaceMonster>().isPlayer;

        FieldUnitsObserver fieldUnitsObserver;
        if (isPlayer) {
            fieldUnitsObserver = PlayMangement.instance.PlayerUnitsObserver;
        }
        else {
            fieldUnitsObserver = PlayMangement.instance.EnemyUnitsObserver;
        }

        var pos = fieldUnitsObserver.GetMyPos(gameObject);
        var selectedUnits = fieldUnitsObserver.GetAllFieldUnits(pos.row);

        foreach(GameObject unit in selectedUnits) {
            var categories = unit.GetComponent<PlaceMonster>().unit.cardCategories.ToList();
            if (categories.Contains("army")) {
                if (!unit.GetComponent<PlaceMonster>().IsBuffAlreadyExist(gameObject)) {
                    unit.GetComponent<PlaceMonster>()
                        .AddBuff(new PlaceMonster.Buff(gameObject, 3, 0));
                    Debug.Log("후방 아군에게 버프 부여");
                }
            }
        }
    }

    void OnDestroy() {
        bool isPlayer = GetComponent<PlaceMonster>().isPlayer;

        FieldUnitsObserver fieldUnitsObserver;
        if (isPlayer) {
            fieldUnitsObserver = PlayMangement.instance.PlayerUnitsObserver;
        }
        else {
            fieldUnitsObserver = PlayMangement.instance.EnemyUnitsObserver;
        }

        var pos = fieldUnitsObserver.GetMyPos(gameObject);
        var selectedUnits = fieldUnitsObserver.GetAllFieldUnits(pos.row);

        foreach (GameObject unit in selectedUnits) {
            var categories = unit.GetComponent<PlaceMonster>().unit.cardCategories.ToList();
            if (categories.Contains("army")) {
                unit.GetComponent<PlaceMonster>().RemoveBuff(gameObject);
                Debug.Log("후방 버프 해제");
            }
        }
    }

    void RemoveListener() {
        eventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.END_CARD_PLAY, OnTriggerEvent);
    }
}
