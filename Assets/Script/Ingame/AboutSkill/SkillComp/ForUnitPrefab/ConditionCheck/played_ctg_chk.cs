using dataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillModules {
    public class played_ctg_chk : ConditionChecker {
        public override bool IsConditionSatisfied(bool isPlayerUnitGenerated, GameObject summonedObj) {
            bool result = false;
            PlaceMonster placeMonster = summonedObj.GetComponent<PlaceMonster>();
            string[] cardCategories = placeMonster.unit.cardCategories;
            foreach (Condition condition in data.conditions) {
                foreach(string ctg in cardCategories) {
                    if (condition.method == ctg) result = true;
                }
            }
            return result;
        }
    }
}
