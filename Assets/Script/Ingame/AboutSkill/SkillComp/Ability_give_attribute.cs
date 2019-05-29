using dataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillModules {
    /// <summary>
    /// 적에게 특정 속성을 부여함(ex. stun)
    /// </summary>
    public class Ability_give_attribute : Ability {
        protected override void OnEventCallback(object parm) {
            var checkers = GetComponents<ConditionChecker>();

            object[] parms = (object[])parm;
            bool isPlayer = (bool)parms[0];
            GameObject summonedObj = (GameObject)parms[1];

            if(summonedObj == gameObject) {
                foreach(Effect effect in skillData.effects) {
                    string[] args = effect.args;
                    Debug.Log("적에게 " + args[0] + "을 부여함");
                }
            }
        }
    }
}
