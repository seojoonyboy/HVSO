using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TurnMachine : MonoBehaviour {
    public UnityEvent onTurnChanged;
    public UnityEvent onPrepareTurn;
    IngameEventHandler eventHandler;
    private PlayerController player;
    private int index = -1;
    TurnType turn;
    void Awake() {
    }

    void Start() {
        eventHandler = PlayMangement.instance.EventHandler;
        eventHandler.AddListener(IngameEventHandler.EVENT_TYPE.END_TURN_BTN_CLICKED, OnEndTurnBtnClicked);
        player = PlayMangement.instance.player;
        OnPrepareTurn();
    }

    private void OnEndTurnBtnClicked(Enum Event_Type, Component Sender, object Param) {
        if(Param != null)
            if((TurnType)Param != turn) 
                return;
        Debug.Log("same thing");
        NextTurn();
    }

    private void NextTurn() {
        turn = (TurnType)((++index) % 4);
        StartCoroutine(InvokeTurnChanged());
        Debug.Log(turn.ToString());
        if(index != 0) StartCoroutine(PlayNextTurnSound());
    }

    private IEnumerator PlayNextTurnSound() {
        yield return new WaitForSeconds(1.0f);
        SoundManager.Instance.PlaySound(SoundType.NEXT_TURN);
    }

    private IEnumerator InvokeTurnChanged() {
        yield return new WaitForSeconds(1.0f);
        onTurnChanged.Invoke();
    }

    private void OnPrepareTurn() {
        onPrepareTurn.Invoke();
        Logger.Log("준비 턴");
    }

    public TurnType CurrentTurn() {
        return turn;
    }

    public bool isPlayerTurn() {
        int num = (int)turn;
        if(player.isHuman)
            return num == 1;
        else
            return num == 0 || num == 2;
    }
}

public enum TurnType {
    ORC = 0,
    HUMAN = 1,
    SECRET = 2,
    BATTLE = 3
}
