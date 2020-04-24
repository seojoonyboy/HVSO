using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Quest;

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


    protected void RefreshTimer(Enum type, Component Sender, object Param) {
        remainTime = AccountManager.Instance.refreshTime.remainTime;
        MenuTimerController.Instance.SetTimer(MenuTimerController.TimerType.QUEST_REFRESH, remainTime, timeText, EndTimer);
        if(remainTime > 0) {
            RerollBtnActivate(false);
        }
        else {
            EndTimer();
        }
    }

    public void EndTimer() {
        RerollBtnActivate(true);
        timeText.text = "갱신 가능";
    }

    public void RerollBtnActivate(bool active) {
        for(int i = 0; i < questList.childCount; i++) {
            if (active) {
                if (questList.GetChild(i).GetComponent<QuestContentController>().data.cleared)
                    questList.GetChild(i).Find("RerollBtn").GetComponent<Button>().interactable = false;
                else
                    questList.GetChild(i).Find("RerollBtn").GetComponent<Button>().interactable = true;
            }
            else 
                questList.GetChild(i).Find("RerollBtn").GetComponent<Button>().interactable = false;
        }
    }
}
