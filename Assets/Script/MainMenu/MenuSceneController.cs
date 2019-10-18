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
    protected SkeletonGraphic selectedAnimation;
    private int currentPage;
    private bool buttonClicked;
    static bool isLoaded = false;
    public MyDecksLoader decksLoader;
    [SerializeField] GameObject newbiLoadingModal;  //최초 접속시 튜토리얼 강제시 등장하는 로딩 화면
    [SerializeField] GameObject reconnectingModal;  //재접속 진행시 등장하는 로딩 화면
    [SerializeField] MenuTutorialManager menuTutorialManager;

    private void Awake() {
        #region 테스트코드
        //NetworkManager.ReconnectData dummyData = new NetworkManager.ReconnectData("11", "human");
        //PlayerPrefs.SetString("ReconnectData", JsonConvert.SerializeObject(dummyData));
        #endregion
        if (!isLoaded)
            isLoaded = true;
        else
            SetCardNumbersPerDic();

        //TODO : api/user의 etcInfo에 tutorialCleared value를 이용하여 처리
        if (PlayerPrefs.GetInt("isFirst") == 1) {
            string prevTutorial = PlayerPrefs.GetString("PrevTutorial");
            if (string.IsNullOrEmpty(prevTutorial)) {
                var newbiComp = newbiLoadingModal.AddComponent<NewbiController>(); //첫 로그인 제어
                newbiComp.menuSceneController = this;
                newbiComp.name = "NewbiController";
                newbiComp.Init(decksLoader, newbiLoadingModal);
            }
            else {
                switch (prevTutorial) {
                    case "Human_Tutorial":
                        //TODO : 보상 받기 처리
                        //issue : 보상을 받고 진행을 중단한 경우, 
                        //다음 튜토리얼에서는 처음부터 진행하다가 skip해야 하는지
                        menuTutorialManager.StartTutorial(MenuTutorialManager.TutorialType.TO_ORC_STORY);
                        break;
                    case "Orc_Tutorial":
                        //TODO : 보상 받기 처리

                        break;
                }
            }
        }
        else if(PlayerPrefs.GetString("ReconnectData") != string.Empty) {
            GameObject modal = Instantiate(reconnectingModal);
            modal.GetComponent<ReconnectController>().Init(decksLoader);
        }
        menuButton.Initialize(true);
        menuButton.Update(0);
        ClickMenuButton(2);

        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.NICKNAME_CHANGED, OnNicknameChanged);
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.NICKNAME_CHANGED, OnNicknameChanged);
        
    }

    private void OnNicknameChanged(Enum Event_Type, Component Sender, object Param) {
        nicknameText.text = (string)Param;
    }

    private void Start() {
        if (AccountManager.Instance.needChangeNickName) {
            Modal.instantiate("사용하실 닉네임을 입력해 주세요.", "새로운 닉네임", AccountManager.Instance.NickName, Modal.Type.INSERT, (str) => {
                if (string.IsNullOrEmpty(str)) {
                    Modal.instantiate("빈 닉네임은 허용되지 않습니다.", Modal.Type.CHECK);
                }
                else {
                    AccountManager.Instance.ChangeNicknameReq(str, (req, res) => {
                        if (res.StatusCode == 200 || res.StatusCode == 304) {
                            Modal.instantiate("닉네임이 변경되었습니다.", Modal.Type.CHECK);
                            AccountManager.Instance.needChangeNickName = false;
                            AccountManager.Instance.NickName = str;
                        }
                        else {
                            Modal.instantiate("에러 발생 \n" + res.DataAsText, Modal.Type.CHECK);
                        }
                    });
                }
            });
        }

        deckSettingManager.AttachDecksLoader(ref decksLoader);
        //cardDictionaryManager.AttachDecksLoader(ref decksLoader);
        decksLoader.OnLoadFinished.AddListener(() => {
            NoneIngameSceneEventHandler.Instance.PostNotification(NoneIngameSceneEventHandler.EVENT_TYPE.NICKNAME_CHANGED, this, AccountManager.Instance.NickName);
        });
        decksLoader.Load();
        AccountManager.Instance.OnCardLoadFinished.AddListener(() => SetCardNumbersPerDic());
        currentPage = 2;
        Transform buttonsParent = fixedCanvas.Find("Footer");
        //for (int i = 0; i < fixedCanvas.Find("Footer").childCount; i++)
        //    buttonSkeletons[i] = buttonsParent.GetChild(i).Find("ButtonImage").GetComponent<SkeletonGraphic>();
        //StartCoroutine(UpdateWindow());
        TouchEffecter.Instance.SetScript();
        if (AccountManager.Instance.dicInfo.inDic) {
            windowScrollSnap.StartingScreen = 0;
            ClickMenuButton(0);
            AccountManager.Instance.dicInfo.inDic = false;
        }
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
        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
    }

    public void OnStoryClicked() {
        //FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MISSION_SELECT_SCENE);
        storyLobbyPanel.SetActive(true);
        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
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
        humanBtn.Find("common/CardNum").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.cardPackage.rarelityHumanCardNum["common"].Count.ToString();
        humanBtn.Find("uncommon/CardNum").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.cardPackage.rarelityHumanCardNum["uncommon"].Count.ToString();
        humanBtn.Find("rare/CardNum").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.cardPackage.rarelityHumanCardNum["rare"].Count.ToString();
        humanBtn.Find("superrare/CardNum").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.cardPackage.rarelityHumanCardNum["superrare"].Count.ToString();
        humanBtn.Find("legend/CardNum").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.cardPackage.rarelityHumanCardNum["legend"].Count.ToString();
        humanBtn.Find("common/NewCard").gameObject.SetActive(AccountManager.Instance.cardPackage.rarelityHumanCardCheck["common"].Count > 0);
        humanBtn.Find("uncommon/NewCard").gameObject.SetActive(AccountManager.Instance.cardPackage.rarelityHumanCardCheck["uncommon"].Count > 0);
        humanBtn.Find("rare/NewCard").gameObject.SetActive(AccountManager.Instance.cardPackage.rarelityHumanCardCheck["rare"].Count > 0);
        humanBtn.Find("superrare/NewCard").gameObject.SetActive(AccountManager.Instance.cardPackage.rarelityHumanCardCheck["superrare"].Count > 0);
        humanBtn.Find("legend/NewCard").gameObject.SetActive(AccountManager.Instance.cardPackage.rarelityHumanCardCheck["legend"].Count > 0);
        //dictionaryMenu.Find("HumanButton/NewCard").gameObject.SetActive(AccountManager.Instance.cardPackage.checkHumanCard.Count > 0);
        Transform orcBtn = dictionaryMenu.Find("OrcButton/CardRarityInfo");
        orcBtn.Find("common/CardNum").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.cardPackage.rarelityOrcCardNum["common"].Count.ToString();
        orcBtn.Find("uncommon/CardNum").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.cardPackage.rarelityOrcCardNum["uncommon"].Count.ToString();
        orcBtn.Find("rare/CardNum").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.cardPackage.rarelityOrcCardNum["rare"].Count.ToString();
        orcBtn.Find("superrare/CardNum").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.cardPackage.rarelityOrcCardNum["superrare"].Count.ToString();
        orcBtn.Find("legend/CardNum").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.cardPackage.rarelityOrcCardNum["legend"].Count.ToString();
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

    public void OpenDictionary(bool isHuman) {
        AccountManager.Instance.dicInfo.isHuman = isHuman;
        AccountManager.Instance.dicInfo.inDic = true;
        FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.DICTIONARY_SCENE);
    }
}