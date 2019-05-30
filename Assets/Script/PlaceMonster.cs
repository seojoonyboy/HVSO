using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using IngameClass;
using TMPro;


public class PlaceMonster : MonoBehaviour
{
    public IngameClass.Unit unit;
    public bool isPlayer;

    public int x { get; private set; }
    public int y { get; private set; }
    public GameObject myTarget;

    public Vector3 unitLocation;
    public int atkCount = 1;
    public int atkRange = 1;

    protected delegate void TimeUpdate(float time);
    protected TimeUpdate timeUpdate;
    protected UnitSpine unitSpine;

    private float currentTime;
    List<Buff> buffList = new List<Buff>();

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

        //Observable.EveryUpdate().Where(_ => attacking == true && unit.power > 0).Subscribe(_ => MoveToTarget()).AddTo(this);
        
        unitSpine = transform.Find("skeleton").GetComponent<UnitSpine>();
        unitSpine.attackCallback += SuccessAttack;

        if(unit.attackRange == "distance") {
            GameObject arrow = Instantiate(unitSpine.arrow, transform);
            arrow.transform.position = gameObject.transform.position;
            arrow.name = "arrow";
            arrow.SetActive(false);
        }

        if(isPlayer == true) 
            unit.race = (PlayMangement.instance.player.race == true ) ? true : false;        
        else
            unit.race = (PlayMangement.instance.enemyPlayer.race == true) ? true : false;


    }

    public void SpawnUnit() {
        SetState(UnitState.APPEAR);
    }


    public void GetTarget() {
        if (isPlayer == true) {
            PlayerController enemy = PlayMangement.instance.enemyPlayer;
            if (enemy.frontLine.transform.GetChild(x).childCount != 0) {
                myTarget = enemy.frontLine.transform.GetChild(x).GetChild(0).gameObject;
            }
            else if (enemy.backLine.transform.GetChild(x).childCount != 0) {
                myTarget = enemy.backLine.transform.GetChild(x).GetChild(0).gameObject;
            }
            else {
                myTarget = enemy.transform.gameObject;
            }
            MoveToTarget();
        }
        else {
            PlayerController player = PlayMangement.instance.player;
            if (player.frontLine.transform.GetChild(x).childCount != 0) {
                myTarget = player.frontLine.transform.GetChild(x).GetChild(0).gameObject;
            }
            else if (player.backLine.transform.GetChild(x).childCount != 0) {
                myTarget = player.backLine.transform.GetChild(x).GetChild(0).gameObject;
            }
            else {
                myTarget = player.transform.gameObject;                
            }
            MoveToTarget();
        }
    }

    public void UnitTryAttack() {
        if (unit.attack <= 0) return;        
        SetState(UnitState.ATTACK);        
    }

    public void SuccessAttack() {

        if (unit.attackRange == "distance") {
            GameObject arrow = transform.Find("arrow").gameObject;
            arrow.transform.position = transform.position;
            arrow.SetActive(true);
            PlaceMonster placeMonster = myTarget.GetComponent<PlaceMonster>();

            if (placeMonster != null)
                iTween.MoveTo(arrow, iTween.Hash("x", gameObject.transform.position.x, "y", myTarget.transform.position.y, "z", gameObject.transform.position.z, "time", 0.2f, "easetype", iTween.EaseType.easeOutExpo, "oncomplete", "SingleAttack", "oncompletetarget", gameObject));
            else
                iTween.MoveTo(arrow, iTween.Hash("x", gameObject.transform.position.x, "y", myTarget.GetComponent<PlayerController>().wallPosition.y, "z", gameObject.transform.position.z, "time", 0.2f, "easetype", iTween.EaseType.easeOutExpo, "oncomplete", "SingleAttack", "oncompletetarget", gameObject));

        }            
        else
            SingleAttack();
    }


    public void SingleAttack() {
        PlaceMonster placeMonster = myTarget.GetComponent<PlaceMonster>();


        if (unit.attack > 0) {         
            if (placeMonster != null) {
                RequestAttackUnit(myTarget, unit.attack);
            }
            else
                myTarget.GetComponent<PlayerController>().PlayerTakeDamage(unit.attack);

            if (unit.attack <= 3) {
                GameObject effect = Instantiate(PlayMangement.instance.effectManager.lowAttackEffect);
                effect.transform.position = (placeMonster != null) ? myTarget.transform.position : new Vector3(gameObject.transform.position.x, myTarget.GetComponent<PlayerController>().wallPosition.y, 0);
                Destroy(effect, effect.GetComponent<ParticleSystem>().main.duration - 0.2f);
                StartCoroutine(PlayMangement.instance.cameraShake(unitSpine.atkDuration / 2));
                SoundManager.Instance.PlaySound(SoundType.NORMAL_ATTACK);
            }
            else if (unit.attack > 4) {
                GameObject effect =  (unit.attack < 6)  ? Instantiate(PlayMangement.instance.effectManager.middileAttackEffect) : Instantiate(PlayMangement.instance.effectManager.highAttackEffect);
                effect.transform.position = (placeMonster != null) ? myTarget.transform.position : new Vector3(gameObject.transform.position.x, myTarget.GetComponent<PlayerController>().wallPosition.y, 0);
                Destroy(effect, effect.GetComponent<ParticleSystem>().main.duration - 0.2f);
                StartCoroutine(PlayMangement.instance.cameraShake(unitSpine.atkDuration / 2));

                if(unit.attack > 4 && unit.attack <= 6) {
                    SoundManager.Instance.PlaySound(SoundType.MIDDLE_ATTACK);
                }
                if(unit.attack > 6) {
                    SoundManager.Instance.PlaySound(SoundType.LARGE_ATTACK);
                }
            }
        }


        if (unit.attackRange == "distance") {
            GameObject arrow = transform.Find("arrow").gameObject;
            arrow.transform.position = transform.position;
            arrow.SetActive(false);
        }
        else {
            ReturnPosition();
            if (placeMonster != null)
                myTarget.GetComponent<PlaceMonster>().ReturnPosition();
        }

    }

    public void MultipleAttack() {

    }



    public void RequestAttackUnit(GameObject target, int amount) {
        target.GetComponent<PlaceMonster>().unit.currentHP -= amount;
        target.GetComponent<PlaceMonster>().UpdateStat();
        target.GetComponent<PlaceMonster>().SetState(UnitState.HIT);
    }

    public void RequestChangePower(int amount) {
        unit.attack += amount;
        UpdateStat();
    }

    public void RequestChangeHp(int amount) {
        unit.currentHP += amount;
        UpdateStat();
    }

    public void RequestChangeStat(int power = 0, int hp = 0) {
        unit.attack += power;
        unit.currentHP += hp;
        UpdateStat();
    }

    public void AddBuff(Buff buff) {
        buffList.Add(buff);
        RequestChangeStat(buff.atk, buff.hp);
    }

    public void RemoveBuff(GameObject origin) {
        var selectBuff = buffList.Find(x => x.origin == origin);
        if (selectBuff == null) return;
        RequestChangeStat(-selectBuff.atk, -selectBuff.hp);
    }

    public void UpdateStat() {
        if (unit.currentHP > 0)
            transform.Find("HP").GetComponentInChildren<TextMeshPro>().text = unit.currentHP.ToString();
        else
            transform.Find("HP").GetComponentInChildren<TextMeshPro>().text = 0.ToString();
        transform.Find("ATK").GetComponentInChildren<TextMeshPro>().text = unit.attack.ToString();
    }

    private void MoveToTarget() {
        if (unit.attack <= 0) return;
        PlaceMonster placeMonster = myTarget.GetComponent<PlaceMonster>();

        if (unit.attackRange == "distance")
            UnitTryAttack();        
        else {
            if (placeMonster != null) {
                if (isPlayer == false) {
                    iTween.MoveTo(gameObject, iTween.Hash("x", gameObject.transform.position.x + 0.3f, "y", myTarget.transform.position.y, "z", gameObject.transform.position.z, "time", 0.3f, "easetype", iTween.EaseType.easeInOutExpo, "oncomplete", "UnitTryAttack", "oncompletetarget", gameObject));
                    iTween.MoveTo(myTarget, iTween.Hash("x", myTarget.transform.position.x - 0.3f, "y", myTarget.transform.position.y, "z", myTarget.transform.position.z, "time", 0.2f, "easetype", iTween.EaseType.easeInOutExpo));
                }
                else {                    
                    iTween.MoveTo(gameObject, iTween.Hash("x", gameObject.transform.position.x - 0.3f, "y", myTarget.transform.position.y, "z", gameObject.transform.position.z, "time", 0.3f, "easetype", iTween.EaseType.easeInOutExpo, "oncomplete", "UnitTryAttack", "oncompletetarget", gameObject));
                    iTween.MoveTo(myTarget, iTween.Hash("x", myTarget.transform.position.x + 0.3f, "y", myTarget.transform.position.y, "z", myTarget.transform.position.z, "time", 0.2f, "easetype", iTween.EaseType.easeInOutExpo));
                }
            }
            else {
                iTween.MoveTo(gameObject, iTween.Hash("x", gameObject.transform.position.x, "y", myTarget.GetComponent<PlayerController>().unitClosePosition.y, "z", gameObject.transform.position.z, "time", 0.3f, "easetype", iTween.EaseType.easeInOutExpo, "oncomplete", "UnitTryAttack", "oncompletetarget", gameObject));
            }
        }
    }

    


    private void ReturnPosition() {
        iTween.MoveTo(gameObject, iTween.Hash("x", unitLocation.x, "y", unitLocation.y, "z", unitLocation.z, "time", 0.3f, "delay", 0.5f, "easetype", iTween.EaseType.easeInOutExpo));
        
    }

    public void CheckHP() {
        if ( unit.currentHP <= 0 ) {
            GameObject tomb = AccountManager.Instance.resource.unitDeadObject;
            GameObject dropTomb = Instantiate(tomb);
            dropTomb.transform.position = transform.position;

            if (isPlayer) {
                PlayMangement.instance.PlayerUnitsObserver.UnitRemoved(x, y);
            }
            else {
                PlayMangement.instance.EnemyUnitsObserver.UnitRemoved(x, y);
            }

            dropTomb.GetComponent<DeadSpine>().target = gameObject;
            dropTomb.GetComponent<DeadSpine>().StartAnimation(unit.race);
        }
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


    public class Buff {
        public GameObject origin;   //발생지
        public int atk;
        public int hp;

        public Buff(GameObject origin, int atk, int hp) {
            this.origin = origin;
            this.atk = atk;
            this.hp = hp;
        }
    }
}
