using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class LeagueInfoGauge : BattleReadyReward {
    [SerializeField] TMPro.TextMeshProUGUI rewardGaugeText;

    protected override void SetUpGauge(ref List<AccountManager.Reward> rewardList) {
        AccountManager.Reward frontReward;
        int ratingPoint = AccountManager.Instance.scriptable_leagueData.prevLeagueInfo.ratingPoint;

        var query1 = rewardList.FindAll(x =>
            ratingPoint < x.point
        );

        query1 = query1.OrderBy(x => x.point).ToList();
        if (query1.Count == 0) frontReward = rewardList[0];
        else frontReward = query1.First();

        if (frontReward == null) return;
        ShowGauge(frontReward, rewardPos + 1);
    }

    protected override void ShowGauge(AccountManager.Reward frontReward, int pos) {
        AccountManager.LeagueInfo currinfo = AccountManager.Instance.scriptable_leagueData.leagueInfo;
        AccountManager.LeagueInfo prevInfo = AccountManager.Instance.scriptable_leagueData.prevLeagueInfo;

        int pointOverThen = currinfo.rankDetail.pointOverThen;
        int pointlessThen = currinfo.rankDetail.pointLessThen;

        nextMMR.gameObject.SetActive(true);
        rewardIcon.gameObject.SetActive(!nextMMR.gameObject.activeSelf);

        mmrUp.text = pointlessThen.ToString();

        mmrDown.text = (currinfo.rankDetail.rankDownBattleCount != null && currinfo.rankDetail.rankDownBattleCount.needTo > 0) ? (pointOverThen - 30).ToString() : pointOverThen.ToString();
        prevSlider.value = 0;

        if (currinfo.rankDetail.rankDownBattleCount != null && currinfo.rankDetail.rankDownBattleCount.battles > 0) {
            currSlider.minValue = (pointOverThen > 0) ? pointOverThen - 30 : 0;
            currSlider.maxValue = pointlessThen;

            currSlider.value = currinfo.ratingPoint;
        }
        else {
            currSlider.minValue = pointOverThen;
            currSlider.maxValue = pointlessThen;

            currSlider.value = currinfo.ratingPoint;
        }
        
    }
}
