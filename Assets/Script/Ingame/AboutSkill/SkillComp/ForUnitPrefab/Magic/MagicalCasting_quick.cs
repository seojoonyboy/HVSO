using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkillModules {
    public class MagicalCasting_quick : MagicalCasting {
        Transform selectedTarget;
        public override void RequestUseMagic() {
            if (!IsSubConditionValid(isPlayer, gameObject)) return;

            IEnumerable<string> query = from effect in skillData.effects.ToList()
                                        select effect.args[0];
            List<string> effects = query.ToList();
            List<string> args = skillData.targets[0].args.ToList();
            if (args.Contains("my")) {
                if (args.Contains("all")) {
                    selectedTarget = CheckUnit();

                    if (selectedTarget != null) {
                        isRequested = true;
                    }
                }
            }
        }

        public override void UseMagic() {
            List<string> args = skillData.targets[0].args.ToList();
            if (args.Contains("my")) {
                Debug.Log("Quick 발동");
                selectedTarget.GetComponent<PlaceMonster>().InstanceAttack();
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
