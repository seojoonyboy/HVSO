using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestWindowController : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI timeText;
    [SerializeField] Transform questList;

    protected int remainTime;
    protected float timerTime;
    protected bool timerOn;
    void Start() {
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_QUEST_REFRESH_TIME_UPDATED, RefreshTimer);
    }

    private void OnDestroy() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_QUEST_REFRESH_TIME_UPDATED, RefreshTimer);
    }

    // Update is called once per frame
    void Update()
    {
        if (!timerOn) return;
        if (timeText != null) {
            if (timerTime >= 0) {
                timerTime -= Time.deltaTime;
                if (timerTime <= 0) {
                    timeText.text = "갱신 가능";
                    RerollBtnActivate(true);
                    timerOn = false;
                    return;
                }
                if ((remainTime * 0.001f) - timerTime >= 1) {
                    timerOn = true;
                    SetRefreshTimer(remainTime - 1000);
                }
            }
            else {
                timeText.text = "갱신 가능";
                RerollBtnActivate(true);
                timerOn = false;
                return;
            }
        }
    }

    protected void RefreshTimer(Enum type, Component Sender, object Param) {
        remainTime = AccountManager.Instance.refreshTime.remainTime;
        timerTime = AccountManager.Instance.refreshTime.remainTime;
        if (remainTime > 0) timerOn = true;
        RerollBtnActivate(!timerOn);
        SetRefreshTimer(remainTime);
    }

    public void SetRefreshTimer(int mainAdTimeRemain) {
        TimeSpan time = TimeSpan.FromMilliseconds(mainAdTimeRemain);
        string timerString;

        remainTime = mainAdTimeRemain;
        timerTime = mainAdTimeRemain * 0.001f;
        if (timerTime > 0) {
            timerString = time.Hours.ToString() + ":" + time.Minutes.ToString() + ":" + time.Seconds.ToString();
        }
        else
            timerString = "갱신 가능";
        if (timeText != null)
            timeText.text = timerString;
    }

    public void RerollBtnActivate(bool active) {
        for(int i = 0; i < questList.childCount; i++) {
            questList.GetChild(i).Find("RerollBtn").GetComponent<Button>().interactable = active;
        }
    }
}
