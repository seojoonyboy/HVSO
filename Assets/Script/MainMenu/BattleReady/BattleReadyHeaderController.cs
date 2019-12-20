using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using System.Text;

public class BattleReadyHeaderController : SerializedMonoBehaviour {
    [SerializeField] BattleReadySceneController battleReadySceneController;
    [SerializeField] GameObject rankObj, normalUI, rankingBattleUI;
    [SerializeField] Image headerImg;
    [SerializeField] Sprite[] headerImages;

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
        TextMeshProUGUI nameTxt = rankObj.transform.Find("NameBg/Name").GetComponent<TextMeshProUGUI>();
        nameTxt.text = data.rankDetail.minorRankName;
        Image rankImg = rankObj.transform.Find("Image").GetComponent<Image>();
        rankImg.sprite = GetRankImage(data.rankDetail.minorRankName);

        SetRank(data.ratingPoint);
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
    public void SetRank(int mmr) {
        StartCoroutine(_SetRank(mmr));
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

        //headerImg.sprite = 
        Transform rankingTable = rankingBattleUI.transform.Find("RankingTable");
        TextMeshProUGUI description = rankingBattleUI.transform.Find("Text").GetComponent<TextMeshProUGUI>();

        StringBuilder message = new StringBuilder();
        AccountManager.RankUpCondition rankCondition;
        if (isUp) {
            rankCondition = data.rankDetail.rankUpBattleCount;
            description.text = "승급전 발생";
            //message.Append("승급전 발생");
        }
        else {
            rankCondition = data.rankDetail.rankDownBattleCount;
            description.text = "강등전 발생";
        }

        for (int i = 0; i < rankCondition.battles; i++) {
            rankingTable.GetChild(i).gameObject.SetActive(true);
            yield return new WaitForSeconds(1.0f);
        }

        if(data.rankingBattleCount != null) {
            for(int i=0; i<data.rankingBattleCount.Length; i++) {
                //승리
                if(data.rankingBattleCount[i] == true) {
                    rankingTable.GetChild(i).Find("Win").gameObject.SetActive(true);
                }
                //패배
                else {
                    rankingTable.GetChild(i).Find("Lose").gameObject.SetActive(true);
                }
            }
        }
        yield return 0;
    }

    IEnumerator SetNormalUI(AccountManager.LeagueInfo data) {
        SetDescription(data);
        StartCoroutine(_SetRank(data.ratingPoint));
        yield return 0;
    }

    /// <summary>
    /// 세부 텍스즈 정보 세팅
    /// </summary>
    public void SetDescription(AccountManager.LeagueInfo info) {
        StringBuilder sb = new StringBuilder();
        TextMeshProUGUI descTxt = normalUI.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        descTxt.gameObject.SetActive(true);

        //연승중
        if (info.winningStreak > 1) {
            sb
                .Append("<color=yellow>")
                .Append(info.winningStreak)
                .Append("연승! </color>");
        }
        //연패중
        else if(info.losingStreak > 1) {
            sb
                .Append("<color=red>")
                .Append(info.losingStreak)
                .Append("연패중</color>");
        }
        else {
            descTxt.gameObject.SetActive(false);
        }
        descTxt.text = sb.ToString();
    }

    IEnumerator _SetRank(int mmr) {
        yield return 0;
        var medalValue = normalUI.transform.Find("Medal/Value").GetComponent<TextMeshProUGUI>();
        medalValue.text = mmr.ToString();
    }
}
