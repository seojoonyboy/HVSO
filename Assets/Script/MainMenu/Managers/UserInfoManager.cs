using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInfoManager : MonoBehaviour {
    [SerializeField] MenuSceneController MenuSceneController;
    [SerializeField] private HUDController _hudController;
    
    void Start() {
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
