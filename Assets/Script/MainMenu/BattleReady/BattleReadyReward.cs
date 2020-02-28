using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using TMPro;
using System.Linq;

public class BattleReadyReward : MonoBehaviour
{
    Dictionary<string, Sprite> rewardIcons;
    [SerializeField]protected Transform rewardTransform;
    [SerializeField]protected TextMeshProUGUI mmrDown, mmrUp;
    [SerializeField]protected Slider prevSlider, currSlider;
    [SerializeField]protected Image nextMMR, rewardIcon;
    protected int rewardPos = 0;
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

    public virtual void SetUpReward() {
        if (AccountManager.Instance.rankTable == null || AccountManager.Instance.rankTable.Count < 1) AccountManager.Instance.RequestRankTable();
        if (AccountManager.Instance.scriptable_leagueData == null) AccountManager.Instance.RequestLeagueInfo();
        StartCoroutine(Wait_Table());
    }

    public IEnumerator Wait_Table() {
        yield return new WaitUntil(() => AccountManager.Instance.scriptable_leagueData.leagueInfo != null);
        yield return new WaitUntil(() => AccountManager.Instance.scriptable_leagueData.leagueInfo.rewards != null);
        List<AccountManager.Reward> mmrRewards = AccountManager.Instance.scriptable_leagueData.leagueInfo.rewards;
        SetUpGauge(ref mmrRewards);
        SetUpRewardBubble(ref mmrRewards);
    }

    protected virtual void SetUpGauge(ref List<AccountManager.Reward> rewardList) {
        AccountManager.Reward frontReward;
        int ratingPoint = AccountManager.Instance.scriptable_leagueData.prevLeagueInfo.ratingPoint;

        var query1 = rewardList.FindAll(x => 
            ratingPoint < x.point
        );

        query1 = query1.OrderBy(x => x.point).ToList();
        if (query1.Count == 0) frontReward = rewardList[0];
        else frontReward = query1.First();

        if (frontReward == null) return;
        rewardTransform.gameObject.SetActive(true);
        ShowGauge(frontReward, rewardPos + 1);
    }

    protected virtual void ShowGauge(AccountManager.Reward frontReward, int pos) {
        //Button rewardButton = rewardTransform.gameObject.GetComponent<Button>();
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

            mmrUp.text = frontReward.point.ToString();
            mmrDown.text = currinfo.ratingPoint.ToString();

            prevSlider.minValue = pointOverThen;
            prevSlider.maxValue = frontReward.point;
            currSlider.minValue = pointOverThen;
            currSlider.maxValue = frontReward.point;

            prevSlider.value = prevInfo.ratingPoint;
            currSlider.value = currinfo.ratingPoint;

            rewardIcon.gameObject.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = GetRewardIcon(frontReward.reward.kind);
        }
        else {
            nextMMR.gameObject.SetActive(true);
            rewardIcon.gameObject.SetActive(!nextMMR.gameObject.activeSelf);

            mmrUp.text = pointlessThen.ToString();

            if(currinfo.rankDetail.rankDownBattleCount != null && currinfo.rankDetail.rankDownBattleCount.battles > 0) {
                mmrDown.text = (pointOverThen - 30).ToString();
            }
            else mmrDown.text = pointOverThen.ToString();

            prevSlider.minValue = (pointOverThen > 0) ? pointOverThen - 30 : 0;
            prevSlider.maxValue = pointlessThen;
            currSlider.minValue = (pointOverThen > 0) ? pointOverThen - 30 : 0;
            currSlider.maxValue = pointlessThen;

            prevSlider.value = prevInfo.ratingPoint;
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

        }
        //if (frontReward.canClaim == true)
        //    rewardButton.onClick.AddListener(() => RequestReward(frontReward, pos));
    }

    protected Sprite GetRewardIcon(string keyword) {
        if (rewardIcons != null) {
            if (rewardIcons.ContainsKey(keyword)) {
                return rewardIcons[keyword];
            }
            else {
                return null;
            }
        }
        else {
            if (AccountManager.Instance.resource.rewardIcon.ContainsKey(keyword)){
                return AccountManager.Instance.resource.rewardIcon[keyword];
            }
            else
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
            var fbl_translator = AccountManager.Instance.GetComponent<Fbl_Translator>();
            string message = fbl_translator.GetLocalizedText("UI", "Mmenu_mailsent");
            string headerText = fbl_translator.GetLocalizedText("UI", "Mmenu_check");
            string okBtnText = fbl_translator.GetLocalizedText("UI", "Mmenu_yes");

            Modal.instantiate(message, Modal.Type.CHECK, () => { }, headerText: headerText, btnTexts: new string[] { okBtnText });
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

    public virtual void SetUpRewardBubble(ref List<AccountManager.Reward> mmrRewards) {
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
        else {
            if (rewardTransform != null)
                rewardTransform.gameObject.SetActive(false);
        }
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
        if (rewardTransform == null) return;
        Animation rewardIcon = rewardTransform.gameObject.GetComponent<Animation>();
        Image icon = rewardTransform.GetChild(0).gameObject.GetComponent<Image>();
        int pos = 0;
        int rewardCount = unClaimedRewards.Count;

        if (bubbleAnimation != null) bubbleAnimation.Dispose();
        if (rewardCount > 1) {
            bubbleAnimation = Observable.Interval(TimeSpan.FromSeconds(2))
                                        .TakeWhile(_ => rewardTransform.gameObject.activeSelf == true)
                                        .Select(_ => pos = pos % unClaimedRewards.Count)
                                        .Select(x => icon.sprite = GetRewardIcon(unClaimedRewards[x].reward.kind))
                                        .Select(_ => pos++)
                                        .Subscribe(_ => { rewardIcon.Play(); })
                                        .AddTo(rewardTransform.gameObject);
        }
        else if (rewardCount == 1) {
            rewardIcon.Stop();
            icon.sprite = GetRewardIcon(unClaimedRewards[0].reward.kind);
            icon.color = new Color(1f, 1f, 1f, 1f);
        }
        else 
            rewardIcon.Stop();
    }
}
