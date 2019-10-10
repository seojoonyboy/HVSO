using TMPro;
using UnityEngine;

namespace SkillModules {
    public class stun : UnitAttribute {
        private TextMeshPro textPro;
        private void Start() {
            EffectSystem.Instance.ContinueEffect(EffectSystem.EffectType.STUN, gameObject.GetComponent<PlaceMonster>().unitSpine.headbone);
            //PlayMangement.instance.AddSkillIcon("stun", transform);
        }

        void OnDestroy() {
            EffectSystem.Instance.DisableEffect(EffectSystem.EffectType.STUN, gameObject.GetComponent<PlaceMonster>().unitSpine.headbone);
            //PlayMangement.instance.DisabelSkillIcon("stun", transform);
        }
    }
}
