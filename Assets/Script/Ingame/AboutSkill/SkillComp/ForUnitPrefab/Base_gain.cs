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
    public Effect effectData;
    IngameEventHandler eventHandler;
    protected Dictionary<IngameEventHandler.EVENT_TYPE, UnityEvent> EventDelegates;
    List<UnityEvent> unityEvents = new List<UnityEvent>();

    List<GameObject> fieldUnits = new List<GameObject>();
    void Start() {
        eventHandler = PlayMangement.instance.EventHandler;

        RemoveListeners();
        AddListeners();

        Init();
    }

    public virtual void Init() {
        fieldUnits = PlayMangement.instance.PlayerUnitsObserver.GetAllFieldUnits();
        var self = fieldUnits.Find(x => x == gameObject);
        fieldUnits.Remove(self);

        foreach(GameObject unit in fieldUnits) {
            unit.transform.Find("ClickableUI").gameObject.SetActive(true);
        }

        PlayMangement.instance.blockPanel.SetActive(true);
    }

    private void OnEventOccured(Enum Event_Type, Component Sender, object Param) {
        var event_type = (IngameEventHandler.EVENT_TYPE)Event_Type;
        EventDelegates[event_type].Invoke();
    }

    public virtual void Update() {
        if (Input.GetMouseButtonDown(0)) {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            LayerMask mask = (1 << LayerMask.NameToLayer("PlayerUnit")) | (1 << LayerMask.NameToLayer("EnemyUnit"));
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                new Vector2(mousePos.x, mousePos.y),
                Vector2.zero,
                Mathf.Infinity,
                mask
            );

            if (hits != null) {
                foreach (RaycastHit2D hit in hits) {
                    int atkBuff = 0;
                    int hpBuff = 0;

                    int.TryParse(effectData.args[0], out atkBuff);
                    int.TryParse(effectData.args[1], out hpBuff);

                    Debug.Log(hit.collider.gameObject.name + "에게 " + atkBuff + "," + hpBuff + "부여");
                    OffUI();
                }
            }
        }
    }

    private void OffUI() {
        foreach (GameObject unit in fieldUnits) {
            unit.transform.Find("ClickableUI").gameObject.SetActive(false);
        }
    }
}

/// <summary>
/// 이벤트 리스너 등록/해제
/// </summary>
public partial class Base_gain {
    void AddListeners() {
        EventDelegates = new Dictionary<IngameEventHandler.EVENT_TYPE, UnityEvent>();
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