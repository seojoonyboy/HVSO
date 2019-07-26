using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SkillModules {
    public class poisonned : MonoBehaviour {
        private TextMeshPro textPro;
        private void Start() {
            TextMeshPro textPro = transform.Find("Status").GetComponent<TextMeshPro>();
            textPro.gameObject.SetActive(true);
            textPro.text = "독 걸림";
            EffectSystem.Instance.ContinueEffect(EffectSystem.EffectType.POISON_GET, gameObject.GetComponent<PlaceMonster>().unitSpine.headbone);
            //PlayMangement.instance.AddSkillIcon("poison", transform);
        }

        void OnDestroy() {
            TextMeshPro textPro = transform.Find("Status").GetComponent<TextMeshPro>();
            textPro.gameObject.SetActive(false);
            EffectSystem.Instance.DisableEffect(EffectSystem.EffectType.POISON_GET, gameObject.GetComponent<PlaceMonster>().unitSpine.headbone);
            //PlayMangement.instance.DisabelSkillIcon("poison", transform);
        }
    }
}