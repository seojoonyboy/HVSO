using dataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SkillModules {
    /// <summary>
    /// 적에게 특정 속성을 부여함(ex. stun)
    /// </summary>
    public class Ability_give_attribute : Ability {
        protected override void OnEventCallback(object parm) {
            object[] parms = (object[])parm;
            bool isPlayer = (bool)parms[0];
            GameObject playedObj = (GameObject)parms[1];

            bool isUnit = true;
            if(playedObj.GetComponent<PlaceMonster>() == null) {
                isUnit = false;
            }

            //Scope : Field
            if (skillData.activate.scope == "field") {
                foreach (var target in skillData.targets) {
                    //공격시 발동되는 것
                    if (isUnit && target.method == "attack_target" && (playedObj == gameObject)) {
                        GameObject attackTarget = playedObj.GetComponent<PlaceMonster>().myTarget;
                        string attributeName = skillData.effects[0].args[0];

                        var comp = attackTarget.GetComponent(System.Type.GetType("SkillModules." + attributeName));
                        if (comp != null) {
                            Debug.Log(comp + "컴포넌트가 이미 존재합니다");
                            ((Attribute)comp).Accumulate();
                        }
                        else {
                            var newComp = attackTarget.AddComponent(System.Type.GetType("SkillModules." + attributeName));
                            if (newComp != null) {
                                ((Attribute)newComp).Init();
                                //if(newComp.GetType() == typeof(stun))
                            }
                        }
                    }

                    //카드를 놓았을 때 발동되는 것
                    if (target.method == "played_target") {

                    }
                }
            }
            //Scope : Playing
            else if(skillData.activate.scope == "playing") {
                //마법 카드
                if (!isUnit) {
                    foreach(Target target in skillData.targets) {
                        //적을 지목
                        if((target.method == "played_target") && (target.args.ToList().Contains("enemy"))) {
                            Transform result = CheckUnit();
                            if(result != null) {
                                if (result.GetComponent<PlaceMonster>().isPlayer) {
                                    return;
                                }
                                Debug.Log("적이 감지되었습니다.");
                                string attributeName = skillData.effects[0].args[0];
                                if(attributeName == "stun") {
                                    stun _stun = result.gameObject.AddComponent<stun>();
                                    _stun.Init();
                                }
                                GetComponent<MagicDragHandler>().AttributeUsed(GetComponent<Ability_give_attribute>());
                            }
                        }

                        else if((target.method == "played_target") && (target.args.ToList().Contains("my"))){
                            Transform result = CheckUnit();
                            if(result != null) {
                                if (!result.GetComponent<PlaceMonster>().isPlayer) {
                                    return;
                                }
                                Debug.Log("아군이 감지되었습니다.");
                                GetComponent<MagicDragHandler>().AttributeUsed(GetComponent<Ability_give_attribute>());
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

            foreach(RaycastHit2D hit in hits) {
                if(hit.transform.GetComponentInParent<PlaceMonster>() != null) {
                    Debug.Log(hit.collider.name);
                    return hit.transform.GetComponentInParent<PlaceMonster>().transform;
                }
            }
            return null;
        }
    }
}
