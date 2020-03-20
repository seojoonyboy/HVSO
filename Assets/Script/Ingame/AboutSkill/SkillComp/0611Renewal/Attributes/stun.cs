using TMPro;
using UnityEngine;

namespace SkillModules {
    public class stun : UnitAttribute {
        private TextMeshPro textPro;
        private void Start() {
            EffectSystem.Instance.ContinueEffect(EffectSystem.EffectType.STUN, gameObject.GetComponent<PlaceMonster>().unitSpine.headbone);
        }

        void OnDestroy() {
            EffectSystem.Instance.DisableEffect(EffectSystem.EffectType.STUN, gameObject.GetComponent<PlaceMonster>().unitSpine.headbone);
        }
    }
}
