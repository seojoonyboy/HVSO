using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkillModules {
    public class Ability_over_a_kill : Ability {
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

            int atk = 0;
            int.TryParse(skillData.activate.conditions[0].args[0], out atk);

            if (isUnit) {

            }
            //마법
            else {
                foreach (var target in skillData.targets) {
                    if (target.method == "played_target") {
                        //내 유닛이 타겟
                        if (target.args.ToList().Contains("enemy")) {
                            Transform result = CheckUnit();
                            if (result != null) {
                                if (result.GetComponent<PlaceMonster>().isPlayer) {
                                    Debug.Log("적군만 타겟으로 지정 가능합니다.");
                                    return;
                                }
                                //success
                                if(result.GetComponent<PlaceMonster>().unit.attack >= atk) {
                                    Debug.Log("공격력이 " + atk + "이상인 적 처치");
                                    GetComponent<PlaceMonster>().InstanceKilled();
                                    GetComponent<MagicDragHandler>().AttributeUsed(GetComponent<Ability_over_a_kill>());
                                }
                                else {
                                    Debug.Log("공격력이 " + atk + "이하라 처치 불가");
                                }
                            }
                        }
                    }
                }
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

