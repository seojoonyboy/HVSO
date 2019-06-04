using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkillModules {
    public class MagicalCasting_blast_enemy : MagicalCasting {
        public override void RequestUseMagic() {
            isRequested = true;
        }

        public override void UseMagic() {
            IEnumerable<string> query = from target in skillData.targets.ToList()
                                        select target.method;
            IEnumerable<List<string>> query2 = from effect in skillData.effects.ToList()
                                               select effect.args.ToList();

            List<string> targetArgs = new List<string>();
            foreach (List<string> args in query2) {
                targetArgs.Add(args[0]);
            }

            List<string> targetMethods = query.ToList();

            if (targetMethods.Contains("played_target")) {
                if (targetArgs.Contains("line")) {
                    Transform selectedLine = GetComponent<MagicDragHandler>()
                        .selectedLine;

                    if (selectedLine == null) return;

                    int row = selectedLine.transform.parent.GetSiblingIndex();

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
                    foreach (GameObject selectedUnit in selectedUnits) {
                        Debug.Log(selectedUnit.name + "에게 " + dmgAmount + " 데미지 부여");
                        selectedUnit.GetComponent<PlaceMonster>().RequestChangeStat(0, -dmgAmount);
                        selectedUnit.GetComponent<PlaceMonster>().TakeMagic();
                    }
                }
            }
        }
    }
}
