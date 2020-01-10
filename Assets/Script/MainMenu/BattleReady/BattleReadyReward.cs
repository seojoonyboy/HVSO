using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using TMPro;

public class BattleReadyReward : MonoBehaviour
{
    Dictionary<string, Sprite> rewardIcons;
    [SerializeField] Transform rewardTransform;
    [SerializeField] TextMeshProUGUI mmrDown, mmrUp;
    [SerializeField] Slider prevSlider, currSlider;
    [SerializeField] Image nextMMR, rewardIcon;
    int rewardPos = 0;
    protected IDisposable bubbleAnimation;
    List<AccountManager.Reward> unClaimedRewards;

    private void Awake() {
        rewardIcons = AccountManager.Instance.resource.rewardIcon;
    }

    private void OnEnable() {
        SetUpReward();
    }

    private void OnDisable() {
        if (bubbleAnimation != null) bubbleAnimation.Dispose();
    }


    private void OnDestroy() {
        StopAllCoroutines();
    }



    public void SetUpReward() {
        List<AccountManager.Reward> mmrRewards = AccountManager.Instance.scriptable_leagueData.leagueInfo.rewards;
        if (mmrRewards.Count < 1 || mmrRewards == null) mmrRewards = AccountManager.Instance.scriptable_leagueData.prevLeagueInfo.rewards;
        

        SetUpGauge(ref mmrRewards);
        SetUpRewardBubble(ref mmrRewards);
    }

    private void SetUpGauge(ref List<AccountManager.Reward> rewardList) {
        AccountManager.Reward frontReward;
        int topLeaguePoint = AccountManager.Instance.scriptable_leagueData.prevLeagueInfo.ratingPointTop ?? default(int);
        //frontReward = rewardList[rewardPos];
        // O(n)? 쩝...
        while (topLeaguePoint > rewardList[rewardPos].point && rewardPos < rewardList.Count - 1)
            rewardPos++;

        frontReward = rewardList[rewardPos];
        if (frontReward == null) return;
        rewardTransform.gameObject.SetActive(true);
        ShowGauge(frontReward, rewardPos + 1);
    }

    private void ShowGauge(AccountManager.Reward frontReward, int pos) {
        //Button rewardButton = rewardTransform.gameObject.GetComponent<Button>();
        AccountManager.LeagueInfo currinfo = AccountManager.Instance.scriptable_leagueData.leagueInfo;
        AccountManager.LeagueInfo prevInfo = AccountManager.Instance.scriptable_leagueData.prevLeagueInfo;

        string rankName = prevInfo.rankDetail.minorRankName;

        int pointOverThen = prevInfo.rankDetail.pointOverThen;
        int pointlessThen = currinfo.rankDetail.pointLessThen;
        int ratingPointTop = prevInfo.ratingPointTop ?? default(int);

        int leagueFarFrom = pointlessThen - currinfo.ratingPoint;
        int rewardFarFrom = frontReward.point - currinfo.ratingPoint;


        if (rewardFarFrom < leagueFarFrom && frontReward.claimed == false) {            
            rewardIcon.gameObject.SetActive(true);
            nextMMR.gameObject.SetActive(!rewardIcon.gameObject.activeSelf);

            mmrUp.text = frontReward.point.ToString();
            mmrDown.text = (pointOverThen - 30).ToString();

            prevSlider.minValue = pointOverThen - 30;
            prevSlider.maxValue = frontReward.point;
            currSlider.minValue = pointOverThen - 30;
            currSlider.maxValue = frontReward.point;

            prevSlider.value = prevInfo.ratingPoint;
            currSlider.value = currinfo.ratingPoint;

            rewardIcon.gameObject.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = GetRewardIcon(frontReward.reward.kind);
        }
        else {
            nextMMR.gameObject.SetActive(true);
            rewardIcon.gameObject.SetActive(!nextMMR.gameObject.activeSelf);

            mmrUp.text = pointlessThen.ToString();
            mmrDown.text = (pointOverThen - 30).ToString();

            prevSlider.minValue = pointOverThen - 30;
            prevSlider.maxValue = pointlessThen;
            currSlider.minValue = pointOverThen - 30;
            currSlider.maxValue = pointlessThen;

            prevSlider.value = prevInfo.ratingPoint;
            currSlider.value = currinfo.ratingPoint;

            AccountManager accountManager = AccountManager.Instance;
            List<AccountManager.RankTableRow> table = AccountManager.Instance.rankTable;
            AccountManager.RankTableRow item = table.Find(x => x.minorRankName == rankName);
            int prevRankIndex = -1;

            if (item != null) {
                if (item.minorRankName == "무명 병사")
                    prevRankIndex = 1;
                else if (item.minorRankName == "전략의 제왕")
                    prevRankIndex = accountManager.rankTable.Count - 1;
                else
                    prevRankIndex = accountManager.rankTable.IndexOf(item);

                nextMMR.sprite = accountManager.resource.rankIcons[accountManager.rankTable[prevRankIndex + 1].minorRankName];
            }

        }
        //if (frontReward.canClaim == true)
        //    rewardButton.onClick.AddListener(() => RequestReward(frontReward, pos));
    }

    public Sprite GetRankImage(string keyword) {
        Dictionary<string, Sprite> rankIcons = AccountManager.Instance.resource.rankIcons;
        Sprite sprite = rankIcons["default"];
        if (!string.IsNullOrEmpty(keyword) && rankIcons.ContainsKey(keyword)) {
            sprite = rankIcons[keyword];
        }
        return sprite;
    }

    Sprite GetRewardIcon(string keyword) {
        if (rewardIcons.ContainsKey(keyword)) {
            return rewardIcons[keyword];
        }
        else {
            return null;
        }
    }


    private void RequestReward(AccountManager.Reward reward, int id) {
        if (reward.canClaim == false) return;
        AccountManager.Instance.RequestLeagueReward(OnRewardCallBack, id);
    }

    private void OnRewardCallBack(BestHTTP.HTTPRequest originalRequest, BestHTTP.HTTPResponse response) {
        if (response.DataAsText.Contains("not allowed")) {
            Modal.instantiate("요청 불가", Modal.Type.CHECK);
        }
        else {
            Modal.instantiate("보상을 우편으로 발송하였습니다.", Modal.Type.CHECK, () => { });
        }
    }


    private void ShowUnreceivedReward(List<AccountManager.Reward> mmrRewards) {
        List<AccountManager.Reward> unreceivedList = new List<AccountManager.Reward>();
        int topLeaguePoint = AccountManager.Instance.scriptable_leagueData.prevLeagueInfo.ratingPointTop ?? default;
        int pos = -1;

        while (pos < mmrRewards.Count - 1) {
            pos++;

            if (mmrRewards[pos].point > topLeaguePoint)
                break;

            if(mmrRewards[pos].canClaim == true && mmrRewards[pos].claimed == false) {
                unreceivedList.Add(mmrRewards[pos]);
            }
        }

    }

    public void SetUpRewardBubble(ref List<AccountManager.Reward> mmrRewards) {
        int topLeaguePoint = AccountManager.Instance.scriptable_leagueData.prevLeagueInfo.ratingPointTop ?? default(int);
        int listPos = -1;

        if(unClaimedRewards == null) unClaimedRewards = new List<AccountManager.Reward>();
        unClaimedRewards.Clear();

        while (++listPos < mmrRewards.Count - 1) {
            if (mmrRewards[listPos].point > topLeaguePoint) break;
            if (mmrRewards[listPos].claimed == false && mmrRewards[listPos].canClaim == true) unClaimedRewards.Add(mmrRewards[listPos]);
        }

        //if (unClaimedRewards.Count > 0) StartCoroutine(TraverseReward(unClaimedRewards));
        if (unClaimedRewards.Count > 0) {
            SetUpAnimation();
        }
        else
            rewardTransform.gameObject.SetActive(false);
    }

    public void RefreshRewardBubble() {
        SetUpReward();
    }


    public void OverWriteRewardBubble(int rewardId) {
        if (unClaimedRewards == null) return;       
        unClaimedRewards.Remove(unClaimedRewards.Find(x => x.id == rewardId));
        if (unClaimedRewards.Count > 0) {
            SetUpAnimation();
        }
        else
            rewardTransform.gameObject.SetActive(false);
    }


    private void SetUpAnimation() {
        Animation rewardIcon = rewardTransform.gameObject.GetComponent<Animation>();
        Image icon = rewardTransform.GetChild(0).gameObject.GetComponent<Image>();
        int pos = 0;

        if (bubbleAnimation != null) bubbleAnimation.Dispose();
        bubbleAnimation = Observable.Interval(TimeSpan.FromSeconds(2))
                                    .TakeWhile(_ => rewardTransform.gameObject.activeSelf == true)                                    
                                    .Select(_ => pos = pos % unClaimedRewards.Count)
                                    .Select(x => icon.sprite = GetRewardIcon(unClaimedRewards[x].reward.kind))
                                    .Select(_ => pos++)
                                    .Subscribe(_ => { rewardIcon.Play();})
                                    .AddTo(rewardTransform.gameObject);
    }


    





    private IEnumerator TraverseReward(List<AccountManager.Reward> unclamimed) {        
        GameObject rewardBubble = rewardTransform.gameObject;
        rewardBubble.SetActive(true);

        while (rewardBubble.activeSelf == true) {
            if (rewardBubble.activeSelf == false) yield break;




        }
    }


}
