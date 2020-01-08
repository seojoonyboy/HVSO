using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuRewardGauge : BattleReadyReward {
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
    }
    


}
