using dataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillModules {
    public class played_camp_chk : ConditionChecker {
        public override bool IsConditionSatisfied(bool isPlayerUnitGenerated, GameObject summonedObj) {
            bool result = false;
            foreach(Condition condition in data.conditions) {
                if(condition.method == "my") {
                    if (isPlayerUnitGenerated) result = true;
                }
            }
            return result;
        }
    }
}
