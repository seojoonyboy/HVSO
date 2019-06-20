using System;
using UnityEngine;

namespace SkillModules {
    public class UnitAbility_assault : MonoBehaviour {
        bool isBuffed = false;
        IngameEventHandler eventHandler;
        FieldUnitsObserver
            playerUnitsObserver,
            enemyUnitsObserver;

        DebugManagement debugManagement;
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
            eventHandler.AddListener(IngameEventHandler.EVENT_TYPE.LINE_BATTLE_FINISHED, OnEndCardPlay);
            eventHandler.AddListener(IngameEventHandler.EVENT_TYPE.MONSTER_MOVED, OnEndCardPlay);
        }

        private void OnEndCardPlay(Enum Event_Type, Component Sender, object Param) {
            CheckEnemy();
        }

        public void CheckEnemy() {
            if (gameObject == null) return;
            var myPos = playerUnitsObserver.GetMyPos(gameObject);
            var enemies = enemyUnitsObserver.GetAllFieldUnits(myPos.row);

            if (debugManagement != null) {
                if (enemies.Count == 0) {
                    if (!isBuffed) {
                        if (!GetComponent<DebugUnit>().IsBuffAlreadyExist(gameObject)) {
                            Debug.Log("Assault 스킬 발동");
                            GetComponent<DebugUnit>().AddBuff(new DebugUnit.Buff(gameObject, 3, 0));
                            isBuffed = true;
                        }
                    }
                }
                //적이 있으면 버프 다시 제거
                else if (isBuffed) {
                    Debug.Log("Assault 스킬 해제");
                    GetComponent<DebugUnit>().RemoveBuff(gameObject);
                    isBuffed = false;
                }
            }
            else {
                if (enemies.Count == 0) {
                    if (!isBuffed) {
                        if (!GetComponent<PlaceMonster>().IsBuffAlreadyExist(gameObject)) {
                            Debug.Log("Assault 스킬 발동");
                            GetComponent<PlaceMonster>().AddBuff(new PlaceMonster.Buff(gameObject, 3, 0));
                            isBuffed = true;
                        }
                    }
                }
                //적이 있으면 버프 다시 제거
                else if (isBuffed) {
                    Debug.Log("Assault 스킬 해제");
                    GetComponent<PlaceMonster>().RemoveBuff(gameObject);
                    isBuffed = false;
                }
            }
        }

        void SetObservers() {
            if (debugManagement != null) {
                if (GetComponent<PlaceMonster>().isPlayer) {
                    playerUnitsObserver = PlayMangement.instance.PlayerUnitsObserver;
                    enemyUnitsObserver = PlayMangement.instance.EnemyUnitsObserver;
                }
                else {
                    playerUnitsObserver = PlayMangement.instance.EnemyUnitsObserver;
                    enemyUnitsObserver = PlayMangement.instance.PlayerUnitsObserver;
                }
            }
        }

        void RemoveListener() {
            eventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.END_CARD_PLAY, OnEndCardPlay);
            eventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.END_BATTLE_TURN, OnEndCardPlay);
            eventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.LINE_BATTLE_FINISHED, OnEndCardPlay);
            eventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.MONSTER_MOVED, OnEndCardPlay);
        }

        void OnDestroy() {
            eventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.END_CARD_PLAY, OnEndCardPlay);
            eventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.END_BATTLE_TURN, OnEndCardPlay);
            eventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.LINE_BATTLE_FINISHED, OnEndCardPlay);
            eventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.MONSTER_MOVED, OnEndCardPlay);
        }
    }
}

