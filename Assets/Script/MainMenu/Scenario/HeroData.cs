using UnityEngine;
using Tutorial;

public class HeroData : ScenarioButton {
    public string args;
    public override void OnClicked() {
        scenarioManager.heroID = args;
    }
}
