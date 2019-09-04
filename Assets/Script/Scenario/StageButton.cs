using UnityEngine;

public class StageButton : ScenarioButton {
    public int chapter;
    public int stage;

    public override void OnClicked() {
        scenarioManager.chapter = chapter;
        scenarioManager.stage_number = stage;
        scenarioManager.OnClickStage();
    }
}
