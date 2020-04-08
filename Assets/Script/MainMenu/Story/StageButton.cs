using UnityEngine;
using Tutorial;
using System.Collections.Generic;

public class StageButton : ScenarioButton {
    public int chapter;
    public int stage;
    public string camp;
    public bool isTutorial;
    public string description;
    public string stageName;

    public ChapterData chapterData;
    public void Init(ChapterData data, bool isHuman) {
        chapter = data.chapter;
        stage = data.stage_number;
        description = data.description;
        stageName = data.stage_Name;
        chapterData = data;

        if (isHuman) camp = "human";
        else camp = "orc";
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
