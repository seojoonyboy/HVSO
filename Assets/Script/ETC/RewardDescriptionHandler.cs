using System;
using dataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardDescriptionHandler : MonoBehaviour {
    public GameObject rewordDescModal;
    private Fbl_Translator _translator;

    private static RewardDescriptionHandler m_instance;
    public static RewardDescriptionHandler instance {
        get {
            if (m_instance == null) m_instance = FindObjectOfType<RewardDescriptionHandler>();

            return m_instance;
        }
    }

    GameObject modal;

    private void Start() {
        _translator = AccountManager.Instance.GetComponent<Fbl_Translator>();
    }

    public void RequestDescriptionModal(string _keyword) {
        Description description = GetDescription(_keyword);

        modal = Instantiate(rewordDescModal);
        modal
            .GetComponent<Button>()
            .onClick.AddListener(() => { DestroyModal(); });
        modal
            .transform
            .Find("InnerModal/Content/Button")
            .GetComponent<Button>()
            .onClick.AddListener(() => { DestroyModal(); });

        //Logger.Log(description.name);
        Transform content = modal.transform.Find("InnerModal/Content");
        content.Find("Header").GetComponent<TextMeshProUGUI>().text = description.name;
        content.Find("Description").GetComponent<TextMeshProUGUI>().text = description.description;
        modal.transform.Find("InnerModal/Slot/Icon").GetComponent<Image>().sprite = AccountManager.Instance.resource.rewardIcon[_keyword];
        EscapeKeyController.escapeKeyCtrl.AddEscape(DestroyModal);
    }

    public void DestroyModal() {
        Destroy(modal);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(DestroyModal);
    }

    public Description GetDescription(string _keyword) {
        string keyword = string.Empty;

        if (_keyword.Contains("card")) {
            keyword = "card";
        }
        else if (_keyword.Contains("gold")) {
            keyword = "goldFree";
        }
        else {
            keyword = _keyword;
        }

        string desc_key = $"goods_{_keyword}_txt";
        var desc_result = _translator.GetLocalizedText("Goods", desc_key);
        if (string.IsNullOrEmpty(desc_result)) {
            Logger.LogWarning("재화 " + desc_key + "에 대한 설명 번역값을 찾을 수 없습니다.");
        }

        string name_key = "goods_" + _keyword;
        var name_result = _translator.GetLocalizedText("Goods", name_key);
        if (string.IsNullOrEmpty(name_result)) {
            Logger.LogWarning("재화 " + name_result + "에 대한 이름 번역값을 찾을 수 없습니다.");
        }
        
        Description description = new Description();
        description.name = name_result;
        description.description = desc_result;
        return description;
    }

    public class Description {
        public string name;
        public string description;
    }
}
