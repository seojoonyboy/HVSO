using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleReadyReward : MonoBehaviour
{
    Dictionary<string, Sprite> rewardIcons;
    [SerializeField] Transform rewardTransform;
    [SerializeField] TextMeshProUGUI mmrDown, mmrUp;
    [SerializeField] Slider prevSlider, currSlider;
    [SerializeField] Image nextMMR, rewardIcon;
    int rewardPos = 0;

    private void Awake() {
        rewardIcons = AccountManager.Instance.resource.rewardIcon;
    }

    private void OnEnable() {
        SetUpReward();
    }


    public void SetUpReward() {
        List<AccountManager.Reward> mmrRewards = AccountManager.Instance.scriptable_leagueData.leagueInfo.rewards;
        if (mmrRewards.Count < 1 || mmrRewards == null) mmrRewards = AccountManager.Instance.scriptable_leagueData.prevLeagueInfo.rewards;

        SetUpGauge(ref mmrRewards);
    }

    private void SetUpGauge(ref List<AccountManager.Reward> rewardList) {
        AccountManager.Reward frontReward;
        int topLeaguePoint = AccountManager.Instance.scriptable_leagueData.prevLeagueInfo.ratingPointTop ?? default(int);
        //frontReward = rewardList[rewardPos];
        // O(n)? 쩝...
        while (rewardList[rewardPos].canClaim == true && rewardList[rewardPos].claimed == false && rewardPos < rewardList.Count - 1)
            rewardPos++;

        frontReward = rewardList[rewardPos];
        if (frontReward == null) return;
        ShowGauge(frontReward, rewardPos + 1);
    }

    private void ShowGauge(AccountManager.Reward frontReward, int pos) {
        Button rewardButton = rewardTransform.gameObject.GetComponent<Button>();
        AccountManager.LeagueInfo currinfo = AccountManager.Instance.scriptable_leagueData.leagueInfo;
        AccountManager.LeagueInfo prevInfo = AccountManager.Instance.scriptable_leagueData.prevLeagueInfo;

        int pointOverThen = prevInfo.rankDetail.pointOverThen;
        int pointlessThen = currinfo.rankDetail.pointLessThen;
        int ratingPointTop = prevInfo.ratingPointTop ?? default(int);

        int leagueFarFrom = pointlessThen - currinfo.ratingPoint;
        int rewardFarFrom = frontReward.point - currinfo.ratingPoint;


        if (rewardFarFrom < leagueFarFrom && frontReward.claimed == false) {            
            rewardIcon.gameObject.SetActive(true);
            nextMMR.gameObject.SetActive(!rewardIcon.gameObject.activeSelf);

            mmrUp.text = frontReward.point.ToString();
            mmrDown.text = pointOverThen.ToString();

            prevSlider.minValue = pointOverThen;
            prevSlider.maxValue = frontReward.point;
            currSlider.minValue = pointOverThen;
            currSlider.maxValue = frontReward.point;

            prevSlider.value = ratingPointTop;
            currSlider.value = currinfo.ratingPoint;

            rewardIcon.gameObject.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = GetRewardIcon(frontReward.reward.kind);
        }
        else {
            nextMMR.gameObject.SetActive(true);
            rewardIcon.gameObject.SetActive(!nextMMR.gameObject.activeSelf);

            mmrUp.text = pointlessThen.ToString();
            mmrDown.text = pointOverThen.ToString();

            prevSlider.minValue = pointOverThen;
            prevSlider.maxValue = pointlessThen;
            currSlider.minValue = pointOverThen;
            currSlider.maxValue = pointlessThen;

            prevSlider.value = ratingPointTop;
            currSlider.value = currinfo.ratingPoint;

            AccountManager accountManager = AccountManager.Instance;
            AccountManager.RankTableRow item = AccountManager.Instance.rankTable.Find(x => x.minorRankName == prevInfo.rankDetail.minorRankName);
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
}
