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

                bool isHuman = PlayMangement.instance.player.isHuman;

                if (isHuman == true && PlayMangement.instance.currentTurn == TurnType.HUMAN)
                    PlayMangement.instance.player.ActivePlayer();

                if (isHuman == false && (PlayMangement.instance.currentTurn == TurnType.ORC || PlayMangement.instance.currentTurn == TurnType.SECRET))
                    PlayMangement.instance.player.ActiveOrcTurn();
                
            }
        }
    }
}