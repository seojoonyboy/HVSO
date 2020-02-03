using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuRewardGauge : BattleReadyReward {

    [SerializeField] Transform rankingBattleUI;
    [SerializeField] Image tierFlag;


    private void Awake() {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_LEAGUE_INFO_UPDATED, OnLeagueInfoUpdated);
    }

    private void OnDestroy() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_LEAGUE_INFO_UPDATED, OnLeagueInfoUpdated);
        StopAllCoroutines();
    }

    private void OnDisable() {
        StopAllCoroutines();
    }


    private void OnEnable() {        
        SetUpReward();
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

        yield return null;
    }
    public void OnLeagueInfoUpdated(System.Enum Event_Type, Component Sender, object Param) {
        SetUpReward();
        StartCoroutine(SetRankChangeChanceUI());
    }





}
