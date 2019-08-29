using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioManager : MonoBehaviour
{
    public static ScenarioManager Instance { get; private set; }
    public ShowSelectRace human, orc;
    public string heroID;
    public string selectStage;
    public bool isHuman;

    private void Awake() {
        Instance = this;
        OnHumanButton();
    }

    private void OnDestroy() {
        Instance = null;
    }   
    

    public void OnBackButton() {
        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
        FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);
    }

    public void OnHumanButton() {
        orc.raceButton.GetComponent<Image>().sprite = orc.deactiveSprite;
        orc.heroSelect.SetActive(false);
        orc.StageCanvas.SetActive(false);

        heroID = "";
        selectStage = "";

        human.raceButton.GetComponent<Image>().sprite = human.activeSprite;
        human.heroSelect.SetActive(true);
        human.StageCanvas.SetActive(true);
        isHuman = true;
    }
    
    public void OnOrcButton() {
        human.raceButton.GetComponent<Image>().sprite = human.deactiveSprite;
        human.heroSelect.SetActive(false);
        human.StageCanvas.SetActive(false);

        heroID = "";
        selectStage = "";

        orc.raceButton.GetComponent<Image>().sprite = orc.activeSprite;
        orc.heroSelect.SetActive(true);
        orc.StageCanvas.SetActive(true);
        isHuman = false;
    }



}

[System.Serializable]
public class ShowSelectRace {
    public GameObject raceButton;
    public Sprite activeSprite;
    public Sprite deactiveSprite;
    public GameObject heroSelect;
    public GameObject StageCanvas;
    public GameObject heroContent;
    public GameObject stageContent;
}

public class ScenarioData : MonoBehaviour {
    protected ScenarioManager scenarioManager;

    private void Start() {
        scenarioManager = ScenarioManager.Instance;
    }

    public virtual void OnClicked() {
        return;
    }
}