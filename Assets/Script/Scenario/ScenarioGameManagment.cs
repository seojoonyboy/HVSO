using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Tutorial;
using System.Reflection;
using System;

public class ScenarioGameManagment : PlayMangement {
    public static ChapterData chapterData;

    Queue<ScriptData> chapterQueue;
    ScriptData currentChapterData;
    Method currentMethod;
    public static ScenarioGameManagment scenarioInstance;
    public bool isTutorial;

    Type thisType;
    public bool canNextChapter = true;

    private void Awake() {
        instance = this;
        scenarioInstance = this;
        SetWorldScale();
        SetPlayerCard();
        GetComponent<TurnMachine>().onTurnChanged.AddListener(ChangeTurn);
        GetComponent<TurnMachine>().onPrepareTurn.AddListener(DistributeCard);
        SetCamera();

        thisType = GetType();
        if (!InitQueue()) Logger.LogError("chapterData가 제대로 세팅되어있지 않습니다!");
    }

    private bool InitQueue() {
        if (chapterData == null) return false;

        chapterQueue = new Queue<ScriptData>();
        foreach (ScriptData scriptData in chapterData.scripts) {
            chapterQueue.Enqueue(scriptData);
        }
        StartCoroutine(ChapterScript());
        ScenarioMask.Instance.gameObject.SetActive(true);
        return true;
    }

    void Start() {
        SetBackGround();
    }

    void OnDestroy() {
        instance = null;
        scenarioInstance = null;
    }

    void FixedUpdate() {
        if (!canNextChapter) return;
        DequeueChapter();
    }

    private void DequeueChapter() {
        canNextChapter = false;
        if(chapterQueue.Count == 0) {
            //EndTutorial();
            return;
        }
        currentChapterData = chapterQueue.Dequeue();
        GetComponent<ScenarioExecuteHandler>().Initialize(currentChapterData);
    }

    IEnumerator ChapterScript() {
        while(chapterQueue.Count > 0) {
            while(chapterQueue.Peek().isExecute == false) {
                DequeueChapter();                
            }
        }
        yield return null;
    }

    //IEnumerator ExecuteMethod(int methodNum) {
    //    ScenarioExecute dataExecute = (ScenarioExecute)Activator.CreateInstance(Type.GetType(chapterQueue.Peek().methods[methodNum].name));
    //    dataExecute.args = chapterQueue.Peek().methods[methodNum].args;
    //    dataExecute.Execute();     
    //    yield return new WaitUntil(() => dataExecute.handler.isDone == true);
    //}


}


public class MissionRequire  {
    public string args;
    public bool isCheck;
    public GameObject targetObject;   
}
