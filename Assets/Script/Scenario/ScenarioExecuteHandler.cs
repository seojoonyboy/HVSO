using System;
using System.Collections;
using System.Collections.Generic;
using Tutorial;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Text;

public class ScenarioExecuteHandler : MonoBehaviour {
    public List<ScenarioExecute> sets;
    public bool isDone = true;
    IEnumerator coroutine;
    public void Initialize(ScriptData data) {
        ScriptData temp = data;
        StartCoroutine(MethodExecute(temp));
    }

    public void RollBack(int index) {
        StopAllCoroutines();

        List<ScenarioExecute> lists = sets.GetRange(index, sets.Count - 1);
        StartCoroutine(RollbackedSkillTrigger(lists));
    }

    IEnumerator MethodExecute(ScriptData data) {
        foreach(var exec in sets) { Destroy(exec); }
        sets = new List<ScenarioExecute>();

        foreach (Method method in data.methods) {
            ScenarioExecute exec = (ScenarioExecute)gameObject.AddComponent(Type.GetType(method.name));
            if(exec == null) { Logger.LogError(method.name + "에 대한 클래스를 찾을 수 없습니다!"); }
            sets.Add(exec);
            exec.Initialize(method.args);
        }
        coroutine = SkillTrigger();
        yield return coroutine;
    }

    IEnumerator RollbackedSkillTrigger(List<ScenarioExecute> list) {
        foreach(ScenarioExecute execute in list) {
            isDone = false;
            execute.Execute();
            ScenarioGameManagment.scenarioInstance.currentExecute = execute;
#if UNITY_EDITOR
            ShowDebugText(execute);
#endif
            yield return new WaitUntil(() => isDone);
        }
        GetComponent<ScenarioGameManagment>().canNextChapter = true;
    }
    
    IEnumerator SkillTrigger() {
        foreach(ScenarioExecute execute in sets) {
            isDone = false;
            execute.Execute();
            ScenarioGameManagment.scenarioInstance.currentExecute = execute;
#if UNITY_EDITOR
            ShowDebugText(execute);
#endif
            yield return new WaitUntil(() => isDone);
        }
        GetComponent<ScenarioGameManagment>().canNextChapter = true;
    }

    private void ShowDebugText(ScenarioExecute execute) {
        StringBuilder sb = new StringBuilder();
        sb.Append(execute.GetType() + "\n");
        foreach(var arg in execute.args) {
            sb.Append("Arg : " + arg);
        }
        execute.scenarioMask.transform.parent.Find("DebugText").GetComponent<Text>().text = sb.ToString();
    }
}
