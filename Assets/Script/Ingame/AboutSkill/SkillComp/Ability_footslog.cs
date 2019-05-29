using System.Collections;
using System.Collections.Generic;
using dataModules;
using UnityEngine;

namespace SkillModules {
    public class Ability_footslog : Ability {
        protected override void OnEventCallback(object parm) {
            CardDropManager.Instance.ShowDropableSlot(FieldType.FOOTSLOG);
        }
    }
}
