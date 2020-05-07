using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UniRx;

namespace SkillModules {
    public class stun : UnitAttribute {
        private TextMeshPro textPro;
        private void Start() {
            EffectSystem.Instance.ContinueEffect(EffectSystem.EffectType.STUN, gameObject.GetComponent<PlaceMonster>().unitSpine.headbone);
            //OnEvent onEvent = PlayMangement.instance.OnBattleTurnEnd;
            //Observable.FromEvent<OnEvent>(h => () => h(onEvent) , h => onEvent += h, h => onEvent -= h).First().Subscribe(_ => { stunRemove();}).AddTo(GetComponent<stun>());
            PlayMangement.instance.EventHandler.AddListener(IngameEventHandler.EVENT_TYPE.END_BATTLE_TURN, this.stunRemove);
        }

        void stunRemove(System.Enum event_type, Component Sender, object Param) {
            EffectSystem.Instance.DisableEffect(EffectSystem.EffectType.STUN, gameObject.GetComponent<PlaceMonster>().unitSpine.headbone);
            Destroy(gameObject.GetComponent<stun>());
        }

        private void OnDestroy() {
            PlayMangement.instance.EventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.END_BATTLE_TURN, this.stunRemove);
        }
    }
}
