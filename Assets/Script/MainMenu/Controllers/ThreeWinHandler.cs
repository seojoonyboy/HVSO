using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreeWinHandler : MonoBehaviour {
    [SerializeField] Transform slots;

    private int winingStack;
    private const int MAX_WIN_NUM = 3;

    public void AddWin() {
        winingStack++;

        if(winingStack == MAX_WIN_NUM) {
            StartCoroutine(GainReward());
            winingStack = 0;
        }
        else {
            StartCoroutine(StackEffect());
        }
    }

    private void Init() {
        foreach(Transform slot in slots) {
            slot.gameObject.SetActive(false);
        }
    }
    
    IEnumerator GainReward() {
        GetComponent<ResourceSpreader>().StartSpread(20);
        yield return new WaitForEndOfFrame();
    }

    IEnumerator StackEffect() {
        foreach(Transform slot in slots) {
            if (!slot.gameObject.activeSelf) {
                slot.gameObject.SetActive(true);
                break;
            }
        }
        yield return new WaitForEndOfFrame();
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

    void OnDisable() {
        StopAllCoroutines();
    }

    void OnDestroy() {
        StopAllCoroutines();
    }
}
