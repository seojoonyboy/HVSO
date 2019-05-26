using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillModules {
    public class Ability_footslog : Ability {
        private void Awake() {
            isChangeDropableSlot = true;
        }

        public override void BeginCardPlay() {
            UnitDropManager.Instance.ShowDropableSlot(FieldType.FOOTSLOG);
        }
    }
}