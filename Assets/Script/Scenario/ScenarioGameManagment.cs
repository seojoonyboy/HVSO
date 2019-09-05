using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Tutorial;

public class ScenarioGameManagment : PlayMangement
{
    public static ChapterData chapterData;
    public static ScenarioGameManagment scenarioInstance;

    private void Awake() {
        instance = this;
        scenarioInstance = this;
        SetWorldScale();
        SetPlayerCard();
        GetComponent<TurnMachine>().onTurnChanged.AddListener(ChangeTurn);
        GetComponent<TurnMachine>().onPrepareTurn.AddListener(DistributeCard);
        SetCamera();
    }

    private void Start() {
        SetBackGround();
    }

    private void OnDestroy() {
        instance = null;
        scenarioInstance = null;
    }

}


public class MissionRequire  {
    public string args;
    public bool isCheck;
    public GameObject targetObject;   
}
