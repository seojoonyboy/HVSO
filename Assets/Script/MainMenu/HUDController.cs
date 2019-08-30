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
    [SerializeField] Image expSlider;

    Transform
        userInfoUI,
        backbuttonUI,
        resourceUI;
    Button backButton;

    public void SetHeader(Type type) {
        switch (type) {
            case Type.RESOURCE_ONLY_WITH_BACKBUTTON:
                userInfoUI.gameObject.SetActive(false);
                backbuttonUI.gameObject.SetActive(true);
                resourceUI.gameObject.SetActive(true);
                break;
            default:
            case Type.SHOW_USER_INFO:
                backbuttonUI.gameObject.SetActive(false);
                userInfoUI.gameObject.SetActive(true);
                resourceUI.gameObject.SetActive(true);
                break;
            case Type.HIDE:
                resourceUI.gameObject.SetActive(false);
                backbuttonUI.gameObject.SetActive(false);
                userInfoUI.gameObject.SetActive(false);
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

        backButton = backbuttonUI.Find("BackButton").GetComponent<Button>();
        SetResourcesUI();
    }

    // Start is called before the first frame update
    void Start() {
        SetHeader(Type.SHOW_USER_INFO);
        main_HorizontalScrollSnap.OnSelectionChangeEndEvent.AddListener(x => OnPageChanged(x));
    }

    public void OnPageChanged(int pageNum) {
        SetHeader(Type.SHOW_USER_INFO);
    }
    
    public void SetResourcesUI() {
        lvValue.text = AccountManager.Instance.userResource.lv.ToString();
        expSlider.fillAmount = (float)AccountManager.Instance.userResource.exp / (float)AccountManager.Instance.userResource.lvExp;
        expValueText.text = AccountManager.Instance.userResource.exp.ToString() + "/" + AccountManager.Instance.userResource.lvExp;
        crystalValue.text = AccountManager.Instance.userResource.crystal.ToString();
        goldValue.text = AccountManager.Instance.userResource.gold.ToString();
    }

    public enum Type {
        SHOW_USER_INFO = 0,
        RESOURCE_ONLY_WITH_BACKBUTTON = 1,
        HIDE = 10
    }
}
