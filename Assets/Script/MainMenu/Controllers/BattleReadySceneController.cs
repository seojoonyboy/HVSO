using dataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Bolt;
using System;
using TMPro;
using UnityEngine.UI.Extensions;

public class BattleReadySceneController : MonoBehaviour {
    public Toggle[] battleTypeToggles;
    public Button[] raceTypeButtons;

    BattleType selectedBattleType;
    RaceType raceType;

    List<GameObject> allDeckObjects = new List<GameObject>();
    public GameObject CardListModal, CardInfoModal;
    GameObject loadingModal;
    bool isIngameButtonClicked;

    [SerializeField] TextMeshProUGUI pageText;
    [SerializeField] public GameObject EmptyMsgShowPanel;
    [SerializeField] public GameObject ButtonGlowEffect;
    [SerializeField] HorizontalScrollSnap BattleTypeHorizontalScrollSnap;

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
        Variables.Saved.Set("SelectedDeckId", "");
        Variables.Saved.Set("SelectedRace", RaceType.NONE);
    }

    public void ChangePageText(string msg) {
        pageText.text = msg;
    }

    public void OnStartButton() {
        if (isIngameButtonClicked) {
            Logger.Log("이미 대전 시작 버튼이 눌려진 상태");
            return;
        }

        string race = Variables.Saved.Get("SelectedRace").ToString().ToLower();
        string selectedDeckId = Variables.Saved.Get("SelectedDeckId").ToString().ToLower();
        if (race != null && !string.IsNullOrEmpty(selectedDeckId)) {
            if (selectedDeck.deckValidate) {
                isIngameButtonClicked = true;
                SceneManager.Instance.LoadScene(SceneManager.Scene.CONNECT_MATCHING_SCENE);
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
        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
        SceneManager.Instance.LoadScene(SceneManager.Scene.MAIN_SCENE);
    }

    public void OnDeckListButton() {
        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
        SceneManager.Instance.LoadScene(SceneManager.Scene.DECK_LIST_SCNE);
    }

    public void ChangeBattleType(BattleType type) {
        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
        selectedBattleType = type;
    }

    public void ChangeRaceType(RaceType type) {
        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
        Variables.Saved.Set("SelectedRace", type);
        Logger.Log(type);
        raceType = type;

        if (EmptyMsgShowPanel.activeSelf) EmptyMsgShowPanel.SetActive(false);
    }

    public void ChangeDeck(string deckId) {
        var msg = string.Format("{0} 선택됨", deckId);
        Variables.Saved.Set("SelectedDeckId", deckId);

        int isNum = 0;
        if(int.TryParse(deckId, out isNum)) {
            Variables.Saved.Set("SelectedDeckType", "custom");
        }
        else {
            Variables.Saved.Set("SelectedDeckType", "basic");
        }

        ButtonGlowEffect.SetActive(true);
    }

    public void ChangeBattleType(int pageIndex) {
        string type = "multi";
        switch (pageIndex) {
            case 0:
            default:
                type = "multi";
                break;
            case 1:
                type = "solo";
                break;
        }
        Variables.Saved.Set("SelectedBattleType", type);
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

    public void OnClickModifyButton(GameObject target) {

    }

    public void OnClickCardListModal() {
        CardListModal.SetActive(true);
    }

    public void OffCardListModal() {
        CardListModal.SetActive(false);
    }

    public void NextBattleType() {
        Logger.Log("Next Battle Type");
        BattleTypeHorizontalScrollSnap.NextScreen();
    }

    public void PrevBattleType() {
        BattleTypeHorizontalScrollSnap.PreviousScreen();
    }
}
