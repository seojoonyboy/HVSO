using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ScenarioGameManagment : PlayMangement
{
    public Queue<MissionRequire> missionData;

    public GameObject human_BackGround;
    public GameObject orc_BackGround;

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

    protected override void SetBackGround() {
        if (player.isHuman) {
            GameObject raceSprite = Instantiate(human_BackGround, backGround.transform);
            raceSprite.transform.SetAsLastSibling();
        }
        else {
            GameObject raceSprite = Instantiate(orc_BackGround, backGround.transform);
            raceSprite.transform.SetAsLastSibling();
        }
    }

}


public class MissionRequire  {
    public string args;
    public bool isCheck;
    public GameObject targetObject;   
}
