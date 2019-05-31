using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SkillModules {
    public class Ability_supply : Ability {
        protected override void OnEventCallback(object parm) {
            var effectData = skillData.effects.ToList().Find(x => x.method == "supply");
            int drawNum = 0;

            int.TryParse(effectData.args[0], out drawNum);
            for(int i=0; i<drawNum; i++) {
                PlayMangement.instance.player.cdpm.AddCard();
            }
        }
    }
}