using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using dataModules;
using System.Linq;

public class RewardProgressController : MonoBehaviour {
    [SerializeField] GameObject rankObj;
    [Space(10)] [SerializeField] GameObject prevSlider, CurrentSlider, content, slidersParent;
    [SerializeField] GameObject indicator;
    public AccountManager.LeagueInfo prevLeagueInfo, currentLeagueInfo;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] GameObject hideModal;

    List<AccountManager.RankTableRow> rankTable;
    List<PointAreaInfo> referenceToRankObj;

    public bool isProgressMoving = false;

    private float heightPerRankObj;
    private GameObject __myRankObj;

    void Start() {
        heightPerRankObj = rankObj.GetComponent<RectTransform>().rect.height;
    }

    void OnEnable() {
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_LEAGUE_INFO_UPDATED, OnLeagueInfoUpdated);
        AccountManager.Instance.RequestLeagueInfo();
        hideModal.SetActive(true);
    }

    /// <summary>
    /// 초기화 Entry Point
    /// </summary>
    /// <returns></returns>
    public IEnumerator StartSetting(bool needForceScrollRect = true) {
        yield return RankSetting();

        indicator
            .transform
            .Find("Value")
            .GetComponent<Text>().text = currentLeagueInfo.ratingPoint.ToString();

        yield return PrevSliderSetting();
        yield return CurrentSliderSetting();

        if(needForceScrollRect) yield return CenterToMyRatingPoint();

        hideModal.SetActive(false);
    }

    void OnDisable() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_LEAGUE_INFO_UPDATED, OnLeagueInfoUpdated);
        StopAllCoroutines();
    }

    void OnDestroy() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_LEAGUE_INFO_UPDATED, OnLeagueInfoUpdated);
        StopAllCoroutines();
    }

    private void OnLeagueInfoUpdated(Enum Event_Type, Component Sender, object Param) {
        AccountManager accountManager = AccountManager.Instance;
        prevLeagueInfo = accountManager.scriptable_leagueData.prevLeagueInfo;
        currentLeagueInfo = accountManager.scriptable_leagueData.leagueInfo;

        accountManager.RequestRankTable((req, res) => {
            if(res.StatusCode == 200 || res.StatusCode == 304) {
                rankTable = JsonReader.Read<List<AccountManager.RankTableRow>>(res.DataAsText);
                var sortDescendingQuery = from row in rankTable
                            orderby row.id ascending
                            select row;
                rankTable = sortDescendingQuery.ToList();

                StartCoroutine(StartSetting());
            }
        });
        
    }

    /// <summary>
    /// Rank 세팅
    /// </summary>
    /// <returns></returns>
    IEnumerator RankSetting() {
        referenceToRankObj = new List<PointAreaInfo>();
        foreach (Transform prevObj in content.transform) {
            if (prevObj.name != "RewardSlider") Destroy(prevObj.gameObject);
        }

        rankTable.Reverse();
        foreach (AccountManager.RankTableRow row in rankTable) {
            GameObject newRankObj = Instantiate(rankObj);
            newRankObj.SetActive(true);
            newRankObj.transform.SetParent(content.transform);
            
            newRankObj.transform.localScale = Vector3.one;
            newRankObj.name = "Rank_" + row.id;

            newRankObj
                .transform
                .Find("RankIcon")
                .GetComponent<Image>().sprite = AccountManager
                    .Instance
                    .resource
                    .rankIcons[row.id.ToString()];

            newRankObj
                .transform
                .Find("RankIcon/Header")
                .GetChild(0)
                .GetComponent<TextMeshProUGUI>().text = AccountManager
                    .Instance
                    .GetComponent<Fbl_Translator>()
                    .GetLocalizedText("Tier", row.minorRankName);

            //RectTransform overthenRect = newRankObj
            //    .transform
            //    .Find("OverThen")
            //    .GetComponent<RectTransform>();

            //overthenRect.anchorMin = new Vector2(0, 0);
            //overthenRect.anchorMax = new Vector2(0, 0);
            //overthenRect.anchoredPosition = new Vector3(52.0f, 0.0f);

            RectTransform lessthenRect = newRankObj
                .transform
                .Find("LessThen")
                .GetComponent<RectTransform>();

            lessthenRect.anchorMin = new Vector2(0, 1);
            lessthenRect.anchorMax = new Vector2(0, 1);
            lessthenRect.anchoredPosition = new Vector3(52.0f, 0.0f);

            newRankObj
                .transform
                .Find("LessThen")
                .GetComponent<Image>().SetNativeSize();

            newRankObj
                .transform
                .Find("LessThen")
                .GetChild(0)
                .GetComponent<TextMeshProUGUI>().text = row.pointLessThen.ToString();

            int pointOverThen = 0;
            int pointLessThen = 0;
            if (row.pointOverThen.HasValue) {
                pointOverThen = row.pointOverThen.Value;
            }
            if (row.pointLessThen.HasValue) {
                pointLessThen = row.pointLessThen.Value;
            }

            referenceToRankObj.Add(new PointAreaInfo(pointOverThen, pointLessThen, newRankObj));
            newRankObj.transform.SetAsFirstSibling();
            yield return RewardSetting(row, newRankObj);

            if(currentLeagueInfo.ratingPoint >= row.pointOverThen && currentLeagueInfo.ratingPoint <= row.pointLessThen) {
                __myRankObj = newRankObj;
            }
        }
        slidersParent.transform.SetAsLastSibling();
        scrollRect.verticalNormalizedPosition = 0;
    }

    private const float slotOffsetY = -65.0f;

    /// <summary>
    /// 보상 세팅
    /// </summary>
    /// <returns></returns>
    IEnumerator RewardSetting(AccountManager.RankTableRow rowData, GameObject obj) {
        var rewardIcons = AccountManager.Instance.resource.rewardIcon;
        var selectedRewards = currentLeagueInfo.rewards.FindAll(x => rowData.pointOverThen <= x.point && rowData.pointLessThen > x.point);
        if (selectedRewards == null || selectedRewards.Count == 0) yield return 0;
        var slots = obj.transform.Find("RewardSlots");

        float ratingPoint = AccountManager.Instance.scriptable_leagueData.leagueInfo.ratingPoint;

        float pointOverThen = 0;
        float pointLessThen = 0;
        if (rowData.pointOverThen.HasValue) {
            pointOverThen = rowData.pointOverThen.Value;
        }
        if (rowData.pointLessThen.HasValue) {
            pointLessThen = rowData.pointLessThen.Value;
        }
        else pointLessThen = pointOverThen + 800;

        float pointWidth = pointLessThen - pointOverThen;

        for (int i=0; i<selectedRewards.Count; i++) {
            var slot = slots.GetChild(i);
            slot.gameObject.SetActive(true);

            var rewardType = selectedRewards[i].reward.kind;

            float pointOverThenToReward = selectedRewards[i].point - pointOverThen;
            float result = pointOverThenToReward * heightPerRankObj / pointWidth;
            RectTransform slotRect = slot.GetComponent<RectTransform>();

            slotRect.anchoredPosition = new Vector2(120, slotOffsetY + result);
            slot.Find("Indicator/Value").GetComponent<Text>().text = selectedRewards[i].point.ToString();
            slot.Find("Indicator2/Value").GetComponent<Text>().text = selectedRewards[i].point.ToString();

            if (rewardIcons.ContainsKey(rewardType)) {
                slot.Find("Image").GetComponent<Image>().sprite = rewardIcons[rewardType];
            }
            slot.Find("Amount").GetComponent<TextMeshProUGUI>().text = "x" + selectedRewards[i].reward.amount;

            slot.GetComponent<RewardButtonHandler>().Init(selectedRewards[i]);
        }
        yield return 0;
    }

    /// <summary>
    /// 이전 보상 진척도 게이지 세팅
    /// </summary>
    /// <returns></returns>
    IEnumerator PrevSliderSetting() {
        yield return new WaitForEndOfFrame();
        var myRankObj = referenceToRankObj.Find(x => x.rankObj == __myRankObj);
        if (myRankObj == null) yield return 0;

        int index = referenceToRankObj.IndexOf(myRankObj);

        Vector2 size = prevSlider.GetComponent<RectTransform>().sizeDelta;
        prevSlider.GetComponent<RectTransform>().sizeDelta = new Vector2(size.x, index * heightPerRankObj);

        float pointToTop = 0;
        if (currentLeagueInfo.ratingPointTop.HasValue) {
            pointToTop = currentLeagueInfo.ratingPointTop.Value;
        }

        float pointWidth = myRankObj.pointLessThen - myRankObj.pointOverThen;
        float pointOverToTargetWidth = pointToTop - myRankObj.pointOverThen;
        float result = pointOverToTargetWidth * heightPerRankObj / pointWidth;

        size = prevSlider.GetComponent<RectTransform>().sizeDelta;
        prevSlider.GetComponent<RectTransform>().sizeDelta = new Vector2(size.x, size.y + result);
    }

    /// <summary>
    /// 현재 보상 진척도 게이지 세팅
    /// </summary>
    /// <returns></returns>
    IEnumerator CurrentSliderSetting() {
        yield return new WaitForEndOfFrame();
        var myRankObj = referenceToRankObj.Find(x => x.rankObj == __myRankObj);
        if (myRankObj == null) yield return 0;

        int index = referenceToRankObj.IndexOf(myRankObj);
        
        Vector2 size = CurrentSlider.GetComponent<RectTransform>().sizeDelta;
        CurrentSlider.GetComponent<RectTransform>().sizeDelta = new Vector2(size.x, index * heightPerRankObj);

        float pointWidth = myRankObj.pointLessThen - myRankObj.pointOverThen;
        float pointOverToTargetWidth = currentLeagueInfo.ratingPoint - myRankObj.pointOverThen;
        float result = pointOverToTargetWidth * heightPerRankObj / pointWidth;

        size = CurrentSlider.GetComponent<RectTransform>().sizeDelta;
        CurrentSlider.GetComponent<RectTransform>().sizeDelta = new Vector2(size.x, size.y + result);
    }

    IEnumerator CenterToMyRatingPoint() {
        yield return new WaitForEndOfFrame();
        int childCount = referenceToRankObj.Count;
        float normalizePosition = 1 - __myRankObj.transform.GetSiblingIndex() / (float)(childCount - 1);
        
        // Logger.Log("GetSiblingIndex : " + __myRankObj.transform.GetSiblingIndex());
        // Logger.Log("childCount : " + childCount);
        // Logger.Log("normalizePosition : " + normalizePosition);
        scrollRect.verticalNormalizedPosition = normalizePosition;
    }

    public class PointAreaInfo {
        public int pointOverThen;
        public int pointLessThen;
        public GameObject rankObj;

        public PointAreaInfo(int pointOverThen, int pointLessThen, GameObject rankObj) {
            this.pointLessThen = pointLessThen;
            this.pointOverThen = pointOverThen;
            this.rankObj = rankObj;
        }
    }
}
