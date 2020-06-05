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

public partial class BattleReadySceneController : MonoBehaviour {
    RaceType raceType;
    GameObject loadingModal;
    bool isIngameButtonClicked;

    [SerializeField] private GameObject StartButton;
    
    [SerializeField] public HUDController HudController;

    [SerializeField] private LeagueInfoUISet _leagueInfoUiSet;                //리그 기본 정보 UI
    [SerializeField] private HeaderUISet _normalUiSet;                        //연승도 연패도 승급/강등전도 아닌 기본 상태
    [SerializeField] private WinningStreakUISet _winningStreakUiSet;          //연승중일때 서브 UI
    [SerializeField] private LosingStreakModalUISet _losingStreakModalUi;     //연패중일때 서브 UI
    [SerializeField] private RankTableModalUISet _rankTableModalUiSet;        //승급/강등전 진행중일때 서브 UI
    
    private AccountManager _accountManager;
    private Fbl_Translator _translator;
    private ResourceManager _resourceManager;
    
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
        _accountManager = AccountManager.Instance;
        _translator = _accountManager.GetComponent<Fbl_Translator>();
        _resourceManager = _accountManager.resource;
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

    public void OnLeagueInfoUpdated(AccountManager.LeagueInfo data) {
        if (gameObject.activeSelf) {
            switch (data.rankingBattleState) {
                case "rank_up":
                case "rank_down":
                    SetUI("RankTable", data);
                    break;
                
                case "normal":
                    if(data.winningStreak > 0) SetUI("WinningStreak", data);
                    else if(data.losingStreak > 0) SetUI("LosingStreak", data);
                    else SetUI("Normal", data);
                    break;
            }
        }
    }

    public void SetUI(string battleState, AccountManager.LeagueInfo data = null) {
        _leagueInfoUiSet.currentRatingPointValue.text = data.ratingPoint.ToString();
        _leagueInfoUiSet.currentLeagueIcon.sprite = _resourceManager.rankIcons[data.rankDetail.id.ToString()];
        _leagueInfoUiSet.currentLeagueName.text = data.rankDetail.minorRankName;
        
        if (battleState == "RankTable") {
            _rankTableModalUiSet.panel.gameObject.SetActive(true);
            _rankTableModalUiSet.Init(data);
            
            _winningStreakUiSet.panel.gameObject.SetActive(false);
            _losingStreakModalUi.panel.gameObject.SetActive(false);
            _normalUiSet.panel.gameObject.SetActive(false);
        }

        else if(battleState == "WinningStreak") {
            _winningStreakUiSet.panel.gameObject.SetActive(true);
            _winningStreakUiSet.Init(data);
            
            _rankTableModalUiSet.panel.gameObject.SetActive(false);
            _losingStreakModalUi.panel.gameObject.SetActive(false);
            _normalUiSet.panel.gameObject.SetActive(false);
        }
        
        else if (battleState == "LosingStreak") {
            _losingStreakModalUi.panel.gameObject.SetActive(true);
            _losingStreakModalUi.Init(data);
            
            _rankTableModalUiSet.panel.gameObject.SetActive(false);
            _winningStreakUiSet.panel.gameObject.SetActive(false);
            _normalUiSet.panel.gameObject.SetActive(false);
        }

        else {
            _normalUiSet.panel.gameObject.SetActive(true);
            _normalUiSet.Init(data);
            
            _rankTableModalUiSet.panel.gameObject.SetActive(false);
            _winningStreakUiSet.panel.gameObject.SetActive(false);
            _losingStreakModalUi.panel.gameObject.SetActive(false);
        }
    }

    public void OnStartButton() {
        if (isIngameButtonClicked) {
            //Logger.Log("이미 대전 시작 버튼이 눌려진 상태");
            return;
        }

        AccountManager.Instance.startSpread = true;
        AccountManager.Instance.beforeBox = AccountManager.Instance.userResource.supplyBox;
        AccountManager.Instance.beforeSupply = AccountManager.Instance.userResource.supply;

        string race = PlayerPrefs.GetString("SelectedRace").ToLower();
        string selectedDeckId = PlayerPrefs.GetString("SelectedDeckId").ToLower();
        
        selectedDeck.deckValidate = true;    //test code
        
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
        StartButton.SetActive(false);
        
        SoundManager.Instance.PlaySound(UISfxSound.BUTTON1);
        HudController.SetHeader(HUDController.Type.SHOW_USER_INFO);
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

    [Serializable]
    public class HeaderUISet {
        public GameObject panel;

        public virtual void Init(AccountManager.LeagueInfo data) { }
    }

    [Serializable]
    public class WinningStreakUISet : HeaderUISet {
        public TextMeshProUGUI streakText;
        public TextMeshProUGUI bonusDescText, subBonusDescText;

        public override void Init(AccountManager.LeagueInfo data) {
            var translator = instance._translator;
            string msg = translator.GetLocalizedText("MainUI", "ui_page_league_winstreak");
            msg = msg.Replace("{n}", data.winningStreak.ToString());
            streakText.text = msg;
            
            string descMsg = translator.GetLocalizedText("MainUI", "ui_ingame_addmedal");
            descMsg = descMsg.Replace("{n}", "4");
            bonusDescText.text = descMsg;
        }
    }

    [Serializable]
    public class LosingStreakModalUISet : WinningStreakUISet {
        public override void Init(AccountManager.LeagueInfo data) {
            var translator = instance._translator;
            string msg = translator.GetLocalizedText("MainUI", "ui_page_league_losestreak");
            msg = msg.Replace("{n}", data.losingStreak.ToString());
            streakText.text = msg;
        }
    }

    [Serializable]
    public class RankTableModalUISet : HeaderUISet {
        public GameObject slotParent;
        public TextMeshProUGUI leftHeader;
        
        public override void Init(AccountManager.LeagueInfo data) {
            var translator = instance._translator;
            bool isRankUpBattle = data.rankDetail.rankUpBattleCount != null;
            var rankUpCondition =
                isRankUpBattle ? data.rankDetail.rankUpBattleCount : data.rankDetail.rankDownBattleCount;
            
            if (isRankUpBattle) {
                string msg = translator.GetLocalizedText("MainUI", "ui_page_promobattle");
                leftHeader.text = "1vs1 " + msg;
            }
            else {
                //string msg = translator.GetLocalizedText("MainUI", "ui_page_promobattle");
                //번역 없음
                leftHeader.text = "1vs1 강등전";
            }
            
            foreach (Transform slot in slotParent.transform) {
                slot.Find("Win").gameObject.SetActive(false);
                slot.Find("Lose").gameObject.SetActive(false);
                slot.gameObject.SetActive(false);
            }

            int slotCnt = rankUpCondition.battles;
            for (int i = 0; i < slotCnt; i++) {
                slotParent.transform.GetChild(i).gameObject.SetActive(true);
            }

            for (int i = 0; i < data.rankingBattleCount.Length; i++) {
                var slotObj = slotParent.transform.GetChild(i);
                if (data.rankingBattleCount[i]) {
                    slotObj.transform.Find("Win").gameObject.SetActive(true);
                }
                else slotObj.transform.Find("Lose").gameObject.SetActive(true);
            }
        }
    }

    [Serializable]
    public class LeagueInfoUISet : HeaderUISet {
        public Image currentLeagueIcon;
        public Text currentRatingPointValue;
        public TextMeshProUGUI currentLeagueName;

        public override void Init(AccountManager.LeagueInfo data) {
            
        }
    }
}

/// <summary>
/// 시즌 타이머 관련 처리
/// </summary>
public partial class BattleReadySceneController {
    [SerializeField] private TextMeshProUGUI remainTimeText;
    public void SetSoftResetTimer() {
        //TODO : 다음 softReset까지 남은 시간 server 처리 필요
        
    }
}

/// <summary>
/// 부대(덱) 관련 처리
/// </summary>
public partial class BattleReadySceneController {
    [SerializeField] private ScrollRect _scrollRect;
    GameObject selectedObj = null;
    
    public void LoadMyDecks(object parm) {
        foreach (Transform deckObj in _scrollRect.content) {
            deckObj.gameObject.SetActive(false);
        }
        
        var res = (BestHTTP.HTTPResponse) parm;
        var result = JsonReader.Read<Decks>(res.DataAsText);
        
        if (_accountManager == null) _accountManager = AccountManager.Instance;
        
        _accountManager.orcDecks = result.orc;
        _accountManager.humanDecks = result.human;

        var humanDecks = _accountManager.humanDecks;
        var orcDecks = _accountManager.orcDecks;

        for (int i = 0; i < humanDecks.Count; i++) {
            int idx = i;
            GameObject deckObject = _scrollRect.content.GetChild(i).gameObject;
            deckObject.SetActive(true);
            var deckHandler = deckObject.GetComponent<DeckHandler>();
            deckHandler.SetNewDeck(humanDecks[i]);
            
            Button button = deckObject.transform.GetChild(0).Find("HeroImg").GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { OnDeckSelected(deckHandler.DECKID, "human", humanDecks[idx], deckObject); });
        }
        int index = 0;
        for (int i = humanDecks.Count; i < humanDecks.Count + orcDecks.Count; i++) {
            int idx = index;
            GameObject deckObject = _scrollRect.content.GetChild(i).gameObject;
            deckObject.SetActive(true);
            var deckHandler = deckObject.GetComponent<DeckHandler>();
            deckHandler.SetNewDeck(orcDecks[index]);
            
            Button button = deckObject.transform.GetChild(0).Find("HeroImg").GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { OnDeckSelected(deckHandler.DECKID, "orc", orcDecks[idx], deckObject); });
            
            index++;
        }
    }
    
    public void OnDeckSelected(string deckId, string camp, Deck deck, GameObject obj) {
        PlayerPrefs.SetString("SelectedRace", camp);
        PlayerPrefs.SetString("SelectedDeckId", deckId);
        PlayerPrefs.SetString("SelectedBattleType", "league");

        selectedDeck = deck;
        PlayerPrefs.SetString("selectedHeroId", deck.heroId);

        selectedObj = obj;

        StartButton.SetActive(true);
    }
}
