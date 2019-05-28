using dataModules;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;

/// <summary>
/// Unit Prefab에 부탁되는 Component
/// </summary>
public partial class Base_gain : SerializedMonoBehaviour {
    public Skill skillData;
    IngameEventHandler eventHandler;
    protected Dictionary<IngameEventHandler.EVENT_TYPE, UnityEvent> EventDelegates;
    List<UnityEvent> unityEvents;
    
    void Start() {
        eventHandler = PlayMangement.instance.EventHandler;

        RemoveListeners();
        AddListeners();

        Init();
    }

    public virtual void Init() { }

    private void OnEventOccured(Enum Event_Type, Component Sender, object Param) {
        var event_type = (IngameEventHandler.EVENT_TYPE)Event_Type;
        EventDelegates[event_type].Invoke();
    }
}

/// <summary>
/// 이벤트 리스너 등록/해제
/// </summary>
public partial class Base_gain {
    void AddListeners() {
        foreach (IngameEventHandler.EVENT_TYPE value in Enum.GetValues(typeof(IngameEventHandler.EVENT_TYPE))){
            eventHandler.AddListener(value, OnEventOccured);
            UnityEvent newEvent = new UnityEvent();
            unityEvents.Add(newEvent);
            EventDelegates[value] = newEvent;
        }
    }

    void AddListenersToDict(IngameEventHandler.EVENT_TYPE type) {
        foreach (IngameEventHandler.EVENT_TYPE value in Enum.GetValues(typeof(IngameEventHandler.EVENT_TYPE))) {
            eventHandler.RemoveListener(value, OnEventOccured);
        }
    }

    void RemoveListeners() {
        unityEvents.Clear();
    }
}