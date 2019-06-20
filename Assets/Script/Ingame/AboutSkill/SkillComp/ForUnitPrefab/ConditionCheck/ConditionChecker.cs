using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using dataModules;

namespace SkillModules {
    public class ConditionChecker : MonoBehaviour {
        public Skill data;
        public Condition condition;

        public bool isPlayer;

        protected FieldUnitsObserver playerUnitsObserver;
        protected FieldUnitsObserver enemyUnitsObserver;

        public virtual bool IsConditionSatisfied(bool isPlayerUnitGenerated, GameObject summonedObj) {
            return false;
        }

        public virtual void Init(Skill data, Condition condition, bool isPlayer) {
            this.data = data;
            this.isPlayer = isPlayer;
            this.condition = condition;

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
            else if(data.activate.scope == "field") {
                if (summonedObj == gameObject) {
                    Debug.Log("Scope 조건 불만족");
                    return false;
                }
                Debug.Log("Scope 조건 만족");
                return true;
            }
            Debug.Log("Scope 조건 불만족");
            return true;
        }
    }
    
}

