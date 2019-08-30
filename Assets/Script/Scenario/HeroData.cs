using UnityEngine;

public class HeroData : ScenarioData {
    public string args;
    public override void OnClicked() {
        scenarioManager.heroID = args;
    }
}
