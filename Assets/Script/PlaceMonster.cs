using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using TMPro;
using UnityEngine.UI;

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
    public EffectSystem.ActionDelegate actionCall;
    public float atkTime {
        get { return unitSpine.atkDuration; }
    }

    public int unitSoringOrder {
        set { unitSpine.transform.GetComponent<MeshRenderer>().sortingOrder = value; }
        get { return unitSpine.transform.GetComponent<MeshRenderer>().sortingOrder; }
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
        ANGRY,
        REPLACE,
        DEAD
    };

    void OnDestroy() {
        if(PlayMangement.instance == null) return;
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

    public void Init(dataModules.CollectionCard data) {
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

        if (unit.attributes.Length > 0 && unit.attributes[0] == "ambush")
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
        transform.Find("ClickableUI").position = unitSpine.bodybone.position;
        transform.Find("FightSpine").position = unitSpine.bodybone.position;
        UpdateStat();
        ChangeAttackProperty();
    }

    public void SetHiding() {
        if (unit.attributes.Length > 0) {
            if (unit.attributes[0] == "ambush") {
                unitSpine.hidingObject = AccountManager.Instance.resource.hideObject;
                GameObject hide = Instantiate(AccountManager.Instance.resource.hideObject, transform);
                hide.transform.position = gameObject.transform.position;
                hideSpine = hide.GetComponent<HideUnit>();
                hideSpine.unitSpine = unitSpine;
                hideSpine.Init();
            }
        }
    }

    public void HideUnit() {
        transform.Find("Numbers").gameObject.SetActive(false);
        transform.Find("UnitAttackProperty").gameObject.SetActive(false);
        unitSpine.gameObject.SetActive(false);
        hideSpine.gameObject.SetActive(true);
        hideSpine.Appear();
    }

    public void DetectUnit() {
        transform.Find("Numbers").gameObject.SetActive(true);
        transform.Find("UnitAttackProperty").gameObject.SetActive(true);
        //transform.Find("UnitTakeEffectIcon").gameObject.SetActive(true);
        SetState(UnitState.DETECT);
    }

    public void OverMask() {
        unitSoringOrder = 55;
    }
    
    public void ResetSorting() {
        unitSoringOrder = 50;
    }


    public void ChangeAttackProperty() {
        if (unit.attackType.Length <= 0) {
            transform.Find("UnitAttackProperty").gameObject.SetActive(false);
            return;
        }
        SpriteRenderer iconImage = transform.Find("UnitAttackProperty/StatIcon").GetComponent<SpriteRenderer>();
        iconImage.sprite = (unit.attackType.Length > 1) ? AccountManager.Instance.resource.skillIcons["complex"] : AccountManager.Instance.resource.skillIcons[unit.attackType[0]];
    }

    public void AddAttackProperty(string status) {
        transform.Find("UnitAttackProperty").gameObject.SetActive(true);
        SpriteRenderer iconImage = transform.Find("UnitAttackProperty/StatIcon").GetComponent<SpriteRenderer>();
        var skillIcons = AccountManager.Instance.resource.skillIcons;

        var attackList = unit.attackType.ToList();
        if (attackList == null || attackList.Count == 0) {
            attackList = new List<string>();
            iconImage.sprite = skillIcons[status];
        }
        else {
            iconImage.sprite = skillIcons["fusion"];
        }
        //Debug.Log(iconImage.sprite.name);
        attackList.Add(status);
        unit.attackType = attackList.ToArray();
    }

    public void AddAttribute(string newAttrName) {
        var attrList = unit.attributes.ToList();
        if (attrList == null || attrList.Count == 0) {
            attrList = new List<string>();
        }
        attrList.Add(newAttrName);
        unit.attributes = attrList.ToArray();
    }

    public void RemoveAttribute(string attrName) {
        var list = unit.attributes.ToList();
        var isExist = list.Exists(x => x == attrName);
        if (isExist) {
            list.Remove(attrName);
            unit.attributes = list.ToArray();
        }
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

        if (unit.attackType.ToList().Exists(x => x == "poison") && (myTarget != null)) {
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

        //pillage 능력 : 앞에 유닛이 없으면 약탈
        if(unit.attackType.Contains("pillage")) {
            PlayMangement playMangement = PlayMangement.instance;
            FieldUnitsObserver observer = playMangement.UnitsObserver;
            //앞에 적 유닛이 없는가
            var myPos = observer.GetMyPos(gameObject);
            bool isHuman = isPlayer ? playMangement.player.isHuman : playMangement.enemyPlayer.isHuman;
            var result = PlayMangement.instance.UnitsObserver.GetAllFieldUnits(myPos.col, !isHuman);
            if(result.Count == 0) {
                Logger.Log("적이 없음. pillage 발동");
                if(isPlayer) playMangement.player.PillageEnemyShield(2);
                else playMangement.enemyPlayer.PillageEnemyShield(2);
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
                    unitSoringOrder = 51;
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
        SoundManager.Instance.PlayAttackSound(unit.id);
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
        BattleConnector battleConnector = PlayMangement.instance.socketHandler;

        if (unit.attack > 0) {
            if (targetMonster != null) {
                RequestAttackUnit(myTarget, unit.attack);
            }
            else {
                if (unit.attackType.Contains("nightaction") || unit.attackType.Contains("pillage"))
                    myTarget.GetComponent<PlayerController>().TakeIgnoreShieldDamage(unit.attack);
                else
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

    public void InstanceAttack(string cardID = "") {
        instanceAttack = true;        

        if (cardID == "ac10016") {
            actionCall += GetTarget;
            EffectSystem.Instance.ShowEffectAfterCall(EffectSystem.EffectType.ANGRY, unitSpine.headbone, actionCall);
            actionCall -= actionCall;
        }
        else
            GetTarget();
    }

    public void ChangePositionMagicEffect() {
        unitSpine.transform.gameObject.SetActive(true);
        SetState(UnitState.APPEAR);
        gameObject.transform.position = unitLocation;
    }

    public void ChangePosition() {
        //gameObject.transform.position = unitLocation;
        iTween.MoveTo(gameObject, unitLocation, 1.0f);
    }


    public void ChangePosition(int x, int y, Vector3 unitLocation, string cardID) {
        this.x = x;
        this.y = y;

        Vector3 portalPosition = new Vector3(unitLocation.x, unitSpine.headbone.transform.position.y, unitLocation.z);
        this.unitLocation = unitLocation;
        //unitSpine.transform.gameObject.SetActive(true);
        //unitSpine.transform.gameObject.GetComponent<Spine.Unity.SkeletonAnimation>().enabled = true;
        

        switch (cardID) {
            case "ac10028":
                actionCall += ChangePositionMagicEffect;
                EffectSystem.Instance.ShowEffectOnEvent(EffectSystem.EffectType.PORTAL, portalPosition, actionCall);
                actionCall = null;
                break;
            case "ac10015":
                ChangePositionMagicEffect();
                break;
            default:
                ChangePosition();
                break;

        }

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
        Vector3 targetPos = (targetMonster != null) ? targetMonster.unitSpine.bodybone.position : new Vector3(gameObject.transform.position.x, myTarget.GetComponent<PlayerController>().wallPosition.y, 0);

        if (unit.attack <= 3) {
            EffectSystem.Instance.ShowEffect(EffectSystem.EffectType.HIT_LOW, targetPos);
            StartCoroutine(PlayMangement.instance.cameraShake(0.4f, 1));
            //SoundManager.Instance.PlaySound(SoundType.NORMAL_ATTACK);
        }
        else if (unit.attack > 3) {        
            if (unit.attack > 3 && unit.attack <= 6) {
                EffectSystem.Instance.ShowEffect(EffectSystem.EffectType.HIT_MIDDLE, targetPos);
                //SoundManager.Instance.PlaySound(SoundType.MIDDLE_ATTACK);
                StartCoroutine(PlayMangement.instance.cameraShake(0.4f, 4));
            }
            else if (unit.attack >= 7) {
                EffectSystem.Instance.ShowEffect(EffectSystem.EffectType.HIT_HIGH, targetPos);
                //SoundManager.Instance.PlaySound(SoundType.LARGE_ATTACK);
                StartCoroutine(PlayMangement.instance.cameraShake(0.4f, 10));
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

    public void RequestChangeStat(int power = 0, int hp = 0, string magicId = null, bool isMain = false) {
        StartCoroutine(buffEffectCoroutine(power, hp, magicId, isMain));
        unit.attack += power;
        if (unit.attack < 0) unit.attack = 0;
        unit.currentHP += hp;
        
        UpdateStat();
    }

    private IEnumerator buffEffectCoroutine(int power, int hp, string magicId = null, bool isMain = false){
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
            //투석공격
            if (magicId == "ac10021") {
                actionCall += Hit;
                EffectSystem.Instance.ShowEffectOnEvent(EffectSystem.EffectType.TREBUCHET, transform.position, actionCall);                
                actionCall -= actionCall;
            }
            //어둠의 가시
            else if (magicId == "ac10074") {
                actionCall += Hit;
                EffectSystem.Instance.ShowEffectOnEvent(EffectSystem.EffectType.DARK_THORN, transform.position, actionCall);
                actionCall -= actionCall;
            }
            else if (magicId == "ac10037") {
                actionCall += Hit;
                EffectSystem.Instance.ShowEffectOnEvent(EffectSystem.EffectType.CHAIN_LIGHTNING, unitSpine.rootbone.position, actionCall);
                actionCall -= actionCall;
            }
            else if (magicId == "ac10034") {
                actionCall += Hit;
                EffectSystem.Instance.ShowEffectOnEvent(EffectSystem.EffectType.FIRE_WAVE, unitSpine.rootbone.position, actionCall, isMain);
                actionCall -= actionCall;
            }
            //버프 혹은 디버프 효과 부여
            else {
                GetComponent<UnitBuffHandler>()
                    .AddBuff(new UnitBuffHandler.BuffStat(power, hp));
                

                if(buff.hp < 0) {
                    EffectSystem.Instance.ShowEffect(EffectSystem.EffectType.DEBUFF, transform.position);
                }
                else {
                    EffectSystem.Instance.ShowEffect(EffectSystem.EffectType.BUFF, transform.position);
                }
            }
        }
        buff.init();
    }

    private void Hit() {
        SetState(UnitState.HIT);
    }

    public void UpdateStat() {
        Text hpText = transform.Find("Numbers/HP").GetComponentInChildren<Text>();
        Text atkText = transform.Find("Numbers/ATK").GetComponentInChildren<Text>();

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
        unitSoringOrder = 50;
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
            case UnitState.REPLACE:
                unitSpine.Appear();
                
                break;
            case UnitState.DEAD:
                break;
        }
    }

    public void TintAnimation(bool onOff) {
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        MeshRenderer render = unitSpine.GetComponent<MeshRenderer>();
        string colorProperty = "_Color";
		string blackTintProperty = "_Black";
        if(onOff) {
            block.SetColor(colorProperty, Color.white);
            block.SetColor(blackTintProperty, Color.grey);
        }
        else {
            block.SetColor(colorProperty, Color.white);
            block.SetColor(blackTintProperty, Color.black);
        }
        render.SetPropertyBlock(block);
    }
}
