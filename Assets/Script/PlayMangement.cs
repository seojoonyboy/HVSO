using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayMangement : MonoBehaviour
{
    public PlayerController player, enemyPlayer;

    public GameObject plantCard, plantBack;
    public GameObject zombieCard, zombieBack;
    public Sprite plantResourceIcon, zombieResourceIcon;


    Camera cam;
    public GameObject uiSlot;
    private int turn = 0;

    public static PlayMangement instance { get; private set; }

    private void Awake()
    {
        instance = this;
        player.card = (player.race == true) ? plantCard : zombieCard;
        enemyPlayer.card = (enemyPlayer.race == true) ? plantCard : zombieCard;
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
        player.resource.Value = turn + 2;
        enemyPlayer.resource.Value = turn + 2;
    }


    public void NextTurn() {
        
        for(int i = 0; i < enemyPlayer.playerUI.transform.Find("CardSlot").childCount; i++) {
            if (enemyPlayer.transform.GetChild(0).GetChild(i).childCount != 0) continue;

            GameObject monster = (enemyPlayer.race == true) ? Instantiate(plantCard.GetComponent<CardHandler>().unit) : Instantiate(zombieCard.GetComponent<CardHandler>().unit);
            monster.transform.SetParent(enemyPlayer.transform.GetChild(0).GetChild(i));
            monster.transform.position = enemyPlayer.transform.GetChild(0).GetChild(i).position;
        }
    }
    
    public void StartBattle() {
        StartCoroutine("battleCoroutine");
    }

    IEnumerator battleCoroutine() {
        int line = 0;
        while(line < 5) {
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
    }
}
