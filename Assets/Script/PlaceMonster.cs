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

    private int x;
    private int y;
    private GameObject myTarget;

    public Vector3 unitLocation;
    public bool attacking = false;
    public int atkCount = 1;
    public int atkRange = 1;

    protected delegate void TimeUpdate(float time);
    protected TimeUpdate timeUpdate;
    protected UnitSpine unitSpine;

    private float currentTime;

    public float atkTime {
        get { return unitSpine.atkDuration; }
    }

    public enum UnitState {
        APPEAR,
        IDLE,
        ATTACK,
        HIT,
        DEAD
    };
    
    public void Init() {
        x = transform.parent.GetSiblingIndex();
        y = transform.parent.parent.GetSiblingIndex();

        unitLocation = gameObject.transform.position;
        UpdateStat();

        Observable.EveryUpdate().Where(_ => attacking == true).Subscribe(_ => MoveToTarget()).AddTo(this);
        
        unitSpine = transform.Find("skeleton").GetComponent<UnitSpine>();
        Observable.EveryUpdate().Where(_ => attacking == false && gameObject.transform.position != unitLocation).Delay(System.TimeSpan.FromSeconds(unitSpine.atkDuration)).Subscribe(_ => ReturnPosition()).AddTo(this);
    }

    public void SpawnUnit() {
        SetState(UnitState.APPEAR);
    }


    public void AttackMonster() {
        if (isPlayer == true) {
            PlayerController enemy = PlayMangement.instance.enemyPlayer;
            if (enemy.transform.Find("Line_2").GetChild(x).childCount != 0) {
                myTarget = enemy.transform.Find("Line_2").GetChild(x).GetChild(0).gameObject;
            }
            else if (enemy.transform.Find("Line_1").GetChild(x).childCount != 0) {
                myTarget = enemy.transform.Find("Line_1").GetChild(x).GetChild(0).gameObject;
            }
            else {
                myTarget = enemy.transform.gameObject;
            }
            attacking = true;
        }
        else {
            PlayerController player = PlayMangement.instance.player;
            if (player.transform.Find("Line_2").GetChild(x).childCount != 0) {
                myTarget = player.transform.Find("Line_2").GetChild(x).GetChild(0).gameObject;
            }
            else if (player.transform.Find("Line_1").GetChild(x).childCount != 0) {
                myTarget = player.transform.Find("Line_1").GetChild(x).GetChild(0).gameObject;
            }
            else {
                myTarget = player.transform.gameObject;                
            }
            attacking = true;
        }
    }

    private void SingleAttack() {

    }

    private void MultipleAttack() {

    }



    public void RequestAttackUnit(GameObject target, int amount) {
        target.GetComponent<PlaceMonster>().unit.HP -= amount;
        target.GetComponent<PlaceMonster>().UpdateStat();
        target.GetComponent<PlaceMonster>().SetState(UnitState.HIT);
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
        transform.Translate((new Vector3(0,myTarget.transform.position.y) - new Vector3(0,gameObject.transform.position.y)).normalized * 30f * Time.deltaTime, Space.Self);
        //iTween.MoveTo(gameObject, iTween.Hash("x", gameObject.transform.position.x, "y", myTarget.transform.position.y, "z", gameObject.transform.position.z, "time", 0.5f, "easetype", iTween.EaseType.easeInOutBack));
        

        if (Vector3.Distance(new Vector3 (0,transform.position.y), new Vector3(0,myTarget.transform.position.y)) < 0.5f) {
            attacking = false;
            PlaceMonster placeMonster = myTarget.GetComponent<PlaceMonster>();
            SetState(UnitState.ATTACK);

            if (unit.power > 0) {
                if (unit.power <= 4)
                    SoundManager.Instance.PlaySound(SoundType.NORMAL_ATTACK);
                else if (unit.power > 4)
                    SoundManager.Instance.PlaySound(SoundType.LARGE_ATTACK);

                if (placeMonster != null) {
                    RequestAttackUnit(myTarget, unit.power);
                }
                else
                    myTarget.GetComponent<PlayerController>().PlayerTakeDamage(unit.power);
            }
        }
    }

    private void ReturnPosition() {
        gameObject.transform.position = Vector3.Lerp(transform.position, unitLocation, 30f * Time.deltaTime);

    }

    public void CheckHP() {
        if ( unit.HP <= 0 ) {
            StartCoroutine("WaitMonsterDead");
        }
    }

    IEnumerator WaitMonsterDead() {
        yield return new WaitForSeconds(0.1f);
        Destroy(gameObject);
    }

    protected void SetState(UnitState state) {
        timeUpdate = null;
        currentTime = 0f;

        switch (state) {
            case UnitState.APPEAR:
                unitSpine.Appear();
                break;
            case UnitState.IDLE:
                unitSpine.Idle();
                break;
            case UnitState.ATTACK:
                unitSpine.Attack();
                break;
            case UnitState.HIT:
                unitSpine.Hit();
                break;
            case UnitState.DEAD:
                break;
        }
    }


}
