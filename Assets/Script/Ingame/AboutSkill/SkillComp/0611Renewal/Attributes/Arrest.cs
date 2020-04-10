using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillModules {
    public class Arrest : UnitAttribute {
        // Start is called before the first frame update
        public int amount = 0;

        private void OnDestroy() {
            bool isPlayer = gameObject.GetComponent<PlaceMonster>().isPlayer;
            if (isPlayer == true)
                PlayMangement.instance.enemyPlayer.resource.Value += amount;
            else {
                PlayMangement.instance.player.resource.Value += amount;

                if (PlayMangement.instance.currentTurn == TurnType.HUMAN)
                    PlayMangement.instance.player.ActivePlayer();
            }
        }
    }
}