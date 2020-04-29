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
    [SerializeField] GameObject rankingProgress;
    [SerializeField] public BattleReadyReward rewarder;
    [SerializeField] Image streakFlag;
    [SerializeField] Sprite[] streakImage;
    [SerializeField] private GameObject normalUIText;    //전투 준비 완료! 텍스트
    
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
        
        SetRank(data);
        SetDescription(data);
        SetSubUI(data);
        
        var leagueDatas = AccountManager.Instance.scriptable_leagueData;
        leagueDatas.prevLeagueInfo = leagueDatas
            .leagueInfo
            .DeepCopy(leagueDatas.leagueInfo);
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
            normalUIText.SetActive(true);
            StartCoroutine(SetNormalUI(data));
        }
        else {
            normalUIText.SetActive(false);
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
        rankingBattleUI.transform.Find("Rank/Image").GetComponent<Image>().sprite = GetRankImage(data.rankDetail.id.ToString());
        rankingBattleUI.transform.Find("Value").GetComponent<Text>().text = data.ratingPoint.ToString();

        Transform rankingTable = rankingBattleUI.transform.Find("RankingTable");
        TextMeshProUGUI description = rankingBattleUI.transform.Find("progressText").GetComponent<TextMeshProUGUI>();

        StringBuilder message = new StringBuilder();
        AccountManager.RankUpCondition rankCondition;
        if (isUp) {
            rankCondition = data.rankDetail.rankUpBattleCount;
            description.text = "승급전!";
        }
        else {
            rankCondition = data.rankDetail.rankDownBattleCount;
            description.text = "강등전!";
        }
        streakFlag.sprite = streakImage[1];
        for (int i = 0; i < rankCondition.battles; i++) {
            if (rankingTable.GetChild(i).name != "Icon") {
                rankingTable.GetChild(i).gameObject.SetActive(true);
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
            }
        }
        yield return 0;
    }

    IEnumerator SetNormalUI(AccountManager.LeagueInfo data) {
        StartCoroutine(_SetRank(data.ratingPoint));

        normalUI.transform.Find("Rank/Image").GetComponent<Image>().sprite = GetRankImage(data.rankDetail.id.ToString());
        yield return 0;
    }

    /// <summary>
    /// 세부 텍스즈 정보 세팅
    /// </summary>
    public void SetDescription(AccountManager.LeagueInfo info) {
        StringBuilder sb = new StringBuilder();
        TextMeshProUGUI descTxt = normalUI.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        string streak;
        if (info.winningStreak > 0) {
            streak = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("MainUI", "ui_page_myinfo_winnum");
            streak = streak.Replace("{n}", "<color=yellow>" + info.winningStreak + "</color>");
            sb
                .Append(streak);
            streakFlag.sprite = streakImage[1];
        }

        else if(info.losingStreak > 0) {
            streak = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("MainUI", "ui_page_myinfo_losenum");
            streak = streak.Replace("{n}", "<color=red>" + info.losingStreak + "</color>");
            sb
               .Append(streak);
            streakFlag.sprite = streakImage[0];
        }
        else {
            normalUIText.SetActive(true);
            streak = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("MainUI", "ui_page_myinfo_winnum");
            streak = streak.Replace("{n}", "<color=yellow>" + 0 + "</color>");
            descTxt.text = streak;
            sb
                .Append(streak);
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
        AccountManager.RankTableRow item = accountManager.rankTable.Find(x => x.id == prevInfo.rankDetail.id);

        int prevRankIndex = -1;

        int pointOverThen = prevInfo.rankDetail.pointOverThen;
        int pointLessThen = prevInfo.rankDetail.pointLessThen;
        int ratingPointTop = prevInfo.ratingPointTop ?? default(int);

        if (item != null) {
            if (item.id == 18)
                prevRankIndex = accountManager.rankTable.Count - 1;
            else if (item.id == 2)
                prevRankIndex = 0;
            else
                prevRankIndex = accountManager.rankTable.IndexOf(item);
        }
        yield return true;
    }
}
