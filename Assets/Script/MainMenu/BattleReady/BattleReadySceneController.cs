using dataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.UI.Extensions;

public class BattleReadySceneController : MonoBehaviour {
    public Toggle[] battleTypeToggles;
    public Button[] raceTypeButtons;

    BattleType selectedBattleType;
    RaceType raceType;

    List<GameObject> allDeckObjects = new List<GameObject>();
    GameObject loadingModal;
    bool isIngameButtonClicked;

    [SerializeField] public GameObject EmptyMsgShowPanel;
    [SerializeField] HorizontalScrollSnap BattleTypeHorizontalScrollSnap;
    [SerializeField] public HUDController HudController;
    [SerializeField] HorizontalScrollSnap ScrollSnap;

    public Deck selectedDeck;

    IEnumerator Start() {
        yield return null;
        isIngameButtonClicked = false;

#if UNITY_EDITOR
#else
        Destroy(BattleTypeHorizontalScrollSnap.transform
            .GetChild(0)
            .GetChild(2)
            .gameObject
        );
#endif
    }

    private void OnEnable() {
        ChangeBattleType(0);
        
        //ScrollSnap.ChangePage(0);
        PlayerPrefs.SetString("SelectedDeckId", "");
        PlayerPrefs.SetString("SelectedRace", RaceType.NONE.ToString());

        HudController.SetHeader(HUDController.Type.RESOURCE_ONLY_WITH_BACKBUTTON);
        HudController.SetBackButton(() => {
            OnBackButton();
        });
    }

    void OnDisable() {
        RectTransform rt = ScrollSnap.transform.Find("Content").GetComponent<RectTransform>();
        rt.offsetMin = new Vector2(0, rt.offsetMin.y);
    }

    public void OnStartButton() {
        if (isIngameButtonClicked) {
            Logger.Log("이미 대전 시작 버튼이 눌려진 상태");
            return;
        }
        string race = PlayerPrefs.GetString("SelectedRace").ToLower();
        string selectedDeckId = PlayerPrefs.GetString("SelectedDeckId").ToLower();
        if (race != null && !string.IsNullOrEmpty(selectedDeckId)) {
            if (selectedDeck.deckValidate) {
                isIngameButtonClicked = true;
                FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.CONNECT_MATCHING_SCENE);
            }
            else {
                Modal.instantiate("유효하지 않은 덱입니다.", Modal.Type.CHECK);
            }
        }
        else {
            if (race == "none") Logger.Log("종족을 선택해야 합니다.");
            if (string.IsNullOrEmpty(selectedDeckId)) Logger.Log("덱을 선택해야 합니다.");

            if(race == "none") {
                Modal.instantiate("종족을 선택해 주세요.", Modal.Type.CHECK);
            }
            else if(string.IsNullOrEmpty(selectedDeckId)) {
                Modal.instantiate("덱을 선택해 주세요.", Modal.Type.CHECK);
            }
        }
        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
    }

    public void OnBackButton() {
        //SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);

        //foreach (Button btn in raceTypeButtons) {
        //    GameObject obj = btn.gameObject;
        //    obj.GetComponent<BooleanIndex>().isOn = false;
        //    RaceTypeToggleHandler toggleHandler = obj.GetComponent<RaceTypeToggleHandler>();
        //    toggleHandler.SwitchOff();
        //    toggleHandler.ClearList();
        //}
        //EmptyMsgShowPanel.SetActive(true);
        HudController.SetHeader(HUDController.Type.SHOW_USER_INFO);

        gameObject.SetActive(false);
    }

    public void ChangeBattleType(BattleType type) {
        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
        selectedBattleType = type;
    }

    public void ChangeRaceType(RaceType type) {
        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
        PlayerPrefs.SetString("SelectedRace", type.ToString());
        Logger.Log(type);
        raceType = type;

        if (EmptyMsgShowPanel.activeSelf) EmptyMsgShowPanel.SetActive(false);
    }

    /// <summary>
    /// 지금은 사용하지 않음
    /// </summary>
    /// <param name="deckId"></param>
    public void ChangeDeck(string deckId) {
        var msg = string.Format("{0} 선택됨", deckId);
        PlayerPrefs.SetString("SelectedDeckId", deckId);
        int isNum = 0;
        if(int.TryParse(deckId, out isNum)) {
            PlayerPrefs.SetString("SelectedDeckType", "custom");
        }
        else {
            PlayerPrefs.SetString("SelectedDeckType", "basic");
        }
    }

    public void ChangeBattleType(int pageIndex) {
        //string type = "multi";
        //switch (pageIndex) {
        //    case 0:
        //    default:
        //        type = "multi";
        //        break;
        //    case 1:
        //        type = "solo";
        //        break;
        //}
        //PlayerPrefs.SetString("SelectedBattleType", type);
    }

    public enum BattleType {
        AI = 0,
        CASUAL = 1,
        RANK = 2
    }

    public enum RaceType {
        HUMAN = 0,
        ORC = 1,
        NONE = 2
    }

    public void NextBattleType() {
        Logger.Log("Next Battle Type");
        BattleTypeHorizontalScrollSnap.NextScreen();
    }

    public void PrevBattleType() {
        BattleTypeHorizontalScrollSnap.PreviousScreen();
    }
}
