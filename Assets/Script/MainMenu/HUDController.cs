using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class HUDController : MonoBehaviour {
    [SerializeField] HorizontalScrollSnap main_HorizontalScrollSnap;
    [SerializeField] TMPro.TextMeshProUGUI crystalValue;
    [SerializeField] TMPro.TextMeshProUGUI goldValue;
    [SerializeField] Text lvValue;
    [SerializeField] TMPro.TextMeshProUGUI expValueText;
    [SerializeField] Transform userInfoCanvas;
    [SerializeField] Image expSlider;
    [SerializeField] private TMPro.TextMeshProUGUI nickName;

    Dictionary<string, Transform> subHeaders;

    Button backButton;
    BoxRewardManager box;

    /// <summary>
    /// 외부에서 사용시 해당 함수로 접근
    /// </summary>
    /// <param name="type">헤더의 포멧형태</param>
    public void SetHeader(Type type) {
        subHeaders["gradation"].gameObject.SetActive(true);
        switch (type) {
            case Type.RESOURCE_ONLY_WITH_BACKBUTTON:
                _SetHeader(new List<string> { "backbuttonUI", "resourceUI" });
                break;
            default:
            case Type.SHOW_USER_INFO:
                _SetHeader(new List<string> { "userInfoUI", "resourceUI" });
                break;
            case Type.HIDE:
                _SetHeader(new List<string>());
                break;
            case Type.BATTLE_READY_CANVAS:
                _SetHeader(new List<string> { "backbuttonUI", "seasonDescUI" });
                break;
            case Type.ONLY_BAKCK_BUTTON:
                _SetHeader(new List<string> { "backbuttonUI" });
                break;
        }
    }

    private void _SetHeader(List<string> activateList) {
        foreach(KeyValuePair<string, Transform> item in subHeaders) {
            if (activateList.Contains(item.Key)) {
                subHeaders[item.Key].gameObject.SetActive(true);
            }
            else {
                subHeaders[item.Key].gameObject.SetActive(false);
            }
        }
    }

    public void SetBackButton(UnityAction action) {
        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(action);
    }

    public void SetBackButtonMsg(string msg) {
        subHeaders["backbuttonUI"].Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = string.Empty;
    }

    private void Awake() {
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_UPDATED, OnUserDataUpdated);
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_NICKNAME_UPDATED, OnUserDataUpdated);

        InitSubHeaders();
    }

    private void InitSubHeaders() {
        subHeaders = new Dictionary<string, Transform>();

        subHeaders["gradation"] = transform
            .GetChild(0)
            .GetChild(0)
            .Find("Gradation");

        subHeaders["userInfoUI"] = transform
            .GetChild(0)
            .GetChild(0)
            .Find("UserInfoUI");

        subHeaders["backbuttonUI"] = transform
            .GetChild(0)
            .GetChild(0)
            .Find("BackButtonUI");

        subHeaders["resourceUI"] = transform
            .GetChild(0)
            .GetChild(0)
            .Find("Right");

        subHeaders["dictionaryUI"] = transform
            .GetChild(0)
            .GetChild(0)
            .Find("DictionaryHeader");

        subHeaders["seasonDescUI"] = transform.GetChild(0).GetChild(0).Find("SeasonDesc");

        backButton = subHeaders["backbuttonUI"]
            .Find("BackButton")
            .GetComponent<Button>();

        box = transform.Find("GetReward").GetComponent<BoxRewardManager>();
    }

    private void OnUserDataUpdated(Enum Event_Type, Component Sender, object Param) {
        SetResourcesUI();
        SetUserNickName();
    }

    void Start() {
        SetHeader(Type.SHOW_USER_INFO);
    }

    void OnDestroy() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_UPDATED, OnUserDataUpdated);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_NICKNAME_UPDATED, OnUserDataUpdated);
    }

    public void OnPageChanged(int pageNum) {
        switch (pageNum) {
            default:
                SetHeader(Type.SHOW_USER_INFO);
                break;
        }
    }
    
    public void SetResourcesUI() {
        var accountManager = AccountManager.Instance;
        var userResource = accountManager.userResource;

        lvValue.text = userResource.lv.ToString();
        expSlider.fillAmount = (float)userResource.lvExp / (float)userResource.nextLvExp;
        expValueText.text = userResource.exp.ToString() + "/" + userResource.lvExp;
        crystalValue.text = userResource.crystal.ToString();
        goldValue.text = userResource.gold.ToString();
        box.SetBoxObj();
    }

    public void SetUserNickName() {
        nickName.text = AccountManager.Instance.NickName;
    }

    public void OpenUserInfo() {
        SetHeader(Type.RESOURCE_ONLY_WITH_BACKBUTTON);
        SetBackButton(() => CloseUserInfo());
        userInfoCanvas.gameObject.SetActive(true);
        AccountManager.Instance.RequestUserStatistics();
        EscapeKeyController.escapeKeyCtrl.AddEscape(CloseUserInfo);
    }

    public void CloseUserInfo() {
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CloseUserInfo);
        SetHeader(Type.SHOW_USER_INFO);
        userInfoCanvas.gameObject.SetActive(false);
    }

    public enum Type {
        SHOW_USER_INFO = 0,
        RESOURCE_ONLY_WITH_BACKBUTTON = 1,
        DICTIONARY_WINDOW = 2,
        BATTLE_READY_CANVAS = 3,
        ONLY_BAKCK_BUTTON =4,
        HIDE = 10
    }
}
