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

    public override void DistributeCard() {
        StartCoroutine(GenerateCard());
    }

    public override IEnumerator GenerateCard() {
        int i = 0;
        while (i < 4) {
            yield return new WaitForSeconds(0.3f);
            if (i < 4)
                StartCoroutine(player.cdpm.FirstDraw());

            GameObject enemyCard;
            if (enemyPlayer.isHuman)
                enemyCard = Instantiate(Resources.Load("Prefabs/HumanBackCard") as GameObject, enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(CountEnemyCard()));
            else
                enemyCard = Instantiate(Resources.Load("Prefabs/OrcBackCard") as GameObject, enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(CountEnemyCard()));
            enemyCard.transform.position = player.cdpm.cardSpawnPos.position;
            enemyCard.transform.localScale = new Vector3(1, 1, 1);
            iTween.MoveTo(enemyCard, enemyCard.transform.parent.position, 0.3f);
            enemyCard.SetActive(true);
            i++;
        }
    }

}


public class MissionRequire  {
    public string args;
    public bool isCheck;
    public GameObject targetObject;   
}
