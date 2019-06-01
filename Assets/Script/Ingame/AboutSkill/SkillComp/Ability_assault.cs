using System;
using System.Collections;
using System.Collections.Generic;
using dataModules;
using UnityEngine;

public class Ability_assault : MonoBehaviour {
    bool isBuffed = false;
    IngameEventHandler eventHandler;
    FieldUnitsObserver 
        playerUnitsObserver,
        enemyUnitsObserver;

    void Awake() {
        eventHandler = PlayMangement.instance.EventHandler;
    }

    void Start() {
        RemoveListener();
        AddListener();

        SetObservers();
        CheckEnemy();
    }

    void AddListener() {
        eventHandler.AddListener(IngameEventHandler.EVENT_TYPE.END_CARD_PLAY, OnEndCardPlay);
        eventHandler.AddListener(IngameEventHandler.EVENT_TYPE.END_BATTLE_TURN, OnEndCardPlay);
    }

    private void OnEndCardPlay(Enum Event_Type, Component Sender, object Param) {
        CheckEnemy();
    }

    private void CheckEnemy() {
        var myPos = playerUnitsObserver.GetMyPos(gameObject);
        var enemies = enemyUnitsObserver.GetAllFieldUnits(myPos.row);

        if(enemies.Count == 0) {
            if (!isBuffed) {
                Debug.Log("Assault 스킬 발동");
                GetComponent<PlaceMonster>().RequestChangeStat(3, 0);
                isBuffed = true;
            }
        }
    }

    void SetObservers() {
        if (GetComponent<PlaceMonster>().isPlayer) {
            playerUnitsObserver = PlayMangement.instance.PlayerUnitsObserver;
            enemyUnitsObserver = PlayMangement.instance.EnemyUnitsObserver;
        }
        else {
            playerUnitsObserver = PlayMangement.instance.EnemyUnitsObserver;
            enemyUnitsObserver = PlayMangement.instance.PlayerUnitsObserver;
        }
    }

    void RemoveListener() {
        eventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.END_CARD_PLAY, OnEndCardPlay);
    }
}
