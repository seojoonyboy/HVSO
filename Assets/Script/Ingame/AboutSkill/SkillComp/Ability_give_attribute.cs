using dataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SkillModules {
    /// <summary>
    /// 적에게 특정 속성을 부여함(ex. stun)
    /// </summary>
    public class Ability_give_attribute : Ability {
        protected override void OnEventCallback(object parm) {
            object[] parms = (object[])parm;
            bool isPlayer = (bool)parms[0];
            GameObject unit = (GameObject)parms[1];

            if (unit == gameObject) {
                GameObject attackTarget = unit.GetComponent<PlaceMonster>().myTarget;
                string attributeName = skillData.effects[0].args[0];
                foreach (var target in skillData.targets) {
                    if (target.method == "attack_target") {
                        var comp = attackTarget.GetComponent(System.Type.GetType("SkillModules." + attributeName));
                        if (comp != null) {
                            Debug.Log(comp + "컴포넌트가 이미 존재합니다");
                            ((Attribute)comp).Accumulate();
                        }
                        else {
                            var newComp = attackTarget.AddComponent(System.Type.GetType("SkillModules." + attributeName));
                            if (newComp != null) {
                                ((Attribute)newComp).Init();
                                //if(newComp.GetType() == typeof(stun))
                            }
                        }
                    }
                }
            }
        }
    }
}
