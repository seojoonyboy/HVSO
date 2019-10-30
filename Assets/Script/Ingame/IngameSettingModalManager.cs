using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngameSettingModalManager : MonoBehaviour {
    [SerializeField] GameObject basePanel;
    [SerializeField] GameObject settingModal, quitModal;
    [SerializeField] Button settingBtn;

    void Awake() {
        if (settingBtn == null) return;
        settingBtn.onClick.AddListener(() => {
            basePanel.SetActive(true);
            settingModal.SetActive(true);
        });    
    }

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
        if (PlayMangement.instance.isGame == false) return;
        quitModal.SetActive(true);
    }

    public void OnCancelBtn() {
        OffAllModals();
    }

    public void Surrend() {
        if (PlayMangement.instance.isGame == false) return;
        PlayMangement.instance.SocketHandler.Surrend(null);
        OffAllModals();
    }

    private void OffAllModals() {
        quitModal.SetActive(false);
        settingModal.SetActive(false);
        basePanel.SetActive(false);
    }
}
