using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Spine.Unity;

public class TurnMachine : MonoBehaviour {
    [HideInInspector] public UnityEvent onTurnChanged;
    [SerializeField] private SkeletonGraphic playerMana, enemyMana;
    [SerializeField] private SkeletonGraphic turnSpine;
    private IngameEventHandler eventHandler;
    private PlayMangement playManagement;
    private PlayerController player;
    private PlayerController enemyPlayer;
    private int index = -1;
    TurnType turn;
    public bool turnStop = false;
    void Awake() {
        onTurnChanged.AddListener(ChangeTurn); //턴 넘김 버튼 누를때 발생하는 이벤트?
    }

    void Start() {
        playManagement = PlayMangement.instance;
        eventHandler = playManagement.EventHandler;
        eventHandler.AddListener(IngameEventHandler.EVENT_TYPE.END_TURN_BTN_CLICKED, OnEndTurnBtnClicked);
        player = playManagement.player;
        enemyPlayer = playManagement.enemyPlayer;

    }

    private void OnEndTurnBtnClicked(Enum Event_Type, Component Sender, object Param) {
        if(Param != null)
            if((TurnType)Param != turn) return;   
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

        switch ((int)turn) {
            case 0:
                SoundManager.Instance.PlayIngameSfx(IngameSfxSound.ORCTURN);
                break;
            case 1:
                SoundManager.Instance.PlayIngameSfx(IngameSfxSound.HUMANTURN);
                break;
            case 2:
                SoundManager.Instance.PlayIngameSfx(IngameSfxSound.ORCMAGICTURN);
                break;
            case 3:
                SoundManager.Instance.PlayIngameSfx(IngameSfxSound.TURNSTART);
                break;
        }
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
        playManagement.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_MULIGUN_CARD, this);
        yield return StopInvokeTurn();        
        orcPanel.SetActive(true);
        yield return new WaitForSeconds(1.0f);        
        orcPanel.SetActive(false);
        SoundManager.Instance.PlayIngameSfx(IngameSfxSound.ORCTURN);
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

    public void ChangeTurn() {
        player.buttonParticle.SetActive(false);
        switch (turn) {
            case TurnType.ORC:

                turnSpine.AnimationState.SetAnimation(0, "1.orc_attack", false);
                playerMana.AnimationState.SetAnimation(0, "animation", false);
                enemyMana.AnimationState.SetAnimation(0, "animation", false);
                if (player.isHuman == false) {
                    player.ActiveOrcTurn();
                    enemyPlayer.DisablePlayer();
                }
                else {
                    player.DisablePlayer();
                    enemyPlayer.ActivePlayer();
                    enemyPlayer.PlayerThinking();
                }
                eventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.BEGIN_ORC_PRE_TURN, this, null);
                break;

            case TurnType.HUMAN:
                turnSpine.AnimationState.SetAnimation(0, "2.human_attack", false);
                if (player.isHuman == true) {
                    player.ActivePlayer();
                    enemyPlayer.DisablePlayer();
                    enemyPlayer.PlayerThinkFinish();
                }
                else {
                    player.DisablePlayer();
                    enemyPlayer.ActivePlayer();
                    enemyPlayer.PlayerThinking();
                }
                eventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.BEGIN_HUMAN_TURN, this, null);
                break;

            case TurnType.SECRET:
                turnSpine.AnimationState.SetAnimation(0, "3.orc_trick", false);
                if (player.isHuman == false) {
                    //player.ActiveOrcSpecTurn();
                    player.ActiveOrcTurn();
                    enemyPlayer.DisablePlayer();
                    enemyPlayer.PlayerThinkFinish();
                }
                else {
                    player.DisablePlayer();
                    enemyPlayer.PlayerThinking();
                }
                eventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.BEGIN_ORC_POST_TURN, this, null);
                break;
            case TurnType.BATTLE:
                turnSpine.AnimationState.SetAnimation(0, "4.battle", false);
                player.DisablePlayer();
                enemyPlayer.PlayerThinkFinish();
                eventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.BEGIN_BATTLE_TURN, this, null);
                break;
        }
        if (player.isHuman)
            StartCoroutine(SetHumanTurnTable(turn));
        else
            StartCoroutine(SetOrcTurnTable(turn));
    }

    public IEnumerator SetHumanTurnTable(TurnType currentTurn) {
        yield return new WaitForSeconds(0.3f);
        switch (currentTurn) {
            case TurnType.HUMAN:
                playManagement.releaseTurnBtn.SetActive(true);
                break;
            case TurnType.ORC:
            case TurnType.SECRET:
            case TurnType.BATTLE:
                playManagement.releaseTurnBtn.SetActive(false);
                break;
        }
    }


    private IEnumerator SetOrcTurnTable(TurnType currentTurn) {
        yield return new WaitForSeconds(0.3f);
        switch (currentTurn) {
            case TurnType.ORC:
            case TurnType.SECRET:
                playManagement.releaseTurnBtn.SetActive(true);
                break;
            case TurnType.HUMAN:
            case TurnType.BATTLE:
                playManagement.releaseTurnBtn.SetActive(false);
                break;
        }
    }
}

public enum TurnType {
    ORC = 0,
    HUMAN = 1,
    SECRET = 2,
    BATTLE = 3
}
