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
    [SerializeField] BattleReadyHeaderController battleReadyHeaderController;

    public Deck selectedDeck;
    public LeagueData userLeagueData;

    private static BattleReadySceneController m_instance;
    public static BattleReadySceneController instance {
        get {
            if (m_instance == null) m_instance = FindObjectOfType<BattleReadySceneController>();

            return m_instance;
        }
    }

    private void Awake() {
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_LEAGUE_INFO_UPDATED, OnLeagueInfoUpdated);
    }

    void OnEnable() {
        isIngameButtonClicked = false;

        //ScrollSnap.ChangePage(0);
        PlayerPrefs.SetString("SelectedDeckId", "");
        PlayerPrefs.SetString("SelectedRace", RaceType.NONE.ToString());

        HudController.SetHeader(HUDController.Type.BATTLE_READY_CANVAS);
        HudController.SetBackButton(() => {
            OnBackButton();
        });
        var seasonDesc = HudController.transform
            .GetChild(0)
            .GetChild(0)
            .Find("SeasonDesc/Description")
            .GetComponent<TextMeshProUGUI>();
        
        var listOfReplacePair = new List<FblTextConverter.ReplacePair>();
        listOfReplacePair.Add(new FblTextConverter.ReplacePair("{d}", "0"));
        listOfReplacePair.Add(new FblTextConverter.ReplacePair("{h}", "00"));
        listOfReplacePair.Add(new FblTextConverter.ReplacePair("{m}", "00"));
        listOfReplacePair.Add(new FblTextConverter.ReplacePair("{s}", "00"));
        seasonDesc.GetComponent<FblTextConverter>().InsertText(listOfReplacePair);
        
        EscapeKeyController.escapeKeyCtrl.AddEscape(OnBackButton);
        AccountManager.Instance.RequestLeagueInfo();
    }

    void OnDisable() {
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(OnBackButton);
    }

    void OnDestroy() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_LEAGUE_INFO_UPDATED, OnLeagueInfoUpdated);
    }

    private void OnLeagueInfoUpdated(Enum Event_Type, Component Sender, object Param) {
        AccountManager.LeagueInfo info = (AccountManager.LeagueInfo)Param;
        //rewardsProvider.Provide();
        if (gameObject.activeSelf) {
            battleReadyHeaderController.SetUI(info);    
        }
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
                    var translator = AccountManager.Instance.GetComponent<Fbl_Translator>();
                    string message = translator.GetLocalizedText("UIPopup", "ui_popup_cantusedeck");
                    string okBtn = translator.GetLocalizedText("UIPopup", "ui_popup_check");
                    string header = translator.GetLocalizedText("UIPopup", "ui_popup_check");

                    Modal.instantiate(
                        message, 
                        Modal.Type.CHECK,
                        btnTexts: new string[] { okBtn },
                        headerText: header
                    );
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

    public void RefreshBubble() {
        battleReadyHeaderController.rewarder.RefreshRewardBubble();
    }

    public void OnSeosonRewardInfo() {
        HudController.SetBackButton(() => {
            OffSeosonRewardInfo();
        });
        transform.GetChild(0).Find("SeosonRewardInfo").gameObject.SetActive(true);
        EscapeKeyController.escapeKeyCtrl.AddEscape(OffSeosonRewardInfo);
    }

    void OffSeosonRewardInfo() {
        HudController.SetBackButton(() => {
            OnBackButton();
        });
        transform.GetChild(0).Find("SeosonRewardInfo").gameObject.SetActive(false);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(OffSeosonRewardInfo);
    }


    public enum RaceType {
        HUMAN = 0,
        ORC = 1,
        NONE = 2
    }
}
