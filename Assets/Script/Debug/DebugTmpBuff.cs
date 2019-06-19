using System.Collections;
using System;
using UnityEngine;
using System.Linq;

public class DebugTmpBuff : MonoBehaviour
{
    IngameEventHandler eventHandler;
    private void Start() {
        eventHandler = DebugManagement.Instance.EventHandler;

        RemoveListener();
        AddListener();

        SearchBackLine();
    }

    void AddListener() {
        eventHandler.AddListener(IngameEventHandler.EVENT_TYPE.END_CARD_PLAY, OnTriggerEvent);
    }

    private void OnTriggerEvent(Enum Event_Type, Component Sender, object Param) {
        //뒤쪽 라인 아군 검사
        SearchBackLine();
    }

    void SearchBackLine() {
        bool isPlayer = GetComponent<DebugUnit>().isPlayer;

        FieldUnitsObserver fieldUnitsObserver;
        if (isPlayer) {
            fieldUnitsObserver = DebugManagement.Instance.PlayerUnitsObserver;
        }
        else {
            fieldUnitsObserver = DebugManagement.Instance.EnemyUnitsObserver;
        }

        var pos = fieldUnitsObserver.GetMyPos(gameObject);
        var selectedUnits = fieldUnitsObserver.GetAllFieldUnits(pos.row);

        foreach (GameObject unit in selectedUnits) {
            var categories = unit.GetComponent<DebugUnit>().unit.cardCategories.ToList();
            if (categories.Contains("army")) {
                if (!unit.GetComponent<DebugUnit>().IsBuffAlreadyExist(gameObject)) {
                    unit.GetComponent<DebugUnit>()
                        .AddBuff(new DebugUnit.Buff(gameObject, 2, 0));
                    Debug.Log("후방 아군에게 버프 부여");
                }
            }
        }
    }

    void OnDestroy() {
        RemoveListener();

        bool isPlayer = GetComponent<DebugUnit>().isPlayer;

        DebugFieldObserver fieldUnitsObserver;
        if (isPlayer) {
            fieldUnitsObserver = DebugManagement.Instance.PlayerUnitsObserver;
        }
        else {
            fieldUnitsObserver = DebugManagement.Instance.EnemyUnitsObserver;
        }

        var pos = fieldUnitsObserver.GetMyPos(gameObject);
        var selectedUnits = fieldUnitsObserver.GetAllFieldUnits(pos.row);

        foreach (GameObject unit in selectedUnits) {
            var categories = unit.GetComponent<DebugUnit>().unit.cardCategories.ToList();
            if (categories.Contains("army")) {
                unit.GetComponent<DebugUnit>().RemoveBuff(gameObject);
                Debug.Log("후방 버프 해제");
            }
        }
    }

    void RemoveListener() {
        eventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.END_CARD_PLAY, OnTriggerEvent);
    }
}
