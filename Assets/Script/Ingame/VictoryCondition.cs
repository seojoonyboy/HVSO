using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;


namespace victoryModule {
    public class VictoryCondition : MonoBehaviour {
        protected PlayerController player, enemyPlayer;

        public VictoryCondition(PlayerController player_1, PlayerController player_2) {
            player = player_1;
            enemyPlayer = player_2;
        }

        public virtual void SetCondition() {
            return;
        }
    }

    public class Annihilation_Match : VictoryCondition {
        public Annihilation_Match(PlayerController player_1, PlayerController player_2) : base(player_1, player_2) { }
        public override void SetCondition() {
            player.HP.Where(x => x <= 0).Subscribe(_=> PlayMangement.instance.GetBattleResult()).AddTo(PlayMangement.instance.transform.gameObject);
            enemyPlayer.HP.Where(x=> x<=0).Subscribe(_=>PlayMangement.instance.GetBattleResult()).AddTo(PlayMangement.instance.transform.gameObject);
        }
    }
}


