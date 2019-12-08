using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using MenuTutorialModules;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using dataModules;

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

    public MenuSceneController menuSceneController;

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

    public void OnMainPageChanged() { }

    /// <summary>
    /// 퀘스트 중간에 등장하는 강제 부분 처리
    /// </summary>
    /// <param name="type">Type</param>
    public void StartQuestSubSet(TutorialType type) {
        if ((int)type <= 10) return;

        int arr_index = -1;
        switch (type) {
            case TutorialType.QUEST_SUB_SET_1:
                arr_index = 2;
                break;
            case TutorialType.QUEST_SUB_SET_2:
                arr_index = 3;
                break;
            case TutorialType.QUEST_SUB_SET_3:
                arr_index = 4;
                break;
            case TutorialType.QUEST_SUB_SET_4:
                arr_index = 5;
                break;
            case TutorialType.QUEST_SUB_SET_5:
                arr_index = 6;
                break;
            case TutorialType.QUEST_SUB_SET_6:
                arr_index = 7;
                break;
        }

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

    public enum TutorialType {
        TO_ORC_STORY = 0,
        UNLOCK_TOTAL_STORY = 1,
        QUEST_SUB_SET_1 = 11,
        QUEST_SUB_SET_2 = 14,
        QUEST_SUB_SET_3 = 15,
        QUEST_SUB_SET_4 = 16,
        QUEST_SUB_SET_5 = 17,
        QUEST_SUB_SET_6 = 18,
        NONE = 99
    }
}
