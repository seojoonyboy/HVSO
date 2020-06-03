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

    /// <summary>
    /// 배경 이미지가 포함되어 있는 보상 아이콘 이미지
    /// </summary>
    /// <param name="_keyword"></param>
    /// <param name="layerOrder"></param>
    public void RequestDescriptionModalWithBg(string _keyword, int layerOrder = -1) {
        string filteredKeyword = FilteringKeyword(_keyword);
        
        Description description = GetDescription(filteredKeyword);
        __instantiateModal(description, filteredKeyword, layerOrder, true);
    }

    private void __instantiateModal(Description description, string _keyword, int layerOrder = -1, bool withBg = false) {
        modal = Instantiate(rewordDescModal);
        modal
            .GetComponent<Button>()
            .onClick.AddListener(() => { DestroyModal(); });
        modal
            .transform
            .Find("InnerModal/Content/Button")
            .GetComponent<Button>()
            .onClick.AddListener(() => { DestroyModal(); });

        Transform content = modal.transform.Find("InnerModal/Content");
        content.Find("Header").GetComponent<TextMeshProUGUI>().text = description.name;
        content.Find("Description").GetComponent<TextMeshProUGUI>().text = description.description;
        EscapeKeyController.escapeKeyCtrl.AddEscape(DestroyModal);

        if (layerOrder != -1) {
            modal.GetComponent<Canvas>().sortingOrder = layerOrder;
        }

        Transform targetSlot = withBg
            ? modal.transform.Find("InnerModal/SlotCase2")
            : modal.transform.Find("InnerModal/SlotCase1");
        
        targetSlot.gameObject.SetActive(true);
        
        var resource = AccountManager.Instance.resource;
        try {
            var targetImg = withBg ? resource.rewardIconsInDescriptionModal[_keyword] : resource.rewardIcon[_keyword];
            targetSlot.Find("Icon").GetComponent<Image>().sprite = targetImg;
        }
        catch (Exception ex) {
            //이곳으로 빠진다면 FilteringKeyword에 해당 값을 추가해 주시오.
            //아예 새로운 리소스라면 rewardIconsInDescriptionModal 딕셔너리에 해당 이미지를 추가해 주시오.
            Logger.LogError(_keyword + "에 대한 보상 아이콘을 찾을 수 없음");
        }
    }

    public string FilteringKeyword(string _keyword) {
        string keyword = _keyword.ToLower();
        if (keyword.Contains("heroSpecific")) return "heroshard";
        if (keyword.Contains("x2")) return "x2coupon";
        if (keyword.Contains("crystal")) return "magiccrystal";
        if (keyword.Contains("reinforcedbox")) return "enhancebox";
        if (keyword.Contains("extralargebox")) return "enormousbox";
        if (keyword.Contains("largebox") && !keyword.Contains("extra")) return keyword;
        if (keyword.Contains("supplybox")) return "enhancebox";
        if (keyword.Contains("gold")) return "gold";
        if (keyword.Equals("supply")) return "presupply";
        if (keyword.Equals("cardsuperrare")) return "cardSuperRare";
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
