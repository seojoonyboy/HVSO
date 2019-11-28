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
    public GameObject TutorialStageReadyCanvas;
    public GameObject BoxRewardPanel;
    public GameObject FixedMenuCanvas;
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
        rewardPanel.SetActive(false);
    }

    public void ActiveTutorialStoryReadyCanvas(string camp) {
        TutorialStageReadyCanvas.SetActive(true);
        Transform stagePanel = TutorialStageReadyCanvas.transform.Find("HUD/StagePanel");
        switch (camp) {
            case "human":
                stagePanel.Find("HumanBack").gameObject.SetActive(true);
                stagePanel.Find("OrcBack").gameObject.SetActive(false);
                break;
            case "orc":
                stagePanel.Find("HumanBack").gameObject.SetActive(false);
                stagePanel.Find("OrcBack").gameObject.SetActive(true);
                break;
        }

        stagePanel
            .transform
            .Find("TextGroup/StageScript")
            .gameObject
            .GetComponent<Text>().text = ScenarioGameManagment.chapterData.description;
    }

    public void DeactiveTutorialStoryReadyCanvas() {
        TutorialStageReadyCanvas.SetActive(false);
    }

    public void ActiveRewardBoxCanvas() {
        BoxRewardPanel.GetComponent<BoxRewardManager>().OpenBox();
    }

    public void OnMainPageChanged() {
        int pageNum = scrollSnap.CurrentPage;
        OnMenuDescPanel(pageNum);
    }

    public void OnMenuDescPanel(int index) {

    }

    public enum TutorialType {
        TO_ORC_STORY = 0,
        TO_HUMAN_STORY_2 = 1,
        TO_ORC_STORY_2 = 2,
        UNLOCK_STORY_AND_BATTLE_MENU = 3,
        MAIN_BUTTON_DESC = 4,
        NONE = 5
    }
}
