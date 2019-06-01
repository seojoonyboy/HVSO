using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;


public class PlaceMonster : MonoBehaviour {
    public IngameClass.Unit unit;
    public bool isPlayer;

    public int x { get; private set; }
    public int y { get; private set; }
    public GameObject myTarget;

    public Vector3 unitLocation;
    public int atkCount = 0;
    public int maxAtkCount = 0;
    public int addAttackPower = 0;
    public int myUnitNum = 0;
    public int itemId = -1;

    protected delegate void TimeUpdate(float time);
    protected TimeUpdate timeUpdate;
    protected UnitSpine unitSpine;

    private float currentTime;
    private bool instanceAttack = false;
    List<Buff> buffList = new List<Buff>();

    public float atkTime {
        get { return unitSpine.atkDuration; }
    }

    public enum UnitState {
        APPEAR,
        IDLE,
        ATTACK,
        HIT,
        MAGICHIT,
        DEAD
    };

    public void Init(CardData data) {
        x = transform.parent.GetSiblingIndex();
        y = transform.parent.parent.GetSiblingIndex();
        atkCount = 0;

        unitLocation = gameObject.transform.position;
        UpdateStat();

        //Observable.EveryUpdate().Where(_ => attacking == true && unit.power > 0).Subscribe(_ => MoveToTarget()).AddTo(this);

        unitSpine = transform.Find("skeleton").GetComponent<UnitSpine>();
        unitSpine.attackCallback += SuccessAttack;
        unitSpine.takeMagicCallback += CheckHP;

        if (unit.attackType.Length > 0 && unit.attackType[0] == "double")
            maxAtkCount = 2;
        else
            maxAtkCount = 1;

        if (unit.attackRange == "distance") {
            GameObject arrow = Instantiate(unitSpine.arrow, transform);
            arrow.transform.position = gameObject.transform.position;
            arrow.name = "arrow";

            if (isPlayer == false)
                arrow.transform.localScale = new Vector3(1, -1, 1);

            arrow.SetActive(false);
        }
        

        if (isPlayer == true) 
            unit.ishuman = (PlayMangement.instance.player.isHuman == true) ? true : false;        
        else
            unit.ishuman = (PlayMangement.instance.enemyPlayer.isHuman == true) ? true : false;

        myUnitNum = PlayMangement.instance.unitNum++;
        GameObject.Find("CardInfoList").GetComponent<CardListManager>().SetFeildUnitInfo(data, myUnitNum);
        
    }

    public void SpawnUnit() {
        SetState(UnitState.APPEAR);
    }


    public void GetTarget() {
        //stun이 있으면 공격을 못함
        if (GetComponent<SkillModules.stun>() != null) {
            SkipAttack();
            return;
        }

        if (atkCount > 0) { GetAnotherTarget(); return; }

        if(unit.attackType.Length > 0 && unit.attackType[0] == "through") {
            if(isPlayer == true) {
                PlayerController enemy = PlayMangement.instance.enemyPlayer;
                myTarget = enemy.transform.gameObject;
            }
            else {
                PlayerController player = PlayMangement.instance.player;
                myTarget = player.transform.gameObject;
            }

        }
        else {
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
            }
        }
        

        MoveToTarget();
    }

    public void GetAnotherTarget() {
        PlaceMonster targetMonster = myTarget.GetComponent<PlaceMonster>();
        if (targetMonster != null) {
            targetMonster.CheckHP();
        }

        if (isPlayer == true) {
            PlayerController enemy = PlayMangement.instance.enemyPlayer;
            if (enemy.frontLine.transform.GetChild(x).childCount != 0 && enemy.frontLine.transform.GetChild(x).GetChild(0).GetComponent<PlaceMonster>().unit.currentHP > 0) {
                myTarget = enemy.frontLine.transform.GetChild(x).GetChild(0).gameObject;
            }
            else if (enemy.backLine.transform.GetChild(x).childCount != 0 && enemy.backLine.transform.GetChild(x).GetChild(0).GetComponent<PlaceMonster>().unit.currentHP > 0) {
                myTarget = enemy.backLine.transform.GetChild(x).GetChild(0).gameObject;
            }
            else {
                myTarget = enemy.transform.gameObject;
            }
        }
        else {
            PlayerController player = PlayMangement.instance.player;
            if (player.frontLine.transform.GetChild(x).childCount != 0 && player.frontLine.transform.GetChild(x).GetChild(0).GetComponent<PlaceMonster>().unit.currentHP > 0) {
                myTarget = player.frontLine.transform.GetChild(x).GetChild(0).gameObject;
            }
            else if (player.backLine.transform.GetChild(x).childCount != 0 && player.backLine.transform.GetChild(x).GetChild(0).GetComponent<PlaceMonster>().unit.currentHP > 0) {
                myTarget = player.backLine.transform.GetChild(x).GetChild(0).gameObject;
            }
            else {
                myTarget = player.transform.gameObject;
            }
        }
        MoveToTarget();
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



    public void UnitTryAttack() {
        if (unit.attack <= 0) return;
        SetState(UnitState.ATTACK);
    }

    public void SuccessAttack() {

        if (unit.attackRange == "distance") {
            GameObject arrow = transform.Find("arrow").gameObject;
            arrow.transform.position = transform.position;
            arrow.SetActive(true);
            PlaceMonster targetMonster = myTarget.GetComponent<PlaceMonster>();

            if (unit.attackType.Length > 0 && unit.attackType[0] == "through") {
                PiercingAttack();
            }
            else {
                if (targetMonster != null)
                    iTween.MoveTo(arrow, iTween.Hash("x", gameObject.transform.position.x, "y", myTarget.transform.position.y, "z", gameObject.transform.position.z, "time", 0.2f, "easetype", iTween.EaseType.easeOutExpo, "oncomplete", "SingleAttack", "oncompletetarget", gameObject));
                else
                    iTween.MoveTo(arrow, iTween.Hash("x", gameObject.transform.position.x, "y", myTarget.GetComponent<PlayerController>().wallPosition.y, "z", gameObject.transform.position.z, "time", 0.2f, "easetype", iTween.EaseType.easeOutExpo, "oncomplete", "SingleAttack", "oncompletetarget", gameObject));
            }
        }
        else
            SingleAttack();
    }


    public void SingleAttack() {
        PlaceMonster targetMonster = myTarget.GetComponent<PlaceMonster>();
        if (unit.attack > 0) {
            if (targetMonster != null) {
                RequestAttackUnit(myTarget, unit.attack);
            }
            else {
                myTarget.GetComponent<PlayerController>().PlayerTakeDamage(unit.attack);
            }

            AttackEffect(myTarget);
        }
        EndAttack();
    }

    public void PiercingAttack() {
        PlayerController targetPlayer = myTarget.GetComponent<PlayerController>();
        PlaceMonster frontMonster = (targetPlayer.frontLine.transform.GetChild(x).childCount > 0) ? targetPlayer.frontLine.transform.GetChild(x).GetChild(0).GetComponent<PlaceMonster>() : null;
        PlaceMonster backMonster = (targetPlayer.backLine.transform.GetChild(x).childCount > 0) ? targetPlayer.backLine.transform.GetChild(x).GetChild(0).GetComponent<PlaceMonster>() : null;
        GameObject arrow = transform.Find("arrow").gameObject;


        if (frontMonster != null) {
            RequestAttackUnit(frontMonster.transform.gameObject, unit.attack);
            iTween.MoveTo(arrow, iTween.Hash("x", gameObject.transform.position.x, "y", myTarget.transform.position.y, "z", gameObject.transform.position.z, "time", 0.1f, "easetype", iTween.EaseType.easeOutExpo));
            AttackEffect(frontMonster.transform.gameObject);
        }
        if (backMonster != null) {
            RequestAttackUnit(backMonster.transform.gameObject, unit.attack);
            iTween.MoveTo(arrow, iTween.Hash("x", gameObject.transform.position.x, "y", myTarget.transform.position.y, "z", gameObject.transform.position.z, "time", 0.05f, "easetype", iTween.EaseType.easeOutExpo));
            AttackEffect(backMonster.transform.gameObject);
        }
        targetPlayer.PlayerTakeDamage(unit.attack);
        iTween.MoveTo(arrow, iTween.Hash("x", gameObject.transform.position.x, "y", myTarget.transform.position.y, "z", gameObject.transform.position.z, "time", 0.05f, "easetype", iTween.EaseType.easeOutExpo));
        AttackEffect(myTarget);

        EndAttack();
    }

    public void InstanceAttack() {
        instanceAttack = true;
        GetTarget();
    }


    public void ChangePosition(int x, int y, Vector3 unitLocation) {
        this.x = x;
        this.y = y;

        this.unitLocation = unitLocation;
    }


    public void EndAttack() {
        if (instanceAttack == true) {
            instanceAttack = false;
        }
        else
            atkCount++;

        PlaceMonster targetMonster = myTarget.GetComponent<PlaceMonster>();
        if (unit.attackRange == "distance") {
            GameObject arrow = transform.Find("arrow").gameObject;
            arrow.transform.position = transform.position;
            arrow.SetActive(false);
        }
        else {
            ReturnPosition();
            if (targetMonster != null)
                myTarget.GetComponent<PlaceMonster>().ReturnPosition();
        }
    }

    private void SkipAttack() {
        atkCount++;
    }

    public void AttackEffect(GameObject target = null) {
        PlaceMonster targetMonster = target.GetComponent<PlaceMonster>();
        if (unit.attack <= 3) {
            GameObject effect = Instantiate(PlayMangement.instance.effectManager.lowAttackEffect);
            effect.transform.position = (targetMonster != null) ? target.transform.position : new Vector3(gameObject.transform.position.x, myTarget.GetComponent<PlayerController>().wallPosition.y, 0);
            Destroy(effect, effect.GetComponent<ParticleSystem>().main.duration - 0.2f);
            StartCoroutine(PlayMangement.instance.cameraShake(unitSpine.atkDuration / 2));
            SoundManager.Instance.PlaySound(SoundType.NORMAL_ATTACK);
        }
        else if (unit.attack > 4) {
            GameObject effect = (unit.attack < 6) ? Instantiate(PlayMangement.instance.effectManager.middileAttackEffect) : Instantiate(PlayMangement.instance.effectManager.highAttackEffect);
            effect.transform.position = (targetMonster != null) ? target.transform.position : new Vector3(gameObject.transform.position.x, myTarget.GetComponent<PlayerController>().wallPosition.y, 0);
            Destroy(effect, effect.GetComponent<ParticleSystem>().main.duration - 0.2f);
            StartCoroutine(PlayMangement.instance.cameraShake(unitSpine.atkDuration / 2));

            if (unit.attack > 4 && unit.attack <= 6) {
                SoundManager.Instance.PlaySound(SoundType.MIDDLE_ATTACK);
            }
            if (unit.attack > 6) {
                SoundManager.Instance.PlaySound(SoundType.LARGE_ATTACK);
            }
        }
    }

    public void InstanceKilled() {
        unit.HP = 0;
    }



    public void RequestAttackUnit(GameObject target, int amount) {
        PlaceMonster targetMonster = target.GetComponent<PlaceMonster>();

        if (targetMonster != null) {
            targetMonster.unit.currentHP -= amount;
            targetMonster.UpdateStat();
            targetMonster.SetState(UnitState.HIT);

            //독성 능력이 있으면 상대에게 공격시 poisonned 부여
            if(GetComponent<SkillModules.poison>() != null) {
                target.AddComponent<poisonned>();
            }

            object[] parms = new object[] { isPlayer, gameObject };
            PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.BEGIN_ATTACK, this, parms);
        }
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

    private void ReturnPosition() {
        iTween.MoveTo(gameObject, iTween.Hash("x", unitLocation.x, "y", unitLocation.y, "z", unitLocation.z, "time", 0.3f, "delay", 0.5f, "easetype", iTween.EaseType.easeInOutExpo));

    }

    public void CheckHP() {
        if (unit.currentHP <= 0 || GetComponent<poisonned>() != null) {
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
            dropTomb.GetComponent<DeadSpine>().StartAnimation(unit.ishuman);
        }
    }

    public void TakeMagic() {
        SetState(UnitState.MAGICHIT);
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
            case UnitState.MAGICHIT:
                unitSpine.MagicHit();
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
