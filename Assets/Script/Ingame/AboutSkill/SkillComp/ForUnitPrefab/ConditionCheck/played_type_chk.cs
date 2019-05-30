using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using dataModules;

namespace SkillModules {
    public class played_type_chk : ConditionChecker {
        public override bool IsConditionSatisfied(bool isPlayerUnitGenerated, GameObject summonedObj) {
            bool isFieldValid = false;
            bool isTypeValid = false;

            if (data.activate.scope == "playing") {
                if (summonedObj == gameObject) isFieldValid = true;
            }
            else {
                isFieldValid = true;
            }

            foreach (Condition condition in data.activate.conditions) {
                foreach(string arg in condition.args) {
                    if (arg == "magic") {
                        if (summonedObj.GetComponent<PlaceMonster>() == null) {
                            Debug.Log("마법카드 사용 조건을 만족함");
                            isTypeValid = true;
                        }
                    }
                }
            }
            return (isFieldValid & isTypeValid);
        }
    }
}