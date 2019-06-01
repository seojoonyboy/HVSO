using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SkillModules {
    public class Ability_r_return : Ability {
        protected override void OnEventCallback(object parm) {
            object[] parms = (object[])parm;

            bool isPlayer = (bool)parms[0];
            GameObject card = (GameObject)parms[1];

            if (card != gameObject) return;

            var effectData = skillData.effects.ToList().Find(x => x.method == "r_return");
            
            GetComponent<MagicDragHandler>().AttributeUsed(GetComponent<Ability_r_return>());
        }
    }
}