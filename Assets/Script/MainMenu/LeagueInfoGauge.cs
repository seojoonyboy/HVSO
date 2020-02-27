using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeagueInfoGauge : BattleReadyReward {
    private void Awake() {

    }

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    private void OnEnable() {
        return;
    }

    public void SetUpReward(ref List<AccountManager.Reward> mmrRewards) {
        SetUpGauge(ref mmrRewards);
    }

    protected override void SetUpGauge(ref List<AccountManager.Reward> rewardList) {
        AccountManager.Reward frontReward;
        int topLeaguePoint = AccountManager.Instance.scriptable_leagueData.prevLeagueInfo.ratingPointTop ?? default(int);
        //frontReward = rewardList[rewardPos];
        // O(n)? 쩝...
        while (topLeaguePoint >= rewardList[rewardPos].point && rewardPos < rewardList.Count - 1)
            rewardPos++;

        frontReward = rewardList[rewardPos];
        if (frontReward == null) return;
        //rewardTransform.gameObject.SetActive(true);
        ShowGauge(frontReward, rewardPos + 1);
    }



    protected override void ShowGauge(AccountManager.Reward frontReward, int pos) {
        AccountManager.LeagueInfo currinfo = AccountManager.Instance.scriptable_leagueData.leagueInfo;
        AccountManager.LeagueInfo prevInfo = AccountManager.Instance.scriptable_leagueData.prevLeagueInfo;

        string rankName = prevInfo.rankDetail.minorRankName;

        int pointOverThen = currinfo.rankDetail.pointOverThen;
        int pointlessThen = currinfo.rankDetail.pointLessThen;
        int ratingPointTop = prevInfo.ratingPointTop ?? default(int);

        int leagueFarFrom = pointlessThen - currinfo.ratingPoint;
        int rewardFarFrom = frontReward.point - currinfo.ratingPoint;

        if (rewardFarFrom < leagueFarFrom && frontReward.claimed == false) {
            rewardIcon.gameObject.SetActive(true);
            nextMMR.gameObject.SetActive(!rewardIcon.gameObject.activeSelf);

            mmrUp.text = frontReward.point.ToString();
            mmrDown.text = (pointOverThen > 0) ? (pointOverThen - 30).ToString() : 0.ToString();

            prevSlider.minValue = (pointOverThen > 0) ? pointOverThen - 30 : 0;
            prevSlider.maxValue = frontReward.point;
            currSlider.minValue = (pointOverThen > 0) ? pointOverThen - 30 : 0;
            currSlider.maxValue = frontReward.point;

            prevSlider.value = prevInfo.ratingPoint;
            currSlider.value = currinfo.ratingPoint;

            rewardIcon.gameObject.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = GetRewardIcon(frontReward.reward.kind);
        }
        else {
            nextMMR.gameObject.SetActive(true);
            rewardIcon.gameObject.SetActive(!nextMMR.gameObject.activeSelf);

            mmrUp.text = pointlessThen.ToString();

            mmrDown.text = (currinfo.rankDetail.rankDownBattleCount != null && currinfo.rankDetail.rankDownBattleCount.needTo > 0) ? (pointOverThen - 30).ToString() : pointOverThen.ToString();

            prevSlider.minValue = (pointOverThen > 0) ? pointOverThen - 30 : 0;
            prevSlider.maxValue = pointlessThen;
            currSlider.minValue = (pointOverThen > 0) ? pointOverThen - 30 : 0;
            currSlider.maxValue = pointlessThen;

            prevSlider.value = prevInfo.ratingPoint;
            currSlider.value = currinfo.ratingPoint;
        }
    }




}
