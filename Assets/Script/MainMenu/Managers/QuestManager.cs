using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using dataModules;
using System;

public class QuestManager : MonoBehaviour
{
    [SerializeField] GameObject QuestCanvas;
    [SerializeField] Transform content;
    [SerializeField] HUDController HUDController;

    public GameObject handSpinePrefab;
    private List<QuestContentController> quests;
    private QuestData[] dataSamples;

    public void OpenQuestCanvas() {
        HUDController.SetHeader(HUDController.Type.RESOURCE_ONLY_WITH_BACKBUTTON);
        HUDController.SetBackButton(OnBackBtnClicked);

        EscapeKeyController.escapeKeyCtrl.AddEscape(OnBackBtnClicked);
        QuestCanvas.SetActive(true);
    }

    void OnBackBtnClicked() {
        SoundManager.Instance.PlaySound(UISfxSound.BUTTON1);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(OnBackBtnClicked);
        HUDController.SetHeader(HUDController.Type.SHOW_USER_INFO);
        QuestCanvas.SetActive(false);
    }

    private void Awake() {
        quests = new List<QuestContentController>();
        content.GetComponentsInChildren<QuestContentController>(true, quests);
        Debug.Log(quests.Count);
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_DECKS_UPDATED, ShowQuestSample);
    }

    public void AddQuest(QuestData data) {
        QuestContentController quest = quests.Find(x=>!x.gameObject.activeSelf);
        quest.data = data;
        quest.manager = this;
        quest.gameObject.SetActive(true);
        if(quest.data.tutorials == null) return;
        quest.ActiveTutorial();
    }

    private void ShowQuestSample(Enum type, Component Sender, object Param) {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_DECKS_UPDATED, ShowQuestSample);
        StartCoroutine(Adding());
    }

    private IEnumerator Adding() {
        yield return null;
        string data = ((TextAsset)Resources.Load("TutorialDatas/questData")).text;
        dataSamples = JsonReader.Read<QuestData[]>(data);
        AddQuest(dataSamples[0]);
    }

    public void AddSecondQuest() {
        AddQuest(dataSamples[1]);
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