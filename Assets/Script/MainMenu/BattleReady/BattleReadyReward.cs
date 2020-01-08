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
    int rewardPos = -1;

    private void Awake() {
        rewardIcons = AccountManager.Instance.resource.rewardIcon;
    }

    public void SetUpReward() {
        List<AccountManager.Reward> mmrRewards = AccountManager.Instance.scriptable_leagueData.leagueInfo.rewards;
        if (mmrRewards.Count < 1 || mmrRewards == null) mmrRewards = AccountManager.Instance.scriptable_leagueData.prevLeagueInfo.rewards;

        SetUpGauge(ref mmrRewards);
    }

    private void SetUpGauge(ref List<AccountManager.Reward> rewardList) {
        AccountManager.Reward frontReward;
        frontReward = rewardList[++rewardPos];
        // O(n)? 쩝...
        while (rewardList[rewardPos].claimed == false && rewardPos < rewardList.Count - 1)
            frontReward = rewardList[++rewardPos];

        if (frontReward == null) return;
        ShowGauge(frontReward, rewardPos + 1);
    }

    private void ShowGauge(AccountManager.Reward frontReward, int pos) {
        Button rewardButton = rewardTransform.gameObject.GetComponent<Button>();
        AccountManager.LeagueInfo currinfo = AccountManager.Instance.scriptable_leagueData.leagueInfo;
        AccountManager.LeagueInfo prevInfo = AccountManager.Instance.scriptable_leagueData.prevLeagueInfo;

        int pointOverThen = prevInfo.rankDetail.pointOverThen;

        mmrUp.text = frontReward.point.ToString();
        mmrDown.text = pointOverThen.ToString();

        prevSlider.minValue = pointOverThen;
        prevSlider.maxValue = frontReward.point;
        currSlider.minValue = pointOverThen;
        currSlider.maxValue = frontReward.point;

        prevSlider.value = prevInfo.ratingPoint;
        currSlider.value = currinfo.ratingPoint;

        if (frontReward.canClaim == true)
            rewardButton.onClick.AddListener(() => RequestReward(frontReward, pos));
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
            SetUpReward();
        }
    }




    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
