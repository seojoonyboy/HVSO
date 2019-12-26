using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInfoManager : MonoBehaviour
{
    public void SetUserInfo() {
        AccountManager.Instance.RequestLeagueInfo();

        Transform contents = transform.Find("InnerCanvas/Content");
        contents.Find("BaseInfo/LevelFrame/LevelValue").GetComponent<Text>().text = AccountManager.Instance.userData.lv.ToString();
        contents.Find("BaseInfo/UserId").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.NickName;
        contents.Find("BaseInfo/Exp/Slider/SliderValue").GetComponent<Image>().fillAmount = (float)AccountManager.Instance.userData.exp / (float)AccountManager.Instance.userData.lvExp;
        contents.Find("BaseInfo/Exp/ExpValue").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userData.exp.ToString() + "/" + AccountManager.Instance.userData.lvExp.ToString();
        string tierName = AccountManager.Instance.scriptable_leagueData.leagueInfo.rankDetail.minorRankName;
        if (AccountManager.Instance.resource.rankIcons.ContainsKey(tierName))
            contents.Find("TierInfo/TierImage").GetComponent<Image>().sprite = AccountManager.Instance.resource.rankIcons[tierName];
        else
            contents.Find("TierInfo/TierImage").GetComponent<Image>().sprite = AccountManager.Instance.resource.rankIcons["default"];
        contents.Find("TierInfo/Score/Value").GetComponent<Text>().text = AccountManager.Instance.scriptable_leagueData.leagueInfo.ratingPoint.ToString();
        contents.Find("TierInfo/Wins/Value").GetComponent<TMPro.TextMeshProUGUI>().text = "";
    }

    public void ChangeId() {
        Modal.instantiate("변경하실 닉네임을 입력해 주세요.", "새로운 닉네임", AccountManager.Instance.NickName, Modal.Type.INSERT, (str) => {
            if (string.IsNullOrEmpty(str)) {
                Modal.instantiate("빈 닉네임은 허용되지 않습니다.", Modal.Type.CHECK);
            }
            else {
                AccountManager.Instance.ChangeNicknameReq(str, ChangeCallback);
            }
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
