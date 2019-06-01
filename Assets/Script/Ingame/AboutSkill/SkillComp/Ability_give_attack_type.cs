using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkillModules {
    public class Ability_give_attack_type : Ability {
        protected override void OnEventCallback(object parm) {
            object[] parms = (object[])parm;
            bool isPlayer = (bool)parms[0];
            GameObject playedObj = (GameObject)parms[1];

            bool isUnit = true;
            if (GetComponent<PlaceMonster>() == null) {
                isUnit = false;
            }

            if (skillData.activate.scope == "playing") {
                if (playedObj != gameObject) return;
            }

            FieldUnitsObserver playerUnitsObserver;
            if (isPlayer) playerUnitsObserver = PlayMangement.instance.PlayerUnitsObserver;
            else playerUnitsObserver = PlayMangement.instance.EnemyUnitsObserver;

            //유닛
            if (isUnit) {

            }
            //마법
            else {
                foreach (var target in skillData.targets) {
                    if (target.method == "played_target") {
                        //내 유닛이 타겟
                        var args = target.args.ToList();
                        if (args.Contains("my") && args.Contains("all")) {
                            var selectedUnits = playerUnitsObserver.GetAllFieldUnits();
                            foreach (GameObject unit in selectedUnits) {
                                var addedComp = unit.AddComponent(System.Type.GetType("SkillModules." + skillData.effects[0].args[0]));
                                if (addedComp != null) Debug.Log("모든 아군에게 " + addedComp.name + "부여");
                            }
                        }
                    }
                }
                GetComponent<MagicDragHandler>().AttributeUsed(GetComponent<Ability_over_a_kill>());
            }
        }
    }
}