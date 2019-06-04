using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkillModules {
    public class MagicalCasting_give_attribute : MagicalCasting {
        Transform target;
        public override void RequestUseMagic() {
            target = CheckUnit();
            if (target == null) {
                Debug.LogError("give_attribute 대상이 정상적으로 지정되지 않았습니다.");
                return;
            }
            else {
                isRequested = true;
            }
        }

        public override void UseMagic() {
            IEnumerable<string> query = from effect in skillData.effects.ToList()
                                        select effect.args[0];

            IEnumerable<string> query2 = from target in skillData.targets.ToList()
                                         select target.args[0];

            List<string> attributeNames = query.ToList();
            List<string> targets = query2.ToList();

            foreach(string attributeName in attributeNames) {
                target.gameObject.AddComponent(System.Type.GetType("SkillModules." + attributeName));
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
