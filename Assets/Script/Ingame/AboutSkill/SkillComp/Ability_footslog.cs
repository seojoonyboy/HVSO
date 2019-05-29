using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillModules {
    public class Ability_footslog : Ability {

        public override void BeginCardPlay() {
            CardDropManager.Instance.ShowDropableSlot(FieldType.FOOTSLOG);
        }
    }
}
