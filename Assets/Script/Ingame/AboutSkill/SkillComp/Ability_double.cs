using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillModules {
    public class Ability_double : Ability {
        protected override void OnEventCallback(object parm) {
            var checkers = GetComponents<ConditionChecker>();

            object[] parms = (object[])parm;
            bool isPlayer = (bool)parms[0];
            GameObject summonedObj = (GameObject)parms[1];

            foreach (ConditionChecker checker in checkers) {
                if (!checker.IsConditionSatisfied(isPlayer, summonedObj)) {
                    Debug.Log("조건 불만족");
                }
            }
        }
    }
}
