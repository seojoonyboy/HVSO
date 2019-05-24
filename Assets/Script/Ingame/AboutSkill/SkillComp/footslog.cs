using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillModules {
    public class footslog : Ability {
        protected override void BeginCardPlay() {
            //Search Available Area
            bool[] retunrnData = new bool[] {
            true,
            true,
            true,
            true,
            false
        };
            OnBeginDragFinished.Invoke(retunrnData);
        }

        protected override void EndCardPlay() {
            base.EndCardPlay();
        }

        public override IEnumerator OnBeginDrag() {
            BeginCardPlay();
            return base.OnBeginDrag();
        }
    }
}