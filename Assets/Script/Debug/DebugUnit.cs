using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugUnit : MonoBehaviour
{
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
    public DebugUnitSpine unitSpine;

    private float currentTime;
    private bool instanceAttack = false;
    List<Buff> buffList = new List<Buff>();
    public GameObject effectObject;
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

    void OnDestroy() {
    }

    private void Start() {
        x = transform.parent.GetSiblingIndex();
        y = transform.parent.parent.GetSiblingIndex();
        atkCount = 0;

        unitLocation = gameObject.transform.position;
        

        unitSpine = transform.Find("skeleton").GetComponent<DebugUnitSpine>();
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

        if (unit.cardCategories[0] == "stealth")
            gameObject.AddComponent<ambush>();

        if (unit.attackType.Length > 0 && unit.attackType[0] == "assault")
            gameObject.AddComponent<SkillModules.UnitAbility_assault>();

        
        UpdateStat();
    }

    public void SpawnUnit() {
        SetState(UnitState.APPEAR);
    }


    public void GetTarget() {
        if (atkCount > 0) { //GetAnotherTarget(); return;
            GetAnotherTarget();
            return;
        }
        DebugPlayer targetPlayer = (isPlayer == true) ? DebugManagement.instance.enemyPlayer : DebugManagement.instance.player;

        if (unit.attackType.Length > 0 && unit.attackType[0] == "through") {
            myTarget = targetPlayer.transform.gameObject;
        }
        else {
            if (targetPlayer.frontLine.transform.GetChild(x).childCount != 0)
                myTarget = targetPlayer.frontLine.transform.GetChild(x).GetChild(0).gameObject;
            else if (targetPlayer.backLine.transform.GetChild(x).childCount != 0)
                myTarget = targetPlayer.backLine.transform.GetChild(x).GetChild(0).gameObject;
            else
                myTarget = targetPlayer.transform.gameObject;
        }

        MoveToTarget();
    }

    public void GetAnotherTarget() {
        DebugPlayer targetPlayer = (isPlayer == true) ? DebugManagement.instance.enemyPlayer : DebugManagement.instance.player;
        DebugUnit targetMonster = myTarget.GetComponent<DebugUnit>();

        if (unit.currentHP <= 0) {
            atkCount = maxAtkCount;
            return;
        }

        if (targetMonster != null) {
            targetMonster.CheckHP();
        }

        if (unit.attackType.Length > 0 && unit.attackType[0] == "through") {
            myTarget = targetPlayer.transform.gameObject;
        }
        else {
            if (targetPlayer.frontLine.transform.GetChild(x).childCount != 0 && targetPlayer.frontLine.transform.GetChild(x).GetChild(0).GetComponent<PlaceMonster>().unit.currentHP > 0)
                myTarget = targetPlayer.frontLine.transform.GetChild(x).GetChild(0).gameObject;
            else if (targetPlayer.backLine.transform.GetChild(x).childCount != 0 && targetPlayer.backLine.transform.GetChild(x).GetChild(0).GetComponent<PlaceMonster>().unit.currentHP > 0)
                myTarget = targetPlayer.backLine.transform.GetChild(x).GetChild(0).gameObject;
            else
                myTarget = targetPlayer.transform.gameObject;
        }
        
        MoveToTarget();
    }


    private void MoveToTarget() {
        if (unit.attack <= 0) return;
        DebugUnit placeMonster = myTarget.GetComponent<DebugUnit>();

        if (unit.attackRange == "distance")
            UnitTryAttack();
        else {
            if (placeMonster != null) {
                if (isPlayer == false) {
                    iTween.MoveTo(gameObject, iTween.Hash("x", gameObject.transform.position.x + 0.75f, "y", myTarget.transform.position.y, "z", gameObject.transform.position.z, "time", 0.3f, "easetype", iTween.EaseType.easeInOutExpo, "oncomplete", "UnitTryAttack", "oncompletetarget", gameObject));
                    iTween.MoveTo(myTarget, iTween.Hash("x", myTarget.transform.position.x - 0.75f, "y", myTarget.transform.position.y, "z", myTarget.transform.position.z, "time", 0.2f, "easetype", iTween.EaseType.easeInOutExpo));
                }
                else {
                    iTween.MoveTo(gameObject, iTween.Hash("x", gameObject.transform.position.x - 0.75f, "y", myTarget.transform.position.y, "z", gameObject.transform.position.z, "time", 0.3f, "easetype", iTween.EaseType.easeInOutExpo, "oncomplete", "UnitTryAttack", "oncompletetarget", gameObject));
                    unitSpine.transform.GetComponent<MeshRenderer>().sortingOrder = 51;
                    iTween.MoveTo(myTarget, iTween.Hash("x", myTarget.transform.position.x + 0.75f, "y", myTarget.transform.position.y, "z", myTarget.transform.position.z, "time", 0.2f, "easetype", iTween.EaseType.easeInOutExpo));
                }
            }
            else {
                iTween.MoveTo(gameObject, iTween.Hash("x", gameObject.transform.position.x, "y", myTarget.GetComponent<DebugPlayer>().unitClosePosition.y, "z", gameObject.transform.position.z, "time", 0.3f, "easetype", iTween.EaseType.easeInOutExpo, "oncomplete", "UnitTryAttack", "oncompletetarget", gameObject));
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
            DebugUnit targetMonster = myTarget.GetComponent<DebugUnit>();

            if (unit.attackType.Length > 0 && unit.attackType[0] == "through") {
                iTween.MoveTo(arrow, iTween.Hash("x", gameObject.transform.position.x, "y", myTarget.GetComponent<DebugPlayer>().wallPosition.y, "z", gameObject.transform.position.z, "time", 0.2f, "easetype", iTween.EaseType.easeOutExpo, "oncomplete", "PiercingAttack", "oncompletetarget", gameObject));
            }
            else {
                if (targetMonster != null)
                    iTween.MoveTo(arrow, iTween.Hash("x", gameObject.transform.position.x, "y", myTarget.transform.position.y, "z", gameObject.transform.position.z, "time", 0.2f, "easetype", iTween.EaseType.easeOutExpo, "oncomplete", "SingleAttack", "oncompletetarget", gameObject));
                else
                    iTween.MoveTo(arrow, iTween.Hash("x", gameObject.transform.position.x, "y", myTarget.GetComponent<DebugPlayer>().wallPosition.y, "z", gameObject.transform.position.z, "time", 0.2f, "easetype", iTween.EaseType.easeOutExpo, "oncomplete", "SingleAttack", "oncompletetarget", gameObject));
            }
        }
        else
            SingleAttack();
    }


    public void SingleAttack() {
        DebugUnit targetMonster = myTarget.GetComponent<DebugUnit>();
        if (unit.attack > 0) {
            if (targetMonster != null) {
                RequestAttackUnit(myTarget, unit.attack);
            }
            else {
                myTarget.GetComponent<DebugPlayer>().PlayerTakeDamage(unit.attack);
            }

            AttackEffect(myTarget);
        }
        EndAttack();
    }

    public void PiercingAttack() {
        DebugPlayer targetPlayer = myTarget.GetComponent<DebugPlayer>();
        DebugUnit frontMonster = (targetPlayer.frontLine.transform.GetChild(x).childCount > 0) ? targetPlayer.frontLine.transform.GetChild(x).GetChild(0).GetComponent<DebugUnit>() : null;
        DebugUnit backMonster = (targetPlayer.backLine.transform.GetChild(x).childCount > 0) ? targetPlayer.backLine.transform.GetChild(x).GetChild(0).GetComponent<DebugUnit>() : null;
        GameObject arrow = transform.Find("arrow").gameObject;
        arrow.SetActive(true);


        if (frontMonster != null) {
            RequestAttackUnit(frontMonster.transform.gameObject, unit.attack);
            //iTween.MoveTo(arrow, iTween.Hash("x", gameObject.transform.position.x, "y", myTarget.transform.position.y, "z", gameObject.transform.position.z, "time", 0.1f, "easetype", iTween.EaseType.easeOutExpo));
            AttackEffect(frontMonster.transform.gameObject);
        }
        if (backMonster != null) {
            RequestAttackUnit(backMonster.transform.gameObject, unit.attack);
            //iTween.MoveTo(arrow, iTween.Hash("x", gameObject.transform.position.x, "y", myTarget.transform.position.y, "z", gameObject.transform.position.z, "time", 0.05f, "easetype", iTween.EaseType.easeOutExpo));
            AttackEffect(backMonster.transform.gameObject);
        }
        targetPlayer.PlayerTakeDamage(unit.attack);

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
            DebugUnit instanceTarget = myTarget.GetComponent<DebugUnit>();

            if (instanceTarget != null)
                myTarget.GetComponent<DebugUnit>().CheckHP();

            instanceAttack = false;
        }
        else
            atkCount = 0;

        DebugUnit targetMonster = myTarget.GetComponent<DebugUnit>();
        if (unit.attackRange == "distance") {
            GameObject arrow = transform.Find("arrow").gameObject;
            arrow.transform.position = transform.position;
            arrow.SetActive(false);
        }
        else {
            ReturnPosition();
            if (targetMonster != null)
                myTarget.GetComponent<DebugUnit>().ReturnPosition();
        }
    }

    private void SkipAttack() {
        atkCount = 0;
    }

    public void AttackEffect(GameObject target = null) {
        DebugUnit targetMonster = target.GetComponent<DebugUnit>();
        Spine.Unity.SkeletonAnimation effectAnimation;
        Vector3 targetPos = (targetMonster != null) ? target.transform.position : new Vector3(gameObject.transform.position.x, myTarget.GetComponent<DebugPlayer>().wallPosition.y, 0);

        if (unit.attack <= 3) {
            EffectSystem.Instance.ShowEffect(EffectSystem.EffectType.HIT_LOW, targetPos);
            StartCoroutine(DebugManagement.instance.cameraShake(unitSpine.atkDuration / 2, 1));
            SoundManager.Instance.PlaySound(SoundType.NORMAL_ATTACK);
        }
        else if (unit.attack > 3) {
            if (unit.attack > 3 && unit.attack <= 6) {
                EffectSystem.Instance.ShowEffect(EffectSystem.EffectType.HIT_MIDDLE, targetPos);
                SoundManager.Instance.PlaySound(SoundType.MIDDLE_ATTACK);
                StartCoroutine(DebugManagement.instance.cameraShake(unitSpine.atkDuration / 2, 2));
            }
            else if (unit.attack > 6) {
                EffectSystem.Instance.ShowEffect(EffectSystem.EffectType.HIT_HIGH, targetPos);
                SoundManager.Instance.PlaySound(SoundType.LARGE_ATTACK);
                StartCoroutine(DebugManagement.instance.cameraShake(unitSpine.atkDuration / 2, 3));
            }
        }
    }

    public void InstanceKilled() {
        UnitDead();
    }



    public void RequestAttackUnit(GameObject target, int amount) {
        DebugUnit targetMonster = target.GetComponent<DebugUnit>();

        if (targetMonster != null) {
            targetMonster.unit.currentHP -= amount;
            targetMonster.UpdateStat();
            targetMonster.SetState(UnitState.HIT);

            //독성 능력이 있으면 상대에게 공격시 poisonned 부여
            if (GetComponent<SkillModules.poison>() != null) {
                target.AddComponent<poisonned>();
            }

            //object[] parms = new object[] { isPlayer, gameObject };
            //PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.BEGIN_ATTACK, this, parms);
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

    public bool IsBuffAlreadyExist(GameObject gameObject) {
        return buffList.Exists(x => x.origin == gameObject);
    }

    public void UpdateStat() {
        TextMeshPro hpText = transform.Find("HP").GetComponentInChildren<TextMeshPro>();
        TextMeshPro atkText = transform.Find("ATK").GetComponentInChildren<TextMeshPro>();

        if (unit.currentHP > 0)
            hpText.text = unit.currentHP.ToString();
        else
            hpText.text = 0.ToString();

        if (unit.currentHP < unit.HP)
            hpText.color = Color.red;
        else if (unit.currentHP > unit.HP)
            hpText.color = Color.green;
        else
            hpText.color = Color.white;

        atkText.text = unit.attack.ToString();

        if (unit.attack < unit.originalAttack)
            atkText.color = Color.red;
        else if (unit.attack > unit.originalAttack)
            atkText.color = Color.green;
        else
            atkText.color = Color.white;
    }

    private void ReturnPosition() {
        unitSpine.transform.GetComponent<MeshRenderer>().sortingOrder = 50;
        iTween.MoveTo(gameObject, iTween.Hash("x", unitLocation.x, "y", unitLocation.y, "z", unitLocation.z, "time", 0.3f, "delay", 0.5f, "easetype", iTween.EaseType.easeInOutExpo));

    }

    public void CheckHP() {
        if (unit.currentHP <= 0) {
            UnitDead();
        }
    }

    public void UnitDead() {
        unit.currentHP = 0;
        GameObject tomb = DebugManagement.instance.unitDeadObject;
        GameObject dropTomb = Instantiate(tomb);
        dropTomb.transform.position = transform.position;

        if (isPlayer) {
            //DebugManagement.instance.PlayerUnitsObserver.UnitRemoved(x, y);
        }
        else {
            //DebugManagement.instance.EnemyUnitsObserver.UnitRemoved(x, y);
        }

        dropTomb.GetComponent<DeadSpine>().target = gameObject;
        dropTomb.GetComponent<DeadSpine>().StartAnimation(unit.ishuman);
    }



    public void CheckDebuff() {
        poisonned poison = GetComponent<poisonned>();

        if (poison != null) {
            GameObject tomb = DebugManagement.instance.unitDeadObject;
            GameObject dropTomb = Instantiate(tomb);
            dropTomb.transform.position = transform.position;

            if (isPlayer) {
                //PlayMangement.instance.PlayerUnitsObserver.UnitRemoved(x, y);
            }
            else {
                //PlayMangement.instance.EnemyUnitsObserver.UnitRemoved(x, y);
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
