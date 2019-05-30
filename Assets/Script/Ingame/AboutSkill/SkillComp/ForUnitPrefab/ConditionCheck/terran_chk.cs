using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using dataModules;

namespace SkillModules {
    public class terran_chk : ConditionChecker {
        public override bool IsConditionSatisfied(bool isPlayerUnitGenerated, GameObject summonedObj) {
            bool isTerrainValid = false;

            if (!isScopeValid(summonedObj)) return false;

            var terrain = summonedObj.transform.parent.GetComponent<Terrain>().terrain;
            foreach (Condition condition in data.activate.conditions) {
                foreach(string arg in condition.args) {
                    if(arg == "hill") {
                        if (terrain == PlayMangement.LineState.hill) {
                            Debug.Log("언덕 지형 조건을 만족함");
                            isTerrainValid = true;
                        }
                    }
                }
            }
            Debug.Log("지형 조건에서 지형 처리 결과 : " + isTerrainValid);
            return isTerrainValid;
        }
    }
}
