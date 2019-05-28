using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillModules {
    public class played_ctg_chk : Base_gain {
        public override void Init() {
            //base.Init();

            EventDelegates[IngameEventHandler.EVENT_TYPE.END_CARD_PLAY].AddListener(() => {
                OnEndCardPlay();
            });

            SetMyActivateCondition("played_ctg_chk");
        }

        private void OnEndCardPlay() {

        }

        public override void GetMouseButtonDownEvent() {

        }
    }

}
