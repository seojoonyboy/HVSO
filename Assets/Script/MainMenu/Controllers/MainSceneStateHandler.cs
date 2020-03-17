using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Convert;

public class MainSceneStateHandler : MonoBehaviour {
    public Dictionary<string, bool> GameStates;

    public bool GetState(string keyword) {
        if (!GameStates.ContainsKey(keyword)) {
            Logger.LogWarning(keyword + "에 대한 State를 찾을 수 없습니다!");
            return false;
        }
        else {
            return GameStates[keyword];
        }
    }

    //GameState 초기화 (게임 처음 시작시 접근)
    //TODO : 게임 흐름 제어에 사용되는 모든 상태 관리하게 수정
    private void InitStateDictionary() {
        GameStates = new Dictionary<string, bool>();
        GameStates.Add("AccountLinkTutorialLoaded", false);
        GameStates.Add("NickNameChangeTutorialLoaded", false);
        GameStates.Add("NeedToCallAttendanceBoard", true);
        GameStates.Add("DailyQuestLoaded", false);
        GameStates.Add("IsTutorialFinished", false);
        GameStates.Add("IsQ5Finished", false);
        GameStates.Add("isLeagueFirst", false);
        SaveDictionaryToPrefabs();
    }

    public void ChangeState(string key, bool state) {
        GameStates[key] = state;
        SaveDictionaryToPrefabs();
    }

    private void SaveDictionaryToPrefabs() {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach(KeyValuePair<string, bool> dict in GameStates) {
            sb.Append(dict.Key + "|" + dict.Value + ",");
        }
        PlayerPrefs.SetString("GameStates", sb.ToString());
    }

    private void GetPrefabToDictionary() {
        GameStates = new Dictionary<string, bool>();
        var prevData = PlayerPrefs.GetString("GameStates");
        string[] slicedData = prevData.Split(',');
        foreach(string data in slicedData) {
            if (string.IsNullOrEmpty(data)) continue;
            var key = data.Split('|')[0];
            var value = ToBoolean(data.Split('|')[1]);
            GameStates.Add(key, value);
        }

        var _isLeagueFirstPlayed = PlayerPrefs.GetInt("isLeagueFirst");
        bool isLeagueFirstPlayed = ToBoolean(_isLeagueFirstPlayed);
        if (GameStates.ContainsKey("isLeagueFirst")) GameStates["isLeagueFirst"] = isLeagueFirstPlayed;
        else GameStates.Add("isLeagueFirst", isLeagueFirstPlayed);
    }

    public static MainSceneStateHandler Instance { get; private set; }

    private void Awake() {
        Instance = this;

        if (PlayerPrefs.GetInt("isFirst") == 1) {
            InitStateDictionary();
        }
        else {
            GetPrefabToDictionary();
        }
    }

    public delegate void _allTutorialFinished();
    public delegate void _allMainMenuUnlocked();
    public delegate void _attendanceBoard();

    public event _allTutorialFinished AllTutorialFinished;
    public event _allMainMenuUnlocked AllMainMenuUnlocked;

    public void TriggerAllMainMenuUnlocked() {
        ChangeState("IsTutorialFinished", true);

        if(AllMainMenuUnlocked != null) AllMainMenuUnlocked.Invoke();
        AccountManager.Instance.RequestShopItems();
    }

    /// <summary>
    /// q1 : 휴먼 0-1 강제 플레이 (AddNewbiController)
    /// q2 : 오크 0-1 강제 플레이
    /// q3 : 휴먼 0-2 강제 플레이
    /// q4 : 오크 0-2 강제 플레이
    /// q5 : 퀘스트 습득하기
    /// t0 : 우편 받기 유도 퀘스트
    /// t2 : 카드 제작하기
    /// t3 : 부대 편집하기
    /// t4 : 리그 대전 진행하기
    /// t5 : 계정연동
    /// etc : 닉네임 변경
    /// </summary>
    /// <returns>현재 Milestone</returns>
    public TutorialMilestone GetCurrentMilestone() {
        string prevMilestone = PlayerPrefs.GetString("TutorialMilestone", null);
        var convertedPrevMilestone = dataModules.JsonReader.Read<TutorialMilestone>(prevMilestone);
        //TutorialMilestone milestone = new TutorialMilestone();
        //if ((int)convertedPrevMilestone.milestoneType < 7) {
        //    MilestoneName nextName = (MilestoneName)(convertedPrevMilestone.milestoneType + 1);
        //    milestone.name = nextName;
        //    if ((int)nextName > 2) milestone.milestoneType = MilestoneType.QUEST;
        //    else milestone.milestoneType = MilestoneType.TUTORIAL;
        //}
        //else {
        //    milestone.name = MilestoneName.END;
        //}
        return convertedPrevMilestone;
    }

    public void SetMilestone(MilestoneType type, MenuTutorialManager.TutorialType tutorialName) {
        TutorialMilestone milestone = new TutorialMilestone();
        milestone.milestoneType = type;
        milestone.name = tutorialName;

        PlayerPrefs.SetString("TutorialMilestone", JsonUtility.ToJson(milestone));
    }

    public class TutorialMilestone {
        public MilestoneType milestoneType;
        public MenuTutorialManager.TutorialType name;
    }

    public enum MilestoneType {
        TUTORIAL,
        QUEST
    }
}
