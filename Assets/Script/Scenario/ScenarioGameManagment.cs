using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ScenarioGameManagment : PlayMangement
{
    public static ChapterData chapterData;

    private void Awake() {
        instance = this;       
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
    }

}


public class MissionRequire  {
    public string args;
    public bool isCheck;
    public GameObject targetObject;   
}
