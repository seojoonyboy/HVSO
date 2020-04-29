using System;
using System.Collections;
using System.Collections.Generic;
using dataModules;
using Tutorial;
using UnityEngine;

public class NewAlertStoryListener : NewAlertListenerBase {
    public override void AddListener() {
        eventHandler.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_CLEARED_STAGE_UPDATED, CheckCondition);
    }

    public override void RemoveListener() {
        eventHandler.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_CLEARED_STAGE_UPDATED, CheckCondition);
    }
    
    private void CheckCondition(Enum event_type, Component sender, object param) {
        var clearedStages = AccountManager.Instance.clearedStages;
        
        string dataAsJson = ((TextAsset)Resources.Load("TutorialDatas/HumanChapterDatas")).text;
        var human_chapterDatas = JsonReader.Read<List<ChapterData>>(dataAsJson);

        dataAsJson = ((TextAsset)Resources.Load("TutorialDatas/OrcChapterDatas")).text;
        var orc_chapterDatas = JsonReader.Read<List<ChapterData>>(dataAsJson);

        foreach (var clearedStage in clearedStages) {
            if (clearedStage.camp == "human") {
                int chapter = clearedStage.chapterNumber ?? 0;
                var item = human_chapterDatas.Find(x =>
                    x.chapter == chapter && x.stage_number == clearedStage.stageNumber);
                if (item != null) human_chapterDatas.Remove(item);
            }

            else {
                int chapter = clearedStage.chapterNumber ?? 0;
                var item = orc_chapterDatas.Find(x =>
                    x.chapter == chapter && x.stage_number == clearedStage.stageNumber);
                if (item != null) orc_chapterDatas.Remove(item);
            }
        }
        
        NewAlertManager alertManager = NewAlertManager.Instance;
        string conditionToRemoveAlert = String.Empty;

        int userLv = (int)AccountManager.Instance.userData.lv;
        human_chapterDatas.RemoveAll(x => x.require_level > userLv);
        orc_chapterDatas.RemoveAll(x => x.require_level > userLv);
        
        bool isHumanLeft = human_chapterDatas.Count > 0;
        bool isOrcLeft = orc_chapterDatas.Count > 0;

        if (isHumanLeft && isOrcLeft) conditionToRemoveAlert = "Both";
        else if (isHumanLeft && !isOrcLeft) {
            conditionToRemoveAlert = "Human";
        }
        else if (!isHumanLeft && isOrcLeft) {
            conditionToRemoveAlert = "Orc";
        }

        if (string.IsNullOrEmpty(conditionToRemoveAlert)) {
            alertManager.DisableButtonToAlert(
                gameObject,
                NewAlertManager.ButtonName.CHAPTER
            );
        }
        else {
            if (isHumanLeft && isOrcLeft) {
                alertManager.SetUpButtonToAlert(
                    gameObject,
                    NewAlertManager.ButtonName.CHAPTER, 
                    false
                );

                alertManager.SetUpButtonToUnlockCondition(
                    NewAlertManager.ButtonName.CHAPTER, 
                    conditionToRemoveAlert
                );    
            }
            else {
                alertManager.DisableButtonToAlert(
                    gameObject,
                    NewAlertManager.ButtonName.CHAPTER
                );
            }
        }
    }
}