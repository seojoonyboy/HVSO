using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuRewardGauge : BattleReadyReward {
    [SerializeField] Transform rankingBattleUI;
    [SerializeField] Image tierFlag;
    [SerializeField] TMPro.TextMeshProUGUI rewardGaugeText;
    
    public bool isLeagueInfoUIUpdated = false;
    // Start is called before the first frame update
    void Start() {
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_LEAGUE_INFO_UPDATED, OnLeagueInfoUpdated);
        isLeagueInfoUIUpdated = false;
    }

    private void OnDestroy() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_LEAGUE_INFO_UPDATED, OnLeagueInfoUpdated);
        StopAllCoroutines();
    }

    private void OnDisable() {
        StopAllCoroutines();
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
        List<AccountManager.Reward> mmrRewards = AccountManager.Instance.scriptable_leagueData.leagueInfo.rewards;
        SetUpGauge(ref mmrRewards);
        
        StartCoroutine(SetRankChangeChanceUI());
        isLeagueInfoUIUpdated = true;
        SetUpRewardBubble(ref mmrRewards);
    }
    
    protected override void ShowGauge(AccountManager.Reward frontReward, int pos) {
        AccountManager.LeagueInfo currinfo = AccountManager.Instance.scriptable_leagueData.leagueInfo;

        int pointOverThen = currinfo.rankDetail.pointOverThen;
        int pointlessThen = currinfo.rankDetail.pointLessThen;

        int leagueFarFrom = pointlessThen - currinfo.ratingPoint;
        int rewardFarFrom = frontReward.point - currinfo.ratingPoint;

        var boxValueObj = currSlider.transform.parent.Find("BoxValue");

        int canRewardNum = (currinfo.rewards.FindAll(x => x.canClaim == true && x.claimed == false)).Count;
        if (canRewardNum > 0) {
            boxValueObj.gameObject.SetActive(true);
            boxValueObj.Find("BoxNum").GetComponent<TMPro.TextMeshProUGUI>().text = canRewardNum.ToString();
        }
        else boxValueObj.gameObject.SetActive(false);
        
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
        }
        else {
            nextMMR.gameObject.SetActive(true);
            rewardIcon.gameObject.SetActive(!nextMMR.gameObject.activeSelf);

            string prevText = string.Empty;
            if (currinfo.rankDetail.rankDownBattleCount != null && currinfo.rankDetail.rankDownBattleCount.battles > 0) {
                prevText = (pointOverThen - 30).ToString();
            }
            else prevText = pointOverThen.ToString();

            StringBuilder sb = new StringBuilder();
            sb
                .Append(prevText)
                .Append("/")
                .Append(pointlessThen);

            currSlider.minValue = (pointOverThen > 0) ? pointOverThen - 30 : 0;
            currSlider.maxValue = pointlessThen;

            rewardGaugeText.text = sb.ToString();
            currSlider.value = currinfo.ratingPoint;

            AccountManager accountManager = AccountManager.Instance;
            List<AccountManager.RankTableRow> table = AccountManager.Instance.rankTable;
            AccountManager.RankTableRow item = table.Find(x => x.id == currinfo.rankDetail.id);

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
        }
    }

    public override void SetUpRewardBubble(ref List<AccountManager.Reward> mmrRewards) {
        rewardTransform.gameObject.SetActive(false);
    }
}
