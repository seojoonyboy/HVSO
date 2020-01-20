using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSceneStateHandler : MonoBehaviour {
    private static MainSceneStateHandler _instance;
    public static MainSceneStateHandler Instance {
        get {
            return _instance;
        }
    }

    private void Awake() {
        _instance = this;

        CanLoadDailyQuest = Convert.ToBoolean(PlayerPrefs.GetInt("IsQuestLoaded", 0));
        NeedToCallAttendanceBoard = Convert.ToBoolean(PlayerPrefs.GetInt("isAttendanceBoardCalled", 0));
    }

    [ReadOnly] bool canLoadDailyQuest = false;  //일일퀘스트 요청을 보낼 수 있는지
    [ReadOnly] bool needToCallAttendanceBoard = false;  //출석 체크 요청을 보낼 수 있는지
    [ReadOnly] bool isTutorialFinished = false; //튜토리얼 이후에 반강제 퀘스트 완료까지

    public bool CanLoadDailyQuest {
        get {
            return canLoadDailyQuest;
        }
        set {
            int val = canLoadDailyQuest ? 1 : 0;
            PlayerPrefs.SetInt("IsQuestLoaded", val);
            canLoadDailyQuest = value;
        }
    }

    public bool NeedToCallAttendanceBoard {
        get {
            return needToCallAttendanceBoard;
        }
        set {
            int val = needToCallAttendanceBoard ? 1 : 0;
            PlayerPrefs.SetInt("isAttendanceBoardCalled", val);
            needToCallAttendanceBoard = value;
        }
    }

    public delegate void _allTutorialFinished();
    public delegate void _allMainMenuUnlocked();

    public event _allTutorialFinished AllTutorialFinished;
    public event _allMainMenuUnlocked AllMainMenuUnlocked;

    public void TriggerAllMainMenuUnlocked() {
        AllMainMenuUnlocked.Invoke();
    }

    public void TriggerAllTutorialFinished() {
        PlayerPrefs.SetInt("IsTutorialFinished", 1);
        AllTutorialFinished.Invoke();
    }
}
