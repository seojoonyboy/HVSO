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
    public static LeagueChangeModalHandler Instance { get; private set; }
    
    #region TestCode
    public void OpenWithDummyData() {
        var rewards = new List<AccountManager.ClaimRewardResFormatReward>();
        
        var reward1 = new AccountManager.ClaimRewardResFormatReward();
        reward1.kind = "extraLargeBox";
        reward1.amount = 1;
        rewards.Add(reward1);
        
        var reward2 = new AccountManager.ClaimRewardResFormatReward();
        reward2.kind = "goldFree";
        reward2.amount = 100;
        rewards.Add(reward2);
        
        AccountManager.LeagueInfo tmp_prevLeagueData = new AccountManager.LeagueInfo();
        tmp_prevLeagueData.rankDetail = new AccountManager.RankDetail();
        tmp_prevLeagueData.rankDetail.minorRankName = "tier_norank";
        tmp_prevLeagueData.rankDetail.id = 18;
        
        AccountManager.LeagueInfo tmp_newLeagueData = new AccountManager.LeagueInfo();
        tmp_newLeagueData.rankDetail = new AccountManager.RankDetail();
        tmp_newLeagueData.rankDetail.minorRankName = "tier_norank";
        tmp_newLeagueData.rankDetail.id = 18;
        
        AccountManager.ClaimRewardResFormat tmp_data = new AccountManager.ClaimRewardResFormat();
        tmp_data.leagueInfoBefore = tmp_prevLeagueData;
        tmp_data.leagueInfoCurrent = tmp_newLeagueData;
        tmp_data.rewards = rewards;
        
        OpenFirstWindow(tmp_data);
    }
    #endregion

    private Fbl_Translator _translator;
    private void Awake() {
        Instance = this;
        _translator = AccountManager.Instance.GetComponent<Fbl_Translator>();
    }

    private AccountManager.ClaimRewardResFormat _resFormat;
    public void OpenFirstWindow(AccountManager.ClaimRewardResFormat resFormat) {
        _resFormat = resFormat;
        if(_resFormat.rewards == null) _resFormat.rewards = new List<AccountManager.ClaimRewardResFormatReward>();
        
        if(_resFormat.rewards.Count == 0){
            newLeagueUISet.modal.gameObject.SetActive(true);
            SetNewRankUi(true);
        }
        else {
            prevLeagueUISet.modal.gameObject.SetActive(true);
            float time = 0.5f;
            var hash = iTween.Hash("scale", Vector3.one, "time", time);
            iTween.ScaleTo(prevLeagueUISet.modal, hash);

            StartCoroutine(__ScaleHeader(time));
            StartCoroutine(__OpenFirstWindow(time));
        }
    }

    private IEnumerator __ScaleHeader(float time) {
        yield return new WaitForSeconds(0.2f);
        var hash = iTween.Hash("scale", Vector3.one, "time", time);
        iTween.ScaleTo(prevLeagueUISet.header, hash);
    }

    private IEnumerator __OpenFirstWindow(float time) {
        yield return new WaitForSeconds(time);
        SetRewardsUi();
        SetPrevRankUi();
    }

    public void CloseFirstWindow() {
        prevLeagueUISet.modal.transform.localScale = Vector3.zero;
        prevLeagueUISet.header.transform.localScale = Vector3.zero;
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
        prevLeagueUISet.rankIcon.enabled = false;
        
        newLeagueUISet.leagueName.text = String.Empty;
    }

    public void SetPrevRankUi() {
        var icons = AccountManager.Instance.resource.rankIcons;
        var rankDetail = _resFormat.leagueInfoBefore.rankDetail;
        prevLeagueUISet.leagueName.text = rankDetail.minorRankName;
        prevLeagueUISet.rankIcon.enabled = true;
        if (icons.ContainsKey(rankDetail.id.ToString())) {
            prevLeagueUISet.rankIcon.sprite = icons[rankDetail.id.ToString()];
        }
    }

    public void SetNewRankUi(bool needScale = false) {
        if (needScale) {
            float time = 0.5f;
            var hash = iTween.Hash("scale", Vector3.one, "time", time);
            iTween.ScaleTo(newLeagueUISet.modal, hash);

            StartCoroutine(ScaleNewRankHeader(time));
        }
        
        var rankDetail = _resFormat.leagueInfoCurrent.rankDetail;
        newLeagueUISet.leagueName.text =  rankDetail.minorRankName;
        
        string animName = rankDetail.id.ToString();
        newLeagueUISet
            .rankIcon
            .Initialize(true);

        if (!string.IsNullOrEmpty(animName)) {
            newLeagueUISet.rankIcon.Skeleton.SetSkin(animName);
            newLeagueUISet.rankIcon.Skeleton.SetSlotsToSetupPose();
            
            newLeagueUISet
                .rankIcon
                .AnimationState
                .SetAnimation(0, "UP", false);
        }
        else {
            Logger.LogError("소프트 리셋 Animation을 찾을 수 없음 : " + rankDetail.id);
        }
        newLeagueUISet.leaguePoint.text = _resFormat.leagueInfoCurrent.ratingPoint.ToString();
    }

    private IEnumerator ScaleNewRankHeader(float time) {
        yield return new WaitForSeconds(0.2f);
        var hash = iTween.Hash("scale", Vector3.one, "time", time);
        iTween.ScaleTo(newLeagueUISet.header, hash);
    }
    
    private string GetTierAnimName(string keyword) {
        var rankIcons = AccountManager.Instance.resource.rankIcons;
        if (rankIcons.ContainsKey(keyword)) {
            return rankIcons[keyword].name;
        }
        return String.Empty;
    }
    
    private void SetRewardsUi() {
        prevLeagueUISet.rewardLayoutGroup.gameObject.SetActive(true);
        StartCoroutine(__rewardSpread());
    }

    IEnumerator __rewardSpread() {
        int count = 1;
        foreach (var reward in _resFormat.rewards) {
            GameObject slot = prevLeagueUISet
                .rewardLayoutGroup
                .GetChild(count)
                .gameObject;
            slot.SetActive(true);
            
            var skel = slot.transform.Find("Effect").GetComponent<SkeletonGraphic>();
            skel.Initialize(true);
            skel.AnimationState.SetAnimation(0, "animation", false);
            
            var image = slot.transform.Find("Image").GetComponent<Image>();
            image.enabled = true;
            image.sprite = AccountManager.Instance.GetComponent<ResourceManager>().rewardIcon[reward.kind];
            slot.transform.Find("Amount").GetComponent<TextMeshProUGUI>().text = "x" + reward.amount;

            Button btn = slot.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => RewardDescriptionHandler.instance.RequestDescriptionModal(reward.kind));
            
            count++;
            yield return new WaitForSeconds(0.2f);
        }
    }
    
    public class PrevLeagueDescUISet {
        public GameObject modal;
        public GameObject header;
        public TextMeshProUGUI rewardDescription, leagueName;
        public Transform rewardLayoutGroup;
        public Image rankIcon;
    }

    public class NewLeagueUISet {
        public GameObject modal;
        public GameObject header;
        public TextMeshProUGUI leagueName, leaguePoint;
        public SkeletonGraphic rankIcon;
    }
}
