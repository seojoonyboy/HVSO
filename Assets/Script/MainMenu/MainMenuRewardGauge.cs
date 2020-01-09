using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuRewardGauge : BattleReadyReward {

    [SerializeField] Transform rankingBattleUI;
    [SerializeField] Image tierFlag;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Wait_Deploy_Data());   
    }

    private void OnEnable() {

    }

    IEnumerator Wait_Deploy_Data() {
        yield return new WaitUntil(() => AccountManager.Instance.scriptable_leagueData.leagueInfo.rewards != null);
        SetUpReward();
        yield return SetRankChangeChanceUI();
    }

    IEnumerator SetRankChangeChanceUI() {
        AccountManager.LeagueInfo currInfo = AccountManager.Instance.scriptable_leagueData.leagueInfo;

        AccountManager.RankUpCondition rankCondition;
        TMPro.TextMeshProUGUI description = rankingBattleUI.Find("Text").gameObject.GetComponent<TMPro.TextMeshProUGUI>();
        Transform rankTable = rankingBattleUI.Find("RankingTable");


        if (currInfo.rankDetail.rankUpBattleCount != null) {
            rankCondition = currInfo.rankDetail.rankUpBattleCount;
            description.text = "승급전!";
        }
        else if (currInfo.rankDetail.rankDownBattleCount != null) {
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


        for (int i = 0; i < rankCondition.battles; i++) {
            if (rankTable.GetChild(i).name != "Icon") {
                rankTable.GetChild(i).gameObject.SetActive(true);
            }
            yield return new WaitForSeconds(1.0f);
        }

        if (currInfo.rankingBattleCount != null) {
            for (int i = 0; i < currInfo.rankingBattleCount.Length; i++) {
                //승리
                if (currInfo.rankingBattleCount[i] == true) {
                    if (rankTable.GetChild(i).name != "Icon") {
                        rankTable.GetChild(i).Find("Win").gameObject.SetActive(true);
                    }
                }
                //패배
                else {
                    if (rankTable.GetChild(i).name != "Icon") {
                        rankTable.GetChild(i).Find("Lose").gameObject.SetActive(true);
                    }
                }
            }
        }


        yield return null;
    }

    


}
