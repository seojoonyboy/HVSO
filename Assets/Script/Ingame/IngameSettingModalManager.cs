using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameSettingModalManager : MonoBehaviour {
    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (!gameObject.activeSelf) {
                gameObject.SetActive(true);
            }
            else {
                SurrendModal();
            }
            
        }
    }

    private void SurrendModal() {

    }
}
