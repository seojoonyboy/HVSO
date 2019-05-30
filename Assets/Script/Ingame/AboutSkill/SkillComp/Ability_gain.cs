using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using dataModules;

namespace SkillModules {
    public class Ability_gain : Ability {
        protected override void OnEventCallback(object parm) {
            var checkers = GetComponents<ConditionChecker>();

            object[] parms = (object[])parm;
            bool isPlayer = (bool)parms[0];
            GameObject summonedObj = (GameObject)parms[1];

            bool isConditionSatisfied = false;
            foreach (ConditionChecker checker in checkers) {
                if (!checker.IsConditionSatisfied(isPlayer, summonedObj)) {
                    isConditionSatisfied = false;
                }
                else {
                    isConditionSatisfied = true;
                }
            }

            string[] args = skillData.effects[0].args;
            int atk = 0;
            int hp = 0;

            int.TryParse(args[0], out atk);
            int.TryParse(args[1], out hp);

            foreach (var target in skillData.targets) {
                if (target.method == "self") {
                    if (isConditionSatisfied) {
                        Debug.Log("Gain 버프");
                        GetComponent<PlaceMonster>().AddBuff(new PlaceMonster.Buff(gameObject, atk, hp));
                    }
                    else {
                        Debug.Log("Gain 버프 해제");
                        GetComponent<PlaceMonster>().RemoveBuff(gameObject);
                    }
                }

                if((target.method == "played") && (target.args[0] == "my")) {
                    if (isPlayer) {
                        if (isConditionSatisfied) {
                            GetComponent<PlaceMonster>().AddBuff(new PlaceMonster.Buff(gameObject, atk, hp));
                        }
                    }
                }
            }
        }
    }
}
