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
    public bool turnStop = false;
    void Awake() {
    }

    void Start() {
        eventHandler = PlayMangement.instance.EventHandler;
        eventHandler.AddListener(IngameEventHandler.EVENT_TYPE.END_TURN_BTN_CLICKED, OnEndTurnBtnClicked);
        player = PlayMangement.instance.player;
    }

    private void OnEndTurnBtnClicked(Enum Event_Type, Component Sender, object Param) {
        if(Param != null)
            if((TurnType)Param != turn) 
                return;
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
        SoundManager.Instance.PlayIngameSfx(IngameSfxSound.TURNBUTTON);
    }

    private IEnumerator InvokeTurnChanged() {
        yield return new WaitForSeconds(1.0f);
        yield return StopInvokeTurn();
        onTurnChanged.Invoke();
    }

    private IEnumerator StopInvokeTurn() {
        yield return new WaitUntil(() => turnStop == false);
    }

    public void StartGame(GameObject orcPanel) {
        StartCoroutine(CheckStart(orcPanel));
    }

    private IEnumerator CheckStart(GameObject orcPanel) {
        PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_MULIGUN_CARD, this);
        yield return StopInvokeTurn();        
        orcPanel.SetActive(true);
        yield return new WaitForSeconds(1.0f);        
        orcPanel.SetActive(false);

        //SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
        SoundManager.Instance.PlayIngameSfx(IngameSfxSound.TURNSTART);
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
