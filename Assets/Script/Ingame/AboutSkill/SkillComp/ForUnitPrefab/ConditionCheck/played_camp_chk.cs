using dataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SkillModules {
    public class played_camp_chk : ConditionChecker {
        public override bool IsConditionSatisfied(bool isPlayerUnitGenerated, GameObject summonedObj) {
            bool isCampValid = false;

            if (!isScopeValid(summonedObj)) return false;

            foreach (Condition condition in data.activate.conditions) {
                foreach (string arg in condition.args) {
                    if(arg == "my") {
                        //아군 마법 카드 사용시
                        if(summonedObj.GetComponent<PlaceMonster>() == null) {
                            isCampValid = true;
                        }
                        //아군 유닛 소환시
                        else {
                            if(summonedObj.GetComponent<PlaceMonster>().isPlayer == isPlayerUnitGenerated) isCampValid = true;
                        }
                    }
                    else if(arg == "enemy") {
                        //아군 마법 카드 사용시
                        if (summonedObj.GetComponent<PlaceMonster>() == null) {
                            isCampValid = true;
                        }
                        //아군 유닛 소환시
                        else {
                            if (summonedObj.GetComponent<PlaceMonster>().isPlayer != isPlayerUnitGenerated) isCampValid = true;
                        }
                    }
                }
            }
            Debug.Log("진영 조건에서 진영 처리 결과 : " + isCampValid);
            return isCampValid;
        }
    }
}
