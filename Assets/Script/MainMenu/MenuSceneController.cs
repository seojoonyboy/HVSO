using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Spine;
using Spine.Unity;
using System;
using Newtonsoft.Json;

public class MenuSceneController : MonoBehaviour {
    [SerializeField] Transform fixedCanvas;
    [SerializeField] HUDController hudController;
    [SerializeField] HorizontalScrollSnap windowScrollSnap;
    [SerializeField] DeckSettingManager deckSettingManager;
    [SerializeField] Transform dictionaryMenu;
    [SerializeField] SkeletonGraphic battleSwordSkeleton;
    [SerializeField] TMPro.TextMeshProUGUI nicknameText;
    [SerializeField] GameObject battleReadyPanel;   //대전 준비 화면
    [SerializeField] public GameObject storyLobbyPanel;    //스토리 메뉴 화면
    [SerializeField] SkeletonGraphic menuButton;
    [SerializeField] GameObject[] offObjects;
    protected SkeletonGraphic selectedAnimation;
    private int currentPage;
    private bool buttonClicked;
    static bool isLoaded = false;
    public MyDecksLoader decksLoader;
    [SerializeField] GameObject newbiLoadingModal;  //최초 접속시 튜토리얼 강제시 등장하는 로딩 화면
    [SerializeField] GameObject reconnectingModal;  //재접속 진행시 등장하는 로딩 화면
    [SerializeField] MenuTutorialManager menuTutorialManager;
    [SerializeField] ScenarioManager scenarioManager;

    public static MenuSceneController menuSceneController;

    bool isTutorialDataLoaded = false;

    private void Awake() {
        //NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_UPDATED, OnUserDataUpdate);

        menuSceneController = this;
        #region 테스트코드
        //NetworkManager.ReconnectData dummyData = new NetworkManager.ReconnectData("11", "human");
        //PlayerPrefs.SetString("ReconnectData", JsonConvert.SerializeObject(dummyData));
        #endregion
        if (!isLoaded)
            isLoaded = true;
        else
            SetCardNumbersPerDic();

        //menuTutorialManager.StartTutorial(MenuTutorialManager.TutorialType.TO_BOX_OPEN_HUMAN);
        //if(PlayerPrefs.GetString("ReconnectData") != string.Empty) {
        //    GameObject modal = Instantiate(reconnectingModal);
        //    modal.GetComponent<ReconnectController>().Init(decksLoader);
        //}

        menuButton.Initialize(true);
        menuButton.Update(0);
        ClickMenuButton(2);
    }

    private void OnUserDataUpdate(Enum Event_Type, Component Sender, object Param) {
        nicknameText.text = AccountManager.Instance.userData.nickName;
    }

    private void CheckTutorial() {
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
            AddNewbiController();
            PlayerPrefs.SetInt("isFirst", 0);

            PlayerPrefs.SetInt("IsFirstCardMenu", 1);
            PlayerPrefs.SetInt("IsFirstDeckListMenu", 1);
            PlayerPrefs.SetInt("IsFirstMainMenu", 1);
        }
        else {
            AccountManager accountManager = AccountManager.Instance;

            accountManager.RequestUserInfo((req, res) => {
                if (res.IsSuccess) {
                    accountManager.SetSignInData(res);

                    //튜토리얼 남았음
                    AccountManager.etcInfo tutorialCleared = etcInfos.Find(x => x.key == "tutorialCleared");
                    var clearedStages = AccountManager.Instance.clearedStages;

                    if (tutorialCleared == null) {
                        var humanDeckClaimed = etcInfos.Find(x => x.key == "humanDeckClaimed");
                        //휴먼 튜토리얼 0-1 이후 메인화면에서 보상을 받지 않았음 (다시 휴먼 0-1 인게임으로)
                        if (humanDeckClaimed == null) {
                            PlayerPrefs.SetInt("TutorialBoxRecieved", 0);
                            AddNewbiController();
                        }
                        //휴먼 튜토리얼 0-1 이후 메인화면에서 보상을 받았음
                        else {
                            //휴먼 튜토리얼 0-2를 진행하지 않았음
                            if(!clearedStages.Exists(x => x.camp == "human" && x.stageNumber == 2)) {
                                tutorialType = MenuTutorialManager.TutorialType.TO_HUMAN_STORY_2;
                            }
                            //휴먼 튜토리얼 0-2를 진행함
                            else {
                                var orcDeckClaimed = etcInfos.Find(x => x.key == "orcDeckClaimed");
                                //오크 튜토리얼 0-1을 진행하지 않았음
                                if (orcDeckClaimed == null) {
                                    tutorialType = MenuTutorialManager.TutorialType.TO_ORC_STORY;
                                }
                                //오크 튜토리얼 0-1을 진행함 
                                else {
                                    //오크 튜토리얼 0-2를 진행하지 않았음
                                    if(!clearedStages.Exists(x => x.camp == "orc" && x.stageNumber == 2)) {
                                        tutorialType = MenuTutorialManager.TutorialType.TO_ORC_STORY_2;
                                    }
                                    //오크 튜토리얼 0-2를 진행함
                                    else {
                                        menuTutorialManager.enabled = false;
                                        needTutorial = false;
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }

        if (needTutorial) {
            if (tutorialType != MenuTutorialManager.TutorialType.NONE) {
                menuTutorialManager.StartTutorial(tutorialType);
            }
        }
    }

    private void OnDestroy() {
        SoundManager.Instance.bgmController.StopSoundTrack();
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_UPDATED, OnUserDataUpdate);
    }

    private void Start() {
        SoundManager.Instance.bgmController.PlaySoundTrack(BgmController.BgmEnum.MENU);

        if (AccountManager.Instance.needChangeNickName) {
            Modal.instantiate("사용하실 닉네임을 입력해 주세요.", "새로운 닉네임", AccountManager.Instance.NickName, Modal.Type.INSERT, (str) => {
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

        CheckTutorial();

        #region 테스트 코드
        //menuTutorialManager.ReadTutorialData();
        //scenarioManager.ReadScenarioData();
        //isTutorialDataLoaded = true;
        //menuTutorialManager.StartTutorial(MenuTutorialManager.TutorialType.TO_ORC_STORY);
        #endregion

        AccountManager.Instance.RequestUserInfo();    //튜토리얼을 어디서부터 진행해야 하는지 판단
    }

    /// <summary>
    /// PVP대전 버튼 클릭
    /// </summary>
    public void OnPVPClicked() {
        battleReadyPanel.SetActive(true);
        hudController.SetHeader(HUDController.Type.BATTLE_READY_CANVAS);
        hudController.SetBackButton(() => {
            battleReadyPanel.SetActive(false);
            hudController.SetHeader(HUDController.Type.SHOW_USER_INFO);
        });
    }

    public void OnStoryClicked() {
        //FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MISSION_SELECT_SCENE);
        storyLobbyPanel.SetActive(true);
    }

    public void ClickMenuButton(int pageNum) {
        buttonClicked = true;
        fixedCanvas.Find("InnerCanvas/Footer").GetChild(currentPage).GetChild(0).gameObject.SetActive(false);
        currentPage = pageNum;
        windowScrollSnap.GoToScreen(currentPage);
        menuButton.AnimationState.SetAnimation(0, "IDLE_" + (currentPage + 1).ToString(), true);
        fixedCanvas.Find("InnerCanvas/Footer").GetChild(currentPage).GetChild(0).gameObject.SetActive(true);
    }

    public void ScrollSnapButtonChange() {
        fixedCanvas.Find("InnerCanvas/Footer").GetChild(currentPage).GetChild(0).gameObject.SetActive(false);
        currentPage = windowScrollSnap.CurrentPage;
        menuButton.AnimationState.SetAnimation(0, "IDLE_" + (currentPage + 1).ToString(), true);
        fixedCanvas.Find("InnerCanvas/Footer").GetChild(currentPage).GetChild(0).gameObject.SetActive(true);
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
        Transform humanBtn = dictionaryMenu.Find("HumanButton/CardRarityInfo");
        humanBtn.Find("common/CardNum").GetComponent<Text>().text = AccountManager.Instance.cardPackage.rarelityHumanCardNum["common"].Count.ToString();
        humanBtn.Find("uncommon/CardNum").GetComponent<Text>().text = AccountManager.Instance.cardPackage.rarelityHumanCardNum["uncommon"].Count.ToString();
        humanBtn.Find("rare/CardNum").GetComponent<Text>().text = AccountManager.Instance.cardPackage.rarelityHumanCardNum["rare"].Count.ToString();
        humanBtn.Find("superrare/CardNum").GetComponent<Text>().text = AccountManager.Instance.cardPackage.rarelityHumanCardNum["superrare"].Count.ToString();
        humanBtn.Find("legend/CardNum").GetComponent<Text>().text = AccountManager.Instance.cardPackage.rarelityHumanCardNum["legend"].Count.ToString();
        humanBtn.Find("common/NewCard").gameObject.SetActive(AccountManager.Instance.cardPackage.rarelityHumanCardCheck["common"].Count > 0);
        humanBtn.Find("uncommon/NewCard").gameObject.SetActive(AccountManager.Instance.cardPackage.rarelityHumanCardCheck["uncommon"].Count > 0);
        humanBtn.Find("rare/NewCard").gameObject.SetActive(AccountManager.Instance.cardPackage.rarelityHumanCardCheck["rare"].Count > 0);
        humanBtn.Find("superrare/NewCard").gameObject.SetActive(AccountManager.Instance.cardPackage.rarelityHumanCardCheck["superrare"].Count > 0);
        humanBtn.Find("legend/NewCard").gameObject.SetActive(AccountManager.Instance.cardPackage.rarelityHumanCardCheck["legend"].Count > 0);
        //dictionaryMenu.Find("HumanButton/NewCard").gameObject.SetActive(AccountManager.Instance.cardPackage.checkHumanCard.Count > 0);
        Transform orcBtn = dictionaryMenu.Find("OrcButton/CardRarityInfo");
        orcBtn.Find("common/CardNum").GetComponent<Text>().text = AccountManager.Instance.cardPackage.rarelityOrcCardNum["common"].Count.ToString();
        orcBtn.Find("uncommon/CardNum").GetComponent<Text>().text = AccountManager.Instance.cardPackage.rarelityOrcCardNum["uncommon"].Count.ToString();
        orcBtn.Find("rare/CardNum").GetComponent<Text>().text = AccountManager.Instance.cardPackage.rarelityOrcCardNum["rare"].Count.ToString();
        orcBtn.Find("superrare/CardNum").GetComponent<Text>().text = AccountManager.Instance.cardPackage.rarelityOrcCardNum["superrare"].Count.ToString();
        orcBtn.Find("legend/CardNum").GetComponent<Text>().text = AccountManager.Instance.cardPackage.rarelityOrcCardNum["legend"].Count.ToString();
        orcBtn.Find("common/NewCard").gameObject.SetActive(AccountManager.Instance.cardPackage.rarelityOrcCardCheck["common"].Count > 0);
        orcBtn.Find("uncommon/NewCard").gameObject.SetActive(AccountManager.Instance.cardPackage.rarelityOrcCardCheck["uncommon"].Count > 0);
        orcBtn.Find("rare/NewCard").gameObject.SetActive(AccountManager.Instance.cardPackage.rarelityOrcCardCheck["rare"].Count > 0);
        orcBtn.Find("superrare/NewCard").gameObject.SetActive(AccountManager.Instance.cardPackage.rarelityOrcCardCheck["superrare"].Count > 0);
        orcBtn.Find("legend/NewCard").gameObject.SetActive(AccountManager.Instance.cardPackage.rarelityOrcCardCheck["legend"].Count > 0);
        //dictionaryMenu.Find("OrcButton/NewCard").gameObject.SetActive(AccountManager.Instance.cardPackage.checkOrcCard.Count > 0);
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
    }

    public void OpenHeroDictionary(bool isHuman) {
        AccountManager.Instance.dicInfo.isHuman = isHuman;
        AccountManager.Instance.dicInfo.inDic = true;
        for (int i = 0; i < offObjects.Length; i++)
            offObjects[i].SetActive(false);
        CardDictionaryManager.cardDictionaryManager.SetHeroDictionary();
    }

    public void CloseDictionary() {
        CardDictionaryManager.cardDictionaryManager.gameObject.SetActive(false);
        for (int i = 0; i < offObjects.Length; i++)
            offObjects[i].SetActive(true);
        SetCardNumbersPerDic();
    }

    public void AddNewbiController() {
        var newbiComp = newbiLoadingModal.AddComponent<NewbiController>(); //첫 로그인 제어
        newbiComp.menuSceneController = this;
        newbiComp.name = "NewbiController";
        newbiComp.Init(decksLoader, scenarioManager, newbiLoadingModal);
    }
}