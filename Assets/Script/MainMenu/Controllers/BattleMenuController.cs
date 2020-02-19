using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleMenuController : MonoBehaviour {
    [SerializeField] HUDController HUDController;
    [SerializeField] GameObject[] subPanels, spineImages;
    [SerializeField] Button directModePlayButton;   //메인화면 바로 플레이 버튼    

    public Sprite[] modeImages;
    int battleTypeIndex = 0;

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
        battleTypeIndex = index;

        BattleType type = (BattleType)index;

        battleType = type;
        PlayerPrefs.SetString("SelectedBattleButton", type.ToString());

        SetMainMenuDirectPlayButton(index);
        OnBackButton();

        Logger.Log("OnSelectBattleType");
    }

    public void SetMainMenuDirectPlayButton(int type) {
        battleTypeIndex = type;

        directModePlayButton.onClick.RemoveAllListeners();
        if (type == 0) {
            spineImages[0].SetActive(true);
            spineImages[1].SetActive(false);
        }
        else if (type == 1) {
            spineImages[1].SetActive(true);
            spineImages[0].SetActive(false);
        }

        directModePlayButton.onClick.AddListener(() => {
            subPanels[type].SetActive(true);
        });
    }

    public void ClearDirectPlayButton() {
        //mainSceneImage.enabled = false;
        directModePlayButton.onClick.RemoveAllListeners();
    }
    
    //public void NextModeButton() {
    //    battleTypeIndex++;
    //    if (battleTypeIndex > modeButtons.Length - 1) battleTypeIndex = 0;

    //    BattleType type = (BattleType)battleTypeIndex;
    //    PlayerPrefs.SetString("SelectedBattleButton", type.ToString());

    //    SetMainMenuDirectPlayButton(battleTypeIndex);
    //}

    //public void PrevModeButton() {
    //    battleTypeIndex--;
    //    if (battleTypeIndex < 0) battleTypeIndex = modeButtons.Length - 1;

    //    BattleType type = (BattleType)battleTypeIndex;
    //    PlayerPrefs.SetString("SelectedBattleButton", type.ToString());

    //    SetMainMenuDirectPlayButton(battleTypeIndex);
    //}

    public enum BattleType {
        LEAGUE = 0,
        STORY = 1
    }
}
