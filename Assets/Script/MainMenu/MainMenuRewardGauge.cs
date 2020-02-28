using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuRewardGauge : BattleReadyReward {
    [SerializeField] Transform rankingBattleUI;
    [SerializeField] Image tierFlag;
    [SerializeField] TMPro.TextMeshProUGUI rewardGaugeText;
    // Start is called before the first frame update
    void Start() {
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_LEAGUE_INFO_UPDATED, OnLeagueInfoUpdated);
    }

    private void OnDestroy() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_LEAGUE_INFO_UPDATED, OnLeagueInfoUpdated);
        StopAllCoroutines();
    }

    private void OnDisable() {
        StopAllCoroutines();
    }

    private void OnEnable() {
        SetUpReward();
    }

    public IEnumerator Wait_Deploy_Data() {
        yield return null;
    }

    IEnumerator SetRankChangeChanceUI() {
        AccountManager.LeagueInfo currInfo = AccountManager.Instance.scriptable_leagueData.leagueInfo;

        AccountManager.RankUpCondition rankCondition;
        TMPro.TextMeshProUGUI description = rankingBattleUI.Find("Text").gameObject.GetComponent<TMPro.TextMeshProUGUI>();
        Transform rankTable = rankingBattleUI.Find("RankingTable");

        if (currInfo.rankingBattleState == "rank_up") {
            rankCondition = currInfo.rankDetail.rankUpBattleCount;
            description.text = "승급전!";
        }
        else if (currInfo.rankingBattleState == "rank_down") {
            rankCondition = currInfo.rankDetail.rankDownBattleCount;
            description.text = "강등전!";
        }
        else {
            rankingBattleUI.gameObject.SetActive(false);
            tierFlag.gameObject.SetActive(false);
            yield break;
        }
        rankingBattleUI.gameObject.SetActive(true);
        tierFlag.gameObject.SetActive(true);

        yield return null;
    }

    public void OnLeagueInfoUpdated(System.Enum Event_Type, Component Sender, object Param) {
        SetUpReward();
        StartCoroutine(SetRankChangeChanceUI());
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

        if (rewardFarFrom < leagueFarFrom) {
            rewardIcon.gameObject.SetActive(true);
            nextMMR.gameObject.SetActive(!rewardIcon.gameObject.activeSelf);

            StringBuilder sb = new StringBuilder();
            sb
                .Append(currinfo.ratingPoint)
                .Append("/")
                .Append(frontReward.point);

            rewardGaugeText.text = sb.ToString();

            currSlider.minValue = pointOverThen;
            currSlider.maxValue = frontReward.point;

            currSlider.value = currinfo.ratingPoint;

            rewardIcon.GetComponent<Image>().sprite = GetRewardIcon(frontReward.reward.kind);

            var boxValueObj = currSlider.transform.parent.Find("BoxValue");
            boxValueObj.gameObject.SetActive(true);
            boxValueObj.Find("BoxNum").GetComponent<TMPro.TextMeshProUGUI>().text = frontReward.reward.amount;
        }
        else {
            nextMMR.gameObject.SetActive(true);
            rewardIcon.gameObject.SetActive(!nextMMR.gameObject.activeSelf);

            StringBuilder sb = new StringBuilder();
            sb
                .Append(currinfo.ratingPoint)
                .Append("/")
                .Append(pointlessThen);

            if (currinfo.rankDetail.rankDownBattleCount != null && currinfo.rankDetail.rankDownBattleCount.battles > 0) {
                rewardGaugeText.text = (pointOverThen - 30).ToString();
            }
            else rewardGaugeText.text = pointOverThen.ToString();

            currSlider.minValue = (pointOverThen > 0) ? pointOverThen - 30 : 0;
            currSlider.maxValue = pointlessThen;

            currSlider.value = currinfo.ratingPoint;

            AccountManager accountManager = AccountManager.Instance;
            List<AccountManager.RankTableRow> table = AccountManager.Instance.rankTable;
            AccountManager.RankTableRow item = table.Find(x => x.id == prevInfo.rankDetail.id);

            int prevRank = -1;
            int nextRank = -1;
            if (item != null) {
                if (item.id == 18) {
                    prevRank = 18;
                    nextRank = item.id - 1;
                }
                else if (item.id <= 2) {
                    nextRank = 2;
                }
                else {
                    prevRank = item.id + 1;
                    nextRank = item.id - 1;
                }

                nextMMR.sprite = accountManager.resource.rankIcons[nextRank.ToString()];
            }

            var boxValueObj = currSlider.transform.parent.Find("BoxValue");
            boxValueObj.gameObject.SetActive(false);
        }
    }

    public override void SetUpRewardBubble(ref List<AccountManager.Reward> mmrRewards) {
        rewardTransform.gameObject.SetActive(false);
    }
}
