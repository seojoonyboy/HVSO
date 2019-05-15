using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Monster;
using TMPro;


public class PlaceMonster : MonoBehaviour
{
    public Unit unit;
    public bool isPlayer;
    public GameObject atkPrefab;

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
        if (isPlayer == true) {
            PlayerController enemy = PlayMangement.instance.enemyPlayer;
            GameObject target;
            GameObject shoot = Instantiate(atkPrefab);

            if (enemy.transform.Find("Line_2").GetChild(x).childCount != 0) {
                target = enemy.transform.Find("Line_2").GetChild(x).GetChild(0).gameObject;
                target.GetComponent<PlaceMonster>().unit.HP -= unit.power;               
            }
            else if (enemy.transform.Find("Line_1").GetChild(x).childCount != 0) {
                target = enemy.transform.Find("Line_1").GetChild(x).GetChild(0).gameObject;
                target.GetComponent<PlaceMonster>().unit.HP -= unit.power;
            }
            else {
                target = enemy.transform.gameObject;
                enemy.PlayerTakeDamage(unit.power);
            }

            shoot.transform.position = target.transform.position;


        }
        else {
            PlayerController player = PlayMangement.instance.player;
            GameObject target;
            GameObject shoot = Instantiate(atkPrefab);

            if (player.transform.Find("Line_2").GetChild(x).childCount != 0) {
                target = player.transform.Find("Line_2").GetChild(x).GetChild(0).gameObject;
                target.GetComponent<PlaceMonster>().unit.HP -= unit.power;
            }
            else if (player.transform.Find("Line_1").GetChild(x).childCount != 0) {
                target = player.transform.Find("Line_1").GetChild(x).GetChild(0).gameObject;
                target.GetComponent<PlaceMonster>().unit.HP -= unit.power;
            }
            else {
                target = player.transform.gameObject;
                player.PlayerTakeDamage(unit.power);
            }

            shoot.transform.position = target.transform.position;
        }
    }





}
