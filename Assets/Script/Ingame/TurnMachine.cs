using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TurnMachine : MonoBehaviour {
    public UnityEvent onTurnChanged;
    public UnityEvent onPrepareTurn;
    IngameEventHandler eventHandler;
    private int index = -1;
    TurnType turn;

    void Awake() {
        eventHandler = PlayMangement.instance.EventHandler;
        eventHandler.AddListener(IngameEventHandler.EVENT_TYPE.END_TURN_BTN_CLICKED, OnEndTurnBtnClicked);
    }

    void Start() {
        OnPrepareTurn();
    }

    private void OnEndTurnBtnClicked(Enum Event_Type, Component Sender, object Param) {
        NextTurn();
    }

    private void NextTurn() {
        turn = (TurnType)((++index) % 4);
        StartCoroutine(InvokeTurnChanged());
        Debug.Log(turn.ToString());
    }

    private IEnumerator InvokeTurnChanged() {
        yield return new WaitForSeconds(1.0f);
        onTurnChanged.Invoke();
    }

    private void OnPrepareTurn() {
        onPrepareTurn.Invoke();
        Logger.Log("준비 턴");
    }

    public string CurrentTurn() {
        return turn.ToString();
    }

    public enum TurnType {
        ORC = 0,
        HUMAN = 1,
        SECRET = 2,
        BATTLE = 3
    }
}
