using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillModules {
    public class terran_chk : Base_gain {
        public override void Init() {
            base.Init();

            EventDelegates[IngameEventHandler.EVENT_TYPE.END_CARD_PLAY].AddListener(() => {
                OnEndCardPlay();
            });
        }

        private void OnEndCardPlay() {

        }
    }
}
