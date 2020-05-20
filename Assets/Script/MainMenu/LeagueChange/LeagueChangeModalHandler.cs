using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeagueChangeModalHandler : SerializedMonoBehaviour {
    public PrevLeagueDescUISet prevLeagueUISet;
    public NewLeagueUISet newLeagueUISet;
    
    #region TestCode
    public void OpenWithDummyData() {
        var rewards = new List<AccountManager.Reward>();
        
        var reward1 = new AccountManager.Reward();
        reward1.reward = new AccountManager.RewardInfo();
        reward1.reward.kind = "extraLargeBox";
        reward1.reward.amount = "1";
        rewards.Add(reward1);
        
        var reward2 = new AccountManager.Reward();
        reward2.reward = new AccountManager.RewardInfo();
        reward2.reward.kind = "goldFree";
        reward2.reward.amount = "100";
        rewards.Add(reward2);
        
        AccountManager.LeagueInfo tmp_prevLeagueData = new AccountManager.LeagueInfo();
        tmp_prevLeagueData.rewards = rewards;
        tmp_prevLeagueData.rankDetail = new AccountManager.RankDetail();
        tmp_prevLeagueData.rankDetail.minorRankName = "tier_norank";
        tmp_prevLeagueData.rankDetail.id = 18;
        
        AccountManager.LeagueInfo tmp_newLeagueData = new AccountManager.LeagueInfo();
        tmp_prevLeagueData.rankDetail = new AccountManager.RankDetail();
        tmp_prevLeagueData.rankDetail.minorRankName = "tier_norank";
        tmp_prevLeagueData.rankDetail.id = 18;
        
        OpenFirstWindow(tmp_prevLeagueData, tmp_newLeagueData);
    }
    #endregion

    private AccountManager.LeagueInfo prevLeagueData, newLeagueData;

    public void OpenFirstWindow(AccountManager.LeagueInfo prevLeagueData, AccountManager.LeagueInfo newLeagueData) {
        if(prevLeagueData.rewards == null) prevLeagueData.rewards = new List<AccountManager.Reward>();
        
        prevLeagueUISet.modal.gameObject.SetActive(true);
        float time = 0.5f;
        var hash = iTween.Hash("scale", Vector3.one, "time", time);
        iTween.ScaleTo(prevLeagueUISet.modal, hash);

        this.prevLeagueData = prevLeagueData;
        this.newLeagueData = newLeagueData;
        
        StartCoroutine(__OpenFirstWindow(time));
    }

    private IEnumerator __OpenFirstWindow(float time) {
        yield return new WaitForSeconds(time);
        SetRewardsUi();
        SetPrevRankUi();
    }

    public void CloseFirstWindow() {
        prevLeagueUISet.modal.transform.localScale = Vector3.zero;
        prevLeagueUISet.modal.SetActive(false);
    }

    public void ClosePopup() {
        // float time = 0.8f;
        // var hash = iTween.Hash("scale", Vector3.zero, "time", time);
        // iTween.ScaleTo(prevLeagueUISet.modal, hash);
        foreach (Transform tf in prevLeagueUISet.rewardLayoutGroup) {
            if(tf.name == "Description" || tf.name == "Background") continue;
            
            tf.transform.Find("Amount").GetComponent<TextMeshProUGUI>().text = String.Empty;
            tf.transform.Find("Image").GetComponent<Image>().enabled = false;
            tf.gameObject.SetActive(false);
        }
        prevLeagueUISet.leagueName.text = String.Empty;
        prevLeagueUISet.rewardLayoutGroup.gameObject.SetActive(false);
        
        newLeagueUISet.leagueName.text = String.Empty;
    }

    private void SetPrevRankUi() {
        // prevLeagueUISet.
        prevLeagueUISet.leagueName.text = prevLeagueData.rankDetail.minorRankName;
    }

    private void SetNewRankUi(int id, string rankName) {
        newLeagueUISet.leagueName.text = rankName;
    }
    
    private void SetRewardsUi() {
        prevLeagueUISet.rewardLayoutGroup.gameObject.SetActive(true);
        StartCoroutine(__rewardSpread(prevLeagueData.rewards));
    }

    IEnumerator __rewardSpread(List<AccountManager.Reward> rewards) {
        int count = 1;
        foreach (var reward in rewards) {
            GameObject slot = prevLeagueUISet
                .rewardLayoutGroup
                .GetChild(count)
                .gameObject;
            slot.SetActive(true);
            var image = slot.transform.Find("Image").GetComponent<Image>();
            image.enabled = true;
            image.sprite = AccountManager.Instance.GetComponent<ResourceManager>().rewardIcon[reward.reward.kind];
            slot.transform.Find("Amount").GetComponent<TextMeshProUGUI>().text = "x" + reward.reward.amount;
            count++;
            
            yield return new WaitForSeconds(0.2f);
        }
    }
    
    public class PrevLeagueDescUISet {
        public GameObject modal;
        public TextMeshProUGUI rewardDescription, leagueName;
        public Transform rewardLayoutGroup;
        public SkeletonGraphic spineEffect;
    }

    public class NewLeagueUISet {
        public GameObject modal;
        public TextMeshProUGUI leagueName, leaguePoint;
    }
}
