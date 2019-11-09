using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace victoryModule {
    public class VictoryCondition : MonoBehaviour {
        public PlayerController player, enemyPlayer;
        public IDisposable enemyWin, playerWin;

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

        public virtual void GetReward() {
            return;
        }

    }

    public class Annihilation_Match : VictoryCondition {
        public Annihilation_Match(PlayerController player_1, PlayerController player_2) : base(player_1, player_2) { }

        public override void SetCondition() {
            enemyWin = player.HP.Where(x => x <= 0).Subscribe(_ => CheckCondition()).AddTo(PlayMangement.instance.gameObject);
            playerWin = enemyPlayer.HP.Where(x => x <= 0).Subscribe(_ => CheckCondition()).AddTo(PlayMangement.instance.gameObject);
        }

        public override void CheckCondition() {            
            PlayMangement.instance.isGame = false;
            PlayMangement.instance.stopBattle = true;
            PlayMangement.instance.stopTurn = true;
            PlayMangement.instance.beginStopTurn = true;
            SoundManager.Instance.bgmController.SoundDownAfterStop();
            enemyWin.Dispose();
            playerWin.Dispose();



            SocketFormat.ResultFormat result = PlayMangement.instance.socketHandler.result;
            if(result != null) {
                PlayerController loserPlayer = (result.result == "win") ? enemyPlayer : player;
                //loserPlayer.PlayerDead();
                //EffectSystem.Instance.CameraZoomIn(loserPlayer.bodyTransform, 5.6f, 1.2f);
                
                Invoke("GetBattleResult", loserPlayer.DeadAnimationTime);
            }
            else {
                StartCoroutine(WaitGetResult());
            }
        }

        private IEnumerator WaitGetResult() {
            yield return new WaitUntil(() => PlayMangement.instance.waitShowResult == false);
            yield return new WaitUntil(() =>  PlayMangement.instance.socketHandler.result != null );
            SocketFormat.ResultFormat result = PlayMangement.instance.socketHandler.result;
            GetBattleResult();
        }

        public override void GetBattleResult() {
            if (PlayMangement.instance.SocketHandler.isOpponentPlayerDisconnected) return;
            GameResultManager resultManager = PlayMangement.instance.resultManager;
            resultManager.gameObject.SetActive(true);

            //EffectSystem.Instance.CameraZoomOut(1.2f);


            if (player.HP.Value <= 0) {
                resultManager.SetResultWindow("lose", player.isHuman);
                //SetResultBgm("lose");
            }
            else if (enemyPlayer.HP.Value <= 0) {
                resultManager.SetResultWindow("win", player.isHuman);
                //SetResultBgm("win");
            }

            
        }

        public void SetResultBgm(string result) {

            switch (result) {
                case "lose":
                    SoundManager.Instance.bgmController.PlaySoundTrack(BgmController.BgmEnum.DEFEAT);
                    break;
                case "win":
                    SoundManager.Instance.bgmController.PlaySoundTrack(BgmController.BgmEnum.VICTORY);
                    break;
                default:
                    SoundManager.Instance.bgmController.PlaySoundTrack(BgmController.BgmEnum.DEFEAT);
                    break;
            }
            

        }

        private void OnDestroy() {
            //SoundManager.Instance.bgmController.StopSoundTrack();
        }

        //보상 분배
        public override void GetReward() {
            if (ScenarioGameManagment.scenarioInstance == null) return;

        }


    }

    public class TurnLimit_Match : VictoryCondition {
        public TurnLimit_Match(PlayerController player_1, PlayerController player_2) : base(player_1, player_2) { }

        
    }

    public class ProtectObject : VictoryCondition {
        public ProtectObject(PlayerController player_1, PlayerController player_2) : base(player_1, player_2) { }

        public PlaceMonster targetUnit;
        public IDisposable targetUnitDestory;

        public override void SetCondition() {
            enemyWin = player.HP.Where(x => x <= 0).Subscribe(_ => CheckCondition()).AddTo(PlayMangement.instance.gameObject);
            playerWin = enemyPlayer.HP.Where(x => x <= 0).Subscribe(_ => CheckCondition()).AddTo(PlayMangement.instance.gameObject);
        }

        public void SetTargetUnit(PlaceMonster targetUnit) {
            targetUnitDestory = Observable.EveryUpdate().Where(_ => targetUnit.unit.currentHP <= 0).Subscribe(_ => { CheckCondition(); }).AddTo(PlayMangement.instance.gameObject); ;
        }

        public override void CheckCondition() {
            PlayMangement.instance.isGame = false;
            PlayMangement.instance.stopBattle = true;
            PlayMangement.instance.stopTurn = true;
            PlayMangement.instance.beginStopTurn = true;
            SoundManager.Instance.bgmController.SoundDownAfterStop();
            enemyWin.Dispose();
            playerWin.Dispose();
            if (targetUnitDestory != null)
                targetUnitDestory.Dispose();

            SocketFormat.ResultFormat result = PlayMangement.instance.socketHandler.result;
            if (result != null) {
                PlayerController loserPlayer = (result.result == "win") ? enemyPlayer : player;
                //loserPlayer.PlayerDead();
                //EffectSystem.Instance.CameraZoomIn(loserPlayer.bodyTransform, 5.6f, 1.2f);

                Invoke("GetBattleResult", loserPlayer.DeadAnimationTime);
            }
            else {
                StartCoroutine(WaitGetResult());
            }
        }

        private IEnumerator WaitGetResult() {
            yield return new WaitUntil(() => PlayMangement.instance.waitShowResult == false);
            yield return new WaitUntil(() => PlayMangement.instance.socketHandler.result != null);
            SocketFormat.ResultFormat result = PlayMangement.instance.socketHandler.result;
            GetBattleResult();
        }

        public override void GetBattleResult() {
            if (PlayMangement.instance.SocketHandler.isOpponentPlayerDisconnected) return;
            GameResultManager resultManager = PlayMangement.instance.resultManager;
            resultManager.gameObject.SetActive(true);

            //EffectSystem.Instance.CameraZoomOut(1.2f);


            if (player.HP.Value <= 0) {
                resultManager.SetResultWindow("lose", player.isHuman);
                //SetResultBgm("lose");
            }
            else if (enemyPlayer.HP.Value <= 0) {
                resultManager.SetResultWindow("win", player.isHuman);
                //SetResultBgm("win");
            }


        }

        public void SetResultBgm(string result) {

            switch (result) {
                case "lose":
                    SoundManager.Instance.bgmController.PlaySoundTrack(BgmController.BgmEnum.DEFEAT);
                    break;
                case "win":
                    SoundManager.Instance.bgmController.PlaySoundTrack(BgmController.BgmEnum.VICTORY);
                    break;
                default:
                    SoundManager.Instance.bgmController.PlaySoundTrack(BgmController.BgmEnum.DEFEAT);
                    break;
            }


        }

        private void OnDestroy() {
            //SoundManager.Instance.bgmController.StopSoundTrack();
        }
        public override void GetReward() {
            if (ScenarioGameManagment.scenarioInstance == null) return;
        }

    }

}


