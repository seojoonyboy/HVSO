using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 메인화면용
/// TODO : 인게임에서도 재활용 가능하게
/// </summary>
public class ThreeWinHandler : MonoBehaviour {
    [SerializeField] Transform slots;

    private int winingStack;
    private const int MAX_WIN_NUM = 3;

    private void Init() {
        foreach(Transform slot in slots) {
            slot.gameObject.SetActive(false);
        }
    }

    IEnumerator Start() {
        Init();

        var accountManager = AccountManager.Instance;
        yield return new WaitUntil(() => accountManager.userData != null);
        var leagueWinCount = accountManager.userData.etcInfo.Find(x => x.key == "leagueWinCount");
        
        if(leagueWinCount != null) {
            int winCount = 0;
            int.TryParse(leagueWinCount.value, out winCount);

            if (winCount != 0) {
                for (int i = 0; i < winCount; i++) {
                    slots.GetChild(i).gameObject.SetActive(true);
                }
            }
        }
    }

    public void GainReward() {
        GetComponent<ResourceSpreader>().StartSpread(20);
        Init();
    }

    void OnDisable() {
        StopAllCoroutines();
    }

    void OnDestroy() {
        StopAllCoroutines();
    }
}
