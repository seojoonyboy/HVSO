using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using dataModules;

namespace SkillModules {
    public class ConditionChecker : MonoBehaviour {
        public Skill data;
        public bool isPlayer;

        protected FieldUnitsObserver playerUnitsObserver;
        protected FieldUnitsObserver enemyUnitsObserver;

        public virtual bool IsConditionSatisfied(bool isPlayerUnitGenerated, GameObject summonedObj) {
            return false;
        }

        public virtual void Init(Skill data, bool isPlayer) {
            this.data = data;
            this.isPlayer = isPlayer;

            if (isPlayer) {
                playerUnitsObserver = PlayMangement.instance.PlayerUnitsObserver;
                enemyUnitsObserver = PlayMangement.instance.EnemyUnitsObserver;
            }
            else {
                playerUnitsObserver = PlayMangement.instance.EnemyUnitsObserver;
                enemyUnitsObserver = PlayMangement.instance.PlayerUnitsObserver;
            }
        }

        public bool isScopeValid(GameObject summonedObj) {
            if(data.activate.scope == "playing") {
                if (summonedObj == gameObject) {
                    Debug.Log("Scope 조건 만족함");
                    return true;
                }
                else {
                    Debug.Log("Scope 조건 불만족");
                    return false;
                }
            }
            Debug.Log("Scope 조건 만족함");
            return true;
        }
    }
}

