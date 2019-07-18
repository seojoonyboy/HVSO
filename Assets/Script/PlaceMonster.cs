using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using TMPro;


public class PlaceMonster : MonoBehaviour {
    public IngameClass.Unit unit;
    public SkillModules.SkillHandler skillHandler;

    public bool isPlayer;

    public int x { get; private set; }
    public int y { get; private set; }
    public GameObject myTarget;

    public Vector3 unitLocation;
    public int atkCount = 0;
    public int maxAtkCount = 0;
    public int myUnitNum = 0;
    public int itemId = -1;

    public bool buffEffect = false;
    
    public UnitSpine unitSpine;
    public HideUnit hideSpine;
    
    protected bool instanceAttack = false;

    public float atkTime {
        get { return unitSpine.atkDuration; }
    }

    struct buffStat {
        public bool running;
        public int atk;
        public int hp;
        public void init() {
            running = false;
            atk = 0;
            hp = 0;
        }
    }
    private buffStat buff = new buffStat();


    public enum UnitState {
        APPEAR,
        IDLE,
        ATTACK,
        HIT,
        MAGICHIT,
        DETECT,
        DEAD
    };

    void OnDestroy() {
        
        if (isPlayer) {
            PlayMangement.instance.UnitsObserver
                .RefreshFields(
                    CardDropManager.Instance.unitLine, 
                    PlayMangement.instance.player.isHuman
                );
        }
        else {
            PlayMangement.instance.UnitsObserver
                .RefreshFields(
                    CardDropManager.Instance.enemyUnitLine, 
                    !PlayMangement.instance.player.isHuman
                );
        }
        skillHandler.RemoveTriggerEvent();
    }

    public void Init(CardData data) {
        x = transform.parent.GetSiblingIndex();
        y = transform.parent.parent.GetSiblingIndex();

        unitLocation = gameObject.transform.position;       
        
        unitSpine = transform.Find("skeleton").GetComponent<UnitSpine>();
        unitSpine.attackCallback += SuccessAttack;
        unitSpine.takeMagicCallback += CheckHP;

        
        if (unit.attackType.Length > 0 && unit.attackType[0] == "double")
            maxAtkCount = 2;
        else
            maxAtkCount = 1;

        if (unit.cardCategories[0] == "stealth")
        gameObject.AddComponent<ambush>();


        if (unit.attackRange == "distance") {
            GameObject arrow = Instantiate(unitSpine.arrow, transform);
            arrow.transform.position = gameObject.transform.position;
            

            if (isPlayer == false) {
                if (arrow.gameObject.name.Contains("Dog") == true)
                    arrow.transform.localScale = new Vector3(1, 1, 1);
                else
                    arrow.transform.localScale = new Vector3(1, -1, 1);
            }
            arrow.name = "arrow";
            arrow.SetActive(false);
        }

        if (isPlayer == true) 
            unit.ishuman = (PlayMangement.instance.player.isHuman == true) ? true : false;        
        else
            unit.ishuman = (PlayMangement.instance.enemyPlayer.isHuman == true) ? true : false;

        myUnitNum = PlayMangement.instance.unitNum++;

        if(unitSpine.hidingObject != null) {
            GameObject hid = Instantiate(unitSpine.hidingObject, transform);
            hid.transform.position = gameObject.transform.position;
            hideSpine = hid.GetComponent<HideUnit>();
            hideSpine.unitSpine = unitSpine;
            hideSpine.Init();
        }
        UpdateStat();
        ChangeAttackProperty();
    }

    public void HideUnit() {        
        transform.Find("HP").gameObject.SetActive(false);
        transform.Find("ATK").gameObject.SetActive(false);
        transform.Find("UnitAttackProperty").gameObject.SetActive(false);
        unitSpine.gameObject.SetActive(false);
        hideSpine.gameObject.SetActive(true);
        hideSpine.Appear();
    }

    public void DetectUnit() {
        transform.Find("HP").gameObject.SetActive(true);
        transform.Find("ATK").gameObject.SetActive(true);
        transform.Find("UnitAttackProperty").gameObject.SetActive(true);
        //transform.Find("UnitTakeEffectIcon").gameObject.SetActive(true);
        SetState(UnitState.DETECT);
    }

    public void ChangeAttackProperty() {
        if (unit.attackType.Length <= 0) {
            transform.Find("UnitAttackProperty").gameObject.SetActive(false);
            return;
        }
        SpriteRenderer iconImage = transform.Find("UnitAttackProperty/StatIcon").GetComponent<SpriteRenderer>();
        iconImage.sprite = (unit.attackType.Length > 1) ? AccountManager.Instance.resource.skillIcons["fusion"] : AccountManager.Instance.resource.skillIcons[unit.attackType[0]];
    }

    public void AddAttackProperty(string status) {
        transform.Find("UnitAttackProperty").gameObject.SetActive(true);
        SpriteRenderer iconImage = transform.Find("UnitAttackProperty/StatIcon").GetComponent<SpriteRenderer>();
        iconImage.sprite = (unit.attackType.Length <= 0) ? AccountManager.Instance.resource.skillIcons[status] : AccountManager.Instance.resource.skillIcons["fusion"];
        Debug.Log(iconImage.sprite.name);
    }


    public void SpawnUnit() {
        SetState(UnitState.APPEAR);
    }


    public void GetTarget() {

        //stun이 있으면 공격을 못함
        if (GetComponent<SkillModules.stun>() != null) {
            Destroy(GetComponent<SkillModules.stun>());
            return;
        }

        PlayerController targetPlayer = (isPlayer == true) ? PlayMangement.instance.enemyPlayer : PlayMangement.instance.player;
        
        if(unit.attackType.Contains("through")) {
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

        if ((GetComponent<SkillModules.poison>() != null) && (myTarget != null)) {
            if(myTarget.GetComponent<PlaceMonster>() != null) {
                myTarget.AddComponent<SkillModules.poisonned>();
            }
            if(unit.attackType.Contains("through")) {
                if (targetPlayer.frontLine.transform.GetChild(x).childCount != 0)
                    targetPlayer.frontLine.transform.GetChild(x).GetChild(0).gameObject.AddComponent<SkillModules.poisonned>();
                if (targetPlayer.backLine.transform.GetChild(x).childCount != 0)
                    targetPlayer.backLine.transform.GetChild(x).GetChild(0).gameObject.AddComponent<SkillModules.poisonned>();
            }
        }

        MoveToTarget();
    }


    protected void MoveToTarget() {
        if (unit.attack <= 0) return;
        PlaceMonster placeMonster = myTarget.GetComponent<PlaceMonster>();

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
                iTween.MoveTo(arrow, iTween.Hash("x", gameObject.transform.position.x, "y", myTarget.GetComponent<PlayerController>().wallPosition.y, "z", gameObject.transform.position.z, "time", 0.2f, "easetype", iTween.EaseType.easeOutExpo, "oncomplete", "PiercingAttack", "oncompletetarget", gameObject));
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
            PlaceMonster instanceTarget = myTarget.GetComponent<PlaceMonster>();

            if (instanceTarget != null)
                myTarget.GetComponent<PlaceMonster>().CheckHP();
            else {
                PlayerController targetPlayer = myTarget.GetComponent<PlayerController>();
                PlaceMonster frontMonster = (targetPlayer.frontLine.transform.GetChild(x).childCount > 0) ? targetPlayer.frontLine.transform.GetChild(x).GetChild(0).GetComponent<PlaceMonster>() : null;
                PlaceMonster backMonster = (targetPlayer.backLine.transform.GetChild(x).childCount > 0) ? targetPlayer.backLine.transform.GetChild(x).GetChild(0).GetComponent<PlaceMonster>() : null;
                if(frontMonster != null) frontMonster.CheckHP();
                if(backMonster != null) backMonster.CheckHP();
            }

            instanceAttack = false;
        }

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
        myTarget = null;
    }

    public void AttackEffect(GameObject target = null) {
        PlaceMonster targetMonster = target.GetComponent<PlaceMonster>();
        Vector3 targetPos = (targetMonster != null) ? target.transform.position : new Vector3(gameObject.transform.position.x, myTarget.GetComponent<PlayerController>().wallPosition.y, 0);

        if (unit.attack <= 3) {
            EffectSystem.Instance.ShowEffect(EffectSystem.EffectType.HIT_LOW, targetPos);
            StartCoroutine(PlayMangement.instance.cameraShake(unitSpine.atkDuration / 2, 1));
            SoundManager.Instance.PlaySound(SoundType.NORMAL_ATTACK);
        }
        else if (unit.attack > 3) {        
            if (unit.attack > 3 && unit.attack <= 5) {
                EffectSystem.Instance.ShowEffect(EffectSystem.EffectType.HIT_MIDDLE, targetPos);
                SoundManager.Instance.PlaySound(SoundType.MIDDLE_ATTACK);
                StartCoroutine(PlayMangement.instance.cameraShake(unitSpine.atkDuration / 2, 2));
            }
            else if (unit.attack > 5) {
                EffectSystem.Instance.ShowEffect(EffectSystem.EffectType.HIT_HIGH, targetPos);
                SoundManager.Instance.PlaySound(SoundType.LARGE_ATTACK);
                StartCoroutine(PlayMangement.instance.cameraShake(unitSpine.atkDuration / 2, 3));
            }
        }
    }

    public void InstanceKilled() {
        UnitDead();
    }



    public void RequestAttackUnit(GameObject target, int amount) {
        PlaceMonster targetMonster = target.GetComponent<PlaceMonster>();

        if (targetMonster != null) {
            targetMonster.UnitTakeDamage(amount);

            object[] parms = new object[] { !isPlayer, targetMonster.gameObject };
            PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.BEGIN_ATTACK, this, parms);
        }
    }

    public void UnitTakeDamage(int amount) {
        if(GetComponent<SkillModules.guarded>() != null) amount = 0;

        if (unit.currentHP >= amount)
            unit.currentHP -= amount;
        else
            unit.currentHP = 0;

        UpdateStat();
        SetState(UnitState.HIT);
    }
    

    public void RequestChangeStat(int power = 0, int hp = 0) {
        StartCoroutine(buffEffectCoroutine(power, hp));
        unit.attack += power;
        if (unit.attack < 0) unit.attack = 0;
        unit.currentHP += hp;
        
        UpdateStat();
    }

    private IEnumerator buffEffectCoroutine(int power, int hp){
        buff.atk += power;
        buff.hp += hp;
        if(buff.running) yield break;
        else buff.running = true;
        yield return null;
        if(buff.atk == 0 && buff.hp == 0) {
            buff.init();
            yield break;
        }
        else {
            if(buff.hp < 0) 
                EffectSystem.Instance.ShowEffect(EffectSystem.EffectType.EXPLOSION, transform.position);
            else {
                EffectSystem.Instance.ShowEffect(EffectSystem.EffectType.BUFF, transform.position);
                if (buffEffect == false) {                    
                    EffectSystem.Instance.ContinueEffect(EffectSystem.EffectType.CONTINUE_BUFF, transform);
                    buffEffect = true;
                }
                else {
                    if(unit.attack <= unit.originalAttack) {
                       EffectSystem.Instance.DisableEffect(transform);
                       buffEffect = false;
                    }
                }
            }
        }
        buff.init();
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

        else if(GetComponent<SkillModules.poisonned>() != null) {
            UnitDead();
        }
    }

    public void UnitDead() {
        PlayMangement playMangement = PlayMangement.instance;
        GameObject[,] slots = null;
        bool isHuman = playMangement.player.isHuman;

        if (isPlayer) {
            if (isHuman) slots = playMangement.UnitsObserver.humanUnits;
            else slots = playMangement.UnitsObserver.orcUnits;

            if(slots[x, y] == null)
                return;
        }
        else {
            if (isHuman) slots = playMangement.UnitsObserver.orcUnits;
            else slots = playMangement.UnitsObserver.humanUnits;

            if (slots[x, y] == null)
                return;
        }
        unit.currentHP = 0;
        PlayMangement.instance.cardInfoCanvas.Find("CardInfoList").GetComponent<CardListManager>().RemoveUnitInfo(myUnitNum);
        GameObject tomb;
        if (AccountManager.Instance.resource != null)
            tomb = AccountManager.Instance.resource.unitDeadObject;
        else
            tomb = PlayMangement.instance.GetComponent<ResourceManager>().unitDeadObject;

        GameObject dropTomb = Instantiate(tomb);
        dropTomb.transform.position = transform.position;

        Logger.Log("X : " + x);
        Logger.Log("Y : " + y);

        if (isPlayer) {
            PlayMangement.instance.UnitsObserver.UnitRemoved(new FieldUnitsObserver.Pos(x, y), isHuman);
        }
        else {
            PlayMangement.instance.UnitsObserver.UnitRemoved(new FieldUnitsObserver.Pos(x, y), !isHuman);
        }

        dropTomb.GetComponent<DeadSpine>().target = gameObject;
        dropTomb.GetComponent<DeadSpine>().StartAnimation(unit.ishuman);

        object[] parms = new object[]{isPlayer, gameObject};

        PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.FIELD_CHANGED, null, null);
        PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.DIE, this, parms);
    }
    
    protected void SetState(UnitState state) {

        switch (state) {
            case UnitState.APPEAR:
                unitSpine.Appear();
                break;
            case UnitState.IDLE: {
                    if (unitSpine.enabled == true)
                        unitSpine.Idle();
                    else
                        hideSpine.Idle();
                }
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
            case UnitState.DETECT:
                hideSpine.Disappear();
                break;
            case UnitState.DEAD:
                break;
        }
    }
}
