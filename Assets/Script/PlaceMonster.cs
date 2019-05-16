using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Monster;
using TMPro;


public class PlaceMonster : MonoBehaviour
{
    public Monster.Unit unit;
    public bool isPlayer;
    public GameObject atkPrefab;

    private int x;
    private int y;
    private GameObject myTarget;

    public Vector3 unitLocation;
    public bool attacking = false;
    public float count = 0f;

    private void Start()
    {
        x = transform.parent.GetSiblingIndex();
        y = transform.parent.parent.GetSiblingIndex();

        unitLocation = gameObject.transform.position;
        UpdateStat();

        Observable.EveryUpdate().Where(_ => isPlayer== true  && attacking == true).Subscribe(_ => MoveToTarget()).AddTo(this);
        Observable.EveryUpdate().Where(_ => isPlayer == true && attacking == false && gameObject.transform.position != unitLocation).Subscribe(_ => ReturnPosition()).AddTo(this);

    }



    public void AttackMonster() {
        if (isPlayer == true) {
            PlayerController enemy = PlayMangement.instance.enemyPlayer;

            if (enemy.transform.Find("Line_2").GetChild(x).childCount != 0) {
                myTarget = enemy.transform.Find("Line_2").GetChild(x).GetChild(0).gameObject;
                RequestAttackUnit(myTarget, unit.power);
            }
            else if (enemy.transform.Find("Line_1").GetChild(x).childCount != 0) {
                myTarget = enemy.transform.Find("Line_1").GetChild(x).GetChild(0).gameObject;
                RequestAttackUnit(myTarget, unit.power);
            }
            else {
                myTarget = enemy.transform.gameObject;
                enemy.PlayerTakeDamage(unit.power);
            }
            
        }
        else {
            PlayerController player = PlayMangement.instance.player;

            if (player.transform.Find("Line_2").GetChild(x).childCount != 0) {
                myTarget = player.transform.Find("Line_2").GetChild(x).GetChild(0).gameObject;
                RequestAttackUnit(myTarget, unit.power);
            }
            else if (player.transform.Find("Line_1").GetChild(x).childCount != 0) {
                myTarget = player.transform.Find("Line_1").GetChild(x).GetChild(0).gameObject;
                RequestAttackUnit(myTarget, unit.power);
            }
            else {
                myTarget = player.transform.gameObject;
                player.PlayerTakeDamage(unit.power);
            }
        }
    }

    public void RequestAttackUnit(GameObject target, int amount) {
        attacking = true;
        target.GetComponent<PlaceMonster>().unit.HP -= amount;
        target.GetComponent<PlaceMonster>().UpdateStat();
    }

    public void RequestChangePower(int amount) {
        unit.power += amount;
        UpdateStat();
    }
    
    public void UpdateStat() {
        if (unit.HP > 0)
            transform.Find("HP").GetComponentInChildren<TextMeshPro>().text = unit.HP.ToString();
        else
            transform.Find("HP").GetComponentInChildren<TextMeshPro>().text = 0.ToString();
        transform.Find("ATK").GetComponentInChildren<TextMeshPro>().text = unit.power.ToString();
    }

    private void MoveToTarget() {
        transform.Translate((myTarget.transform.position - gameObject.transform.position).normalized * 5f * Time.deltaTime, Space.Self);

        if (transform.position.y >= myTarget.transform.position.y - 1f)
            attacking = false;
    }

    private void ReturnPosition() {
        gameObject.transform.position = Vector3.Lerp(transform.position, unitLocation, 2f * Time.deltaTime);

    }

}
