using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Monster;
using TMPro;


public class PlaceMonster : MonoBehaviour
{
    public Unit unit;
    public bool isPlayer;
    public int x;
    public int y;

    private void Start()
    {
        x = transform.parent.GetSiblingIndex();
        y = transform.parent.parent.GetSiblingIndex();

        transform.Find("HP").GetComponentInChildren<TextMeshPro>().text = unit.HP.ToString();
        transform.Find("ATK").GetComponentInChildren<TextMeshPro>().text = unit.power.ToString();        
    }

    public void AttackMonster() {
        if(isPlayer == true) {
            PlayerController enemy = PlayMangement.instance.enemyPlayer;

            if (enemy.transform.Find("PlaceMentLine_2").GetChild(x).childCount != 0)
                enemy.transform.Find("PlaceMentLine_2").GetChild(x).GetChild(0).GetComponent<PlaceMonster>().unit.HP -= unit.power;
            else if (enemy.transform.Find("PlaceMentLine_1").GetChild(x).childCount != 0)
                enemy.transform.Find("PlaceMentLine_1").GetChild(x).GetChild(0).GetComponent<PlaceMonster>().unit.HP -= unit.power;
            else
                enemy.HP -= unit.power;
        }
        else {
            PlayerController player = PlayMangement.instance.player;

            if (player.transform.Find("PlaceMentLine_2").GetChild(x).childCount != 0)
                player.transform.Find("PlaceMentLine_2").GetChild(x).GetChild(0).GetComponent<PlaceMonster>().unit.HP -= unit.power;
            else if (player.transform.Find("PlaceMentLine_1").GetChild(x).childCount != 0)
                player.transform.Find("PlaceMentLine_1").GetChild(x).GetChild(0).GetComponent<PlaceMonster>().unit.HP -= unit.power;
            else
                player.HP -= unit.power;
        }

    }





}
