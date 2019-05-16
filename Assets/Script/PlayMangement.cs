using Bolt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayMangement : MonoBehaviour
{
    public PlayerController player, enemyPlayer;

    public GameObject card, back;
    public GameObject plant, zombie;
    public Sprite plantResourceIcon, zombieResourceIcon;

    public GameObject cardDB;
    Camera cam;
    public GameObject uiSlot;

    public bool isGame = true;
    public static PlayMangement instance { get; private set; }

    private void Awake()
    {
        instance = this;        
        SetPlayerCard();
        gameObject.GetComponent<TurnChanger>().onTurnChanged.AddListener(() => ChangeTurn());
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
            player.card = cardDB.transform.Find("Plant").gameObject;
            player.back = cardDB.transform.Find("PlantsBackCard").gameObject;
            enemyPlayer.card = cardDB.transform.Find("Zombie").gameObject;
            enemyPlayer.back = cardDB.transform.Find("ZombieBackCard").gameObject;
        }
        else {
            player.card = cardDB.transform.Find("Zombie").gameObject;
            player.back = cardDB.transform.Find("ZombieBackCard").gameObject;
            enemyPlayer.card = cardDB.transform.Find("Plant").gameObject;
            enemyPlayer.back = cardDB.transform.Find("PlantsBackCard").gameObject;
        }
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
        StartCoroutine(player.GenerateCard());
    }

    public void DistributeResource() {
        player.resource.Value  += 2;
        enemyPlayer.resource.Value  += 2;
    }


    IEnumerator EnemySummonMonster() {
        int i = 0;        
        while(i < 4) {
            yield return new WaitForSeconds(0.5f);
            if (enemyPlayer.transform.GetChild(0).GetChild(i).childCount != 0) { i++; continue; }
            GameObject monster = Instantiate(enemyPlayer.card.GetComponent<CardHandler>().unit);
            monster.transform.SetParent(enemyPlayer.transform.GetChild(0).GetChild(i));
            monster.transform.position = enemyPlayer.transform.GetChild(0).GetChild(i).position;
            i++;            
        }
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

    IEnumerator WaitSecond() {
        yield return new WaitForSeconds(2f);
        CustomEvent.Trigger(gameObject, "EndTurn");
        StopCoroutine("WaitSecond");
    }

    public void GetPlayerTurnRelease() {
        CustomEvent.Trigger(gameObject, "EndTurn");
    }


    IEnumerator battleCoroutine() {
        int line = 0;
        while (line < 5) {
            if(player.transform.Find("Line_1").GetChild(line).childCount != 0) {
                player.transform.Find("Line_1").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().AttackMonster();
                yield return new WaitForSeconds(0.5f);
            }

            if(player.transform.Find("Line_2").GetChild(line).childCount != 0) {
                player.transform.Find("Line_2").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().AttackMonster();
                yield return new WaitForSeconds(0.5f);
            }

            if (enemyPlayer.transform.Find("Line_1").GetChild(line).childCount != 0) {
                enemyPlayer.transform.Find("Line_1").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().AttackMonster();
                yield return new WaitForSeconds(0.5f);
            }

            if (enemyPlayer.transform.Find("Line_2").GetChild(line).childCount != 0) {
                enemyPlayer.transform.Find("Line_2").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().AttackMonster();
                yield return new WaitForSeconds(0.5f);
            }
            line++;
        }
        line = 0;
        CustomEvent.Trigger(gameObject, "EndTurn");
        StopCoroutine("battleCoroutine");
    }
}
