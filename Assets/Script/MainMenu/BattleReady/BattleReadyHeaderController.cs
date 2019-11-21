using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using System.Text;

public class BattleReadyHeaderController : SerializedMonoBehaviour {
    [SerializeField] BattleReadySceneController battleReadySceneController;
    [SerializeField] GameObject rankObj, descObj;

    public Dictionary<string, Sprite> rankSprites;

    /// <summary>
    /// 승급의 기회 발생 (승급전)
    /// </summary>
    public void RankUpChanceOccured() {

    }

    IEnumerator RankUpChanceProceed() {
        yield return 0;
    }

    /// <summary>
    /// 강등의 위기 발생 (강등전)
    /// </summary>
    public void RankDownEmergency() {

    }

    IEnumerator RankDownEmergencyProceed() {
        yield return 0;
    }

    /// <summary>
    /// 승급 발생
    /// </summary>
    public void RankUpOccured() {

    }

    IEnumerator RankUpProceed() {
        yield return 0;
    }

    /// <summary>
    /// 강등 발생
    /// </summary>
    public void RankDownOccured() {

    }

    IEnumerator RankDownProceed() {
        yield return 0;
    }

    public void SetUI(AccountManager.LeagueInfo data) {
        TextMeshProUGUI nameTxt = rankObj.transform.Find("NameBg/Name").GetComponent<TextMeshProUGUI>();
        nameTxt.text = data.rankDetail.minor;
        Image rankImg = rankObj.transform.Find("Image").GetComponent<Image>();
        rankImg.sprite = GetRankImage(data.rankDetail.minor);

        SetRank(data.ratingPoint);
        SetDescription(data);
    }

    public Sprite GetRankImage(string keyword) {
        Sprite sprite = rankSprites["default"];
        if (!string.IsNullOrEmpty(keyword) && rankSprites.ContainsKey(keyword)) {
            sprite = rankSprites[keyword];
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
    /// 세부 텍스즈 정보 세팅
    /// </summary>
    public void SetDescription(AccountManager.LeagueInfo info) {
        StringBuilder sb = new StringBuilder();
        TextMeshProUGUI descTxt = descObj.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        //연승중
        if(info.winningStreak > 0) {
            sb
                .Append("<color=yellow>")
                .Append(info.winningStreak)
                .Append("연승! </color>");
        }
        //연패중
        else if(info.losingStreak > 0) {
            sb
                .Append("<color=red>")
                .Append(info.losingStreak)
                .Append("연패중</color>");
        }
        descTxt.text = sb.ToString();
    }

    IEnumerator _SetRank(int mmr) {
        yield return 0;
        var medalValue = descObj.transform.Find("Medal/Value").GetComponent<TextMeshProUGUI>();
        medalValue.text = mmr.ToString();
    }
}
