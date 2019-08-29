using UnityEngine;

public class StageData : ScenarioData {
    public string args;

    public override void OnClicked() {
        scenarioManager.selectStage = args;
    }
}
