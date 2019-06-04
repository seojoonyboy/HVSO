using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkillModules {
    public class MagicalCasting_gain : MagicalCasting {
        Transform selectedTarget;
        public override void RequestUseMagic() {
            IEnumerable<string> query = from target in skillData.targets.ToList()
                                        select target.method;
            List<string> targets = query.ToList();
            if (targets.Contains("self")) {
                selectedTarget = CheckUnit();
                if (selectedTarget != null) {
                    isRequested = true;
                }
            }
        }

        public override void UseMagic() {
            var effect = skillData.effects[0];
            int atk = 0;
            int hp = 0;

            int.TryParse(effect.args[0], out atk);
            int.TryParse(effect.args[1], out hp);

            selectedTarget.GetComponent<PlaceMonster>().RequestChangeStat(atk, hp);
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
