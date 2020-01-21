using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using System.Text;

public class BattleReadyHeaderController : SerializedMonoBehaviour {
    [SerializeField] BattleReadySceneController battleReadySceneController;
    [SerializeField] GameObject normalUI, rankingBattleUI;
    [SerializeField] Image headerImg;
    [SerializeField] Sprite[] headerImages;
    [SerializeField] GameObject rankingProgress;
    [SerializeField] public BattleReadyReward rewarder;
    [SerializeField] Image streakFlag;
    [SerializeField] Sprite[] streakImage;

    void OnDisable() {
        normalUI.SetActive(true);
        foreach(Transform child in rankingBattleUI.transform.Find("RankingTable")) {
            foreach(Transform mark in child) {
                mark.gameObject.SetActive(false);
            }
            child.gameObject.SetActive(false);
        }
        rankingBattleUI.SetActive(false);
    }

    public void SetUI(AccountManager.LeagueInfo data) {
        TextMeshProUGUI mmrName = transform.Find("Desc/MMR/MinorName").GetComponent<TextMeshProUGUI>();
        mmrName.text = data.rankDetail.minorRankName;
        //Image rankImg = rankObj.transform.Find("Image").GetComponent<Image>();
        //rankImg.sprite = GetRankImage(data.rankDetail.minorRankName);
        
        SetRank(data);
        SetDescription(data);
        SetSubUI(data);
    }

    public Sprite GetRankImage(string keyword) {
        var rankIcons = AccountManager.Instance.resource.rankIcons;
        Sprite sprite = rankIcons["default"];
        if (!string.IsNullOrEmpty(keyword) && rankIcons.ContainsKey(keyword)) {
            sprite = rankIcons[keyword];
        }
        return sprite;
    }

    /// <summary>
    /// 랭킹 점수 세팅
    /// </summary>
    public void SetRank(AccountManager.LeagueInfo mmr) {
        StartCoroutine(_SetRank(mmr.ratingPoint));
        //StartCoroutine(_SetRankProgress(mmr));
    }

    /// <summary>
    /// 승급/강등전 발생한 경우 UI 세팅
    /// </summary>
    public void SetSubUI(AccountManager.LeagueInfo data) {
        var rankDetail = data.rankDetail;
        if(data.rankingBattleState == "normal") {
            StartCoroutine(SetNormalUI(data));
        }
        else {
            //승급전 발생
            if (rankDetail.rankUpBattleCount != null) {
                StartCoroutine(SetRankChangeChanceUI(data, true));
            }
            //강등전 발생
            else if (rankDetail.rankDownBattleCount != null) {
                StartCoroutine(SetRankChangeChanceUI(data, false));
            }
        }
    }

    IEnumerator SetRankChangeChanceUI(AccountManager.LeagueInfo data, bool isUp) {
        normalUI.SetActive(false);
        rankingBattleUI.SetActive(true);

        rankingBattleUI.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = data.rankDetail.minorRankName;
        rankingBattleUI.transform.Find("Rank/Image").GetComponent<Image>().sprite = GetRankImage(data.rankDetail.minorRankName);
        rankingBattleUI.transform.Find("Value").GetComponent<Text>().text = data.ratingPoint.ToString();

        Transform rankingTable = rankingBattleUI.transform.Find("RankingTable");
        TextMeshProUGUI description = rankingBattleUI.transform.Find("progressText").GetComponent<TextMeshProUGUI>();

        StringBuilder message = new StringBuilder();
        AccountManager.RankUpCondition rankCondition;
        if (isUp) {
            rankCondition = data.rankDetail.rankUpBattleCount;
            description.text = "승급전!";
            //message.Append("승급전 발생");
        }
        else {
            rankCondition = data.rankDetail.rankDownBattleCount;
            description.text = "강등전!";
        }
        streakFlag.sprite = streakImage[1];
        for (int i = 0; i < rankCondition.battles; i++) {
            if (rankingTable.GetChild(i).name != "Icon") {
                rankingTable.GetChild(i).gameObject.SetActive(true);
                rankingTable.GetChild(i).Find("exclamation").gameObject.SetActive(true);
            }   
            yield return new WaitForSeconds(1.0f);
        }

        if(data.rankingBattleCount != null) {
            for(int i=0; i<data.rankingBattleCount.Length; i++) {
                //승리
                if(data.rankingBattleCount[i] == true) {
                    if(rankingTable.GetChild(i).name != "Icon") {
                        rankingTable.GetChild(i).Find("Win").gameObject.SetActive(true);                        
                    }
                }
                //패배
                else {
                    if (rankingTable.GetChild(i).name != "Icon") {
                        rankingTable.GetChild(i).Find("Lose").gameObject.SetActive(true);
                    }
                }
                rankingTable.GetChild(i).Find("exclamation").gameObject.SetActive(false);
            }
        }
        yield return 0;
    }

    IEnumerator SetNormalUI(AccountManager.LeagueInfo data) {
        SetDescription(data);
        StartCoroutine(_SetRank(data.ratingPoint));
        //StartCoroutine(_SetRankProgress(data));

        normalUI.transform.Find("Rank/Image").GetComponent<Image>().sprite = GetRankImage(data.rankDetail.minorRankName);
        yield return 0;
    }

    /// <summary>
    /// 세부 텍스즈 정보 세팅
    /// </summary>
    public void SetDescription(AccountManager.LeagueInfo info) {
        StringBuilder sb = new StringBuilder();
        TextMeshProUGUI descTxt = normalUI.transform.Find("Text").GetComponent<TextMeshProUGUI>();

        if (info.winningStreak > 0) {
            if(info.winningStreak > 1) {
                sb
                .Append("<color=yellow>")
                .Append(info.winningStreak)
                .Append("연승! </color>");
            }
            else {
                sb
                .Append("<color=yellow>")
                .Append(info.winningStreak)
                .Append("승 </color>");
            }
            streakFlag.sprite = streakImage[1];
        }

        else if(info.losingStreak > 0) {
            if(info.losingStreak > 1) {
                sb
                .Append("<color=red>")
                .Append(info.losingStreak)
                .Append("연패중! </color>");
            }
            else {
                sb
                .Append("<color=red>")
                .Append(info.losingStreak)
                .Append("패 </color>");
            }
            streakFlag.sprite = streakImage[0];
        }
        else {
            sb
                .Append("<color=white>")
                .Append(info.losingStreak)
                .Append("승 </color>");

            streakFlag.sprite = streakImage[1];
        }
        descTxt.text = sb.ToString();
    }

    IEnumerator _SetRank(int mmr) {
        yield return 0;
        var mmrValue = normalUI.transform.Find("MMR/Value").GetComponent<Text>();
        mmrValue.text = mmr.ToString();

    }


    IEnumerator _SetRankProgress(AccountManager.LeagueInfo mmr) {
        AccountManager accountManager = AccountManager.Instance;
        AccountManager.LeagueInfo currinfo = mmr;
        AccountManager.LeagueInfo prevInfo = accountManager.scriptable_leagueData.prevLeagueInfo;
        AccountManager.RankTableRow item = accountManager.rankTable.Find(x => x.minorRankName == prevInfo.rankDetail.minorRankName);

        int prevRankIndex = -1;

        int pointOverThen = prevInfo.rankDetail.pointOverThen;
        int pointLessThen = prevInfo.rankDetail.pointLessThen;
        int ratingPointTop = prevInfo.ratingPointTop ?? default(int);

        if (item != null) {
            if (item.minorRankName == "무명 병사")
                prevRankIndex = 1;
            else if (item.minorRankName == "전략의 제왕")
                prevRankIndex = accountManager.rankTable.Count - 1;
            else
                prevRankIndex = accountManager.rankTable.IndexOf(item);
        }
        yield return true;
    }
}
