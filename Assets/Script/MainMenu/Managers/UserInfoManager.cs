using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInfoManager : MonoBehaviour {
    [SerializeField] MenuSceneController MenuSceneController;
    [SerializeField] private HUDController _hudController;

    void Start() {
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_STATISTICS, SetUserStatistis);
        var stateHandler = MainSceneStateHandler.Instance;
        bool NickNameChangeTutorialLoaded = stateHandler.GetState("NickNameChangeTutorialLoaded");
        bool isTutoFinished = stateHandler.GetState("IsTutorialFinished");
        if (isTutoFinished && !NickNameChangeTutorialLoaded) {
            MenuSceneController.StartQuestSubSet(MenuTutorialManager.TutorialType.SUB_SET_103);
            stateHandler.ChangeState("NickNameChangeTutorialLoaded", true);
        }
    }

    private void OnEnable() {
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_LEAGUE_INFO_UPDATED, OnLeagueInfoUpdated);
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_UPDATED, OnUserDataUpdated);

        SetUserInfo();
    }

    void OnDisable() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_LEAGUE_INFO_UPDATED, OnLeagueInfoUpdated);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_UPDATED, OnUserDataUpdated);
    }

    void OnDestroy() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_LEAGUE_INFO_UPDATED, OnLeagueInfoUpdated);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_UPDATED, OnUserDataUpdated);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_STATISTICS, SetUserStatistis);
    }

    private void OnLeagueInfoUpdated(Enum Event_Type, Component Sender, object Param) {
        SetUserInfo();
    }
    
    private void OnUserDataUpdated(Enum Event_Type, Component Sender, object Param) {
        SetUserInfo();
    }

    public void SetUserInfo() {
        Transform contents = transform.Find("InnerCanvas/Viewport/Content");
        contents.Find("PlayerInfoPanel/Info/Level/Value").GetComponent<Text>().text = AccountManager.Instance.userData.lv.ToString();
        contents.Find("PlayerInfoPanel/Info/UserName").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.NickName;
        contents.Find("PlayerInfoPanel/Info/LevelGauge/ValueSlider").GetComponent<Slider>().value = (float)AccountManager.Instance.userData.exp / (float)AccountManager.Instance.userData.lvExp;
        contents.Find("PlayerInfoPanel/Info/LevelGauge/ValueText").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userData.exp + "/" + AccountManager.Instance.userData.lvExp;

        int tier = AccountManager.Instance.scriptable_leagueData.leagueInfo.rankDetail.id;
        if (AccountManager.Instance.resource.rankIcons.ContainsKey(tier.ToString()))
            contents.Find("BattleInfoPanel/TierBannerBtn/RankIcon").GetComponent<Image>().sprite = AccountManager.Instance.resource.rankIcons[tier.ToString()];
        else
            contents.Find("BattleInfoPanel/TierBannerBtn/RankIcon").GetComponent<Image>().sprite = AccountManager.Instance.resource.rankIcons["default"];

        contents.Find("BattleInfoPanel/TierBannerBtn/RankName").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.scriptable_leagueData.leagueInfo.rankDetail.minorRankName;
        contents.Find("BattleInfoPanel/TierBannerBtn/MMRValue").GetComponent<Text>().text = AccountManager.Instance.scriptable_leagueData.leagueInfo.ratingPoint.ToString();
    }

    public void SetUserStatistis(Enum Event_Type, Component Sender, object Param) {
        var statistics = AccountManager.Instance.userStatistics;
        SetUserRankTable(statistics.rankTable);
        Transform contents = transform.Find("InnerCanvas/Viewport/Content/StatisticsPanel/Statistics");
        contents.Find("Wins").GetComponent<TMPro.TextMeshProUGUI>().text = statistics.playStatistics.winning.ToString();
        string campText = "";
        switch (statistics.playStatistics.mainCamp) {
            case "human":
                campText = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("MainUI", "ui_page_myinfo_human");
                contents.Find("Camp").GetComponent<TMPro.TextMeshProUGUI>().color = Color.blue;
                break;
            case "orc":
                campText = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("MainUI", "ui_page_myinfo_orc");
                contents.Find("Camp").GetComponent<TMPro.TextMeshProUGUI>().color = Color.red;
                break;
            default:
                campText = "-";
                contents.Find("Camp").GetComponent<TMPro.TextMeshProUGUI>().color = Color.gray;
                break;
        }
        contents.Find("Camp").GetComponent<TMPro.TextMeshProUGUI>().text = campText;

        if (statistics.playStatistics.medalMax != null)
            contents.Find("MaxMedal").GetComponent<TMPro.TextMeshProUGUI>().text = statistics.playStatistics.medalMax.ToString();
        else
            contents.Find("MaxMedal").GetComponent<TMPro.TextMeshProUGUI>().text = "-";


        int totalCards = 0;
        int myCards = 0;
        foreach (dataModules.CollectionCard card in AccountManager.Instance.allCards) {
            if (card.unownable) continue;
            if (!card.isHeroCard) {
                totalCards++;
                if (AccountManager.Instance.cardPackage.data.ContainsKey(card.id)) myCards++;
            }
        }
        contents.Find("CardCollection").GetComponent<TMPro.TextMeshProUGUI>().text = myCards + "/" + totalCards;

        contents.Find("TopGrade").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("Tier", statistics.playStatistics.gradeTop);
        if (statistics.playStatistics.rankTop != null)
            contents.Find("TopRank").GetComponent<TMPro.TextMeshProUGUI>().text = statistics.playStatistics.rankTop.ToString();
        else
            contents.Find("TopRank").GetComponent<TMPro.TextMeshProUGUI>().text = "-";
        contents.Find("TopFamePoint").GetComponent<TMPro.TextMeshProUGUI>().text = statistics.playStatistics.famePointTop.ToString();
    }

    public void SetUserRankTable(dataModules.UserRank[] rankTable) {
        Transform contents = transform.Find("InnerCanvas/Viewport/Content/BattleInfoPanel/LeaderBoard");
        for (int i = 0; i < 5; i++) {
            contents.GetChild(i).gameObject.SetActive(false);
            contents.GetChild(i).Find("Player").gameObject.SetActive(false);
        }
        int userNum = 0;
        foreach(dataModules.UserRank user in rankTable) {
            contents.GetChild(userNum).gameObject.SetActive(true);
            if (user.suid == AccountManager.Instance.userData.suid && user.serverId == AccountManager.Instance.userData.serverId)
                contents.GetChild(userNum).Find("Player").gameObject.SetActive(true);
            contents.GetChild(userNum).Find("RankIcon").GetComponent<Image>().sprite = AccountManager.Instance.resource.rankIcons[user.rankId.ToString()];
            contents.GetChild(userNum).Find("NickName").GetComponent<TMPro.TextMeshProUGUI>().text = user.nickName;
            contents.GetChild(userNum).Find("Medals").GetComponent<TMPro.TextMeshProUGUI>().text = user.score.ToString();
            userNum++;
        }
    }

    public void ChangeNickName() {
        var userData = AccountManager.Instance.userData;
        var nickNameChangeRight = userData.etcInfo.Find(x => x.key == "nicknameChange");
        int nickNameChangeRightValue = 0;
        int.TryParse(nickNameChangeRight.value, out nickNameChangeRightValue);
        if (nickNameChangeRightValue > 0) {
            Modal.instantiate("닉네임 첫 변경은 무료입니다.\n닉네임을 변경하시겠습니까?", Modal.Type.YESNO, () => {
                __ChangeNickNameModal(true);
            });
        }
        else {
            __ChangeNickNameModal(false);
        }
    }

    private void __ChangeNickNameModal(bool ticketHave) {
        Fbl_Translator translator = AccountManager.Instance.GetComponent<Fbl_Translator>();
        string header1 = translator.GetLocalizedText("UIPopup", "ui_popup_myinfo_namechangecost");
        header1 = header1.Replace("{n}", "100");
        string header2 = translator.GetLocalizedText("UIPopup", "ui_popup_myinfo_namechangeconfirm");
        string headerText = header1 + "\n" + header2;
        
        GameObject modal = Modal.instantiate(headerText, "새로운 닉네임", AccountManager.Instance.NickName, Modal.Type.INSERT, (str) => {
            AccountManager.Instance.ChangeNicknameReq(str, ticketHave, InvalidCallbackReceived);
        });
        
        modal
            .transform
            .Find("ModalWindow/InsertModal/InputField")
            .GetComponent<InputField>()
            .characterLimit = 10;
        
        modal.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => {
            Destroy(modal);
        });
    }

    private void InvalidCallbackReceived(string msg) {
        Fbl_Translator translator = AccountManager.Instance.GetComponent<Fbl_Translator>();
        if (msg == "Gold") {
            string header1 = translator.GetLocalizedText("UIPopup", "ui_popup_myinfo_lackofgold");
            string header2 = translator.GetLocalizedText("UIPopup", "ui_popup_myinfo_goshopforgold");
            string headerText = header1 + "\n" + header2;
            
            Modal.instantiate(headerText, Modal.Type.YESNO, () => {
                _hudController.CloseUserInfo();
                MenuSceneController.ClickMenuButton("Shop");
            });
        }
        
        // else if (msg == "ChangeRight") {
        //     Modal.instantiate("닉네임 변경권이 없습니다.", Modal.Type.CHECK, () => { });
        // }
    }
}
