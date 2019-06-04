using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkillModules {
    public class MagicalCasting_quick : MagicalCasting {
        Transform selectedTarget;
        public override void RequestUseMagic() {
            if (!IsSubConditionValid(isPlayer, gameObject)) return;

            IEnumerable<string> query = from target in skillData.targets.ToList()
                                        select target.args[0];
            List<string> effects = query.ToList();
            List<string> args = skillData.targets[0].args.ToList();
            if (args.Contains("my")) {
                selectedTarget = GetComponent<MagicDragHandler>()
                        .highlightedSlot
                        .GetComponentInParent<PlaceMonster>()
                        .transform;

                if (selectedTarget != null) {
                    isRequested = true;
                    GetComponent<MagicDragHandler>().AttributeUsed();
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
    }
}
