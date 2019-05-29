using dataModules;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using TMPro;
/// <summary>
/// Unit Prefab에 부탁되는 Component
/// </summary>
public partial class Base_gain : SerializedMonoBehaviour {
    public Effect effectData;
    public Skill totalData;
    public Condition condition;

    IngameEventHandler eventHandler;
    protected Dictionary<IngameEventHandler.EVENT_TYPE, UnityEvent> EventDelegates;
    List<UnityEvent> unityEvents = new List<UnityEvent>();

    List<GameObject> fieldUnits = new List<GameObject>();
    protected FieldUnitsObserver 
        playerUnitsObserver, 
        enemyUnitsObserver;

    void Start() {
        eventHandler = PlayMangement.instance.EventHandler;

        RemoveListeners();
        AddListeners();

        GetObservers();
        Init();
    }

    public virtual void Init() {
        fieldUnits = playerUnitsObserver.GetAllFieldUnits();
        var self = fieldUnits.Find(x => x == gameObject);
        fieldUnits.Remove(self);

        if(fieldUnits.Count > 0) {
            foreach (GameObject unit in fieldUnits) {
                unit.transform.Find("ClickableUI").gameObject.SetActive(true);
            }
            GameObject blockPanel = PlayMangement.instance.blockPanel;
            blockPanel.SetActive(true);
            blockPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "부여 대상을 지정하세요";
        }
    }

    private void GetObservers() {
        playerUnitsObserver = PlayMangement.instance.PlayerUnitsObserver;
        enemyUnitsObserver = PlayMangement.instance.EnemyUnitsObserver;
    }

    private void OnEventOccured(Enum Event_Type, Component Sender, object Param) {
        var event_type = (IngameEventHandler.EVENT_TYPE)Event_Type;
        EventDelegates[event_type].Invoke();
    }

    void Update() {
        GetMouseButtonDownEvent();
    }

    public virtual void GetMouseButtonDownEvent() {
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

                    //Debug.Log(hit.collider.gameObject.name + "에게 " + atkBuff + "," + hpBuff + "부여");

                    PlaceMonster placeMonster = hit.collider.gameObject.GetComponent<PlaceMonster>();
                    if (placeMonster != null) {
                        placeMonster.RequestChangeStat(atkBuff, hpBuff);
                    }
                    OffUI();
                }
            }
        }
    }

    public void SetMyActivateCondition(string keyword) {
        foreach(Condition condition in totalData.activate.conditions) {
            if (condition.method == keyword) this.condition = condition;
        }
    }

    private void OffUI() {
        foreach (GameObject unit in fieldUnits) {
            unit.transform.Find("ClickableUI").gameObject.SetActive(false);
        }

        PlayMangement.instance.blockPanel.SetActive(false);
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