using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using dataModules;

namespace SkillModules {
    public class Ability_gain : Ability {
        public override void EndCardPlay(ref GameObject card) {
            base.EndCardPlay(ref card);

            foreach(var cond in skillData.activate.conditions) {
                var newComp = card.AddComponent(System.Type.GetType("SkillModules." + cond.method));
                if(newComp == null) {
                    card.AddComponent<Base_gain>();
                }
                card.GetComponent<Base_gain>().effectData = effectData;
            }
        }
    }
}
