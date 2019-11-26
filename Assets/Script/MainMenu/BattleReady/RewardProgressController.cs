using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using dataModules;
using System.Linq;

public class RewardProgressController : MonoBehaviour {
    [SerializeField]
    GameObject
        prevProgressBar,        //이전 진척도
        currentProgressBar,     //현재 진척도
        backgroundProgressBar;
    [SerializeField] HorizontalLayoutGroup layoutGroup;
    [SerializeField] LeagueData leagueData;
    ScrollRect scrollRect;
    const float width = 140.0f;
    int minMMR = 0;
    int maxMMR = 0;
    RewardsProvider rewardsProvider;

    public bool isMoving = false;

    void Awake() {
        scrollRect = GetComponent<ScrollRect>();
    }

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
        StartCoroutine(Progress(currentProgressBar));
    }

    public void Init() {
        //테스트 코드
        SetProgress(progressType.CURR_PROGRESS_BAR, leagueData.prevMMR);
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

        StartCoroutine(SetProgress(progressBar));
    }

    IEnumerator SetProgress(GameObject progressBar) {
        yield return new WaitForEndOfFrame();
        float progressBarOffsetLeft = 50;
        int prevMMR = leagueData.prevMMR;

        float firstBtnPosX = rewardsProvider
            .buttons[0]
            .GetComponent<RectTransform>()
            .localPosition.x;
        int firstBtnMMR = rewardsProvider.standards[0];

        float startPosX = prevMMR * firstBtnPosX / firstBtnMMR;

        RectTransform rect = progressBar.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(startPosX + progressBarOffsetLeft, rect.rect.height);

        RectTransform bgRect = backgroundProgressBar.GetComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x - 10, bgRect.sizeDelta.y);
    }

    bool isProgressAscending = true;

    /// <summary>
    /// 이전 진척도와 비교하여 변화량 반영
    /// </summary>
    IEnumerator Progress(GameObject progressBar) {
        yield return new WaitForEndOfFrame();

        if (leagueData.newMMR > leagueData.prevMMR) isProgressAscending = true;
        else isProgressAscending = false;

        isMoving = true;

        if (isProgressAscending) {
            yield return StartCoroutine(ProgressAscending());
        }
        else {
            yield return StartCoroutine(ProgressDescending());
        }

        isMoving = false;
    }

    IEnumerator ProgressAscending() {
        int closestBtnIndex = GetIndex(leagueData.newMMR);
        RectTransform closestBtnRect = rewardsProvider.buttons[closestBtnIndex].GetComponent<RectTransform>();
        var closestBtnPosX = closestBtnRect.localPosition.x;
        int btnMMR = GetButtonStandard(closestBtnIndex);

        if(btnMMR != 0) {
            RectTransform rect = currentProgressBar.GetComponent<RectTransform>();
            var progressPosX = currentProgressBar.GetComponent<RectTransform>().localPosition.x;

            float targetPosX = closestBtnPosX * leagueData.newMMR / btnMMR;
            float offset = targetPosX - progressPosX;
            float interval = 0;

            int val = 0;
            while (progressPosX + val < offset) {
                rect.sizeDelta = new Vector2(progressPosX + val - 50, rect.rect.height);
                if (interval > 0 && interval % 40 == 0) {
                    var totalWidth = scrollRect.content.sizeDelta.x;
                    var tmp = rect.sizeDelta / totalWidth;
                    //Logger.Log("rect.sizeDelta + val / totalWidth : " + tmp);
                    scrollRect.horizontalNormalizedPosition = tmp.x;
                }
                yield return new WaitForEndOfFrame();
                val += 10;
                interval++;
            }
        }

        SnapTo(closestBtnIndex);
    }

    IEnumerator ProgressDescending() {
        int closestBtnIndex = GetIndex(leagueData.newMMR);
        RectTransform closestBtnRect = rewardsProvider.buttons[closestBtnIndex].GetComponent<RectTransform>();
        var closestBtnPosX = closestBtnRect.localPosition.x;
        int btnMMR = GetButtonStandard(closestBtnIndex);

        if(btnMMR != 0) {
            RectTransform rect = currentProgressBar.GetComponent<RectTransform>();
            var progressPosX = currentProgressBar.GetComponent<RectTransform>().rect.width;

            float targetPosX = closestBtnPosX * leagueData.newMMR / btnMMR;
            float interval = 0;

            float val = 0;
            while (progressPosX - 50 >= targetPosX) {
                rect.sizeDelta = new Vector2(progressPosX + val - 50, rect.rect.height);
                if (interval > 0 && interval % 40 == 0) {
                    var totalWidth = scrollRect.content.sizeDelta.x;
                    var tmp = rect.sizeDelta / totalWidth;
                    //Logger.Log("rect.sizeDelta + val / totalWidth : " + tmp);
                    scrollRect.horizontalNormalizedPosition = tmp.x;
                }
                progressPosX = rect.rect.width;
                yield return new WaitForSeconds(0.01f);
                val -= 0.1f;
                interval++;
            }
        }

        SnapTo(closestBtnIndex);
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

    private int GetButtonStandard(GameObject button) {
        return button.transform.Find("MMR").GetComponent<IntergerIndex>().Id;
    }

    private int GetButtonStandard(int btnIndex) {
        return rewardsProvider.standards[btnIndex];
    }

    public void SnapTo(int closestBtnIndex) {
        var rect = rewardsProvider.buttons[closestBtnIndex].GetComponent<RectTransform>();
        var posX = rect.localPosition.x;

        var totalWidth = scrollRect.content.sizeDelta.x;
        scrollRect.horizontalNormalizedPosition = posX / totalWidth;
    }

    public enum progressType {
        PREV_PROGRESS_BAR,
        CURR_PROGRESS_BAR
    }
}
