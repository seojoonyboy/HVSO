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
            var effect = skillData.effects[0];
            int atk = 0;
            int hp = 0;

            int.TryParse(effect.args[0], out atk);
            int.TryParse(effect.args[1], out hp);

            selectedTarget.GetComponent<PlaceMonster>().RequestChangeStat(atk, hp);
        }
    }
}
