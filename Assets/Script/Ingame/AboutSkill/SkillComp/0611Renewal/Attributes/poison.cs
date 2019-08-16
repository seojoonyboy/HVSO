using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace SkillModules {
    public class poison : MonoBehaviour { 
        private TextMeshPro textPro;
        private void Start() {
            gameObject.GetComponent<PlaceMonster>().AddAttackProperty("poison");
            
        }

        void OnDestroy() {
            gameObject.GetComponent<PlaceMonster>().ChangeAttackProperty();
        } 
    }
}
