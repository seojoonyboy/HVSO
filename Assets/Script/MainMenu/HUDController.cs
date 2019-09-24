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

    Transform
        gradation,
        userInfoUI,
        backbuttonUI,
        resourceUI,
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
                dictionaryUI.gameObject.SetActive(false);
                break;
            default:
            case Type.SHOW_USER_INFO:
                backbuttonUI.gameObject.SetActive(false);
                userInfoUI.gameObject.SetActive(true);
                resourceUI.gameObject.SetActive(true);
                dictionaryUI.gameObject.SetActive(false);
                break;
            case Type.HIDE:
                resourceUI.gameObject.SetActive(false);
                backbuttonUI.gameObject.SetActive(false);
                userInfoUI.gameObject.SetActive(false);
                dictionaryUI.gameObject.SetActive(false);
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
    }

    public void SetBackButtonMsg(string msg) {
        backbuttonUI.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = string.Empty;
        //backbuttonUI.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = msg;
    }

    private void Awake() {
        AccountManager.Instance.OnUserResourceRefresh.AddListener(() => SetResourcesUI());

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

        dictionaryUI = transform
                    .GetChild(0)
                    .GetChild(0)
                    .Find("DictionaryHeader");

        backButton = backbuttonUI.Find("BackButton").GetComponent<Button>();
        box = transform.Find("GetReward").GetComponent<BoxRewardManager>();
        SetResourcesUI();
    }

    // Start is called before the first frame update
    void Start() {
        SetHeader(Type.SHOW_USER_INFO);
        main_HorizontalScrollSnap.OnSelectionPageChangedEvent.AddListener(x => OnPageChanged(x));
    }

    public void OnPageChanged(int pageNum) {
        switch (pageNum) {
            case 0:
                SetHeader(Type.DICTIONARY_WINDOW);
                break;
            default:
                SetHeader(Type.SHOW_USER_INFO);
                break;
        }
    }
    
    public void SetResourcesUI() {
        lvValue.text = AccountManager.Instance.userResource.lv.ToString();
        expSlider.fillAmount = (float)AccountManager.Instance.userResource.exp / (float)AccountManager.Instance.userResource.lvExp;
        expValueText.text = AccountManager.Instance.userResource.exp.ToString() + "/" + AccountManager.Instance.userResource.lvExp;
        crystalValue.text = AccountManager.Instance.userResource.crystal.ToString();
        goldValue.text = AccountManager.Instance.userResource.gold.ToString();
        box.SetBoxObj();
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
        HIDE = 10
    }
}
