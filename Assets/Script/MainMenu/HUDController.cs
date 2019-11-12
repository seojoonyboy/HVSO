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
    [SerializeField] TMPro.TextMeshProUGUI lvValue;
    [SerializeField] TMPro.TextMeshProUGUI expValueText;
    [SerializeField] Transform userInfoCanvas;
    [SerializeField] Image expSlider;
    [SerializeField] private TMPro.TextMeshProUGUI nickName;
    
    Transform
        gradation,
        userInfoUI,
        backbuttonUI,
        resourceUI,
        battleCoinUI,
        dictionaryUI;
    Button backButton;
    BoxRewardManager box;

    public void SetHeader(Type type) {
        gradation.gameObject.SetActive(true);
        switch (type) {
            case Type.RESOURCE_ONLY_WITH_BACKBUTTON:
                userInfoUI.gameObject.SetActive(false);
                backbuttonUI.gameObject.SetActive(true);
                resourceUI.gameObject.SetActive(true);
                battleCoinUI.gameObject.SetActive(false);
                dictionaryUI.gameObject.SetActive(false);
                break;
            default:
            case Type.SHOW_USER_INFO:
                backbuttonUI.gameObject.SetActive(false);
                userInfoUI.gameObject.SetActive(true);
                resourceUI.gameObject.SetActive(true);
                battleCoinUI.gameObject.SetActive(false);
                dictionaryUI.gameObject.SetActive(false);
                break;
            case Type.HIDE:
                resourceUI.gameObject.SetActive(false);
                userInfoUI.gameObject.SetActive(false);
                dictionaryUI.gameObject.SetActive(false);
                backbuttonUI.gameObject.SetActive(false);
                gradation.gameObject.SetActive(false);
                break;
            case Type.DICTIONARY_WINDOW:
                //backbuttonUI.gameObject.SetActive(false);
                //userInfoUI.gameObject.SetActive(true);
                //resourceUI.gameObject.SetActive(true);
                //dictionaryUI.gameObject.SetActive(true);
                backbuttonUI.gameObject.SetActive(false);
                userInfoUI.gameObject.SetActive(true);
                resourceUI.gameObject.SetActive(true);
                backbuttonUI.gameObject.SetActive(false);
                dictionaryUI.gameObject.SetActive(false);
                break;
            case Type.BATTLE_READY_CANVAS:
                backbuttonUI.gameObject.SetActive(true);
                userInfoUI.gameObject.SetActive(false);
                resourceUI.gameObject.SetActive(false);
                battleCoinUI.gameObject.SetActive(true);
                dictionaryUI.gameObject.SetActive(false);
                break;
            case Type.ONLY_BAKCK_BUTTON:
                backbuttonUI.gameObject.SetActive(true);
                userInfoUI.gameObject.SetActive(false);
                resourceUI.gameObject.SetActive(false);
                battleCoinUI.gameObject.SetActive(false);
                dictionaryUI.gameObject.SetActive(false);
                break;
        }
    }

    public void SetHeader(int type) {
        SetHeader((Type)type);
    }

    public void SetBackButton(UnityAction action) {
        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(action);
        backButton.onClick.AddListener(() => main_HorizontalScrollSnap.GoToScreen(2));
    }

    public void SetBackButtonMsg(string msg) {
        backbuttonUI.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = string.Empty;
        //backbuttonUI.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = msg;
    }

    private void Awake() {
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_UPDATED, OnUserDataUpdated);
        gradation = transform.GetChild(0).GetChild(0).Find("Gradation");

        userInfoUI = transform
                    .GetChild(0)
                    .GetChild(0)
                    .Find("UserInfoUI");

        backbuttonUI = transform
                    .GetChild(0)
                    .GetChild(0)
                    .Find("BackButtonUI");

        resourceUI = transform
                    .GetChild(0)
                    .GetChild(0)
                    .Find("Right");

        battleCoinUI = transform
                    .GetChild(0)
                    .GetChild(0)
                    .Find("BattleCoin");

        dictionaryUI = transform
                    .GetChild(0)
                    .GetChild(0)
                    .Find("DictionaryHeader");

        backButton = backbuttonUI.Find("BackButton").GetComponent<Button>();
        box = transform.Find("GetReward").GetComponent<BoxRewardManager>();
    }

    private void OnUserDataUpdated(Enum Event_Type, Component Sender, object Param) {
        SetResourcesUI();
        SetUserNickName();
    }

    // Start is called before the first frame update
    void Start() {
        SetHeader(Type.SHOW_USER_INFO);
        //main_HorizontalScrollSnap.OnSelectionPageChangedEvent.AddListener(x => OnPageChanged(x));
    }

    void OnDestroy() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_UPDATED, OnUserDataUpdated);
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
        expSlider.fillAmount = (float)userResource.exp / (float)userResource.lvExp;
        expValueText.text = userResource.exp.ToString() + "/" + userResource.lvExp;
        crystalValue.text = userResource.crystal.ToString();
        goldValue.text = userResource.gold.ToString();
        box.SetBoxObj();
    }

    public void SetUserNickName() {
        nickName.text = AccountManager.Instance.NickName;
    }

    public void HideDictionaryUI() {
        dictionaryUI.gameObject.SetActive(false);
    }

    public void OpenUserInfo() {
        userInfoCanvas.GetComponent<UserInfoManager>().SetUserInfo();
        userInfoCanvas.gameObject.SetActive(true);
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
