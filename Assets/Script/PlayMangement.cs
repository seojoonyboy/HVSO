using Bolt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayMangement : MonoBehaviour
{
    public PlayerController player, enemyPlayer;

    public GameObject card, back;
    public Sprite plantResourceIcon, zombieResourceIcon;
    public Sprite humanBtn, orcBtn;

    public GameObject cardDB;
    Camera cam;
    public GameObject uiSlot;

    public bool isGame = true;
    public static PlayMangement instance { get; private set; }

    private void Awake()
    {
        //string selectedRace = Variables.Saved.Get("SelectedRace").ToString();


        instance = this;        
        SetPlayerCard();
        gameObject.GetComponent<TurnChanger>().onTurnChanged.AddListener(() => ChangeTurn());
        gameObject.GetComponent<TurnChanger>().onPrepareTurn.AddListener(() => DistributeCard());
    }
    private void OnDestroy()
    {
        instance = null; 
    }    

    private void Start()
    {
        cam = Camera.main;        
        RequestStartData();
        SetPlayerSlot();
        DistributeResource();

    }    

    public void SetPlayerCard() {
        if (player.race == true) {
            player.card = cardDB.transform.Find("Card").gameObject;
            player.back = cardDB.transform.Find("HumanBackCard").gameObject;
            enemyPlayer.card = cardDB.transform.Find("Card").gameObject; ;
            enemyPlayer.back = cardDB.transform.Find("OrcBackCard").gameObject;
        }
        else {
            player.card = cardDB.transform.Find("Card").gameObject;
            player.back = cardDB.transform.Find("OrcBackCard").gameObject;
            enemyPlayer.card = cardDB.transform.Find("Card").gameObject;
            enemyPlayer.back = cardDB.transform.Find("HumanBackCard").gameObject;
        }
    }

    public void DistributeCard() {
        StartCoroutine(player.GenerateCard());
    }
    

    public void SetPlayerSlot() {
        for (int i = 0; i < player.transform.childCount; i++) {
            for (int j = 0; j < player.transform.GetChild(i).childCount; j++) {
                GameObject slot = Instantiate(uiSlot);
                slot.transform.SetParent(player.playerUI.transform.parent.Find("IngamePanel").Find("PlayerSlot").GetChild(i));
                slot.transform.position = cam.WorldToScreenPoint(player.transform.GetChild(i).GetChild(j).position);
            }
        }
    }

    public void RequestStartData() {
        player.SetPlayerStat(20);
        enemyPlayer.SetPlayerStat(20);
        
    }

    public void DistributeResource() {
        player.resource.Value  += 2;
        enemyPlayer.resource.Value  += 2;
    }


    IEnumerator EnemySummonMonster() {
        int i = 0;
        CardData cardData;
        CardDataPackage cardDataPackage = Resources.Load("CardDatas/CardDataPackage_01") as CardDataPackage;
        string cardID;
        int enemyCardCount = enemyPlayer.playerUI.transform.Find("CardSlot").childCount;


        if (enemyPlayer.race == false)
            cardID = "ac10014";
        else
            cardID = "ac10009";


        while (i < enemyCardCount) {
            if (enemyCardCount < 1) break;
            if (i >= 3) break;
            if (enemyPlayer.transform.GetChild(0).GetChild(i).childCount != 0) { i++; continue; }
            if (cardDataPackage.data.ContainsKey(cardID) == false) { i++; continue; }
            

            yield return new WaitForSeconds(0.5f);
            cardData = cardDataPackage.data[cardID];

            GameObject monster = Instantiate(enemyPlayer.card.GetComponent<CardHandler>().unit);
            monster.transform.SetParent(enemyPlayer.transform.GetChild(0).GetChild(i));
            monster.transform.position = enemyPlayer.transform.GetChild(0).GetChild(i).position;

            monster.GetComponent<PlaceMonster>().unit.HP = (int)cardData.hp;
            monster.GetComponent<PlaceMonster>().unit.power = (int)cardData.attack;
            monster.GetComponent<PlaceMonster>().unit.name = cardData.name;
            monster.GetComponent<PlaceMonster>().unit.type = cardData.type;
            monster.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprite/" + cardID);

            Destroy(enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(0).gameObject);
            i++;
        }

        yield return new WaitForSeconds(1f);
        enemyPlayer.ReleaseTurn();
        StopCoroutine("EnemySummonMonster");
    }

    public void ChangeTurn() {
        string currentTurn = Variables.Scene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene()
            ).Get("CurrentTurn").ToString();

        switch (currentTurn) {
            case "ZOMBIE":
                if(player.race == false) {
                    player.ActivePlayer();
                    enemyPlayer.DisablePlayer();
                }
                else {
                    player.DisablePlayer();
                    enemyPlayer.ActivePlayer();
                    StartCoroutine("EnemySummonMonster");                    
                }
                break;

            case "PLANT":
                if(player.race == true) {
                    player.ActivePlayer();
                    enemyPlayer.DisablePlayer();
                }
                else {
                    player.DisablePlayer();
                    enemyPlayer.ActivePlayer();
                    StartCoroutine("EnemySummonMonster");
                }
                break;

            case "SECRET":
                if (player.race == false) {
                    player.ActivePlayer();
                    enemyPlayer.DisablePlayer();
                }
                else {
                    player.DisablePlayer();
                    StartCoroutine("WaitSecond");
                }                
                break;
            case "BATTLE":
                StartBattle();
                break;
        }
    }
    
    public void StartBattle() {
        StartCoroutine("battleCoroutine");
    }

    public IEnumerator WaitSecond() {
        yield return new WaitForSeconds(2f);
        CustomEvent.Trigger(gameObject, "EndTurn");
    }

    public void GetPlayerTurnRelease() {
        CustomEvent.Trigger(gameObject, "EndTurn");
    }


    IEnumerator battleCoroutine() {
        int line = 0;
        while (line < 5) {
            if (player.race == false) {
                if (player.transform.Find("Line_1").GetChild(line).childCount != 0) {
                    player.transform.Find("Line_1").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().AttackMonster();
                    yield return new WaitForSeconds(1f);
                }

                if (player.transform.Find("Line_2").GetChild(line).childCount != 0) {
                    player.transform.Find("Line_2").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().AttackMonster();
                    yield return new WaitForSeconds(1f);
                }

                if (enemyPlayer.transform.Find("Line_1").GetChild(line).childCount != 0) {
                    enemyPlayer.transform.Find("Line_1").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().AttackMonster();
                    yield return new WaitForSeconds(1f);
                }

                if (enemyPlayer.transform.Find("Line_2").GetChild(line).childCount != 0) {
                    enemyPlayer.transform.Find("Line_2").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().AttackMonster();
                    yield return new WaitForSeconds(1f);
                }
            }

            else {
                if (enemyPlayer.transform.Find("Line_1").GetChild(line).childCount != 0) {
                    enemyPlayer.transform.Find("Line_1").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().AttackMonster();
                    yield return new WaitForSeconds(1f);
                }

                if (enemyPlayer.transform.Find("Line_2").GetChild(line).childCount != 0) {
                    enemyPlayer.transform.Find("Line_2").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().AttackMonster();
                    yield return new WaitForSeconds(1f);
                }
                if (player.transform.Find("Line_1").GetChild(line).childCount != 0) {
                    player.transform.Find("Line_1").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().AttackMonster();
                    yield return new WaitForSeconds(1f);
                }

                if (player.transform.Find("Line_2").GetChild(line).childCount != 0) {
                    player.transform.Find("Line_2").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().AttackMonster();
                    yield return new WaitForSeconds(1f);
                }
            }

            if (player.transform.Find("Line_1").GetChild(line).childCount != 0) 
                player.transform.Find("Line_1").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().CheckHP();  
            if (player.transform.Find("Line_2").GetChild(line).childCount != 0) 
                player.transform.Find("Line_2").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().CheckHP();
            if (enemyPlayer.transform.Find("Line_1").GetChild(line).childCount != 0) 
                enemyPlayer.transform.Find("Line_1").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().CheckHP();
            if (enemyPlayer.transform.Find("Line_2").GetChild(line).childCount != 0) 
                enemyPlayer.transform.Find("Line_2").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().CheckHP();



            line++;
        }
        CustomEvent.Trigger(gameObject, "EndTurn");
        StopCoroutine("battleCoroutine");
    }
}
