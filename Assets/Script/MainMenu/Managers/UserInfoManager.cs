using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInfoManager : MonoBehaviour {
    [SerializeField] MenuSceneController MenuSceneController;

    void Start() {
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_LEAGUE_INFO_UPDATED, OnLeagueInfoUpdated);
        AccountManager.Instance.RequestLeagueInfo();

        var stateHandler = MainSceneStateHandler.Instance;
        bool NickNameChangeTutorialLoaded = stateHandler.GetState("NickNameChangeTutorialLoaded");
        bool isTutoFinished = stateHandler.GetState("IsTutorialFinished");
        if (isTutoFinished && !NickNameChangeTutorialLoaded) {
            MenuSceneController.StartQuestSubSet(MenuTutorialManager.TutorialType.SUB_SET_103);
            stateHandler.ChangeState("NickNameChangeTutorialLoaded", true);
        }
    }

    void OnDisable() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_LEAGUE_INFO_UPDATED, OnLeagueInfoUpdated);
    }

    void OnDestroy() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_LEAGUE_INFO_UPDATED, OnLeagueInfoUpdated);
    }

    private void OnLeagueInfoUpdated(Enum Event_Type, Component Sender, object Param) {
        SetUserInfo();
    }

    public void SetUserInfo() {
        Transform contents = transform.Find("InnerCanvas/Viewport/Content");
        contents.Find("PlayerInfoPanel/Info/Level/Value").GetComponent<Text>().text = AccountManager.Instance.userData.lv.ToString();
        contents.Find("PlayerInfoPanel/Info/UserName").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.NickName;
        contents.Find("PlayerInfoPanel/Info/LevelGauge/ValueSlider").GetComponent<Slider>().value = (float)AccountManager.Instance.userData.lvExp / (float)AccountManager.Instance.userData.nextLvExp;
        contents.Find("PlayerInfoPanel/Info/LevelGauge/ValueText").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userData.lvExp.ToString() + "/" + AccountManager.Instance.userData.nextLvExp.ToString();
        string tierName = AccountManager.Instance.scriptable_leagueData.leagueInfo.rankDetail.minorRankName;
        if (string.IsNullOrEmpty(tierName)) tierName = "detail";
        if (AccountManager.Instance.resource.rankIcons.ContainsKey(tierName))
            contents.Find("BattleInfoPanel/TierBannerBtn/RankIcon").GetComponent<Image>().sprite = AccountManager.Instance.resource.rankIcons[tierName];
        else
            contents.Find("BattleInfoPanel/TierBannerBtn/RankIcon").GetComponent<Image>().sprite = AccountManager.Instance.resource.rankIcons["default"];

        contents.Find("BattleInfoPanel/TierBannerBtn/RankName").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.scriptable_leagueData.leagueInfo.rankDetail.minorRankName;
        contents.Find("BattleInfoPanel/TierBannerBtn/MMRValue").GetComponent<Text>().text = AccountManager.Instance.scriptable_leagueData.leagueInfo.ratingPoint.ToString();
    }

    public void ChangeId() {
        GameObject modal = Modal.instantiate("변경하실 닉네임을 입력해 주세요.", "새로운 닉네임", AccountManager.Instance.NickName, Modal.Type.INSERT, (str) => {
            if (string.IsNullOrEmpty(str)) {
                Modal.instantiate("빈 닉네임은 허용되지 않습니다.", Modal.Type.CHECK);
            }
            else {
                AccountManager.Instance.ChangeNicknameReq(str, ChangeCallback);
            }
        });
        
        modal.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => {
            Destroy(modal);
        });
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
