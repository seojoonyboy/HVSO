using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using dataModules;
using System;
using SkillModules;
using System.Linq;

/// <summary>
/// 카드 고유 능력 처리에 대한 스크립트
/// </summary>
public partial class Ability : MonoBehaviour {
    public bool isPlayer = true;
    protected Skill skillData;

    public virtual void InitData(Skill data, bool isPlayer) {
        eventHandler = PlayMangement.instance.EventHandler;
        RemoveListeners();
        AddListeners();

        skillData = data;

        IngameEventHandler.EVENT_TYPE _eventType = ((IngameEventHandler.EVENT_TYPE)Enum.Parse(typeof(IngameEventHandler.EVENT_TYPE), skillData.activate.trigger.ToUpper()));
        EventDelegates[_eventType].AddListener(OnEventCallback);
        
        foreach(var condition in data.activate.conditions) {
            var newComp = gameObject.AddComponent(System.Type.GetType("SkillModules." + condition.method));

            if(newComp != null) {
                ((ConditionChecker)newComp).Init(data, condition, isPlayer);
            }
        }
    }

    protected virtual void OnEventCallback(object parm) {

    }

    public void OnPlayerUnitTargetUI() {
        FieldUnitsObserver fieldUnitsObserver;
        if (isPlayer) {
            fieldUnitsObserver = PlayMangement.instance.PlayerUnitsObserver;
        }
        else {
            fieldUnitsObserver = PlayMangement.instance.EnemyUnitsObserver;
        }

        var selectedUnits = fieldUnitsObserver.GetAllFieldUnits();

        foreach (GameObject unit in selectedUnits) {
            unit.transform.Find("ClickableUI").gameObject.SetActive(true);
        }
    }

    public void OnEnemyUnitTargetUI() {
        FieldUnitsObserver fieldUnitsObserver;
        if (isPlayer) {
            fieldUnitsObserver = PlayMangement.instance.EnemyUnitsObserver;
        }
        else {
            fieldUnitsObserver = PlayMangement.instance.PlayerUnitsObserver;
        }

        var selectedUnits = fieldUnitsObserver.GetAllFieldUnits();

        foreach (GameObject unit in selectedUnits) {
            unit.transform.Find("ClickableUI").gameObject.SetActive(true);
        }
    }

    public void OffPlayerUnitTargetUI() {
        FieldUnitsObserver fieldUnitsObserver;
        if (isPlayer) {
            fieldUnitsObserver = PlayMangement.instance.PlayerUnitsObserver;
        }
        else {
            fieldUnitsObserver = PlayMangement.instance.EnemyUnitsObserver;
        }

        var selectedUnits = fieldUnitsObserver.GetAllFieldUnits();

        foreach (GameObject unit in selectedUnits) {
            unit.transform.Find("ClickableUI").gameObject.SetActive(false);
        }
    }

    public void OffPlayerEnemyTargetUI() {
        FieldUnitsObserver fieldUnitsObserver;
        if (isPlayer) {
            fieldUnitsObserver = PlayMangement.instance.EnemyUnitsObserver;
        }
        else {
            fieldUnitsObserver = PlayMangement.instance.PlayerUnitsObserver;
        }

        var selectedUnits = fieldUnitsObserver.GetAllFieldUnits();

        foreach (GameObject unit in selectedUnits) {
            unit.transform.Find("ClickableUI").gameObject.SetActive(false);
        }
    }

    public void OnBeginDrag() {
        //마법 카드 조작중인 경우
        if(GetComponent<MagicDragHandler>() != null) {
            foreach(var target in skillData.targets) {
                List<string> args = target.args.ToList();
                if (args.Contains("my")) {
                    OnPlayerUnitTargetUI();
                }
                else if (args.Contains("enemy")) {
                    OnEnemyUnitTargetUI();
                }
            }
        }
    }

    public void OnEndDrag() {
        //마법 카드 조작중인 경우
        if (GetComponent<MagicDragHandler>() != null) {
            foreach (var target in skillData.targets) {
                List<string> args = target.args.ToList();
                if (args.Contains("my")) {
                    OffPlayerUnitTargetUI();
                }
                else if (args.Contains("enemy")) {
                    OffPlayerEnemyTargetUI();
                }
            }
        }
    }
}

/// <summary>
/// 이벤트 관련 처리
/// </summary>
public partial class Ability {
    IngameEventHandler eventHandler;
    protected Dictionary<IngameEventHandler.EVENT_TYPE, MyObjectEvent> EventDelegates;
    List<MyObjectEvent> unityEvents = new List<MyObjectEvent>();

    public class MyObjectEvent : UnityEvent<object> { }

    void AddListeners() {
        EventDelegates = new Dictionary<IngameEventHandler.EVENT_TYPE, MyObjectEvent>();
        foreach (IngameEventHandler.EVENT_TYPE value in Enum.GetValues(typeof(IngameEventHandler.EVENT_TYPE))) {
            eventHandler.AddListener(value, OnEventOccured);
            MyObjectEvent newEvent = new MyObjectEvent();
            unityEvents.Add(newEvent);
            EventDelegates[value] = newEvent;
        }
    }

    void OnEventOccured(Enum Event_Type, Component Sender, object Param) {
        var event_type = (IngameEventHandler.EVENT_TYPE)Event_Type;
        EventDelegates[event_type].Invoke(Param);
    }

    void AddListenersToDict(IngameEventHandler.EVENT_TYPE type) {
        foreach (IngameEventHandler.EVENT_TYPE value in Enum.GetValues(typeof(IngameEventHandler.EVENT_TYPE))) {
            eventHandler.RemoveListener(value, OnEventOccured);
        }
    }

    void RemoveListeners() {
        unityEvents.Clear();
    }

    void OnDestroy() {
        foreach (IngameEventHandler.EVENT_TYPE value in Enum.GetValues(typeof(IngameEventHandler.EVENT_TYPE))) {
            eventHandler.RemoveListener(value, OnEventOccured);
        }
    }
}