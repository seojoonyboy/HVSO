using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuTutorialManager : SerializedMonoBehaviour {
    public TutorialType currentTutorial;
    public List<TutorialSet> sets;
    [HideInInspector] public TutorialSet selectedTutorialData;
    MenuExecuteHandler executeHandler;

    /// <summary>
    /// 튜토리얼 시작
    /// </summary>
    /// <param name="type">튜토리얼 종류</param>
    public void StartTutorial(TutorialType type) {
        selectedTutorialData = sets[(int)type];
        currentTutorial = type;
        switch (type) {
            case TutorialType.TO_ORC_STORY:
                executeHandler = gameObject.AddComponent<ToOrcStoryExecuteHandler>();
                break;
            case TutorialType.TO_AI_BATTLE:
                executeHandler = gameObject.AddComponent<ToAIBattleExecuteHandler>();
                break;
            case TutorialType.TO_BOX_OPEN:
                executeHandler = gameObject.AddComponent<ToBoxOpenExecuteHandler>();
                break;
        }

        if (executeHandler == null) return;
        executeHandler.Initialize();
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

    public enum TutorialType {
        TO_ORC_STORY = 0,
        TO_AI_BATTLE = 1,
        TO_BOX_OPEN = 2
    }
}
