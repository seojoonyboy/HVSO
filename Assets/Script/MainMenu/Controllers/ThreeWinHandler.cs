using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;

/// <summary>
/// 메인화면용
/// TODO : 인게임에서도 재활용 가능하게
/// </summary>
public class ThreeWinHandler : MonoBehaviour {
    [SerializeField] Transform slots;
    [SerializeField] SkeletonGraphic slotSkeleton;
    [SerializeField] SkeletonGraphic rewardSkeleton;

    private int winingStack;
    private const int MAX_WIN_NUM = 3;

    IEnumerator Start() {
        var accountManager = AccountManager.Instance;
        yield return new WaitUntil(() => accountManager.userData != null);
        var leagueWinCount = accountManager.userData.etcInfo.Find(x => x.key == "leagueWinCount");
        
        if(leagueWinCount != null) {
            int winCount = 0;
            int.TryParse(leagueWinCount.value, out winCount);

            if (winCount != 0) slotSkeleton.AnimationState.SetAnimation(0, winCount+"_winner", false);
        }
    }

    public void GainReward() {
        slotSkeleton
            .AnimationState
            .SetAnimation(0, "supply_idle", false);
    }

    void OnDisable() {
        StopAllCoroutines();
    }

    void OnDestroy() {
        StopAllCoroutines();
    }
}
