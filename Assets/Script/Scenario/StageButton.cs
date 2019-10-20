using UnityEngine;
using Tutorial;
using System.Collections.Generic;

public class StageButton : ScenarioButton {
    public int chapter;
    public int stage;
    public string camp;
    public bool isTutorial;

    public void Init(int chapter, int stage, bool isHuman) {
        this.chapter = chapter;
        this.stage = stage;
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
        
        PlayerPrefs.SetString("StageNum", (chapter + 1).ToString());
        scenarioManager.OnClickStage(scenarioManager.selectedChapterData, isTutorial);
    }
}
