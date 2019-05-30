using dataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillModules {
    public class played_camp_chk : ConditionChecker {
        public override bool IsConditionSatisfied(bool isPlayerUnitGenerated, GameObject summonedObj) {
            bool isCampValid = false;

            if (!isScopeValid(summonedObj)) return false;

            foreach (Condition condition in data.activate.conditions) {
                foreach (string arg in condition.args) {
                    if(arg == "my") {
                        if (isPlayerUnitGenerated) isCampValid = true;
                    }
                    else if(arg == "enemy") {
                        if (!isPlayerUnitGenerated) isCampValid = true;
                    }
                }
            }
            Debug.Log("진영 조건에서 진영 처리 결과 : " + isCampValid);
            return isCampValid;
        }
    }
}
