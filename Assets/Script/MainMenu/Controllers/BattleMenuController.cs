using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleMenuController : MonoBehaviour {
    [SerializeField] HUDController HUDController;
    [SerializeField] GameObject[] subPanels;

    BattleType battleType;
    private void OnEnable() {
        EscapeKeyController.escapeKeyCtrl.AddEscape(OnBackButton);
        HUDController.SetHeader(HUDController.Type.RESOURCE_ONLY_WITH_BACKBUTTON);
        HUDController.SetBackButton(() => {
            OnBackButton();
        });
    }

    private void OnDisable() {
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(OnBackButton);
    }

    public void OnBackButton() {
        SoundManager.Instance.PlaySound(UISfxSound.BUTTON1);

        HUDController.SetHeader(HUDController.Type.SHOW_USER_INFO);
        gameObject.SetActive(false);
    }

    public void OnSelectBattleType(int index) {
        BattleType type = (BattleType)index;

        battleType = type;
        PlayerPrefs.SetString("SelectedBattleButton", type.ToString());

        subPanels[index].SetActive(true);
    }

    public enum BattleType {
        LEAGUE = 0,
        STORY = 1
    }
}
