using dataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillModules {
    public class played_ctg_chk : ConditionChecker {
        public override bool IsConditionSatisfied(bool isPlayerUnitGenerated, GameObject summonedObj) {
            bool isFieldValid = false;
            bool isCtgValid = false;

            PlaceMonster placeMonster = summonedObj.GetComponent<PlaceMonster>();

            if (data.activate.scope == "playing") {
                if (summonedObj == gameObject) isFieldValid = true;
            }
            else {
                isFieldValid = true;
            }

            if(data.targets[0].method == "line") {
                if(data.targets[0].args[0] == "behind") {
                    var pos = playerUnitsObserver.GetMyPos(summonedObj);
                    var selectedUnits = playerUnitsObserver.GetAllFieldUnits(pos.row);
                    selectedUnits.Remove(summonedObj);

                    foreach(GameObject unit in selectedUnits) {
                        PlaceMonster comp = unit.GetComponent<PlaceMonster>();
                        string[] cardCategories = comp.unit.cardCategories;

                        foreach (string ctg in cardCategories) {
                            foreach (Condition condition in data.activate.conditions) {
                                if (condition.method == ctg) {
                                    Debug.Log(ctg + "속성 조건을 만족함");
                                    isCtgValid = true;
                                }
                                else {
                                    isCtgValid = false;
                                }
                            }
                        }
                    }
                }
            }
            else {
                string[] cardCategories = placeMonster.unit.cardCategories;
                foreach (Condition condition in data.activate.conditions) {
                    foreach (string ctg in cardCategories) {
                        if (condition.method == ctg) {
                            Debug.Log(ctg + "속성 조건을 만족함");
                            isCtgValid = true;
                        }
                        else {
                            isCtgValid = false;
                        }
                    }
                }
            }
            
            Debug.Log("카테고리 조건에서 Scope 처리 결과 : " + isFieldValid);
            Debug.Log("카테고리 조건에서 카테고리 처리 결과 : " + isCtgValid);
            return (isFieldValid & isCtgValid);
        }
    }
}
