using System;
using UnityEngine;
using Tutorial;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx.Async;
using UnityEngine.UI;

public class StageButton : ScenarioButton {
    public int chapter;
    public int stage;
    public string camp;
    public bool isTutorial;
    public string description;
    public string stageName;
    public string enemyHeroId;
    public ScenarioManager ScenarioManager;

    private bool isHuman;
    public ChapterData chapterData;
    public int requireLevel;

    private GameObject lockerObject;
    private TextMeshProUGUI mainMessage, middleMessage, endMessage;

    public void Init(ChapterData data, bool isHuman, ScenarioManager scenarioManager, int requireLevel) {
        lockerObject = transform.Find("Locker").gameObject;
        mainMessage = lockerObject.transform.Find("Message").GetComponent<TextMeshProUGUI>();
        middleMessage = mainMessage.transform.Find("MiddleMessage").GetComponent<TextMeshProUGUI>();
        endMessage = middleMessage.transform.Find("EndMessage").GetComponent<TextMeshProUGUI>();
        
        chapter = data.chapter;
        stage = data.stage_number;
        description = data.description;
        stageName = data.stage_Name;
        chapterData = data;
        enemyHeroId = data.enemyHeroId;
        this.scenarioManager = scenarioManager;
        this.isHuman = isHuman;
        this.requireLevel = requireLevel;
        
        isTutorial = chapter < 1;
        
        if (isHuman) camp = "human";
        else camp = "orc";
        
        if(chapter > 0) CheckLockOrUnlock();
        else CheckTutorilalUnlock();
    }

    public void CheckTutorilalUnlock() {
        lockerObject.SetActive(true);
        GetComponent<Button>().enabled = false;
        
        var clearedStageList = AccountManager.Instance.clearedStages;
        if(stage == 1) Unlock();
        else {
            int prevTutorialStageNumber = 0;
            string prevTutorialCamp = String.Empty;
            
            if (camp == "human") {
                if (stage == 2) {
                    prevTutorialStageNumber = 1;
                    prevTutorialCamp = "orc";
                }
            }
            else {
                if (stage == 2) {
                    prevTutorialStageNumber = 2;
                    prevTutorialCamp = "human";
                }
            }
            
            bool isPrevStageCleared = clearedStageList.Exists(x => 
                x.camp == prevTutorialCamp &&
                (x.chapterNumber == 0 || x.chapterNumber == null) && 
                x.stageNumber == prevTutorialStageNumber
            );
            
            if(isPrevStageCleared) Unlock();
            else {
                mainMessage.text = "?????? ??????????????? ?????????????????????.";
                middleMessage.text = string.Empty;
                endMessage.text = String.Empty;
            }
        }
    }

    public void CheckLockOrUnlock() {
        lockerObject.SetActive(true);
        GetComponent<Button>().enabled = false;
        var clearedStageList = AccountManager.Instance.clearedStages;
        var myLv = AccountManager.Instance.userData.lv;
        
        int prevChapter = chapter;
        if (stage == 1) prevChapter = chapter - 1;
        
        int prevStage = stage;
        if (stage == 1) {
            prevStage = clearedStageList
                .FindAll(x => x.camp == camp && x.chapterNumber == prevChapter)
                .OrderBy(x => x.stageNumber)
                .Last().stageNumber;
        }
        else prevStage -= 1;

        //????????? ?????? ??????????
        bool isPrevStageCleared = clearedStageList.Exists(x => 
            x.camp == camp &&
            (x.chapterNumber == prevChapter || x.chapterNumber == null && prevChapter == 0) && 
            x.stageNumber == prevStage
        );
        //????????? ?????????????
        bool isLevelQualified = myLv >= requireLevel;

        if (!isLevelQualified) {
            //????????? ????????? ?????? ?????? ??????
            mainMessage.text = "?????? ?????? ";
            middleMessage.text = requireLevel.ToString();
            endMessage.text = " ??????";
        }
        else{
            if(!isPrevStageCleared) { 
                //????????? ????????? ?????? ???????????? ?????? ??????????????? ?????? ??????
                mainMessage.text = "?????? ??????????????? ?????????????????????.";
                middleMessage.text = string.Empty;
                endMessage.text = String.Empty;
            }
        }

        if(isLevelQualified && isPrevStageCleared) {
            transform.Find("Alert").gameObject.SetActive(true);
            Unlock();
        }
    }

    public void Unlock() {
        lockerObject.SetActive(false);
        GetComponent<Button>().enabled = true;
    }

    public override void OnClicked() {
        List<ChapterData> list;
        scenarioManager = ScenarioManager.Instance;

        if (camp == "human") {
            list = scenarioManager.human_chapterDatas;
        }
        else {
            list = scenarioManager.orc_chapterDatas;
        }

        scenarioManager.selectedChapterData = list
            .Find(x => x.chapter == chapter && x.stage_number == stage);
        scenarioManager.SelectChallengeData(chapter, stage, camp);
        scenarioManager.selectedChapterObject = gameObject;
        
        PlayerPrefs.SetString("StageNum", (stage).ToString());
        PlayerPrefs.SetString("ChapterNum", chapter.ToString());
        
        PlayerPrefs.SetString("SelectedRace", camp);
        scenarioManager.OnClickStage();
    }
}
