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
        
        if(chapter == 0) Unlock();
    }

    public void Lock() {
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

        //이전꺼 진행 했는지?
        bool isPrevStageCleared = clearedStageList.Exists(x => 
            x.camp == camp &&
            (x.chapterNumber == prevChapter || x.chapterNumber == null && prevChapter == 0) && 
            x.stageNumber == prevStage
        );
        //레벨이 안되는지?
        bool isLevelQualified = myLv >= requireLevel;

        if (!isLevelQualified) {
            //레벨이 충족이 되지 않는 경우
            mainMessage.text = "계정 레벨 ";
            middleMessage.text = requireLevel.ToString();
            endMessage.text = " 필요";
        }
        else{
            if(!isPrevStageCleared) { 
                //레벨은 되지만 이전 스테이지 아직 클리어하지 않은 경우
                mainMessage.text = "이전 스테이지를 클리어해주세요.";
                middleMessage.text = string.Empty;
                endMessage.text = String.Empty;
            }
        }

        if(isLevelQualified && isPrevStageCleared) Unlock();
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
        PlayerPrefs.SetString("SelectedRace", camp);
        scenarioManager.OnClickStage();
    }
}
