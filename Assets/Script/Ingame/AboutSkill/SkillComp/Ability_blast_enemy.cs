using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SkillModules {
    public class Ability_blast_enemy : Ability {
        protected override void OnEventCallback(object parm) {
            object[] parms = (object[])parm;

            bool isPlayer = (bool)parms[0];
            GameObject card = (GameObject)parms[1];

            if(skillData.activate.scope == "playing") {
                if (card != gameObject) return;
            }

            foreach(var target in skillData.targets) {
                var args = target.args.ToList();

                if (args.Contains("line")) {
                    if(target.method == "played_target") {
                        //case : 라인 타겟 지정 후 처리하는 경우
                        Transform selectedLine = GetComponent<MagicDragHandler>().selectedLine;
                        if (selectedLine == null) return;

                        int row = selectedLine.transform.GetSiblingIndex();

                        FieldUnitsObserver enemyUnitsObserver = null;
                        if (isPlayer) {
                            enemyUnitsObserver = PlayMangement.instance.EnemyUnitsObserver;
                        }
                        else {
                            enemyUnitsObserver = PlayMangement.instance.PlayerUnitsObserver;
                        }

                        var selectedUnits = enemyUnitsObserver.GetAllFieldUnits(row);
                        if (selectedUnits.Count == 0) return;

                        int dmgAmount = 0;
                        int.TryParse(skillData.effects[0].args[0], out dmgAmount);
                        foreach(GameObject selectedUnit in selectedUnits) {
                            selectedUnit.GetComponent<PlaceMonster>().RequestChangeStat(0, dmgAmount);
                        }
                    }
                }
            }
        }
    }
}
