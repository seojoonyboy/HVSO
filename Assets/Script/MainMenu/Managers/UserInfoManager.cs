using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInfoManager : MonoBehaviour
{
    public void SetUserInfo() {
        transform.Find("Content/BaseInfo/LevelFrame/LevelValue").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userData.lv.ToString();
        transform.Find("Content/BaseInfo/UserId").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.NickName;
        transform.Find("Content/BaseInfo/Exp/Slider/SliderValue").GetComponent<Image>().fillAmount = (float)AccountManager.Instance.userData.exp / (float)AccountManager.Instance.userData.lvExp;
        transform.Find("Content/BaseInfo/Exp/ExpValue").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userData.exp.ToString() + "/" + AccountManager.Instance.userData.lvExp.ToString();
    }

    public void ExitUserInfo() {
        gameObject.SetActive(false);
    }
}
