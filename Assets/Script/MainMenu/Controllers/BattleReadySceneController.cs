using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleReadySceneController : MonoBehaviour {
    public Toggle[] battleTypeToggles, raceTypeToggles;

    BattleType selectedBattleType;
    RaceType raceType;

    void Start() {
        battleTypeToggles[0].isOn = true;
        raceTypeToggles[0].isOn = true;
    }

    public void OnStartButton() {
        SceneManager.Instance.LoadScene(SceneManager.Scene.MISSION_INGAME);
    }

    public void OnBackButton() {
        SceneManager.Instance.LoadScene(SceneManager.Scene.MAIN_SCENE);
    }

    public void OnDeckListButton() {
        SceneManager.Instance.LoadScene(SceneManager.Scene.DECK_LIST_SCNE);
    }

    public void ChangeBattleType(BattleType type) {
        selectedBattleType = type;
    }

    public void ChangeRaceType(RaceType type) {
        raceType = type;
    }

    public enum BattleType {
        AI = 0,
        CASUAL = 1,
        RANK = 2
    }

    public enum RaceType {
        HUMAN = 0,
        ORC = 1
    }
}
