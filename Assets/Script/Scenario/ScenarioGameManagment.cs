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

    Type thisType;

    bool canNextChapter = true;
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
            EndTutorial();
            return;
        }
        currentChapterData = chapterQueue.Dequeue();
        AnalysisChapterData(currentChapterData);
    }

    /// <summary>
    /// reflection을 통한 scriptData 처리
    /// </summary>
    /// <param name="data"></param>
    private void AnalysisChapterData(ScriptData data) {
        List<IEnumerator> funcs = new List<IEnumerator>();
        foreach (Method method in data.methods) {
            var _method = thisType.GetMethod(method.name);
            object[] args = new object[] { method.args };
            IEnumerator func = (IEnumerator)_method.Invoke(this, args);
            funcs.Add(func);
        }
        StartCoroutine(ExecuteAnalizedChapterData(funcs));
    }

    /// <summary>
    /// 튜토리얼의 한 단위 실행
    /// </summary>
    /// <param name="data">한 단위에서 수행되는 함수들</param>
    /// <returns></returns>
    IEnumerator ExecuteAnalizedChapterData(List<IEnumerator> data) {
        foreach(IEnumerator func in data) {
            yield return func;
        }
        canNextChapter = true;
        Debug.Log("=============== 다음 튜토리얼 스크립트 그룹 실행 ==============");
    }

    private void EndTutorial() {
        Logger.Log("=============== 튜토리얼 끝! ===============");
    }

    /// <summary>
    /// 일정 시간 대기
    /// </summary>
    /// <param name="args">시간(초)</param>
    /// <returns></returns>
    public IEnumerator wait_until(List<string> args) {
        Logger.Log("wait_until");
        var parms = args;
        float sec = 0;
        float.TryParse(parms[0], out sec);
        yield return new WaitForSeconds(sec);
        Logger.Log("wait_until End");
    }

    /// <summary>
    /// 설명 텍스트 출력
    /// </summary>
    /// <param name="args">내용</param>
    /// <returns></returns>
    public IEnumerator print_message(List<string> args) {
        Logger.Log("print_message");
        yield return new WaitForSeconds(5.0f);
        Logger.Log("print_message End");
    }

    /// <summary>
    /// 하이라이트 처리
    /// </summary>
    /// <param name="args">1.타겟 형태 2.세부 대상 정보</param>
    /// <returns></returns>
    public IEnumerator highlight(List<string> args) {
        Logger.Log("highlight");
        yield return new WaitForSeconds(5.0f);
        Logger.Log("highlight End");
    }

    /// <summary>
    /// 타겟이 클릭되기 전까지 대기
    /// </summary>
    /// <param name="args">타겟</param>
    /// <returns></returns>
    public IEnumerator wait_click(List<string> args) {
        Logger.Log("wait_click");
        yield return new WaitForSeconds(5.0f);
        //TODO : waitUntil 적용
        Logger.Log("wait_click End");
    }
}


public class MissionRequire  {
    public string args;
    public bool isCheck;
    public GameObject targetObject;   
}
