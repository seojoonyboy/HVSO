using dataModules;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillModules {
    public class played_place_chk : ConditionChecker {
        public override bool IsConditionSatisfied(bool isPlayerUnitGenerated, GameObject summonedObj) {
            bool result = false;
            foreach(Condition condition in data.activate.conditions) {
                if(condition.method == "behind") {
                    result = CheckBehind();
                }
            }
            return result;
        }

        private bool CheckBehind() {
            var pos = playerUnitsObserver.GetMyPos(gameObject);
            var selectedUnits = playerUnitsObserver.GetAllFieldUnits(pos.row);
            if (selectedUnits.Count != 0) return true;
            else return false;
        }
    }
}
