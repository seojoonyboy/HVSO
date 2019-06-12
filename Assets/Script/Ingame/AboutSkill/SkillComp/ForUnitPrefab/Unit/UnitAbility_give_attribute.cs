using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace SkillModules {
    public class UnitAbility_give_attribute : Ability {
        protected override void OnEventCallback(object parm) {
            if(parm == null) return;
            object[] parms = (object[])parm;
            bool isPlayer = (bool)parms[0];
            GameObject playedObj = (GameObject)parms[1];

            if (playedObj != gameObject) return;

            //condition.method가 존재하지 않는 경우에는 그냥 True임.
            if (!IsSubConditionValid(isPlayer, playedObj)) return;

            IEnumerable<string> query = from target in skillData.targets.ToList()
                                        select target.method;
            IEnumerable<List<string>> query2 = from effect in skillData.effects.ToList()
                                               select effect.args.ToList();

            List<string> targetMethods = query.ToList();

            List<string> attributeNames = new List<string>();
            foreach (List<string> args in query2) {
                attributeNames.Add(args[0]);
            }

            //공격할 대상에게 특성 부여
            if (targetMethods.Contains("attack_target")) {
                GameObject attackTarget = playedObj.GetComponent<PlaceMonster>().myTarget;

                foreach(string attributeName in attributeNames) {
                    var comp = attackTarget.GetComponent(System.Type.GetType("SkillModules." + attributeName));
                    if (comp != null) {
                        Debug.Log("상대방에게 이미 " + comp.name + "속성이 부여되어 있어 누적시킴");
                        ((Attribute)comp).Accumulate();
                    }
                    else {
                        var newComp = attackTarget.AddComponent(System.Type.GetType("SkillModules." + attributeName));
                        if (newComp != null) {
                            Debug.Log("상대방에게 " + newComp.name + "속성 부여");
                            ((Attribute)newComp).Init();
                            GameObject status = attackTarget.gameObject.transform.Find("Status").gameObject;
                            status.gameObject.SetActive(true);
                            status.GetComponent<TextMeshPro>().text = newComp.name + " 걸림";
                        }
                    }
                }
            }
        }
    }
}
