using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuExecuteHandler : MonoBehaviour {
    MenuTutorialManager tutorialManager;
    Queue<MenuTutorialManager.InnerSet> setQueue;
    MenuTutorialManager.InnerSet currentSet;
    List<MenuExecute> executes;

    public bool canNextChapter = true;
    public bool isDone = true;
    IEnumerator coroutine;

    void Start() {
        tutorialManager = GetComponent<MenuTutorialManager>();
    }

    public virtual void Initialize(MenuTutorialManager.TutorialSet set) {
        setQueue = new Queue<MenuTutorialManager.InnerSet>();
        foreach(MenuTutorialManager.InnerSet InnerSet in set.innerDatas) {
            setQueue.Enqueue(InnerSet);
        }
    }

    void FixedUpdate() {
        if (!canNextChapter) return;
        DequeueChapter();
    }

    protected void DequeueChapter() {
        canNextChapter = false;
        if (setQueue.Count == 0) {
            tutorialManager.EndTutorial();
            return;
        }
        currentSet = setQueue.Dequeue();
        StartCurrentSet();
    }

    public void StartCurrentSet() {
        StartCoroutine(MethodExecute());
    }

    IEnumerator MethodExecute() {
        foreach(var exec in executes) { Destroy(exec); }
        executes = new List<MenuExecute>();

        foreach (MenuTutorialManager.Method method in currentSet.methods) {
            MenuExecute exec = (MenuExecute)gameObject.AddComponent(Type.GetType(method.name));
            if(exec == null) {
                Logger.LogError(method.name + "에 대한 클래스를 찾을 수 없습니다.");
                continue;
            }
            exec.Initialize(method.args);
        }
        coroutine = SkillTrigger();
        yield return coroutine;
    }

    IEnumerator SkillTrigger() {
        foreach(MenuExecute exec in executes) {
            isDone = false;
            exec.Execute();

            yield return new WaitUntil(() => isDone);
        }
        canNextChapter = true;
    }
}

public class ToOrcStoryExecuteHandler : MenuExecuteHandler { }
public class ToAIBattleExecuteHandler : MenuExecuteHandler { }
public class ToBoxOpenExecuteHandler : MenuExecuteHandler { }