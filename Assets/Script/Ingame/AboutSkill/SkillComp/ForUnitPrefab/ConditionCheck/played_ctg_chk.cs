using dataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SkillModules {
    public class played_ctg_chk : ConditionChecker {
        public override bool IsConditionSatisfied(bool isPlayerUnitGenerated, GameObject summonedObj) {
            bool isCtgValid = false;

            PlaceMonster placeMonster = summonedObj.GetComponent<PlaceMonster>();
            if (!isScopeValid(summonedObj)) return false;

            List<string> myCategories = placeMonster.unit.cardCategories.ToList();
            foreach (string ctg in myCategories) {
                foreach (var targetCtg in condition.args) {
                    Debug.Log("Target Catagory : " + targetCtg);
                    isCtgValid = myCategories.Exists(x => x == targetCtg);
                }
            }
            Debug.Log("카테고리 조건에서 카테고리 처리 결과 : " + isCtgValid);
            return isCtgValid;
        }
    }
}
