using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using dataModules;

namespace SkillModules {
    public class Ability_gain : Ability {
        public override void EndCardPlay(ref GameObject unitPrefab) {
            base.EndCardPlay(ref unitPrefab);

            foreach(var cond in skillData.activate.conditions) {
                var newComp = unitPrefab.AddComponent(System.Type.GetType("SkillModules." + cond.method));
                if(newComp != null) {
                    ((Base_gain)newComp).effectData = effectData;
                    ((Base_gain)newComp).totalData = skillData;
                }
                else {
                    Base_gain base_Gain = unitPrefab.AddComponent<Base_gain>();
                    base_Gain.effectData = effectData;
                    base_Gain.totalData = skillData;
                }
            }
        }
    }
}
