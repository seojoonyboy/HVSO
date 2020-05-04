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
        contents.Find("PlayerInfoPanel/Info/LevelGauge/ValueSlider").GetComponent<Slider>().value = (float)AccountManager.Instance.userData.lvExp / (float)AccountManager.Instance.userData.nextLvExp;
        contents.Find("PlayerInfoPanel/Info/LevelGauge/ValueText").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userData.lvExp.ToString() + "/" + AccountManager.Instance.userData.nextLvExp.ToString();

        int tier = AccountManager.Instance.scriptable_leagueData.leagueInfo.rankDetail.id;
        if (AccountManager.Instance.resource.rankIcons.ContainsKey(tier.ToString()))
            contents.Find("BattleInfoPanel/TierBannerBtn/RankIcon").GetComponent<Image>().sprite = AccountManager.Instance.resource.rankIcons[tier.ToString()];
        else
            contents.Find("BattleInfoPanel/TierBannerBtn/RankIcon").GetComponent<Image>().sprite = AccountManager.Instance.resource.rankIcons["default"];

        contents.Find("BattleInfoPanel/TierBannerBtn/RankName").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.scriptable_leagueData.leagueInfo.rankDetail.minorRankName;
        contents.Find("BattleInfoPanel/TierBannerBtn/MMRValue").GetComponent<Text>().text = AccountManager.Instance.scriptable_leagueData.leagueInfo.ratingPoint.ToString();
    }

    public void ChangeNickName() {
        Fbl_Translator translator = AccountManager.Instance.GetComponent<Fbl_Translator>();
        string header1 = translator.GetLocalizedText("UIPopup", "ui_popup_myinfo_namechangecost");
        header1 = header1.Replace("{n}", "100");
        string header2 = translator.GetLocalizedText("UIPopup", "ui_popup_myinfo_namechangeconfirm");
        string headerText = header1 + "\n" + header2;
        
        GameObject modal = Modal.instantiate(headerText, "새로운 닉네임", AccountManager.Instance.NickName, Modal.Type.INSERT, (str) => {
            AccountManager.Instance.ChangeNicknameReq(str, InvalidCallbackReceived);
        });
        
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
        
        else if (msg == "ChangeRight") {
            Modal.instantiate("닉네임 변경권이 없습니다.", Modal.Type.CHECK, () => { });
        }
    }

    private void ChangeCallback() {
        AccountManager.Instance.RequestUserInfo((req, res) => {
            if(res.IsSuccess) {
                Modal.instantiate("닉네임 변경이 적용 됐습니다.", Modal.Type.CHECK);
                SetUserInfo();
            }
            else {
                Modal.instantiate("닉네임 변경이 실패했습니다.\n다시 한번 시도바랍니다.", Modal.Type.CHECK);
            }
        });
    }
}
