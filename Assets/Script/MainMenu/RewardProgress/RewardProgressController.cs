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

    List<AccountManager.RankTableRow> rankTable;
    List<PointAreaInfo> referenceToRankObj;

    public bool isProgressMoving = false;

    private float heightPerRankObj;
    void Start() {
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_LEAGUE_INFO_UPDATED, OnLeagueInfoUpdated);
        AccountManager.Instance.RequestLeagueInfo();

        heightPerRankObj = rankObj.GetComponent<RectTransform>().rect.height;
    }

    /// <summary>
    /// 초기화 Entry Point
    /// </summary>
    /// <returns></returns>
    IEnumerator StartSetting() {
        yield return RankSetting();

        yield return PrevSliderSetting();
        yield return CurrentSliderSetting();
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

        foreach(AccountManager.RankTableRow row in rankTable) {
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

            RectTransform overthenRect = newRankObj
                .transform
                .Find("OverThen")
                .GetComponent<RectTransform>();

            overthenRect.anchorMin = new Vector2(0, 0);
            overthenRect.anchorMax = new Vector2(0, 0);
            overthenRect.anchoredPosition = new Vector3(52.0f, 0.0f);

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
            yield return RewardSetting(row, newRankObj);
        }
        slidersParent.transform.SetAsLastSibling();
    }

    /// <summary>
    /// 보상 세팅
    /// </summary>
    /// <returns></returns>
    IEnumerator RewardSetting(AccountManager.RankTableRow rowData, GameObject obj) {
        var rewardIcons = AccountManager.Instance.resource.rewardIcon;
        var selectedRewards = currentLeagueInfo.rewards.FindAll(x => rowData.pointOverThen <= x.point && rowData.pointLessThen > x.point);
        if (selectedRewards == null || selectedRewards.Count == 0) yield return 0;
        var slots = obj.transform.Find("RewardSlots");
        for (int i=0; i<selectedRewards.Count; i++) {
            var slot = slots.GetChild(i);
            slot.gameObject.SetActive(true);

            var rewardType = selectedRewards[i].reward.kind;
            //if (rewardType == "gold") rewardType = "goldFree";
            //else if (rewardType == "manaCrystal") rewardType = "jewel";

            if (rewardIcons.ContainsKey(rewardType)) {
                slot.Find("Image").GetComponent<Image>().sprite = rewardIcons[rewardType];
            }
            slot.Find("Amount").GetComponent<TextMeshProUGUI>().text = "x" + selectedRewards[i].reward.amount;
        }
        yield return new WaitForEndOfFrame();
    }

    /// <summary>
    /// 이전 보상 진척도 게이지 세팅
    /// </summary>
    /// <returns></returns>
    IEnumerator PrevSliderSetting() {
        yield return new WaitForEndOfFrame();
    }

    /// <summary>
    /// 현재 보상 진척도 게이지 세팅
    /// </summary>
    /// <returns></returns>
    IEnumerator CurrentSliderSetting() {
        yield return new WaitForEndOfFrame();
        var myRankObj = referenceToRankObj.Find(x => x.pointOverThen <= currentLeagueInfo.ratingPoint && x.pointLessThen > currentLeagueInfo.ratingPoint);
        if (myRankObj == null) yield return 0;

        referenceToRankObj.Reverse();
        int index = referenceToRankObj.IndexOf(myRankObj);
        
        Vector2 size = CurrentSlider.GetComponent<RectTransform>().sizeDelta;
        CurrentSlider.GetComponent<RectTransform>().sizeDelta = new Vector2(size.x, index * heightPerRankObj);
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
