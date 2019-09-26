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
            player.HP.Where(x => x <= 0).Subscribe(_ => CheckCondition()).AddTo(PlayMangement.instance.gameObject);
            enemyPlayer.HP.Where(x => x <= 0).Subscribe(_ => CheckCondition()).AddTo(PlayMangement.instance.gameObject);
        }

        public override void CheckCondition() {            
            PlayMangement.instance.isGame = false;
            PlayMangement.instance.StopAllCoroutines();
            PlayerController loserPlayer = (player.HP.Value <= 0) ? player : enemyPlayer;
            loserPlayer.PlayerDead();
            EffectSystem.Instance.CameraZoomIn(loserPlayer.bodyTransform, 5.6f, 1.2f);
            StartCoroutine(WaitDeadAnimation(loserPlayer));
        }

        private IEnumerator WaitDeadAnimation(PlayerController loser) {
            yield return new WaitForSeconds(loser.DeadAnimationTime);
            GetBattleResult();
        }

        public override void GetBattleResult() {
            if (PlayMangement.instance.SocketHandler.isOpponentPlayerDisconnected) return;
            GameResultManager resultManager = PlayMangement.instance.resultManager;
            resultManager.gameObject.SetActive(true);

            EffectSystem.Instance.CameraZoomOut(1.2f);


            if (player.HP.Value <= 0) 
                resultManager.SetResultWindow("lose", player.isHuman);
            else if (enemyPlayer.HP.Value <= 0) 
                resultManager.SetResultWindow("win", player.isHuman);
                
        }
    }
}


