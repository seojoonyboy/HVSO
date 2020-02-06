using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSceneStateHandler : MonoBehaviour {
    public Dictionary<string, bool> GameStates;

    public bool GetState(string keyword) {
        if (GameStates == null) InitStateDictionary();

        if (GameStates.ContainsKey(keyword)) {
            return GameStates[keyword];
        }
        else {
            Logger.LogWarning(keyword + "에 대한 State를 찾을 수 없습니다!");
            return false;
        }
    }

    //GameState 초기화 (게임 처음 시작시 접근)
    //TODO : 게임 흐름 제어에 사용되는 모든 상태 관리하게 수정
    public void InitStateDictionary() {
        GameStates = new Dictionary<string, bool>();
        GameStates.Add("AccountLinkTutorialLoaded", false);
        GameStates.Add("NickNameChangeTutorialLoaded", false);
        GameStates.Add("NeedToCallAttendanceBoard", true);
        GameStates.Add("DailyQuestLoaded", false);
        GameStates.Add("IsTutorialFinished", false);

        SaveDictionaryToPrefabs();
    }

    public void ChangeState(string key, bool state) {
        if (GameStates.ContainsKey(key)) {
            GameStates[key] = state;
        }

        SaveDictionaryToPrefabs();
    }

    private void SaveDictionaryToPrefabs() {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach(KeyValuePair<string, bool> dict in GameStates) {
            sb.Append(dict.Key + "|" + dict.Value + ",");
        }
        PlayerPrefs.SetString("GameStates", sb.ToString());
    }

    private static MainSceneStateHandler _instance;
    public static MainSceneStateHandler Instance {
        get {
            return _instance;
        }
    }

    private void Awake() {
        _instance = this;
    }

    public delegate void _allTutorialFinished();
    public delegate void _allMainMenuUnlocked();
    public delegate void _attendanceBoard();

    public event _allTutorialFinished AllTutorialFinished;
    public event _allMainMenuUnlocked AllMainMenuUnlocked;
    public event _attendanceBoard AttendanceBoardInvoked;

    public void TriggerAllMainMenuUnlocked() {
        ChangeState("IsTutorialFinished", true);

        if(AllMainMenuUnlocked != null) AllMainMenuUnlocked.Invoke();
        AccountManager.Instance.RequestShopItems();
    }

    public void TriggerAttendanceBoard() {
        AttendanceBoardInvoked.Invoke();
    }
}
