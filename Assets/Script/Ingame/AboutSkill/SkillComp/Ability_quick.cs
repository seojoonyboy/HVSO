using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillModules {
    public class Ability_quick : Ability {
        protected override void OnEventCallback(object parm) {
            object[] parms = (object[])parm;
            bool isPlayer = (bool)parms[0];
            GameObject playedObj = (GameObject)parms[1];

            bool isUnit = true;
            if (playedObj.GetComponent<PlaceMonster>() == null) {
                isUnit = false;
            }
        }
    }
}

