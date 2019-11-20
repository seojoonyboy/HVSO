using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using dataModules;
using System.Linq;

public class RewardProgressController : MonoBehaviour {
    [SerializeField] GameObject 
        prevProgressBar, //이전 진척도
        currentProgressBar; //현재 진척도
    [SerializeField] HorizontalLayoutGroup layoutGroup;

    const float width = 140.0f;
    int minMMR = 0;
    int maxMMR = 0;
    RewardsProvider rewardsProvider;

    public void OnRewardObjectSettingFinished() {
        rewardsProvider = GetComponent<RewardsProvider>();

        minMMR = rewardsProvider.buttons[0].transform.Find("MMR").GetComponent<IntergerIndex>().Id;
        maxMMR = rewardsProvider.buttons[rewardsProvider.buttons.Count - 1].transform.Find("MMR").GetComponent<IntergerIndex>().Id;

        //테스트 코드
        //GetIndex(100);
        //GetIndex(300);
        //GetIndex(1000);
        //GetIndex(1200);

        Init();
    }

    public void Init() {
        int currMMR = PlayerPrefs.GetInt("currentMMR");
        //TODO : 이전에 진척도 정보를 따로 보관하고 있어야함.

        currMMR = 300;
        SetProgress(progressType.PREV_PROGRESS_BAR, currMMR);
    }

    //진척도 초기 세팅
    private void SetProgress(progressType type, int mmr) {
        GameObject progressBar = null;

        if(type == progressType.CURR_PROGRESS_BAR) {
            progressBar = currentProgressBar;
        }
        else if(type == progressType.PREV_PROGRESS_BAR) {
            progressBar = prevProgressBar;
        }
        else {
            progressBar = currentProgressBar;
            Logger.Log("argument is wrong");
        }

        int closestBtnIndex = GetIndex(mmr);
        float progressBarOffsetLeft = 50;
        StartCoroutine(SetProgress(closestBtnIndex, progressBarOffsetLeft, progressBar));
    }

    IEnumerator SetProgress(int closestBtnIndex, float offset, GameObject progressBar) {
        yield return new WaitForEndOfFrame();
        var pos = rewardsProvider.buttons[closestBtnIndex].GetComponent<RectTransform>().localPosition.x;

        RectTransform rect = progressBar.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(pos - offset, rect.rect.height);
    }

    /// <summary>
    /// mmr에서 가장 가까운 버튼의 index를 구함
    /// </summary>
    /// <param name="mmr">내 현재 mmr</param>
    /// <returns></returns>
    private int GetIndex(int mmr) {
        if(mmr < minMMR) {
            return 0;
        }
        else {
            if(mmr > maxMMR) {
                return rewardsProvider.buttons.Count - 1;
            }
            else {
                int closest = rewardsProvider.standards.Aggregate((x, y) => Math.Abs(x - mmr) < Math.Abs(y - mmr) ? x : y);
                int index = rewardsProvider.standards.IndexOf(closest);
                if(closest > mmr) {
                    index -= 1;
                }
                //Logger.Log(index);
                return index;
            }
        }
    }

    public enum progressType {
        PREV_PROGRESS_BAR,
        CURR_PROGRESS_BAR
    }
}
