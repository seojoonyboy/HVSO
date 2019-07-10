using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SkillModules {
    public class stun : MonoBehaviour {
        private TextMeshPro textPro;
        private void Start() {
            TextMeshPro textPro = transform.Find("Status").GetComponent<TextMeshPro>();
            textPro.gameObject.SetActive(true);
            textPro.text = "스턴";
            //PlayMangement.instance.AddSkillIcon("stun", transform);
        }

        void OnDestroy() {
            TextMeshPro textPro = transform.Find("Status").GetComponent<TextMeshPro>();
            textPro.gameObject.SetActive(false);
            //PlayMangement.instance.DisabelSkillIcon("stun", transform);
        }
    }
}
