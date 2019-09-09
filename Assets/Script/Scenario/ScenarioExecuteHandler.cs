using System;
using System.Collections;
using System.Collections.Generic;
using Tutorial;
using UnityEngine;
using System.Linq;

public class ScenarioExecuteHandler : MonoBehaviour {
    public List<ScenarioExecute> sets;
    public bool isExecute;
    public bool isDone = true;

    public void Initialize(ScriptData data) {
        ScriptData temp = data;
        StartCoroutine(MethodExecute(temp));        
    }

    IEnumerator MethodExecute(ScriptData data) {
        sets = new List<ScenarioExecute>();

        foreach (Method method in data.methods) {
            ScenarioExecute exec = (ScenarioExecute)gameObject.AddComponent(Type.GetType(method.name));
            sets.Add(exec);
            exec.Initialize(method.args);
        }
        yield return SkillTrigger();
    }

    
    IEnumerator SkillTrigger() {
        foreach(ScenarioExecute execute in sets) {
            isDone = false;
            execute.Execute();
            yield return new WaitUntil(() => isDone);
        }
        isExecute = true;
    }
}
