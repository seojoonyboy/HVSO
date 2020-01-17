using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Spine;
using Spine.Unity;
using System;
using Newtonsoft.Json;
using UIModule;
using UniRx;
using BestHTTP;
using Quest;

public class MenuSceneController : MonoBehaviour {
    [SerializeField] Transform fixedCanvas;
    [SerializeField] GameObject OptionCanvas;
    [SerializeField] HUDController hudController;
    [SerializeField] HorizontalScrollSnap windowScrollSnap;
    [SerializeField] Transform dictionaryMenu;
    [SerializeField] TMPro.TextMeshProUGUI nicknameText;
    [SerializeField] GameObject battleReadyPanel;   //대전 준비 화면
    [SerializeField] public GameObject storyLobbyPanel;    //스토리 메뉴 화면
    [SerializeField] SkeletonGraphic menuButton;
    [SerializeField] GameObject[] offObjects;
    [SerializeField] ShopManager shopManager;
    [SerializeField] GameObject dailyQuestAlarmCanvas;
    [SerializeField] public BattleReadyReward userMmrGauge;

    protected SkeletonGraphic selectedAnimation;
    private int currentPage;
    private bool buttonClicked;
    static bool isLoaded = false;
    public MyDecksLoader decksLoader;
    [SerializeField] GameObject newbiLoadingModal;  //최초 접속시 튜토리얼 강제시 등장하는 로딩 화면
    public GameObject hideModal;

    [SerializeField] GameObject reconnectingModal;  //재접속 진행시 등장하는 로딩 화면
    [SerializeField] MenuTutorialManager menuTutorialManager;
    [SerializeField] ScenarioManager scenarioManager;
    [SerializeField] BattleMenuController battleMenuController;

    public static MenuSceneController menuSceneController;

    bool isTutorialDataLoaded = false;
    GameObject quitModal;

    [SerializeField] MedalUIFormat medalUI;

    [Serializable] public class MedalUIFormat {
        public Image tierImage;
        public TMPro.TextMeshProUGUI tierName;
        public Text tierValue;
        public BattleReadyHeaderController readyHeader;
        public Image mmrUpIcon;
        public Image mmrDownIcon;
        public TMPro.TextMeshProUGUI mmrUpValue;
        public TMPro.TextMeshProUGUI mmrDownValue;
        public Slider prevMmrSlider;
        public Slider currMmrSlider;



        public void OnLeagueInfoUpdated(Enum Event_Type, Component Sender, object Param) {
            AccountManager accountManager = AccountManager.Instance;
            AccountManager.LeagueInfo info = (AccountManager.LeagueInfo)Param;
            AccountManager.LeagueInfo prevInfo = accountManager.scriptable_leagueData.prevLeagueInfo;

            tierImage.sprite = readyHeader.GetRankImage(info.rankDetail.minorRankName);
            tierName.text = info.rankDetail.minorRankName;
            tierValue.text = info.ratingPoint.ToString();

            AccountManager.RankTableRow item = accountManager.rankTable.Find(x => x.minorRankName == prevInfo.rankDetail.minorRankName);
            int prevRankIndex = -1;
            menuSceneController.userMmrGauge.SetUpReward();

            if (item != null) {
                if (item.minorRankName == "무명 병사")
                    prevRankIndex = 1;
                else if (item.minorRankName == "전략의 제왕")
                    prevRankIndex = accountManager.rankTable.Count - 1;
                else
                    prevRankIndex = accountManager.rankTable.IndexOf(item);


                mmrDownIcon.sprite = AccountManager.Instance.resource.rankIcons[accountManager.rankTable[prevRankIndex - 1].minorRankName];
                mmrUpIcon.sprite = accountManager.resource.rankIcons[accountManager.rankTable[prevRankIndex + 1].minorRankName];
            }
        }
    }

    private void Awake() {
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_TUTORIAL_PRESETTING_COMPLETE, CheckTutorial);
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_SHOP_ITEM_UPDATED, UpdateShop);
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_LEAGUE_INFO_UPDATED, medalUI.OnLeagueInfoUpdated);
        menuSceneController = this;

        #region 테스트코드
        //menuTutorialManager.ReadTutorialData();
        //scenarioManager.ReadScenarioData();
        //isTutorialDataLoaded = true;
        //menuTutorialManager.StartTutorial(MenuTutorialManager.TutorialType.TO_ORC_STORY_2);
        #endregion

        if (!isLoaded)
            isLoaded = true;
        else
            SetCardNumbersPerDic();

        menuButton.Initialize(true);
        menuButton.Update(0);
        ClickMenuButton("Main");
        EscapeKeyController.escapeKeyCtrl.AddEscape(OpenQuitModal);

        hideModal.SetActive(true);
    }

    private void QuitApp() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OpenQuitModal() {
        var fbl_translator = AccountManager.Instance.GetComponent<fbl_Translator>();
        string message = fbl_translator.GetLocalizedText("UI", "Mmenu_exitgame");
        string okBtnText = fbl_translator.GetLocalizedText("UI", "Mmenu_yes");
        string noBtnText = fbl_translator.GetLocalizedText("UI", "Mmenu_no");
        string headerText = fbl_translator.GetLocalizedText("UI", "Mmenu_check");

        quitModal = Modal.instantiate(message, Modal.Type.YESNO, QuitApp, CloseQuitModal, btnTexts: new string[] { okBtnText, noBtnText }, headerText: headerText);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(OpenQuitModal);
    }

    public void CloseQuitModal() {
        DestroyImmediate(quitModal, true);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CloseQuitModal);
        EscapeKeyController.escapeKeyCtrl.AddEscape(OpenQuitModal);
    }

    IDisposable observable_1;

    private void CheckTutorial(Enum Event_Type, Component Sender, object Param) {
        if (!isTutorialDataLoaded) {
            menuTutorialManager.ReadTutorialData();
            scenarioManager.ReadScenarioData();
            isTutorialDataLoaded = true;
        }
        string prevTutorial = PlayerPrefs.GetString("PrevTutorial");
        var etcInfos = AccountManager.Instance.userData.etcInfo;
        hudController.SetResourcesUI();

        bool needTutorial = true;

        //첫 로그인
        MenuTutorialManager.TutorialType tutorialType = MenuTutorialManager.TutorialType.NONE;
        if (PlayerPrefs.GetInt("isFirst") == 1) {
            PlayerPrefs.SetInt("isFirst", 0);
            AddNewbiController();
            PlayerPrefs.SetInt("IsQuestLoaded", 0);

            hideModal.SetActive(false);
            PlayerPrefs.SetString("Vibrate", "On");
        }
        else {
            hideModal.SetActive(true);
            //튜토리얼 남았음
            AccountManager.etcInfo tutorialCleared = etcInfos.Find(x => x.key == "tutorialCleared");
            var clearedStages = AccountManager.Instance.clearedStages;

            if (tutorialCleared == null) {
                //휴먼 튜토리얼 0-1을 진행하지 않았음
                if (!clearedStages.Exists(x => x.camp == "human" && x.stageNumber == 1)) {
                    AddNewbiController();
                    PlayerPrefs.SetString("Vibrate", "On");
                }
                else {
                    SoundManager.Instance.bgmController.PlaySoundTrack(BgmController.BgmEnum.MENU);
                    BattleConnector.canPlaySound = true;

                    //오크 튜토리얼 0-1을 진행하지 않았음
                    if (!clearedStages.Exists(x => x.camp == "orc" && x.stageNumber == 1)) {
                        tutorialType = MenuTutorialManager.TutorialType.TO_ORC_STORY;
                    }
                    else {
                        string storyUnlocked = PlayerPrefs.GetString("StoryUnlocked", "false");
                        if(storyUnlocked == "false") {
                            tutorialType = MenuTutorialManager.TutorialType.UNLOCK_TOTAL_STORY;
                        }
                        else {
                            needTutorial = false;
                            //퀘스트 제어
                        }
                    }
                }
            }
            else needTutorial = false;
        }

        //테스트 코드
        //needTutorial = true;
        //tutorialType = MenuTutorialManager.TutorialType.UNLOCK_TOTAL_STORY;
        /////////////////////////////////////////////////////////////////
        if (needTutorial) {
            if (tutorialType != MenuTutorialManager.TutorialType.NONE) {
                menuTutorialManager.StartTutorial(tutorialType);
            }
        }
        else {
            var prevScene = AccountManager.Instance.prevSceneName;
            if (prevScene == "Story") {
                StartQuestSubSet(MenuTutorialManager.TutorialType.QUEST_SUB_SET_100);
            }
            else if(prevScene == "League") {
                StartQuestSubSet(MenuTutorialManager.TutorialType.QUEST_SUB_SET_101);
            }
            else {
                needTutorial = false;
                hideModal.SetActive(false);
                menuTutorialManager.enabled = false;
            }

            SoundManager.Instance.bgmController.PlaySoundTrack(BgmController.BgmEnum.MENU);
            BattleConnector.canPlaySound = true;

            CheckDailyQuest();
        }

        //StartQuestSubSet(MenuTutorialManager.TutorialType.QUEST_SUB_SET_7);
    }

    public void CheckDailyQuest() {
        bool isQuestLoaded = Convert.ToBoolean(PlayerPrefs.GetInt("IsQuestLoaded"));
        bool isAllUnlocked = menuTutorialManager.lockController.isAllUnlocked;
        if (!isQuestLoaded && isAllUnlocked) {
            //DateTime currentTime = DateTime.UtcNow;
            //var korCurrentTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(currentTime, "Korea Standard Time");

            DateTime tommorowTime = System.DateTime.UtcNow.AddDays(1).AddHours(9.0).AddTicks(-1); //korCurrentTime.AddDays(1).AddTicks(-1);
            DateTime resetStandardTime = new DateTime(
                tommorowTime.Year,
                tommorowTime.Month,
                tommorowTime.Day,
                0,
                0,
                0
            );
            AccountManager.Instance.GetDailyQuest(OnDailyQuestRequestFinished);
            PlayerPrefs.SetInt("IsQuestLoaded", 1);
        }
    }

    public void OpenDailyQuestInstantly() {
        bool isQuestLoaded = Convert.ToBoolean(PlayerPrefs.GetInt("IsQuestLoaded"));
        bool isAllUnlocked = menuTutorialManager.lockController.isAllUnlocked;
        if (!isQuestLoaded) {
            DateTime tommorowTime = System.DateTime.UtcNow.AddDays(1).AddHours(9.0).AddTicks(-1); //korCurrentTime.AddDays(1).AddTicks(-1);
            DateTime resetStandardTime = new DateTime(
                tommorowTime.Year,
                tommorowTime.Month,
                tommorowTime.Day,
                0,
                0,
                0
            );
            AccountManager.Instance.GetDailyQuest(OnDailyQuestRequestFinished);
            PlayerPrefs.SetInt("IsQuestLoaded", 1);
        }
    }

    private void OnDailyQuestRequestFinished(HTTPRequest originalRequest, HTTPResponse response) {
        if (response.IsSuccess) {
            List<QuestData> datas = dataModules.JsonReader.Read<List<QuestData>>(response.DataAsText);
            if (dailyQuestAlarmCanvas == null) {
                Logger.LogWarning("dailyQuestAlarmCanvas를 찾을 수 없습니다!");
                return;
            }
            dailyQuestAlarmCanvas.gameObject.SetActive(true);
            dailyQuestAlarmCanvas.GetComponent<DailyQuestAlarmHandler>().ShowQuestList(datas);

            AccountManager.Instance.RequestQuestInfo();
        }
        else {
            Modal.instantiate("일일 퀘스트를 불러오는 과정에서 문제가 발생하였습니다.", Modal.Type.CHECK);
        }
    }

    private void OnUserDataUpdate(Enum Event_Type, Component Sender, object Param) {
        nicknameText.text = AccountManager.Instance.userData.nickName;
    }

    private void OnDestroy() {
        if(SoundManager.Instance != null)
            SoundManager.Instance.bgmController.StopSoundTrack();
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_TUTORIAL_PRESETTING_COMPLETE, CheckTutorial);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_SHOP_ITEM_UPDATED, UpdateShop);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_LEAGUE_INFO_UPDATED, medalUI.OnLeagueInfoUpdated);
    }

    private void Start() {
        if (AccountManager.Instance.needChangeNickName) {
            Modal.instantiate("변경하실 닉네임을 입력해 주세요.", "새로운 닉네임", AccountManager.Instance.NickName, Modal.Type.INSERT, (str) => {
                if (string.IsNullOrEmpty(str)) {
                    Modal.instantiate("빈 닉네임은 허용되지 않습니다.", Modal.Type.CHECK);
                }
                else {
                    AccountManager.Instance.ChangeNicknameReq(str);
                }
            });
        }

        //deckSettingManager.AttachDecksLoader(ref decksLoader);
        decksLoader.Load();
        AccountManager.Instance.OnCardLoadFinished.AddListener(() => SetCardNumbersPerDic());
        currentPage = 2;
        Transform buttonsParent = fixedCanvas.Find("Footer");
        TouchEffecter.Instance.SetScript();

#region 테스트 코드
        //menuTutorialManager.ReadTutorialData();
        //scenarioManager.ReadScenarioData();
        //isTutorialDataLoaded = true;
        //menuTutorialManager.StartTutorial(MenuTutorialManager.TutorialType.TO_ORC_STORY_2);
#endregion

        AccountManager.Instance.RequestTutorialPreSettings();
        if(AccountManager.Instance.visitDeckNow == 1) {
            Invoke("OnPVPClicked", 0.1f);
            AccountManager.Instance.visitDeckNow = 0;
        }

        var SelectedBattleButton = PlayerPrefs.GetString("SelectedBattleButton", "STORY");
        if(SelectedBattleButton == "STORY") {
            battleMenuController.SetMainMenuDirectPlayButton(1);
        }
        else if(SelectedBattleButton == "LEAGUE") {
            battleMenuController.SetMainMenuDirectPlayButton(0);
        }
        else {
            battleMenuController.ClearDirectPlayButton();
            //Modal.instantiate("선택된 모드 정보가 없습니다. 모드를 직접 선택해주세요!", Modal.Type.CHECK);
        }
        
    }

    public void OpenOption() {
        OptionCanvas.SetActive(true);
        EscapeKeyController.escapeKeyCtrl.AddEscape(CloseOption);
        hudController.SetHeader(HUDController.Type.RESOURCE_ONLY_WITH_BACKBUTTON);
        hudController.SetBackButton(CloseOption);
    }

    void CloseOption() {
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CloseOption);
        OptionCanvas.SetActive(false);
        hudController.SetHeader(HUDController.Type.SHOW_USER_INFO);
    }

    public void OnStoryClicked() {
        //FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MISSION_SELECT_SCENE);
        storyLobbyPanel.SetActive(true);
    }

    public void ClickMenuButton(string btnName) {
        buttonClicked = true;
        foreach(GameObject window in windowScrollSnap.ChildObjects) {
            if(window.name.Contains(btnName)) {
                windowScrollSnap.GoToScreen(window.transform.GetSiblingIndex());
            }
        }

        Transform footer = fixedCanvas.Find("InnerCanvas/Footer");
        foreach(Transform btn in footer) {
            if(btn.name == btnName) {
                int animIndex = btn.GetSiblingIndex() + 1;
                menuButton.AnimationState.SetAnimation(0, "IDLE_" + (animIndex).ToString(), true);
                btn.Find("Text").gameObject.SetActive(true);
            }
            else {
                btn.Find("Text").gameObject.SetActive(false);
            }
        }
        //fixedCanvas.Find("InnerCanvas/Footer").GetChild(currentPage).GetChild(0).gameObject.SetActive(false);
        //currentPage = pageNum;
        //windowScrollSnap.GoToScreen(currentPage - 1);
        //menuButton.AnimationState.SetAnimation(0, "IDLE_" + (currentPage + 1).ToString(), true);
        //fixedCanvas.Find("InnerCanvas/Footer").GetChild(currentPage).GetChild(0).gameObject.SetActive(true);
    }

    public void ScrollSnapButtonChange() {
        //Logger.Log("ScrollSnapButtonChange current page : " + windowScrollSnap.CurrentPage);

        Transform footer = fixedCanvas.Find("InnerCanvas/Footer");
        int pageIndex = windowScrollSnap.CurrentPageObject().GetComponent<dataModules.IntergerIndex>().Id + 1;
        menuButton.AnimationState.SetAnimation(0, "IDLE_" + (pageIndex).ToString(), true);
        //fixedCanvas.Find("InnerCanvas/Footer").GetChild(currentPage).GetChild(0).gameObject.SetActive(false);
        fixedCanvas.Find("InnerCanvas/Footer").GetChild(pageIndex).GetChild(0).gameObject.SetActive(true);

        foreach (Transform btn in footer) {
            if (windowScrollSnap.CurrentPageObject().name.Contains(btn.name)) {
                int animIndex = btn.GetSiblingIndex() + 1;
                menuButton.AnimationState.SetAnimation(0, "IDLE_" + (animIndex).ToString(), true);
                btn.Find("Text").gameObject.SetActive(true);
            }
            else {
                btn.Find("Text").gameObject.SetActive(false);
            }
        }
    }


    public void Idle(TrackEntry trackEntry = null) {
        selectedAnimation.AnimationState.SetAnimation(0, "IDLE", true);
    }

    public void SetCardNumbersPerDic() {
        int humanTotalCards = 0;
        int orcTotalCards = 0;
        int myHumanCards = 0;
        int myOrcCards = 0;
        foreach (dataModules.CollectionCard card in AccountManager.Instance.allCards) {
            if (!card.isHeroCard) {
                if (card.camp == "human") {
                    humanTotalCards++;
                    if (AccountManager.Instance.cardPackage.data.ContainsKey(card.id)) {
                        myHumanCards++;
                    }
                }
                else {
                    orcTotalCards++;
                    if (AccountManager.Instance.cardPackage.data.ContainsKey(card.id)) {
                        myOrcCards++;
                    }
                }
            }
        }
        dictionaryMenu.Find("HumanButton/CardNum").GetComponent<TMPro.TextMeshProUGUI>().text = myHumanCards.ToString() + "/" + humanTotalCards.ToString();
        dictionaryMenu.Find("OrcButton/CardNum").GetComponent<TMPro.TextMeshProUGUI>().text = myOrcCards.ToString() + "/" + orcTotalCards.ToString();
        SetCardInfoByRarelity();
    }

    public void SetCardInfoByRarelity() {
        CardDataPackage cdp = AccountManager.Instance.cardPackage;
        Transform humanBtn = dictionaryMenu.Find("HumanButton/CardRarityInfo");
        Transform orcBtn = dictionaryMenu.Find("OrcButton/CardRarityInfo");
        for (int i = 0; i < 5; i++) {
            string rarelity = humanBtn.GetChild(i).name;
            humanBtn.GetChild(i).Find("CardNum").GetComponent<Text>().text = cdp.rarelityHumanCardNum[rarelity].Count.ToString();
            humanBtn.GetChild(i).Find("NewCard").gameObject.SetActive(cdp.rarelityHumanCardCheck[rarelity].Count > 0);
            orcBtn.GetChild(i).Find("CardNum").GetComponent<Text>().text = cdp.rarelityOrcCardNum[rarelity].Count.ToString();
            orcBtn.GetChild(i).Find("NewCard").gameObject.SetActive(cdp.rarelityOrcCardCheck[rarelity].Count > 0);
        }
        for(int i = 0; i < 5; i++) {
            if(humanBtn.GetChild(i).Find("NewCard").gameObject.activeSelf || orcBtn.GetChild(i).Find("NewCard").gameObject.activeSelf) {
                menuButton.transform.Find("Dictionary").gameObject.SetActive(true);
                menuButton.transform.Find("Dictionary").GetComponent<BoneFollowerGraphic>().SetBone("ex3");
                break;
            }
            if(i == 4)
                menuButton.transform.Find("Dictionary").gameObject.SetActive(false);
        }
    }

    IEnumerator UpdateWindow() {
        yield return new WaitForSeconds(1.0f);
        while (true) {
            if (!buttonClicked && currentPage != windowScrollSnap.CurrentPage) {
                currentPage = windowScrollSnap.CurrentPage;
                //SetButtonAnimation(currentPage);
            }
            else {
                if (currentPage == windowScrollSnap.CurrentPage)
                    buttonClicked = false;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void OpenCardDictionary(bool isHuman) {
        AccountManager.Instance.dicInfo.isHuman = isHuman;
        AccountManager.Instance.dicInfo.inDic = true;
        for (int i = 0; i < offObjects.Length; i++)
            offObjects[i].SetActive(false);
        CardDictionaryManager.cardDictionaryManager.SetCardDictionary();
        EscapeKeyController.escapeKeyCtrl.AddEscape(CloseDictionary);
    }

    public void OpenHumanRarelity(string rarelity) {
        AccountManager.Instance.dicInfo.isHuman = true;
        AccountManager.Instance.dicInfo.inDic = true;
        for (int i = 0; i < offObjects.Length; i++)
            offObjects[i].SetActive(false);
        CardDictionaryManager.cardDictionaryManager.GoToRerelity(rarelity);
        EscapeKeyController.escapeKeyCtrl.AddEscape(CloseDictionary);
    }
    public void OpenOrcRarelity(string rarelity) {
        AccountManager.Instance.dicInfo.isHuman = false;
        AccountManager.Instance.dicInfo.inDic = true;
        for (int i = 0; i < offObjects.Length; i++)
            offObjects[i].SetActive(false);
        CardDictionaryManager.cardDictionaryManager.GoToRerelity(rarelity);
        EscapeKeyController.escapeKeyCtrl.AddEscape(CloseDictionary);
    }

    public void OpenHeroDictionary(bool isHuman) {
        AccountManager.Instance.dicInfo.isHuman = isHuman;
        AccountManager.Instance.dicInfo.inDic = true;
        for (int i = 0; i < offObjects.Length; i++)
            offObjects[i].SetActive(false);
        CardDictionaryManager.cardDictionaryManager.SetHeroDictionary();
        EscapeKeyController.escapeKeyCtrl.AddEscape(CloseDictionary);
    }

    public void CloseDictionary() {
        CardDictionaryManager.cardDictionaryManager.gameObject.SetActive(false);
        for (int i = 0; i < offObjects.Length; i++)
            offObjects[i].SetActive(true);
        SetCardNumbersPerDic();
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CloseDictionary);
    }

    public void AddNewbiController() {
        PlayerPrefs.SetString("StoryUnlocked", "false");
        PlayerPrefs.SetString("SelectedBattleButton", BattleMenuController.BattleType.STORY.ToString());

        var newbiComp = newbiLoadingModal.AddComponent<NewbiController>(); //첫 로그인 제어
        newbiComp.menuSceneController = this;
        newbiComp.name = "NewbiController";
        newbiComp.Init(decksLoader, scenarioManager, newbiLoadingModal);
        BattleConnector.canPlaySound = false;
    }

    private void UpdateShop(Enum Event_Type, Component Sender, object Param) {
        shopManager.SetShop();
    }

    /// <summary>
    /// 퀘스트 중간에 등장하는 강제 부분 처리
    /// </summary>
    /// <param name="type">Type</param>
    public void StartQuestSubSet(MenuTutorialManager.TutorialType type) {
        //Logger.Log("SubSet 시작 : " + type.ToString());
        menuTutorialManager.enabled = true;
        menuTutorialManager.StartQuestSubSet(type);
    }
    
    UnityEngine.Events.UnityAction tutoAction;

    public void DictionaryShowHand(Quest.QuestContentController quest, string[] args) {
        Transform cardMenu = dictionaryMenu.Find("HumanButton/CardDic");
        Instantiate(quest.manager.handSpinePrefab, cardMenu.transform, false).name = "tutorialHand";
        tutoAction = () => quest.DictionaryCardHand(args);
        UnityEngine.Events.UnityAction firstShow = null;
        firstShow = () => {
            quest.ReadyEnterCardMenu();
            cardMenu.GetComponent<Button>().onClick.RemoveListener(firstShow);
        };
        cardMenu.GetComponent<Button>().onClick.AddListener(tutoAction);
        cardMenu.GetComponent<Button>().onClick.AddListener(firstShow);
    }

    public void DictionaryRemoveHand() {
        dictionaryMenu.Find("HumanButton/CardDic").GetComponent<Button>().onClick.RemoveListener(tutoAction);
        tutoAction = null;
        Transform hand = dictionaryMenu.Find("HumanButton/CardDic").Find("tutorialHand");
        if(hand == null) return;
        Destroy(hand.gameObject);
    }

    public void RefreshRewardBubble() {
        userMmrGauge.RefreshRewardBubble();
    }


}