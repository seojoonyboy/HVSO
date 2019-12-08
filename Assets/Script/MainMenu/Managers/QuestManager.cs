using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using dataModules;
using System;

public class QuestManager : MonoBehaviour
{
    [SerializeField] Transform content;
    private List<QuestContentController> quests;


    private void Awake() {
        quests = new List<QuestContentController>();
        content.GetComponentsInChildren<QuestContentController>(true, quests);
        Debug.Log(quests.Count);
    }

    public void AddQuest(string data) {
        QuestContentController quest = quests.Find(x=>!x.gameObject.activeSelf);
        quest.data = JsonReader.Read<QuestData>(data);
        quest.gameObject.SetActive(true);
        if(quest.data.tutorials == null) return;
        quest.ActiveTutorial();
    }
}

[Serializable] public class QuestData {
    public string title;
    public string info;
    public int reachNum;
    public int currentNum;
    public bool isClear = false;
    public TutorialShowList[] tutorials;
}

public class TutorialShowList {
    public string method;
    public string[] args;
}