using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using dataModules;

namespace SkillModules {
    public class terran_chk : ConditionChecker {
        public override bool IsConditionSatisfied(bool isPlayerUnitGenerated, GameObject summonedObj) {
            bool isFieldValid = false;
            bool isTerrainValid = false;

            if (data.activate.scope == "playing") {
                if (summonedObj == gameObject) isFieldValid = true;
            }
            else {
                isFieldValid = true;
            }

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
            Debug.Log("진영 조건에서 Scope 처리 결과 : " + isFieldValid);
            Debug.Log("지형 조건에서 지형 처리 결과 : " + isFieldValid);
            return (isFieldValid & isTerrainValid);
        }
    }
}
