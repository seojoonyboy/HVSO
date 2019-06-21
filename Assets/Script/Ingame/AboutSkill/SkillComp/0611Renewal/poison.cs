using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace SkillModules {
    public class poison : MonoBehaviour { 
        private TextMeshPro textPro;
        private void Start() {
            TextMeshPro textPro = transform.Find("Status").GetComponent<TextMeshPro>();
            textPro.gameObject.SetActive(true);
            textPro.text = "독성";
        }
    }
}
