using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkillModules {
    public class MagicalCasting_give_attack_type : MagicalCasting {
        Transform target;
        List<GameObject> selectedUnits;

        public override void RequestUseMagic() {
            if (!IsSubConditionValid(isPlayer, gameObject)) return;

            IEnumerable<string> query = from effect in skillData.effects.ToList()
                                        select effect.args[0];
            List<string> effects = query.ToList();
            List<string> args = skillData.targets[0].args.ToList();
            if (args.Contains("my")) {
                if (args.Contains("all")) {
                    selectedUnits = GetUnits(true);

                    if(selectedUnits.Count != 0) {
                        isRequested = true;
                    }
                }
            }
        }

        public override void UseMagic() {
            IEnumerable<string> query = from effect in skillData.effects.ToList()
                                        select effect.args[0];
            List<string> effects = query.ToList();
            List<string> args = skillData.targets[0].args.ToList();
            if (args.Contains("my")) {
                if (args.Contains("all")) {
                    foreach(GameObject unit in selectedUnits) {
                        unit.AddComponent(System.Type.GetType("SkillModules." + effects[0]));
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

        List<GameObject> GetUnits(bool isMy) {
            List<GameObject> selectedUnits = new List<GameObject>();

            FieldUnitsObserver playerUnitsObserver = PlayMangement.instance.PlayerUnitsObserver;
            FieldUnitsObserver enemyUnitsObserver = PlayMangement.instance.EnemyUnitsObserver;

            if (isMy) {
                selectedUnits = playerUnitsObserver.GetAllFieldUnits();
            }
            else {
                selectedUnits = enemyUnitsObserver.GetAllFieldUnits();
            }
            return selectedUnits;
        }
    }
}

