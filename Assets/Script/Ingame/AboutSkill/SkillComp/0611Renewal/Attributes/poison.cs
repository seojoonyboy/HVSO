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
            gameObject.GetComponent<PlaceMonster>().AddAttackProperty("poison");
            
        }

        void OnDestroy() {
            TextMeshPro textPro = transform.Find("Status").GetComponent<TextMeshPro>();
            textPro.gameObject.SetActive(false);
            gameObject.GetComponent<PlaceMonster>().ChangeAttackProperty();
        } 
    }
}
