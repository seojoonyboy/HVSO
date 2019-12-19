using dataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.UI.Extensions;
using UnityEngine.Events;

public class BattleReadySceneController : MonoBehaviour {
    public Toggle[] battleTypeToggles;
    public Button[] raceTypeButtons;
    RaceType raceType;

    List<GameObject> allDeckObjects = new List<GameObject>();
    GameObject loadingModal;
    bool isIngameButtonClicked;

    [SerializeField] public GameObject EmptyMsgShowPanel;
    [SerializeField] HorizontalScrollSnap BattleTypeHorizontalScrollSnap;
    [SerializeField] public HUDController HudController;
    [SerializeField] HorizontalScrollSnap ScrollSnap;
    [SerializeField] GameObject deckListPanel;
    [SerializeField] MenuSceneController menuSceneController;
    [SerializeField] RewardsProvider rewardsProvider;
    [SerializeField] BattleReadyHeaderController battleReadyHeaderController;
    [SerializeField] BattleMenuController BattleMenuController;

    public Deck selectedDeck;
    public LeagueData userLeagueData;

    private static BattleReadySceneController m_instance;
    public static BattleReadySceneController instance {
        get {
            if (m_instance == null) m_instance = FindObjectOfType<BattleReadySceneController>();

            return m_instance;
        }
    }

    void Awake() {
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_LEAGUE_INFO_UPDATED, OnLeagueInfoUpdated);
    }

    void OnEnable() {
        isIngameButtonClicked = false;

        //ScrollSnap.ChangePage(0);
        PlayerPrefs.SetString("SelectedDeckId", "");
        PlayerPrefs.SetString("SelectedRace", RaceType.NONE.ToString());

        HudController.SetHeader(HUDController.Type.RESOURCE_ONLY_WITH_BACKBUTTON);
        HudController.SetBackButton(() => {
            OnBackButton();
        });
        EscapeKeyController.escapeKeyCtrl.AddEscape(OnBackButton);

        AccountManager.Instance.RequestLeagueInfo();
    }

    void OnDisable() {
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(OnBackButton);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_LEAGUE_INFO_UPDATED, OnLeagueInfoUpdated);
    }

    void OnDestroy() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_LEAGUE_INFO_UPDATED, OnLeagueInfoUpdated);
    }

    private void OnRankUp() {

    }

    private void OnLeagueInfoUpdated(Enum Event_Type, Component Sender, object Param) {
        AccountManager.LeagueInfo info = (AccountManager.LeagueInfo)Param;
        rewardsProvider.Provide();
        battleReadyHeaderController.SetUI(info);
    }

    public void OnStartButton() {
        if (isIngameButtonClicked) {
            //Logger.Log("이미 대전 시작 버튼이 눌려진 상태");
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
                if(selectedDeck.totalCardCount < 40) {
                    Modal.instantiate("부대에 포함된 카드의 수가 부족합니다.", Modal.Type.CHECK);
                }
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
        gameObject.SetActive(false);
        SoundManager.Instance.PlaySound(UISfxSound.BUTTON1);
        HudController.SetHeader(HUDController.Type.SHOW_USER_INFO);
    }

    public enum RaceType {
        HUMAN = 0,
        ORC = 1,
        NONE = 2
    }
}
