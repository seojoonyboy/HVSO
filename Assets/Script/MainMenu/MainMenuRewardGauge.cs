using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuRewardGauge : BattleReadyReward {

    [SerializeField] Transform rankingBattleUI;
    [SerializeField] Image tierFlag;


    private void Awake() {
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_LEAGUE_INFO_UPDATED, OnLeagueInfoUpdated);
    }

    // Start is called before the first frame update
    void Start()
    {        
        //StartCoroutine(Wait_Deploy_Data());   
    }

    private void OnDestroy() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_LEAGUE_INFO_UPDATED, OnLeagueInfoUpdated);
        StopAllCoroutines();
    }

    private void OnDisable() {
        StopAllCoroutines();
    }


    private void OnEnable() {
        if (AccountManager.Instance.rankTable == null || AccountManager.Instance.rankTable.Count < 1) AccountManager.Instance.RequestRankTable();
        if (AccountManager.Instance.scriptable_leagueData == null) AccountManager.Instance.RequestLeagueInfo();
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


        //for (int i = 0; i < rankCondition.battles; i++) {
        //    if (rankTable.GetChild(i).name != "Icon") {
        //        rankTable.GetChild(i).gameObject.SetActive(true);
        //    }
        //    yield return new WaitForSeconds(1.0f);
        //}

        //if (currInfo.rankingBattleCount != null) {
        //    for (int i = 0; i < currInfo.rankingBattleCount.Length; i++) {
        //        //승리
        //        if (currInfo.rankingBattleCount[i] == true) {
        //            if (rankTable.GetChild(i).name != "Icon") {
        //                rankTable.GetChild(i).Find("Win").gameObject.SetActive(true);
        //            }
        //        }
        //        //패배
        //        else {
        //            if (rankTable.GetChild(i).name != "Icon") {
        //                rankTable.GetChild(i).Find("Lose").gameObject.SetActive(true);
        //            }
        //        }
        //    }
        //}


        yield return null;
    }
    public void OnLeagueInfoUpdated(System.Enum Event_Type, Component Sender, object Param) {
        SetUpReward();
        StartCoroutine(SetRankChangeChanceUI());
    }





}
