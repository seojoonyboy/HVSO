using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace SkillModules {
    public class nightaction : MonoBehaviour {
        // Start is called before the first frame update
        private TextMeshPro textPro;
        void Start() {
            TextMeshPro textPro = transform.Find("Status").GetComponent<TextMeshPro>();
            textPro.gameObject.SetActive(true);
            textPro.text = "야간작전";
            gameObject.GetComponent<PlaceMonster>().AddAttackProperty("nightaction");

        }

        private void OnDestroy() {
            TextMeshPro textPro = transform.Find("Status").GetComponent<TextMeshPro>();
            textPro.gameObject.SetActive(false);
            gameObject.GetComponent<PlaceMonster>().ChangeAttackProperty();
        }

    }
}