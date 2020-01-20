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
        IsTutorialFinished = Convert.ToBoolean(PlayerPrefs.GetInt("IsTutorialFinished", 0));
    }

    [ReadOnly] bool canLoadDailyQuest = false;  //일일퀘스트 요청을 보낼 수 있는지
    [ReadOnly] bool needToCallAttendanceBoard = false;  //출석 체크 요청을 보낼 수 있는지
    [ReadOnly] bool isTutorialFinished = false; //튜토리얼 이후에 반강제 퀘스트 완료까지

    public bool CanLoadDailyQuest {
        get {
            return canLoadDailyQuest;
        }
        set {
            canLoadDailyQuest = value;
            int val = canLoadDailyQuest ? 1 : 0;
            PlayerPrefs.SetInt("IsQuestLoaded", val);
        }
    }

    public bool NeedToCallAttendanceBoard {
        get {
            return needToCallAttendanceBoard;
        }
        set {
            needToCallAttendanceBoard = value;
            int val = needToCallAttendanceBoard ? 1 : 0;
            PlayerPrefs.SetInt("isAttendanceBoardCalled", val);
        }
    }

    public bool IsTutorialFinished {
        get {
            return isTutorialFinished;
        }
        set {
            isTutorialFinished = value;
            int val = isTutorialFinished ? 1 : 0;
            PlayerPrefs.SetInt("IsTutorialFinished", val);
        }
    }

    public delegate void _allTutorialFinished();
    public delegate void _allMainMenuUnlocked();
    public delegate void _attendanceBoard();

    public event _allTutorialFinished AllTutorialFinished;
    public event _allMainMenuUnlocked AllMainMenuUnlocked;
    public event _attendanceBoard AttendanceBoardInvoked;

    public void TriggerAllMainMenuUnlocked() {
        if(AllMainMenuUnlocked != null) AllMainMenuUnlocked.Invoke();
        AccountManager.Instance.RequestShopItems();
        IsTutorialFinished = true;
    }

    public void TriggerAttendanceBoard() {
        AttendanceBoardInvoked.Invoke();
    }
}
