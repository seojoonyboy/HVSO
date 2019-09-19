using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameSettingModalManager : MonoBehaviour {
    [SerializeField] GameObject basePanel;
    [SerializeField] GameObject settingModal, quitModal;

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if(!basePanel.activeSelf) basePanel.SetActive(true);

            if (!settingModal.activeSelf) {
                settingModal.SetActive(true);
            }
            else {
                quitModal.SetActive(true);
            }
        }
    }

    public void OnSurrendBtn() {
        quitModal.SetActive(true);
    }

    public void OnCancelBtn() {
        OffAllModals();
    }

    private void OffAllModals() {
        quitModal.SetActive(false);
        settingModal.SetActive(false);
        basePanel.SetActive(false);
    }
}
