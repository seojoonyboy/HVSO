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
        string filteredKeyword = FilteringKeyword(_keyword);
        
        Description description = GetDescription(filteredKeyword);

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
        if(AccountManager.Instance.resource.rewardIcon.ContainsKey(_keyword))
            modal.transform.Find("InnerModal/Slot/Icon").GetComponent<Image>().sprite = AccountManager.Instance.resource.rewardIcon[_keyword];
        EscapeKeyController.escapeKeyCtrl.AddEscape(DestroyModal);
    }

    public string FilteringKeyword(string _keyword) {
        string keyword = _keyword.ToLower();
        if (keyword.Contains("x2")) return "supplyX2Coupon";
        if (keyword.Contains("crystal")) return "magiccrystal";
        if (keyword.Contains("reinforcedbox")) return "enhancebox";
        if (keyword.Contains("extralargebox")) return "enormousbox";
        if (keyword.Contains("largebox") && !keyword.Contains("extra")) return keyword;
        if (keyword.Contains("supplybox")) return "enhancebox";
        if (keyword.Contains("gold")) return "gold";
        return _keyword;
    }

    public void DestroyModal() {
        Destroy(modal);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(DestroyModal);
    }

    public Description GetDescription(string _keyword) {
        string keyword = string.Empty;
        string rarelity = string.Empty;
        
        if (_keyword.Contains("card")) {
            keyword = "randomgradecard";
            string temp = _keyword.Remove(0, 4);
            temp = temp.ToLower();
            rarelity = _translator.GetLocalizedText("MainUI", "ui_page_cardmanage_" + temp);
        }

        else if (_keyword.ToLower().Contains("crystal")) {
            keyword = "magiccrystal";
        }
        else if (_keyword.ToLower().Contains("h10001") || _keyword.ToLower().Contains("h10002") ||
                 _keyword.ToLower().Contains("h10003") || _keyword.ToLower().Contains("h10004")) {
            keyword = "heroshard";
        }
        else {
            keyword = _keyword;
        }

        string desc_key = $"goods_{keyword}_txt";
        var desc_result = _translator.GetLocalizedText("Goods", desc_key);
        if (string.IsNullOrEmpty(desc_result)) {
            Logger.LogWarning("재화 " + desc_key + "에 대한 설명 번역값을 찾을 수 없습니다.");
        }

        string name_key = "goods_" + keyword;
        var name_result = _translator.GetLocalizedText("Goods", name_key);
        if (string.IsNullOrEmpty(name_result)) {
            Logger.LogWarning("재화 " + name_result + "에 대한 이름 번역값을 찾을 수 없습니다.");
        }

        if (keyword == "randomgradecard") {
            desc_result = desc_result.Replace("{n}", rarelity);
            name_result = name_result.Replace("{n}", rarelity);
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
