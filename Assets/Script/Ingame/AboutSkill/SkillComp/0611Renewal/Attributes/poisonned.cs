using TMPro;
using UnityEngine;

namespace SkillModules {
    public class poisonned : UnitAttribute {
        private TextMeshPro textPro;
        private void Start() {
            EffectSystem.Instance.ContinueEffect(EffectSystem.EffectType.POISON_GET, gameObject.GetComponent<PlaceMonster>().unitSpine.headbone);
            //PlayMangement.instance.AddSkillIcon("poison", transform);
        }

        void OnDestroy() {
            EffectSystem.Instance.DisableEffect(EffectSystem.EffectType.POISON_GET, gameObject.GetComponent<PlaceMonster>().unitSpine.headbone);
            //PlayMangement.instance.DisabelSkillIcon("poison", transform);
        }
    }
}