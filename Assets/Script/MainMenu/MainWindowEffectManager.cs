using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 메인화면에서 보여주는 이펙트 관리 (예 : 3승 보상, 리그전 보상)
/// </summary>
public class MainWindowEffectManager : MonoBehaviour {
    [SerializeField] private ThreeWinHandler _threeWinHandler;
    [SerializeField] private BoxRewardManager _boxRewardManager;
    [SerializeField] private ResourceSpreader _storySpreader, _leagueSpreader, _threewinSpreader;
    [SerializeField] private MenuSceneController menuSceneController;
    
    public delegate void OnFinished();

    void Start() {
        StartCoroutine(MainProceed(AccountManager.Instance.mainSceneEffects));
    }

    private Queue<Effect> _effects;
    IEnumerator Dequeing() {
        var _effect = _effects.Dequeue();
        bool isDone = false;
        if (_effect.effectType == EffectType.THREE_WIN) {
            _threewinSpreader.StartSpread(20, null, () => {
                AccountManager.Instance.RequestThreeWinReward((req, res) => {
                    if (res.StatusCode == 200 || res.StatusCode == 304) {
                        var resFormat = dataModules.JsonReader.Read<NetworkManager.ThreeWinResFormat>(res.DataAsText);
                        if (resFormat.claimComplete) {
                            _threeWinHandler.GainReward();
                            isDone = true;
                        }
                    }
                });
            });
        }
        else if (_effect.effectType == EffectType.LEAGUE_REWARD) {
            var args = _effect.infos;
            try {
                var leagueType = (string) args[0];
                if (leagueType == "league") {
                    var amount = (int) args[1];
                    _leagueSpreader.StartSpread(amount, null, () => {
                        isDone = true;
                    });
                }
                else if (leagueType == "story") {
                    var amount = (int) args[1];
                    _storySpreader.StartSpread(amount, null, () => {
                        isDone = true;
                    });
                }
            }
            catch (Exception ex) {
                Logger.LogError(ex.ToString());
            }
        }
        yield return new WaitUntil(() => isDone);
    }

    IEnumerator MainProceed(Queue<Effect> effects) {
        yield return new WaitForSeconds(1.0f);
        yield return new WaitUntil(() =>
            !menuSceneController.hideModal.activeSelf
            && !menuSceneController.storyLobbyPanel.activeSelf
            && !menuSceneController.battleReadyPanel.activeSelf
        );
        _effects = effects;
        while (_effects.Count > 0) {
            yield return Dequeing();
        }
        yield return null;
    }

    public class Effect {
        public EffectType effectType;
        public object[] infos;
        
        public Effect(EffectType effectType, object[] infos) {
            this.effectType = effectType;
            this.infos = infos;
        }
    }
    
    public enum EffectType {
        THREE_WIN,
        LEAGUE_REWARD
    }
}