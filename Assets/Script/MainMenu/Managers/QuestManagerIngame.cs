using Quest;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestManagerIngame : QuestManager {
    protected override void OnEnable() {
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_QUEST_UPDATED, ShowQuest);
        AccountManager.Instance.RequestQuestInfo();

        QuestCanvas.SetActive(true);
        SwitchPanel(0);
    }

    public override void SwitchPanel(int page) {
        for (int i = 0; i < 2; i++) {
            if (i == 1) continue;
            QuestCanvas.transform.Find("InnerCanvas/MainPanel").GetChild(i).gameObject.SetActive(i == page);
            header.GetChild(i).GetComponent<Button>().interactable = !(i == page);
        }
    }
}
