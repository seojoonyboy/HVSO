using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillModules {
    public class played_place_chk : Base_gain {
        public override void Init() {
            //base.Init();

            EventDelegates[IngameEventHandler.EVENT_TYPE.END_CARD_PLAY].AddListener(() => {
                OnEndCardPlay();
            });

            SetMyActivateCondition("played_place_chk");
        }

        private void OnEndCardPlay() {

        }

        public override void GetMouseButtonDownEvent() {

        }
    }
}
