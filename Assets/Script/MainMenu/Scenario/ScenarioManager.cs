using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using Tutorial;
using UnityEngine.Events;
using System;
using System.Linq;
using System.IO;
using dataModules;
using System.Text;

public class ScenarioManager : SerializedMonoBehaviour
{
    public static ScenarioManager Instance { get; private set; }
    public ShowSelectRace human, orc;
    public string heroID;
    public bool isIngameButtonClicked = false;
    public GameObject stageCanvas;
    public GameObject deckContent;

    public GameObject selectedDeckObject = null;
    public object selectedDeck;
    private int currentPageIndex = 0;   //현재 페이지
    private int maxPageIndex = 0;       //최대 페이지

    public GameObject headerMenu;
    public bool isHuman;
    
    public ChapterData selectedChapterData;
    public ChallengeData selectedChallengeData;
    public GameObject selectedChapterObject;

    [SerializeField] GameObject orcDeckPrefab;
    [SerializeField] GameObject humanDeckPrefab;
    [SerializeField] GameObject enemyHeroInfoModal;

    [SerializeField] Image backgroundImage;
    [SerializeField] BattleMenuController BattleMenuController;
    [SerializeField] Dictionary<string, Sprite> stroyBackgroundImages;
    [SerializeField] Dictionary<string, Sprite> storyHeroPortraits;
    [SerializeField] MenuSceneController menuSceneController;

    //파일 경로
    [FilePath] public string 
        human_scenarioDataPath, 
        orc_scenarioDataPath,
        human_challengeDataPath,
        orc_challengeDataPath;

    //파일 읽어 세팅함
    public List<ChapterData> human_chapterDatas, orc_chapterDatas;
    public List<ChallengeData> human_challengeDatas, orc_challengeDatas;

    //읽어온 파일을 재분류함
    Dictionary<int, List<ChapterData>> pageHumanStoryList, pageOrcStoryList;

    public static UnityEvent OnLobbySceneLoaded = new UnityEvent();
    private void Awake() {
        Instance = this;
        OnLobbySceneLoaded.Invoke();
        isIngameButtonClicked = false;
    }

    private void OnDestroy() {
        Instance = null;
    }

    [SerializeField] HUDController HUDController;
    void OnEnable() {
        ReadScenarioData();
        
        SetBackButton(1);
        EscapeKeyController.escapeKeyCtrl.AddEscape(OnBackButton);
        
        int prevChapter = int.Parse(PlayerPrefs.GetString("ChapterNum", "0"));
        string prevRace = PlayerPrefs.GetString("SelectedRace").ToLower();

        if (MainSceneStateHandler.Instance.GetState("IsTutorialFinished")) {
            if (prevRace == "human") OnHumanCategories();
            else OnOrcCategories();
        
            SetSubStoryListInfo(prevChapter);
            SetChapterHeaderAlert(prevChapter);
        }
        else {
            OnHumanCategories();
        }
    }

    void OnDisable() {
        orc.StageCanvas.transform.Find("HUD/StageSelect/Buttons").gameObject.SetActive(false);
        human.StageCanvas.transform.Find("HUD/StageSelect/Buttons").gameObject.SetActive(false);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(OnBackButton);
    }
    
    private void OnDecksUpdated(Enum Event_Type, Component Sender, object Param) {
        
    }

    /// <summary>
    /// 휴먼튜토리얼 강제 호출시 Awake가 호출되지 않은 상태이기 때문에 MenuSceneController에서 호출함
    /// </summary>
    public void ReadScenarioData() {
        string dataAsJson = ((TextAsset)Resources.Load("TutorialDatas/HumanChapterDatas")).text;
        human_chapterDatas = JsonReader.Read<List<ChapterData>>(dataAsJson);

        dataAsJson = ((TextAsset)Resources.Load("TutorialDatas/OrcChapterDatas")).text;
        orc_chapterDatas = JsonReader.Read<List<ChapterData>>(dataAsJson);

        dataAsJson = ((TextAsset)Resources.Load("TutorialDatas/humanChallengeData")).text;
        human_challengeDatas = JsonReader.Read<List<ChallengeData>>(dataAsJson);

        dataAsJson = ((TextAsset)Resources.Load("TutorialDatas/orcChallengeData")).text;
        orc_challengeDatas = JsonReader.Read<List<ChallengeData>>(dataAsJson);

        MakeStoryPageList();
    }

    public void OnBackButton() {
        SoundManager.Instance.PlaySound(UISfxSound.BUTTON1);
        PlayerPrefs.SetString("SelectedRace", "");
        PlayerPrefs.SetString("SelectedDeckId", "");
        PlayerPrefs.SetString("SelectedDeckType", "");
        PlayerPrefs.SetString("SelectedBattleType", "");
        PlayerPrefs.SetString("BattleMode", "");

        offAllGlowEffect();

        gameObject.SetActive(false);
        HUDController.SetHeader(HUDController.Type.SHOW_USER_INFO);
    }

    public void SetBackButton(int depth) {
        switch (depth) {
            case 1:
                HUDController.SetBackButton(() => {
                    OnBackButton();
                });
                HUDController.SetHeader(HUDController.Type.RESOURCE_ONLY_WITH_BACKBUTTON);
                break;
            case 2:
                HUDController.SetBackButton(() => {
                    CloseStoryDetail();
                    SetBackButton(1);
                });
                HUDController.SetHeader(HUDController.Type.ONLY_BAKCK_BUTTON);
                break;
            case 3:
                HUDController.SetBackButton(() => {
                    CloseDeckList();
                    SetBackButton(2);
                });
                HUDController.SetHeader(HUDController.Type.ONLY_BAKCK_BUTTON);
                break;
        }
    }

    void CloseStoryDetail() {
        stageCanvas.SetActive(false);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CloseStoryDetail);
    }

    void CloseDeckList() {
        stageCanvas.transform.Find("DeckSelectPanel").gameObject.SetActive(false);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CloseDeckList);
    }

    public void OnHumanCategories() {
        SoundManager.Instance.PlaySound(UISfxSound.BUTTON1);
        //heroID = "";
        isHuman = true;
        PlayerPrefs.SetString("SelectedRace", "human");
        ToggleUI();
    }
    
    public void OnOrcCategories() {
        SoundManager.Instance.PlaySound(UISfxSound.BUTTON1);
        //heroID = "";
        isHuman = false;
        PlayerPrefs.SetString("SelectedRace", "orc");
        ToggleUI();
    }

    /// <summary>
    /// 종족 선택시 UI 세팅
    /// </summary>
    private void ToggleUI() {
        SetSubStoryListInfo();
        SetChapterHeaderAlert();

        var backgroundImages = AccountManager.Instance.resource.campBackgrounds;
        if (isHuman) {
            orc.raceButton.GetComponent<Image>().sprite = orc.deactiveSprite;
            orc.heroSelect.SetActive(false);
            orc.StageCanvas.SetActive(false);

            human.raceButton.GetComponent<Image>().sprite = human.activeSprite;
            human.heroSelect.SetActive(true);
            human.StageCanvas.SetActive(true);

            backgroundImage.sprite = backgroundImages["human"];
        }
        else {
            human.raceButton.GetComponent<Image>().sprite = human.deactiveSprite;
            human.heroSelect.SetActive(false);
            human.StageCanvas.SetActive(false);

            orc.raceButton.GetComponent<Image>().sprite = orc.activeSprite;
            orc.heroSelect.SetActive(true);
            orc.StageCanvas.SetActive(true);

            backgroundImage.sprite = backgroundImages["orc"];
        }
    }
    
    /// <summary>
    /// 페이지별 리스트 생성 (ex. 0챕터 리스트, 1챕터 리스트)
    /// </summary>
    private void MakeStoryPageList() {
        pageHumanStoryList = new Dictionary<int, List<ChapterData>>();
        pageOrcStoryList = new Dictionary<int, List<ChapterData>>();

        var queryPages =
            from _chapterData in human_chapterDatas
            group _chapterData by _chapterData.chapter into newGroup
            orderby newGroup.Key
            select newGroup;

        foreach(var newGroup in queryPages) {
            if (!pageHumanStoryList.ContainsKey(newGroup.Key)) pageHumanStoryList[newGroup.Key] = new List<ChapterData>();

            foreach(var chapter in newGroup) {
                pageHumanStoryList[newGroup.Key].Add(chapter);
            }
        }

        queryPages =
            from _chapterData in orc_chapterDatas
            group _chapterData by _chapterData.chapter into newGroup
            orderby newGroup.Key
            select newGroup;

        foreach(var newGroup in queryPages) {
            if (!pageOrcStoryList.ContainsKey(newGroup.Key)) pageOrcStoryList[newGroup.Key] = new List<ChapterData>();

            foreach(var chapter in newGroup) {
                pageOrcStoryList[newGroup.Key].Add(chapter);
            }
        }
    }

    public void NextPage() {
        currentPageIndex++;
        if (currentPageIndex > maxPageIndex) currentPageIndex = maxPageIndex;

        SetSubStoryListInfo(currentPageIndex);
        SetChapterHeaderAlert(currentPageIndex);
    }

    public void PrevPage() {
        currentPageIndex--;
        if (currentPageIndex < 0) currentPageIndex = 0;

        SetSubStoryListInfo(currentPageIndex);
        SetChapterHeaderAlert(currentPageIndex);
    }

    private void SetChapterHeaderAlert(int page = 0) {
        orc.chapterHeader.transform.Find("Alert").gameObject.SetActive(false);
        human.chapterHeader.transform.Find("Alert").gameObject.SetActive(false);
        
        if (page >= 1) return;
        bool isUnclearedStoryExist = false;
        
        var clearedStageList = AccountManager.Instance.clearedStages;
        int nextChapter = page + 1;

        string camp = isHuman ? "human" : "orc";
        
        List<ChapterData> selectedList = isHuman ? pageHumanStoryList[nextChapter].ToList() : pageOrcStoryList[nextChapter].ToList();
        
        var _clearedStageList = clearedStageList.FindAll(x => x.chapterNumber == nextChapter && x.camp == camp);

        //이미 클리어 한거 제거
        foreach (var stage in _clearedStageList) {
            var selectedItem = selectedList.Find(x => x.chapter == stage.chapterNumber && stage.camp == camp);
            if (selectedItem != null) selectedList.Remove(selectedItem);
        }

        //레벨 충족 안되는거 제거
        selectedList.RemoveAll(x => x.require_level > AccountManager.Instance.userData.lv);
        bool isAlertNeededInHeader = selectedList != null && selectedList.Count > 0;
        
        if (isAlertNeededInHeader) {
            if (isHuman) {
                human.chapterHeader.transform.Find("Alert").gameObject.SetActive(true);
            }
            else {
                orc.chapterHeader.transform.Find("Alert").gameObject.SetActive(true);
            }
        }
    }

    private void SetSubStoryListInfo(int page = 0) {
        currentPageIndex = page;
        
        Transform canvas, content;
        List<ChapterData> selectedList;
        if (isHuman) {
            canvas = human.StageCanvas.transform;
            selectedList = pageHumanStoryList[page];
            content = human.stageContent.transform;

            maxPageIndex = pageHumanStoryList.Count - 1;
        }
        else {
            canvas = orc.StageCanvas.transform;
            selectedList = pageOrcStoryList[page];
            content = orc.stageContent.transform;

            maxPageIndex = pageOrcStoryList.Count - 1;
        }

        AccountManager accountManager = AccountManager.Instance;
        var translator = accountManager.GetComponent<Fbl_Translator>();

        foreach (Transform child in content) {
            child.gameObject.SetActive(false);
            child.transform.Find("ClearCheckMask").gameObject.SetActive(false);
        }
        canvas
            .Find("HUD/ChapterSelect/BackGround/Text")
            .GetComponent<Text>()
            .text = translator
                .GetLocalizedText(
                    "StoryLobby", 
                    GetChapterNameLocalizeKeyword(page, isHuman)
                );

        for (int i=0; i < selectedList.Count; i++) {
            //if (selectedList[i].match_type == "testing") continue;
            GameObject item = content.GetChild(i).gameObject;

            string headerTxt = translator.GetLocalizedText("StoryLobby", selectedList[i].stage_Name);
            string str = string.Format("Stage {0}. {1}", selectedList[i].stage_number, headerTxt);
            item.transform.Find("StageName").GetComponent<TextMeshProUGUI>().text = str;
            //ShowReward(item ,selectedList[i]);
            StageButton stageButtonComp = item.GetComponent<StageButton>();
            stageButtonComp.Init(selectedList[i], isHuman, this, selectedList[i].require_level);

            var backgroundImage = GetStoryBackgroundImage(stageButtonComp.camp, stageButtonComp.chapter, stageButtonComp.stage);
            item.transform.Find("BackGround").GetComponent<Image>().sprite = backgroundImage;

            var clearedStageList = AccountManager.Instance.clearedStages;
            foreach (var list in clearedStageList) {
                if (list.chapterNumber == null) list.chapterNumber = 0;
            }

            bool isUnclearedStoryExist = false;
            if(clearedStageList.Exists(x => x.chapterNumber == stageButtonComp.chapter && x.camp == stageButtonComp.camp && x.stageNumber == stageButtonComp.stage)) {
                item.transform.Find("ClearCheckMask").gameObject.SetActive(true);
                if(stageButtonComp.chapter > 0) stageButtonComp.Unlock();

                item.transform.Find("Alert").gameObject.SetActive(false);
                isUnclearedStoryExist = true;
            }
            else {
                if(stageButtonComp.chapter > 0) stageButtonComp.CheckLockOrUnlock();
                isUnclearedStoryExist = false;
            }
            
            item.SetActive(true);
            
            string desc = translator.GetLocalizedText("StoryLobby", selectedList[i].description);

            SetStorySummaryText(
                desc, 
                item.transform.Find("StageScript").GetComponent<TextMeshProUGUI>()
            );

            if (item.transform.Find("Glow").gameObject.activeSelf == true)
                item.transform.Find("Glow").gameObject.SetActive(false);
        }

        if (isHuman == false)
            orc.StageCanvas.transform.Find("HUD/StageSelect/Buttons").gameObject.SetActive(false);
        else
            human.StageCanvas.transform.Find("HUD/StageSelect/Buttons").gameObject.SetActive(false);


        ShowTutoHand(isHuman ? "human" : "orc");
    }

    private string GetChapterNameLocalizeKeyword(int chapterNum, bool isHuman) {
        switch (chapterNum) {
            case 0:
                if (isHuman) return "txt_stage_lobby_h_tuto_chap_head";
                else return "txt_stage_lobby_o_tuto_chap_head";
            case 1:
                if (isHuman) return "txt_stage_lobby_h1_1_head";
                else return "txt_stage_lobby_o1_1_head";
        }
        return null;
    }

    private void offAllGlowEffect() {
        foreach (Transform item in human.stageContent.transform) {
            item.Find("Glow").gameObject.SetActive(false);
        }

        foreach (Transform item in orc.stageContent.transform) {
            item.Find("Glow").gameObject.SetActive(false);
        }
    }

    private void SetStorySummaryText(string data, TextMeshProUGUI targetTextComp) {
        int cutStandard = 45;
        StringBuilder cutStr = new StringBuilder();
        if(data.Length > cutStandard) {
            cutStr.Append(data.Substring(0, cutStandard));
            cutStr.Append("...");
        }
        else {
            cutStr.Append(data);
        }
        targetTextComp.text = cutStr.ToString();
    }

    private void ShowReward(GameObject item) {
        var stageButton = item.GetComponent<StageButton>();
        var rewards = stageButton.chapterData.scenarioReward;
        Color32 ReceivedBgColor = new Color32(140, 140, 140, 255);

        if (rewards == null) return;

        Transform rewardParent = stageCanvas.transform.Find("HUD/StagePanel/Rewards/HorizontalGroup");
        var clearedStageList = AccountManager.Instance.clearedStages;
        foreach(Transform tf in rewardParent) {
            tf.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            tf.Find("Image").GetComponent<Image>().color = new Color32(255, 255, 255, 255);

            tf.Find("Image").gameObject.SetActive(false);
            tf.Find("Image/ClearedMark").gameObject.SetActive(false);
        }

        for(int i=0; i<rewards.Length; i++) {
            string rewardType = rewards[i].reward;
            Sprite rewardImage = null;
            if (AccountManager.Instance.resource.rewardIcon.ContainsKey(rewardType)) {
                rewardImage = AccountManager.Instance.resource.rewardIcon[rewardType];

                rewardParent.GetChild(i).GetComponent<Button>().onClick.RemoveAllListeners();
                rewardParent.GetChild(i).GetComponent<Button>().onClick.AddListener(() => {
                    RewardDescriptionHandler.instance.RequestDescriptionModal(rewardType);
                });
            }

            rewardParent.GetChild(i).Find("Image").gameObject.SetActive(true);
            rewardParent.GetChild(i).Find("Image").GetComponent<Image>().sprite = rewardImage;
            rewardParent.GetChild(i).Find("Image/Amount").GetComponent<TextMeshProUGUI>().text = "x" + rewards[i].count;
        }

        if(clearedStageList.Exists(x => stageButton.chapter == 0 && x.camp == stageButton.camp && x.stageNumber == stageButton.stage)) {
            for (int i = 0; i < rewards.Length; i++) {
                rewardParent.GetChild(i).Find("Image/ClearedMark").gameObject.SetActive(true);
                rewardParent.GetChild(i).GetComponent<Image>().color = ReceivedBgColor;
                rewardParent.GetChild(i).Find("Image").GetComponent<Image>().color = ReceivedBgColor;
            }
        }
    }

    public void OnStageCloseBtn() {
        SoundManager.Instance.PlaySound(UISfxSound.BUTTON1);
        stageCanvas.SetActive(false);
    }

    private void CreateTutorialDeck(bool isHuman) {
        Deck dummyDeck = new Deck();
        dummyDeck.deckValidate = true;

        GameObject deckPrefab;
        string deckName = "";
        if (isHuman) {
            deckPrefab = humanDeckPrefab;
            deckName = "휴먼 기본부대";
            dummyDeck.heroId = "h10001";
        }
        else {
            deckPrefab = orcDeckPrefab;
            deckName = "오크 기본부대";
            dummyDeck.heroId = "h10002";
        }

        GameObject setDeck = Instantiate(deckPrefab, deckContent.transform);
        
        setDeck.transform.Find("Deck").GetComponent<Button>().onClick.AddListener(() => {
            OnDeckSelected(setDeck, dummyDeck, true);
        });
        setDeck.transform.Find("Deck/Name").GetComponent<TextMeshProUGUI>().text = deckName;

        var deckCountText = stageCanvas.transform.Find("DeckSelectPanel/StagePanel/Header/Count").GetComponent<TextMeshProUGUI>();
        deckCountText.text = "1/1";
    }

    private void LoadMyDecks(bool isHuman) {
        List<Deck> totalDecks = new List<Deck>();
        GameObject deckPrefab = humanDeckPrefab;
        AccountManager accountManager = AccountManager.Instance;

        switch (isHuman) {
            case true:
                totalDecks.AddRange(accountManager.humanDecks);
                deckPrefab = humanDeckPrefab;
                break;
            case false:
                totalDecks.AddRange(accountManager.orcDecks);
                deckPrefab = orcDeckPrefab;
                break;
            default:
                totalDecks = null;
                break;
        }

        if (totalDecks == null) return;
        PlayerPrefs.SetString("SelectedDeckId", "");

        int deckIndex = 0;

        for (int i = 0; i < totalDecks.Count; i++) {
            GameObject setDeck = Instantiate(deckPrefab, deckContent.transform);
            setDeck.transform.Find("Deck/Name").GetComponent<TextMeshProUGUI>().text = totalDecks[deckIndex].name;

            setDeck.transform.Find("Deck/Info/Text").GetComponent<TextMeshProUGUI>().text =
                    totalDecks[deckIndex].totalCardCount + "/40";
            setDeck.transform.Find("Deck").GetComponent<StringIndex>().Id = totalDecks[deckIndex].id;
            int temp = deckIndex;
            setDeck.transform.Find("Deck").GetComponent<Button>().onClick.AddListener(() => {
                Instance.OnDeckSelected(setDeck, totalDecks[temp], true);
            });
            deckIndex++;
        }

        var deckCountText = stageCanvas.transform.Find("DeckSelectPanel/StagePanel/Header/Count").GetComponent<TextMeshProUGUI>();
        deckCountText.text = totalDecks.Count + "/8";
    }

    public void OnDeckSelected(GameObject selectedDeckObject, dataModules.Deck data, bool isTutorial) {
        SoundManager.Instance.PlaySound(UISfxSound.BUTTON1);
        if (this.selectedDeckObject != null) {
            this.selectedDeckObject.transform.Find("Selected").gameObject.SetActive(false);
        }
        //selectedDeckObject.transform.Find("Outline").gameObject.SetActive(true);
        this.selectedDeckObject = selectedDeckObject;
        this.selectedDeckObject.transform.Find("Selected").gameObject.SetActive(true);
        //GameObject twinkle = selectedDeckObject.transform.Find("Deck/Twinkle").gameObject;
        //twinkle.SetActive(true);
        //twinkle.GetComponent<DeckClickSpine>().Click();
        object[] selectedInfo = new object[] { isTutorial, data };
        PlayerPrefs.SetString("SelectedDeckId", data.id);
        PlayerPrefs.SetString("selectedHeroId", data.heroId);

        selectedDeck = selectedInfo;
        GameObject startButton = stageCanvas.transform.Find("DeckSelectPanel/StagePanel/StartButton").gameObject;
        if (!startButton.activeSelf) {
            startButton.SetActive(true);
        }
    }

    private void ClearDeckList() {
        foreach (Transform child in deckContent.transform) {
            Destroy(child.gameObject);
        }
    }

    public void OnCloseBtn() {
        SoundManager.Instance.PlaySound(UISfxSound.BUTTON1);
        stageCanvas.SetActive(false);
        HUDController.gameObject.SetActive(true);
    }

    public void OnClickStage() {
        SoundManager.Instance.PlaySound(UISfxSound.BUTTON1);
        if (isHuman) {
            orc.StageCanvas.transform.Find("HUD/StageSelect/Buttons").gameObject.SetActive(false);
            human.StageCanvas.transform.Find("HUD/StageSelect/Buttons").gameObject.SetActive(true);
        }
        else {
            human.StageCanvas.transform.Find("HUD/StageSelect/Buttons").gameObject.SetActive(false);
            orc.StageCanvas.transform.Find("HUD/StageSelect/Buttons").gameObject.SetActive(true);
        }

        offAllGlowEffect();
        selectedChapterObject.transform.Find("Glow").gameObject.SetActive(true);
    }

    public void OpenStoryDetailWindow() {
        SetBackButton(2);
        EscapeKeyController.escapeKeyCtrl.AddEscape(CloseStoryDetail);

        var stageButton = selectedChapterObject.GetComponent<StageButton>();
        if (stageButton == null) return;

        stageCanvas.SetActive(true);
        bool isTutorial = stageButton.isTutorial;

        Image background = stageCanvas.transform.Find("HUD/BackGround").GetComponent<Image>();
        Image descBackground = stageCanvas.transform.Find("HUD/StagePanel/Body").GetComponent<Image>();
        Image victoryBackground = stageCanvas.transform.Find("HUD/StagePanel/VictoryConditions/Portrait").GetComponent<Image>();

        var translator = AccountManager.Instance.GetComponent<Fbl_Translator>();

        if (isHuman) {
            background.sprite = human.background;
            descBackground.sprite = human.readyCanvasBg;

            var heroNamekey = GetHeroLocalizeKey(stageButton.chapterData.enemyHeroId);
            stageCanvas
            .transform
            .Find("HUD/StagePanel/VictoryConditions/HeroName")
            .gameObject
            .GetComponent<TextMeshProUGUI>().text = translator.GetLocalizedText("Hero", heroNamekey);

            if (storyHeroPortraits.ContainsKey(stageButton.chapterData.enemyHeroId)) {
                victoryBackground.sprite = storyHeroPortraits[stageButton.chapterData.enemyHeroId];
            }
            
        }
        else {
            background.sprite = orc.background;
            descBackground.sprite = orc.readyCanvasBg;

            var heroNamekey = GetHeroLocalizeKey(stageButton.chapterData.enemyHeroId);
            stageCanvas
            .transform
            .Find("HUD/StagePanel/VictoryConditions/HeroName")
            .gameObject
            .GetComponent<TextMeshProUGUI>().text = translator.GetLocalizedText("Hero", heroNamekey);

            if (storyHeroPortraits.ContainsKey(stageButton.chapterData.enemyHeroId)) {
                victoryBackground.sprite = storyHeroPortraits[stageButton.chapterData.enemyHeroId];
            }
        }

        stageCanvas
                 .transform
                 .Find("HUD/StagePanel/Body/StageName")
                 .gameObject
                 .GetComponent<TextMeshProUGUI>().text = translator.GetLocalizedText("StoryLobby", stageButton.stageName);

        stageCanvas
            .transform
            .Find("HUD/StagePanel/Body/Description")
            .gameObject
            .GetComponent<Text>().text = translator.GetLocalizedText("StoryLobby", stageButton.description);

        ShowReward(selectedChapterObject);

        stageCanvas
            .transform
            .Find("HUD/StagePanel/VictoryConditions/Description")
            .gameObject
            .GetComponent<TextMeshProUGUI>().text = stageButton.chapterData.specialRule;

        var storyEnemyHeroInfo = stageCanvas
            .transform
            .Find("HUD/HeroInfo")
            .GetComponent<StoryEnemyHeroInfo>();
        object[] data = new object[] { isHuman, stageButton };
        storyEnemyHeroInfo.SetData(data);
    }

    public void OpenDeckListWindow() {
        ClearDeckList();

        SetBackButton(3);
        EscapeKeyController.escapeKeyCtrl.AddEscape(CloseDeckList);

        stageCanvas.gameObject.SetActive(true);
        stageCanvas.transform.Find("DeckSelectPanel").gameObject.SetActive(true);
        var stageButton = selectedChapterObject.GetComponent<StageButton>();
        bool isTutorial = stageButton.isTutorial;

        if (isTutorial) {
            CreateTutorialDeck(isHuman);
        }
        else {
            LoadMyDecks(isHuman);
        }

        Image background = stageCanvas.transform.Find("DeckSelectPanel/BackGround").GetComponent<Image>();
        Image headerImage = stageCanvas.transform.Find("DeckSelectPanel/StagePanel/Header").GetComponent<Image>();

        if (isHuman) {
            background.sprite = human.background;
            headerImage.sprite = human.headerBg;
        }
        else {
            background.sprite = orc.background;
            headerImage.sprite = orc.headerBg;
        }
    }

    public void OnStartBtn() {
        if (isIngameButtonClicked) {
            Logger.Log("이미 대전 시작 버튼이 눌려진 상태");
            return;
        }
        PlayerPrefs.SetString("SelectedBattleType", "story");
        string race = PlayerPrefs.GetString("SelectedRace").ToLower();

        if(selectedDeck == null) {
            Modal.instantiate("덱을 선택해 주세요", Modal.Type.CHECK);
            SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
            return;
        }

        object[] selectedDeckInfo = (object[])selectedDeck;
        bool isTutorial = (bool)selectedDeckInfo[0];
        if (isTutorial) {
            FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.CONNECT_MATCHING_SCENE);
            PlayMangement.chapterData = selectedChapterData;
            PlayerPrefs.SetString("BattleMode", selectedChapterData.match_type);
        }
        else {
            string selectedDeckId = PlayerPrefs.GetString("SelectedDeckId").ToLower();
            Deck selectedDeck = (dataModules.Deck)selectedDeckInfo[1];

            if (race != null && !string.IsNullOrEmpty(selectedDeckId)) {
                if (selectedDeck.deckValidate) {
                    isIngameButtonClicked = true;

                    FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.CONNECT_MATCHING_SCENE);
                    PlayMangement.chapterData = selectedChapterData;
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

                if (race == "none") {
                    Modal.instantiate("종족을 선택해 주세요.", Modal.Type.CHECK);
                }
                else if (string.IsNullOrEmpty(selectedDeckId)) {
                    Modal.instantiate("덱을 선택해 주세요.", Modal.Type.CHECK);
                }
            }
        }
        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
    }

    public void SelectChallengeData(int chapterNum, int stageNum, string camp) {
        try {
            List<ChallengeData> list;
            list = camp == "human" ? human_challengeDatas : orc_challengeDatas;
            selectedChallengeData = list.Find(x => x.chapterNum == chapterNum && x.stageNum == stageNum);
            ScenarioGameManagment.challengeDatas = selectedChallengeData.challenges;
        }
        catch(NullReferenceException ex) { }
    }

    private QuestTutorial questTutorial;

    public void SetTutoQuest(Quest.QuestContentController quest, int stage) {
        questTutorial = new QuestTutorial();
        questTutorial.quest = quest;
        questTutorial.stage = stage;
        if(Instance == null) return;
        ShowTutoHand("human");
    }

    

    public class QuestTutorial {
        public Quest.QuestContentController quest;
        public int stage;
        [HideInInspector] public GameObject handUI;
    }

    public void ShowTutoHand(string camp) {
        if(questTutorial == null) return;
        if(questTutorial.handUI != null) {
            DestroyImmediate(questTutorial.handUI);
            questTutorial.handUI = null;
        }
        
        bool isClear = AccountManager.Instance.clearedStages.Exists(x=>(
            x.chapterNumber == null && 
            x.stageNumber == questTutorial.stage && 
            String.Compare(x.camp, camp, StringComparison.Ordinal) == 0)
        );
        
        if(isClear) return;
        StageButton[] stages = transform.GetComponentsInChildren<StageButton>();
        StageButton stage = Array.Find(stages, x => (x.chapter == 0 && x.stage == questTutorial.stage && x.camp.CompareTo(camp) == 0));
        if(stage == null) return;
        questTutorial.handUI = Instantiate(questTutorial.quest.manager.handSpinePrefab, stage.transform, false);
    }

    public TutorialSerializedList tutorialSerializedList;

    [Serializable] public class TutorialSerializedList {
        public ScenarioManager scenarioManager;
    }

    public Sprite GetStoryBackgroundImage(string camp, int chapterNumber, int stageNumber) {
        
        string defaultKey = camp + "_default";
        //Logger.Log("defaultKey : " + defaultKey);
        Sprite selectedImage = null;
        stroyBackgroundImages
            .TryGetValue(camp + "_" + chapterNumber + "-" + stageNumber, out selectedImage);
        if(selectedImage == null) selectedImage = stroyBackgroundImages[defaultKey];
        return selectedImage;
    }

    public string GetHeroLocalizeKey(string heroId) {
        switch (heroId) {
            case "h10001":
                return "hero_pc_h10001_name";
            case "h10002":
                return "hero_pc_h10002_name";
            case "qh10001":
                return "hero_npc_qh10001_name";
            case "qh10002":
                return "hero_npc_qh10002_name";
        }
        return "default";
    }
}

namespace Tutorial {
    [System.Serializable]
    public class ShowSelectRace {
        public GameObject raceButton;
        public Sprite activeSprite;
        public Sprite deactiveSprite;
        public Sprite victoryConditionBg;
        public GameObject heroSelect;
        public GameObject StageCanvas;
        public GameObject heroContent;
        public GameObject stageContent;
        public Sprite readyCanvasBg;
        public Sprite headerBg;
        public Sprite background;
        public GameObject chapterHeader;
    }

    public class ScenarioButton : MonoBehaviour {
        protected ScenarioManager scenarioManager;

        private void Start() {
            scenarioManager = ScenarioManager.Instance;
        }

        public virtual void OnClicked() { }
    }

    public class ChapterData {
        public int chapter;
        public int stage_number;
        public int require_level;
        public string stage_Name;
        public string match_type;
        public string map;
        public string myHeroId;
        public string enemyHeroId;

        [MultiLineProperty(10)] public string description;
        [MultiLineProperty(5)] public string specialRule;
        public List<ScriptData> scripts;
        public ScenarioReward[] scenarioReward;

        public int stageSerial;
    }

    public class CommonTalking {
        public string talkingTiming;
        public List<ScriptData> scripts;
    }

    public class ScriptEndChapterDatas {
        public int chapter;
        public int stage_number;
        public int isWin;
        public List<Method> methods;
    }

    public class ScriptData {
        [MultiLineProperty(10)] public string Print_text;
        public List<Method> methods;

        public bool isExecute;
    }

    public class Method {
        public string name;
        public List<string> args;
    }

    public class ScenarioReward {
        public string reward;
        public int count;
    }


}