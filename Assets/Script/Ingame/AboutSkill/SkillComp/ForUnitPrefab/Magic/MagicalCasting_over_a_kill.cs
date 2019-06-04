using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkillModules {
    public class MagicalCasting_over_a_kill : MagicalCasting {
        Transform selectedTarget;

        public override void RequestUseMagic() {
            selectedTarget = CheckUnit();

            IEnumerable<string> query = from effect in skillData.effects.ToList()
                                        select effect.args[0];
            IEnumerable<string> query2 = from _target in skillData.targets.ToList()
                                        select _target.args[0];
            List<string> effects = query.ToList();
            List<string> target = query2.ToList();

            int standard = 0;

            int.TryParse(skillData.activate.conditions[0].args[0], out standard);
            if (selectedTarget != null && target.Contains("enemy")) {
                if(selectedTarget.GetComponent<PlaceMonster>().unit.attack >= standard) {
                    isRequested = true;
                }
            }
        }

        public override void UseMagic() {
            selectedTarget.GetComponent<PlaceMonster>().InstanceKilled();
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
