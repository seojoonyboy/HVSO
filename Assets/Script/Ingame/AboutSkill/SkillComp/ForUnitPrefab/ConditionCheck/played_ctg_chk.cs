using dataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillModules {
    public class played_ctg_chk : ConditionChecker {
        public override bool IsConditionSatisfied(bool isPlayerUnitGenerated, GameObject summonedObj) {
            bool result = false;
            PlaceMonster placeMonster = summonedObj.GetComponent<PlaceMonster>();

            foreach (Condition condition in data.conditions) {
                if (condition.method == "army") {
                    //placeMonster.unit.
                }
            }
            return result;
        }
    }
}
