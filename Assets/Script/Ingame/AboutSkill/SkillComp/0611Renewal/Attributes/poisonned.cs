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
            //PlayMangement.instance.AddSkillIcon("poison", transform);
        }

        void OnDestroy() {
            TextMeshPro textPro = transform.Find("Status").GetComponent<TextMeshPro>();
            textPro.gameObject.SetActive(false);
            //PlayMangement.instance.DisabelSkillIcon("poison", transform);
        }
    }
}