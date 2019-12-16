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
        SetProgress(progressType.CURR_PROGRESS_BAR);
    }

    //진척도 초기 세팅
    private void SetProgress(progressType type) {
        GameObject progressBar = null;

        if(type == progressType.CURR_PROGRESS_BAR) {
            progressBar = currentProgressBar;
        }
        else if(type == progressType.PREV_PROGRESS_BAR) {
            progressBar = prevProgressBar;
        }
        else {
            progressBar = currentProgressBar;
            //Logger.Log("argument is wrong");
        }

        StartCoroutine(SetProgress(progressBar));
    }

    const float progressBarOffsetLeft = 50;

    IEnumerator SetProgress(GameObject progressBar) {
        yield return new WaitForEndOfFrame();
        
        int prevMMR = leagueData.prevLeagueInfo.ratingPoint;

        int closestRightTargetIndex = 0;
        int closestLeftTargetIndex = GetIndex(prevMMR);
        if (closestLeftTargetIndex == rewardsProvider.buttons.Count - 1) closestRightTargetIndex = rewardsProvider.buttons.Count - 1;
        else closestRightTargetIndex = GetIndex(prevMMR) + 1;

        float closestLeftTargetMMR = rewardsProvider
            .standards[closestLeftTargetIndex];
        float closestLeftTargetX = rewardsProvider
            .buttons[closestLeftTargetIndex]
            .GetComponent<RectTransform>()
            .localPosition.x;
        float closestRightTargetMMR = rewardsProvider
            .standards[closestRightTargetIndex];
        float closestRightTargetX = rewardsProvider
            .buttons[closestRightTargetIndex]
            .GetComponent<RectTransform>()
            .localPosition.x;

        if (closestLeftTargetIndex < closestRightTargetIndex) {
            float mmrWidth = closestRightTargetMMR - closestLeftTargetMMR;
            float localPosWidth = closestRightTargetX - closestLeftTargetX;

            var val = localPosWidth * (prevMMR - closestLeftTargetMMR) / mmrWidth;

            RectTransform rect = progressBar.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(closestLeftTargetX + val - progressBarOffsetLeft, rect.rect.height);
        }
        else {
            int tmpVal = 60;
            RectTransform rect = progressBar.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(closestLeftTargetX + tmpVal - progressBarOffsetLeft, rect.rect.height);
        }

        RectTransform bgRect = backgroundProgressBar.GetComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x - 10, bgRect.sizeDelta.y);
    }

    bool isProgressAscending = true;

    /// <summary>
    /// 이전 진척도와 비교하여 변화량 반영
    /// </summary>
    IEnumerator Progress(GameObject progressBar) {
        yield return new WaitForEndOfFrame();

        isProgressAscending = (leagueData.leagueInfo.ratingPoint > leagueData.prevLeagueInfo.ratingPoint);
        //테스트 코드
        //isProgressAscending = false;
        
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
        int newMMR = leagueData.prevLeagueInfo.ratingPoint;
        //테스트 코드
        //newMMR = 3000;

        int closestRightTargetIndex = 0;
        int closestLeftTargetIndex = GetIndex(newMMR);
        if (closestLeftTargetIndex == rewardsProvider.buttons.Count - 1) closestRightTargetIndex = rewardsProvider.buttons.Count - 1;
        else closestRightTargetIndex = GetIndex(newMMR) + 1;

        float closestLeftTargetMMR = rewardsProvider
            .standards[closestLeftTargetIndex];
        float closestLeftTargetX = rewardsProvider
            .buttons[closestLeftTargetIndex]
            .GetComponent<RectTransform>()
            .localPosition.x;
        float closestRightTargetMMR = rewardsProvider
            .standards[closestRightTargetIndex];
        float closestRightTargetX = rewardsProvider
            .buttons[closestRightTargetIndex]
            .GetComponent<RectTransform>()
            .localPosition.x;

        if (closestLeftTargetIndex < closestRightTargetIndex) {
            float mmrWidth = closestRightTargetMMR - closestLeftTargetMMR;
            float localPosWidth = closestRightTargetX - closestLeftTargetX;

            float val = 0;
            if (newMMR != closestLeftTargetMMR) {
                val = closestLeftTargetX + (localPosWidth * (newMMR - closestLeftTargetMMR) / mmrWidth) - progressBarOffsetLeft;
            }
            else {
                val = closestLeftTargetX - progressBarOffsetLeft;
            }
            
            float targetPosX = val;
            RectTransform rect = currentProgressBar.GetComponent<RectTransform>();
            float progressValue = rect.sizeDelta.x;

            int interval = 0;
            while (progressValue < targetPosX) {
                progressValue += 10;
                rect.sizeDelta = new Vector2(progressValue, rect.rect.height);
                if (interval > 0 && interval % 40 == 0) {
                    var totalWidth = scrollRect.content.sizeDelta.x;
                    var tmp = rect.sizeDelta / totalWidth;
                    scrollRect.horizontalNormalizedPosition = tmp.x;
                }
                yield return new WaitForEndOfFrame();
                interval++;
            }
        }
        SnapTo(closestRightTargetIndex);
    }

    IEnumerator ProgressDescending() {
        int newMMR = leagueData.prevLeagueInfo.ratingPoint;
        //테스트 코드
        //newMMR = 2000;

        int closestRightTargetIndex = 0;
        int closestLeftTargetIndex = GetIndex(newMMR);
        if (closestLeftTargetIndex == rewardsProvider.buttons.Count - 1) closestRightTargetIndex = rewardsProvider.buttons.Count - 1;
        else closestRightTargetIndex = GetIndex(newMMR) + 1;

        float closestLeftTargetMMR = rewardsProvider
            .standards[closestLeftTargetIndex];
        float closestLeftTargetX = rewardsProvider
            .buttons[closestLeftTargetIndex]
            .GetComponent<RectTransform>()
            .localPosition.x;
        float closestRightTargetMMR = rewardsProvider
            .standards[closestRightTargetIndex];
        float closestRightTargetX = rewardsProvider
            .buttons[closestRightTargetIndex]
            .GetComponent<RectTransform>()
            .localPosition.x;

        if (closestLeftTargetIndex < closestRightTargetIndex) {
            float mmrWidth = closestRightTargetMMR - closestLeftTargetMMR;
            float localPosWidth = closestRightTargetX - closestLeftTargetX;

            float val = 0;
            if (newMMR != closestLeftTargetMMR) {
                val = closestLeftTargetX + (localPosWidth * (newMMR - closestLeftTargetMMR) / mmrWidth) - progressBarOffsetLeft;
            }
            else {
                val = closestLeftTargetX - progressBarOffsetLeft;
            }

            float targetPosX = val;
            RectTransform rect = currentProgressBar.GetComponent<RectTransform>();
            float progressValue = rect.sizeDelta.x;

            int interval = 0;
            while (progressValue > targetPosX) {
                progressValue -= 10;
                rect.sizeDelta = new Vector2(progressValue, rect.rect.height);
                if (interval > 0 && interval % 40 == 0) {
                    var totalWidth = scrollRect.content.sizeDelta.x;
                    var tmp = rect.sizeDelta / totalWidth;
                    scrollRect.horizontalNormalizedPosition = tmp.x;
                }
                yield return new WaitForEndOfFrame();
                interval++;
            }
        }
        SnapTo(closestRightTargetIndex);
    }

    /// <summary>
    /// mmr에서 가장 가까운 버튼(자신보다 작은)의 index를 구함
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
