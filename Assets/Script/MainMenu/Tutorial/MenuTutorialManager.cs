using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using MenuTutorialModules;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using dataModules;
using System;

public class MenuTutorialManager : SerializedMonoBehaviour {
    [FilePath] public string tutorialSetsPath;

    public List<TutorialSet> sets;
    [HideInInspector] public TutorialSet selectedTutorialData;
    [ShowInInspector] MenuExecuteHandler executeHandler;
    public UnityEngine.UI.Extensions.HorizontalScrollSnap scrollSnap;

    public GameObject mainDescCanvas;    //한 이미지로 표현하는 튜토리얼 parent 객체

    public GameObject HUDCanvas, BattleReadydeckListPanel;

    public ScenarioManager scenarioManager;
    public GameObject handUIPools;
    public GameObject rewardPanel;  //튜토리얼 중간에 보상받기 패널
    public GameObject BoxRewardPanel;
    public GameObject FixedMenuCanvas;
    public GameObject battleMenuCanvas;
    public GameObject menuTextCanvas;
    public GameObject deckEditWindow;   //부대편집 메인화면
    public GameObject deckEditCanvas;   //부대편집 화면

    public Quest.QuestManager questManager;

    public MenuSceneController menuSceneController;
    public MainSceneStateHandler MainSceneStateHandler;
    public MenuLockController lockController;
    public MailBoxManager MailBoxManager;

    public void ReadTutorialData() {
        string dataAsJson = ((TextAsset)Resources.Load("TutorialDatas/TutorialDatas")).text;
        sets = JsonReader.Read<List<TutorialSet>>(dataAsJson);
    }

    /// <summary>
    /// 튜토리얼 시작
    /// </summary>
    /// <param name="type">튜토리얼 종류</param>
    public void StartTutorial(TutorialType type) {
        Logger.Log(type + " 튜토리얼 시작");

        if(type != TutorialType.NONE) {
            AccountManager.Instance.RequestUserInfo();
        }

        var execs = GetComponents<MenuExecute>();
        if (execs != null) {
            foreach (var exec in (execs)) {
                Destroy(exec);
            }
        }
        
        selectedTutorialData = sets[(int)type];
        if(executeHandler == null) executeHandler = gameObject.AddComponent<MenuExecuteHandler>();

        if (executeHandler == null) return;
        executeHandler.Initialize(selectedTutorialData);
    }

    /// <summary>
    /// (현재)튜토리얼 종료
    /// </summary>
    public void EndTutorial() {
        if (executeHandler == null) return;
        Destroy(executeHandler);
    }

    public class TutorialSet {
        public int id;
        [MultiLineProperty(10)] public string description;
        public List<InnerSet> innerDatas;
    }

    public class InnerSet {
        public int id;
        [MultiLineProperty(5)] public string description;

        public List<Method> methods;
        public bool isExecute;
    }

    public class Method {
        public string name;
        public List<string> args;
    }

    /// <summary>
    /// 보상받기 패널
    /// </summary>
    public void ActiveRewardPanel() {
        rewardPanel.SetActive(true);
    }

    public void DeactiveRewardPanel() {
        rewardPanel.transform.Find("SubBackground").gameObject.SetActive(true);
        rewardPanel.SetActive(false);
    }

    public void ActiveRewardBoxCanvas() {
        BoxRewardPanel.GetComponent<BoxRewardManager>().OpenBox();
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
    public void StartQuestSubSet(TutorialType type) {
        int arr_index = -1;
        switch (type) {
            case TutorialType.Q2:
                arr_index = 0;
                break;
            case TutorialType.Q3:
                arr_index = 1;
                break;
            case TutorialType.Q4:
                arr_index = 2;
                break;
            case TutorialType.Q5:
                arr_index = 3;
                break;
            case TutorialType.t0:
                arr_index = 4;
                break;
            case TutorialType.t2:
                arr_index = 5;
                break;
            case TutorialType.t3:
                arr_index = 6;
                break;
            case TutorialType.t4:
                arr_index = 7;
                break;
            case TutorialType.SUB_SET_100:
                arr_index = 8;
                break;
            case TutorialType.SUB_SET_101:
                arr_index = 9;
                break;
        }

        if (arr_index == -1) return;

        try {
            var selectedSets = sets[arr_index];

            var execs = GetComponents<MenuExecute>();
            if (execs != null) {
                foreach (var exec in (execs)) {
                    Destroy(exec);
                }
            }

            if (executeHandler == null) executeHandler = gameObject.AddComponent<MenuExecuteHandler>();

            if (executeHandler == null) return;
            executeHandler.Initialize(selectedSets);
        }
        catch (Exception ex) {
            Logger.LogError("Index : " + arr_index);
        }
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
    public enum TutorialType {
        Q2 = 0,
        Q3 = 1,
        Q4 = 2,
        Q5 = 3,
        t0 = 4,
        t2 = 5,
        t3 = 6,
        t4 = 7,
        NONE = 99,
        SUB_SET_100 = 100,
        SUB_SET_101 = 101
    }
}
