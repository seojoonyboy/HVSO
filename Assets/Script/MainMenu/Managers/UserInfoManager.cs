using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInfoManager : MonoBehaviour
{
    public void SetUserInfo() {
        transform.Find("InnerCanvas/Content/BaseInfo/LevelFrame/LevelValue").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userData.lv.ToString();
        transform.Find("InnerCanvas/Content/BaseInfo/UserId").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.NickName;
        transform.Find("InnerCanvas/Content/BaseInfo/Exp/Slider/SliderValue").GetComponent<Image>().fillAmount = (float)AccountManager.Instance.userData.exp / (float)AccountManager.Instance.userData.lvExp;
        transform.Find("InnerCanvas/Content/BaseInfo/Exp/ExpValue").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userData.exp.ToString() + "/" + AccountManager.Instance.userData.lvExp.ToString();
        EscapeKeyController.escapeKeyCtrl.AddEscape(ExitUserInfo);
    }

    public void ExitUserInfo() {
        gameObject.SetActive(false);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(ExitUserInfo);
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
