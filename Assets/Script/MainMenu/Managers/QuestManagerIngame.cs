using Quest;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestManagerIngame : QuestManager {
    protected override void OnBackBtnClicked() { }

    public override void OpenQuestCanvas() {
        AccountManager.Instance.RequestQuestInfo();
        OpenWindow(windowList.Find("QuestPanel").gameObject);
        QuestCanvas.SetActive(true);
        SwitchPanel(0);
        showNewIcon(false);
    }
}
