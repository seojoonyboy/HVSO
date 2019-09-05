using UnityEngine;
using Tutorial;
using System.Collections.Generic;

public class StageButton : ScenarioButton {
    public int chapter;
    public int stage;
    public string camp;

    public void Init(int chapter, int stage) {
        this.chapter = chapter;
        this.stage = stage;
    }

    public override void OnClicked() {
        List<ChapterData> list;
        if(camp == "human") {
            list = scenarioManager.human_chapterDatas;
        }
        else {
            list = scenarioManager.orc_chapterDatas;
        }

        scenarioManager.selectedChapterData = list
            .Find(x => x.chapter == chapter && x.stage_number == stage);

        scenarioManager.OnClickStage(scenarioManager.selectedChapterData);
    }
}
