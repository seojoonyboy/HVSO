using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using dataModules;
using System.Linq;

namespace SkillModules {
    public class Ability_gain : Ability {
        protected override void OnEventCallback(object parm) {
            var checkers = GetComponents<ConditionChecker>();

            object[] parms = (object[])parm;
            bool isPlayer = (bool)parms[0];
            GameObject summonedObj = (GameObject)parms[1];

            bool isUnit = true;
            if (GetComponent<PlaceMonster>() == null) {
                isUnit = false;
            }

            if (skillData.activate.scope == "playing") {
                if (summonedObj != gameObject) return;
            }
            //유닛
            if (isUnit) {
                bool isConditionSatisfied = false;
                foreach (ConditionChecker checker in checkers) {
                    if (!checker.IsConditionSatisfied(isPlayer, summonedObj)) {
                        isConditionSatisfied = false;
                    }
                    else {
                        isConditionSatisfied = true;
                    }
                }

                string[] args = skillData.effects[0].args;
                int atk = 0;
                int hp = 0;

                int.TryParse(args[0], out atk);
                int.TryParse(args[1], out hp);

                foreach (var target in skillData.targets) {
                    if (target.method == "self") {
                        if (isConditionSatisfied) {
                            Debug.Log("Gain 버프");
                            GetComponent<PlaceMonster>().AddBuff(new PlaceMonster.Buff(gameObject, atk, hp));
                        }
                        else {
                            Debug.Log("Gain 버프 해제");
                            GetComponent<PlaceMonster>().RemoveBuff(gameObject);
                        }
                    }

                    if ((target.method == "played") && (target.args[0] == "my")) {
                        if (isPlayer) {
                            if (isConditionSatisfied) {
                                GetComponent<PlaceMonster>().AddBuff(new PlaceMonster.Buff(gameObject, atk, hp));
                            }
                        }
                    }
                    
                    //if((target.method == "line") && (target.args[0] == "behind")) {
                    //    if (isPlayer) {
                    //        //뒤쪽 라인 검사하여 부여대상에게 실제 부여 (방패병 전용)
                    //        if (isConditionSatisfied) {

                    //        }
                    //        //버프 해제
                    //        else {

                    //        }
                    //    }
                    //}
                }
            }
            //마법
            else {
                foreach (var target in skillData.targets) {
                    if (target.method == "played_target") {
                        //내 유닛이 타겟
                        if (target.args.ToList().Contains("my")) {
                            Transform result = CheckUnit();
                            if (result != null) {
                                if (!result.GetComponent<PlaceMonster>().isPlayer) {
                                    Debug.Log("아군만 타겟으로 지정 가능합니다.");
                                    return;
                                }
                                Debug.Log("버프 발동");
                                string[] args = skillData.effects[0].args;
                                int atk = 0;
                                int hp = 0;

                                int.TryParse(args[0], out atk);
                                int.TryParse(args[1], out hp);

                                Debug.Log("atk : " + atk);
                                Debug.Log("hp : " + hp);

                                result.GetComponent<PlaceMonster>().RequestChangeStat(atk, hp);
                                GetComponent<MagicDragHandler>().AttributeUsed(GetComponent<Ability_gain>());
                            }
                        }
                    }
                }
            }

            //1회성인지
            if (skillData.deactivate.trigger == "immidiate"){

            }
        }

        Transform CheckUnit() {
            Vector3 origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            origin = new Vector3(origin.x, origin.y, origin.z);
            Ray2D ray = new Ray2D(origin, Vector2.zero);

            RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction, Mathf.Infinity);

            foreach (RaycastHit2D hit in hits) {
                if (hit.transform.GetComponentInParent<PlaceMonster>() != null) {
                    Debug.Log(hit.collider.name);
                    return hit.transform.GetComponentInParent<PlaceMonster>().transform;
                }
            }
            return null;
        }
    }
}
