using System;
using System.Collections;
using System.Collections.Generic;
using SocketFormat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroLevelUpHandler : MonoBehaviour {
    [SerializeField] private GameObject bg;
    [SerializeField] private GameObject slotParent;
    private ResourceManager _resourceManager;
    private void Awake() {
        _resourceManager = AccountManager.Instance.resource;
    }

    public void Init(List<LevelReward> rewards) {
        StartCoroutine(__Proceed(rewards));
    }

    IEnumerator __Proceed(List<LevelReward> rewards) {
        bg.SetActive(true);
        int slotIndex = 1;
        foreach (var reward in rewards) {
            if (reward.kind == "heroSpecific") { reward.kind = "heroSpecific_" + reward.detail; }
            
            var slotObj = slotParent.transform.GetChild(slotIndex);
            slotObj.gameObject.SetActive(true);
            
            Image img = slotObj.Find("Image").GetComponent<Image>();
            var selectedImg = _resourceManager.GetRewardIconWithBg(reward.kind);
            img.sprite = selectedImg;
            
            slotObj.Find("Amount").GetComponent<TextMeshProUGUI>().text = "x" + reward.amount;

            var btn = slotObj.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => RewardDescriptionHandler.instance.RequestDescriptionModalWithBg(reward.kind, 1000));
            
            slotIndex++;
            yield return new WaitForSeconds(0.3f);
        }
    }
}