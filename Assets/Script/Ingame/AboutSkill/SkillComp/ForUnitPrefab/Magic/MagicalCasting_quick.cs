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
                    selectedTarget = GetComponent<MagicDragHandler>()
                        .highlightedSlot
                        .GetComponentInParent<PlaceMonster>()
                        .transform;

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
    }
}
