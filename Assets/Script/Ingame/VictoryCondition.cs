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

        public virtual IEnumerator WaitAction() {
            yield return null;
        }

        public virtual void SetCondition() {
            return;
        }

        public virtual void CheckCondition() {
            return;
        }


        public virtual void GetBattleResult() {
            return;
        }

    }

    public class Annihilation_Match : VictoryCondition {
        public Annihilation_Match(PlayerController player_1, PlayerController player_2) : base(player_1, player_2) { }
        public override void SetCondition() {
            player.HP.TakeWhile(x => x > 0 || enemyPlayer.HP.Value > 0).Where(_ => player.HP.Value <= 0 || enemyPlayer.HP.Value <= 0).Subscribe(_ => CheckCondition());
        }

        public override void CheckCondition() {            
            PlayMangement.instance.isGame = false;
            PlayerController loserPlayer = (player.HP.Value <= 0) ? player : enemyPlayer;
            loserPlayer.PlayerAddAction(delegate () { GetBattleResult(); });
            loserPlayer.PlayerDead();
        }


        public override void GetBattleResult() {
            if (PlayMangement.instance.SocketHandler.isOpponentPlayerDisconnected) return;
            GameResultManager resultManager = PlayMangement.instance.resultManager;
            resultManager.gameObject.SetActive(true);

            if (player.HP.Value <= 0) 
                resultManager.SetResultWindow("lose", player.isHuman);
            else if (enemyPlayer.HP.Value <= 0) 
                resultManager.SetResultWindow("win", player.isHuman);
                
        }
    }
}


