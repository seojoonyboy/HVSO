using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInfoManager : MonoBehaviour {
    [SerializeField] MenuSceneController MenuSceneController;
    [SerializeField] private HUDController _hudController;
    [SerializeField] Transform battleRankWindow;
    [SerializeField] Transform totalRankWindow;

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
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_BATTLERANK_RECEIVED, OpenBattleRank);
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_TOTALRANK_RECEIVED, OpenTotalRank);
        SetUserInfo();
    }

    void OnDisable() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_LEAGUE_INFO_UPDATED, OnLeagueInfoUpdated);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_UPDATED, OnUserDataUpdated);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_BATTLERANK_RECEIVED, OpenBattleRank);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_TOTALRANK_RECEIVED, OpenTotalRank);
    }

    void OnDestroy() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_LEAGUE_INFO_UPDATED, OnLeagueInfoUpdated);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_UPDATED, OnUserDataUpdated);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_STATISTICS, SetUserStatistis);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_BATTLERANK_RECEIVED, OpenBattleRank);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_TOTALRANK_RECEIVED, OpenTotalRank);
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
        for (int i = 0; i < 4; i++) {
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
            contents.GetChild(userNum).Find("Image/Text").GetComponent<Text>().text = user.rank.ToString();
            if (user.rank < 4) {
                contents.GetChild(userNum).Find("Image").GetComponent<Image>().sprite = AccountManager.Instance.resource.rankIcons["ranking_" + user.rank];
                contents.GetChild(userNum).Find("Image").GetComponent<Image>().enabled = true;
            }
            else
                contents.GetChild(userNum).Find("Image").GetComponent<Image>().enabled = false;
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
            Fbl_Translator translator = AccountManager.Instance.GetComponent<Fbl_Translator>();
            string header1 = translator.GetLocalizedText("UIPopup", "ui_popup_myinfo_namechangecost");
            header1 = header1.Replace("{n}", "100");
            string header2 = translator.GetLocalizedText("UIPopup", "ui_popup_myinfo_namechangeconfirm");
            string headerText = header1 + "\n" + header2;
            Modal.instantiate(headerText, Modal.Type.YESNO, () => {
                __ChangeNickNameModal(false);
            });
        }
    }

    private void __ChangeNickNameModal(bool ticketHave) {
        if (ticketHave) {
            string headerText = AccountManager.Instance.GetComponent<Fbl_Translator>()
                .GetLocalizedText("UIPopup", "ui_popup_myinfo_changenameenter");
            GameObject modal = Modal.instantiate(
                headerText,
                "새로운 닉네임",
                AccountManager.Instance.NickName, 
                Modal.Type.INSERT, (str) => {
                    AccountManager.Instance.ChangeNicknameReq(str, ticketHave);
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
        else {
            Fbl_Translator translator = AccountManager.Instance.GetComponent<Fbl_Translator>();
            if (AccountManager.Instance.userData.gold < 100) {
                string header1 = translator.GetLocalizedText("UIPopup", "ui_popup_myinfo_lackofgold");
                string header2 = translator.GetLocalizedText("UIPopup", "ui_popup_myinfo_goshopforgold");
                string headerText = header1 + "\n" + header2;
            
                Modal.instantiate(headerText, Modal.Type.YESNO, () => {
                    _hudController.CloseUserInfo();
                    MenuSceneController.ClickMenuButton("Shop");
                });
            }
        }
    }

    public void RequestBattleRank() {
        AccountManager.Instance.RequestBattleRank();
        transform.Find("InnerCanvas/Blocker").gameObject.SetActive(true);
    }

    void OpenBattleRank(Enum Event_Type, Component Sender, object Param) {
        transform.Find("InnerCanvas/Blocker").gameObject.SetActive(false);
        Transform top5 = battleRankWindow.Find("Top5LeaderBoard");
        Transform myBoard = battleRankWindow.Find("MyLeaderBoard");
        var rankTable = AccountManager.Instance.battleRank;
        for (int i = 0; i < top5.childCount; i++) {
            top5.GetChild(i).gameObject.SetActive(false);
            top5.GetChild(i).Find("Player").gameObject.SetActive(false);
        }
        for (int i = 0; i < myBoard.childCount; i++) {
            myBoard.GetChild(i).gameObject.SetActive(false);
            myBoard.GetChild(i).Find("Player").gameObject.SetActive(false);
        }
        int num = 0;
        if (rankTable.top.Length > 0) {
            foreach (dataModules.UserRank user in rankTable.top) {
                top5.GetChild(num).Find("RankIcon").GetComponent<Image>().sprite = AccountManager.Instance.resource.rankIcons[user.rankId.ToString()];
                top5.GetChild(num).Find("NickName").GetComponent<TMPro.TextMeshProUGUI>().text = user.nickName;
                top5.GetChild(num).Find("Medals").GetComponent<TMPro.TextMeshProUGUI>().text = user.score.ToString();
                if (AccountManager.Instance.userData.suid == user.suid && AccountManager.Instance.userData.serverId == user.serverId)
                    top5.GetChild(num).Find("Player").gameObject.SetActive(true);
                top5.GetChild(num).gameObject.SetActive(true);
                num++;
            }
        }
        num = 0;
        if (rankTable.my != null) {
            foreach (dataModules.UserRank user in rankTable.my) {
                myBoard.GetChild(num).Find("RankIcon").GetComponent<Image>().sprite = AccountManager.Instance.resource.rankIcons[user.rankId.ToString()];
                myBoard.GetChild(num).Find("Rank/Text").GetComponent<Text>().text = user.rank.ToString();
                myBoard.GetChild(num).Find("NickName").GetComponent<TMPro.TextMeshProUGUI>().text = user.nickName;
                myBoard.GetChild(num).Find("Medals").GetComponent<TMPro.TextMeshProUGUI>().text = user.score.ToString();
                if (AccountManager.Instance.userData.suid == user.suid && AccountManager.Instance.userData.serverId == user.serverId)
                    myBoard.GetChild(num).Find("Player").gameObject.SetActive(true);
                myBoard.GetChild(num).gameObject.SetActive(true);
                num++;
            }
        }
        battleRankWindow.gameObject.SetActive(true);

        EscapeKeyController.escapeKeyCtrl.AddEscape(CloseBattleRank);
    }

    public void CloseBattleRank() {
        battleRankWindow.gameObject.SetActive(false);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CloseBattleRank);
    }

    public void BattleRankInformation() {
        string[] check = new string[1];
        check[0] = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("UIPopup", "ui_popup_check");
        Modal.instantiate(AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("UIPopup", "ui_popup_myinfo_battlerank_refreshrank"), Modal.Type.CHECK, null, null, null, check, null);
    }



    public void RequestTotalRank() {
        AccountManager.Instance.RequestTotalRank();
        transform.Find("InnerCanvas/Blocker").gameObject.SetActive(true);
    }
    void OpenTotalRank(Enum Event_Type, Component Sender, object Param) {
        transform.Find("InnerCanvas/Blocker").gameObject.SetActive(false);
        Transform my = totalRankWindow.Find("MyPlayRank");
        Transform totalBoard = totalRankWindow.Find("TotalRank/ViewPort/TotalRankBoard");
        for (int i = 0; i < totalBoard.childCount; i++) {
            totalBoard.GetChild(i).gameObject.SetActive(false);
            totalBoard.GetChild(i).Find("Player").gameObject.SetActive(false);
        }
        var rankTable = AccountManager.Instance.totalRank;
        my.GetChild(0).Find("Rank/Text").GetComponent<TMPro.TextMeshProUGUI>().text = rankTable.myInfo.rank.ToString();
        my.GetChild(0).Find("RankIcon").GetComponent<Image>().sprite = AccountManager.Instance.resource.rankIcons[rankTable.myInfo.rankId.ToString()];
        my.GetChild(0).Find("NickName").GetComponent<TMPro.TextMeshProUGUI>().text = rankTable.myInfo.nickName;
        my.GetChild(0).Find("PlayScore").GetComponent<TMPro.TextMeshProUGUI>().text = rankTable.myInfo.score.ToString();
        

        int num = 0;
        
        foreach (dataModules.UserRank user in rankTable.top) {
            totalBoard.GetChild(num).Find("RankIcon").GetComponent<Image>().sprite = AccountManager.Instance.resource.rankIcons[user.rankId.ToString()];
            totalBoard.GetChild(num).Find("Rank/Text").GetComponent<Text>().text = user.rank.ToString();
            totalBoard.GetChild(num).Find("NickName").GetComponent<TMPro.TextMeshProUGUI>().text = user.nickName;
            totalBoard.GetChild(num).Find("Medals").GetComponent<TMPro.TextMeshProUGUI>().text = user.score.ToString();
            if (AccountManager.Instance.userData.suid == user.suid && AccountManager.Instance.userData.serverId == user.serverId)
                totalBoard.GetChild(num).Find("Player").gameObject.SetActive(true);
            totalBoard.GetChild(num).gameObject.SetActive(true);
            num++;
        }
        totalRankWindow.gameObject.SetActive(true);

        EscapeKeyController.escapeKeyCtrl.AddEscape(CloseTotalRank);
    }

    public void CloseTotalRank() {
        totalRankWindow.gameObject.SetActive(false);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CloseTotalRank);
    }

    public void TotalRankInformation() {
        string[] check = new string[1];
        check[0] = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("UIPopup", "ui_popup_check");
        Modal.instantiate(AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("UIPopup", "ui_popup_myinfo_totalrank"), Modal.Type.CHECK, null, null, null, check, null);
    }
}
