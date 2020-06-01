using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SoftResetRewardDescModal : SerializedMonoBehaviour {
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private List<RowData> rowDatas;
    
    private Fbl_Translator _translator;
    private ResourceManager _resourceManager;
    
    private bool isInit = false;
    
    private void Awake() {
        var accountManager = AccountManager.Instance;
        _translator = accountManager.GetComponent<Fbl_Translator>();
        _resourceManager = accountManager.resource.GetComponent<ResourceManager>();
    }

    void Start() {
        if(isInit) return;
        
        SetRowsUi();
        isInit = true;
    }

    public class RowData {
        public List<SlotData> SlotDatas;
        public List<RewardData> rewards;
        public Sprite rowBg;
        public bool isTopTier = false;
    }
    
    public class SlotData {
        public int tier;
    }
    
    public class RewardData {
        public rewardType type;
        public uint amount;
    }
    
    public enum rewardType {
        NormalBox,
        reinforcedBox,
        extraLargeBox,
        largeBox,
        cardCommon,
        cardUncommon,
        cardRare,
        cardSuperrare,
        cardLegend,
        gold,
        goldFree,
        crystal
    }

    private void SetRowsUi() {
        int rowIndex = 1;
        foreach (var rowData in rowDatas) {
            Transform rowObj = _scrollRect.content.GetChild(rowIndex);
            rowObj.transform.Find("Bg").GetComponent<Image>().sprite = rowData.rowBg;
            rowObj.transform.Find("Bg").gameObject.SetActive(true);
            rowObj.gameObject.SetActive(true);
            
            Transform innerObj = !rowData.isTopTier ? rowObj.transform.Find("Type1") : rowObj.transform.Find("Type2");
            innerObj.gameObject.SetActive(true);
            
            SetRewardsUi(innerObj.gameObject, rowData);
            SetTierUi(innerObj.gameObject, rowData.SlotDatas);
            
            rowIndex++;
        }
    }

    private void SetRewardsUi(GameObject rowObj, RowData rowData) {
        Transform rewardsParent = rowObj.transform.Find("RewardArea");
        int rewardIndex = 0;
        foreach (var rewardData in rowData.rewards) {
            Transform rewardObj = rewardsParent.GetChild(rewardIndex);
            rewardObj.gameObject.SetActive(true);
            if (_resourceManager.rewardIcon.ContainsKey(rewardData.type.ToString())) {
                var imgObj = rewardObj.Find("Image");
                imgObj.gameObject.SetActive(true);
                imgObj
                    .GetComponent<Image>()
                    .sprite = _resourceManager.rewardIcon[rewardData.type.ToString()];
            }

            var amountObj = rewardObj.Find("Amount");
            amountObj.gameObject.SetActive(true);
            var amountTxt = amountObj.GetComponent<TextMeshProUGUI>();
            amountTxt.text = "x" + rewardData.amount;
                
            rewardIndex++;
        }
    }

    private void SetTierUi(GameObject rowObj, List<SlotData> slotDatas) {
        Transform tierParent = rowObj.transform.Find("TierArea");
        int tierIndex = 0;
        foreach (var slotData in slotDatas) {
            Transform tierObj = tierParent.GetChild(tierIndex);
            tierObj.gameObject.SetActive(true);
            if (_resourceManager.rankIcons.ContainsKey(slotData.tier.ToString())) {
                tierObj
                    .GetComponent<Image>()
                    .sprite = _resourceManager.rankIcons[slotData.tier.ToString()];
            }
            tierIndex++;
        }
    }
}