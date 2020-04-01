using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using TMPro;
using System;
using Spine;
using Spine.Unity;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    public bool isHuman;
    public bool isPlayer;
    private bool myTurn = false;
    public GameObject card;
    public GameObject back;
    public GameObject playerUI;
    public SkeletonGraphic shieldFeedBack;
    public SkeletonGraphic shieldGauge;
    protected Transform sheildRemain;
    [SerializeField] public CardHandManager cdpm;

    public GameObject backLine;
    public GameObject frontLine;
    protected Text costText;
    protected Text HPText;
    protected Transform HPGauge;
    public GameObject buttonParticle;
    public bool dragCard = false;

    public Vector3 unitClosePosition;
    public Vector3 wallPosition;

    public ReactiveProperty<int> HP = new ReactiveProperty<int>(20);
    public ReactiveProperty<int> resource = new ReactiveProperty<int>(2);
    public ReactiveProperty<bool> isPicking = new ReactiveProperty<bool>(false);
    public ReactiveProperty<int> shieldStack = new ReactiveProperty<int>(0);
    protected int shieldCount = 0;

    public int remainShieldCount {
        get { return shieldCount; }
        set { 
            shieldCount = value;
            ConsumeShieldStack(); 
        }
    }

    protected HeroSpine heroSpine;
    public static int activeCardMinCost;
    public string heroID;
    
    public GameObject effectObject;
    public enum HeroState {
        IDLE,
        ATTACK,
        SHIELD,
        HIT,
        DEAD,
        THINKING,
        THINKDONE
    }

    public float DeadAnimationTime {
        get { return heroSpine.deadTime; }
    }

    public Transform bodyTransform {
        get { return heroSpine.gameObject.transform.Find("effect_body"); }
    }

    public int MaximumCardCount {
        get { return cdpm.transform.childCount; }
    }

    public int CurrentCardCount {
        get {
            int temp = 0;
            Transform deck = (isPlayer) ? cdpm.gameObject.transform : playerUI.transform.Find("CardSlot");
            foreach (Transform child in deck)
                temp = (child.childCount > 0) ? ++temp : temp;
            return temp;
        }
    }

    public Transform latestCardSlot {
        get {
            return isPlayer ? cdpm.gameObject.transform.GetChild(CurrentCardCount - 1)
                : playerUI.transform.Find("CardSlot").GetChild(CurrentCardCount - 1);
        }
    }

    public Transform EmptyCardSlot {
        get {
            if (CurrentCardCount >= 10) return null;
            return isPlayer ? cdpm.gameObject.transform.GetChild(CurrentCardCount)
                : playerUI.transform.Find("CardSlot").GetChild(CurrentCardCount);
        }
    }

    public bool getPlayerTurn {
        get { return myTurn; }
    }

    public bool initCompleted = false;
    public virtual void Init() {
        if(PlayMangement.instance.socketHandler.gameState == null) return;
        
        Debug.Assert(!PlayerPrefs.GetString("SelectedRace").Any(char.IsUpper), "Race 정보는 소문자로 입력해야 합니다!");

        string race = PlayerPrefs.GetString("SelectedRace").ToLower();
        if (race == "human") isHuman = isPlayer;
        else isHuman = !isPlayer;
        costText = playerUI.transform.Find("PlayerResource").GetChild(0).Find("Text").GetComponent<Text>();
        HPText = playerUI.transform.Find("PlayerHealth/HealthText").GetComponent<Text>();
        HPGauge = playerUI.transform.Find("PlayerHealth/Helth&Shield/HpParent1/HpParent2/HpParent3/HpGage");
        //shieldGauge = playerUI.transform.Find("PlayerHealth/Helth&Shield/SheildGauge").GetComponent<Image>();
        if (isHuman) {
            playerUI.transform.Find("PlayerHealth/Flag/Human").gameObject.SetActive(true);
            sheildRemain = playerUI.transform.Find("PlayerHealth/HumanSheild");
        }
        else {
            playerUI.transform.Find("PlayerHealth/Flag/Orc").gameObject.SetActive(true);
            sheildRemain = playerUI.transform.Find("PlayerHealth/OrcSheild");
        }
        sheildRemain.gameObject.SetActive(true);
        if (isPlayer) {
            buttonParticle = playerUI.transform.Find("TurnUI/ResourceOut").gameObject;
            if (isHuman)
                buttonParticle.GetComponent<SkeletonGraphic>().color = new Color(0.552f, 0.866f, 1);
            else
                buttonParticle.GetComponent<SkeletonGraphic>().color = new Color(1, 0.556f, 0.556f);
            buttonParticle.SetActive(false);
        }
        else {
            playerUI.transform.Find("EnemyNickname").GetComponent<Text>().text = 
                PlayMangement.instance.socketHandler.gameState.players.enemyPlayer(isHuman).user.nickName;
        }
        
        

        SetPlayerHero(isHuman);
        if (!isPlayer)
            transform.Find("FightSpine").localPosition = new Vector3(0, 3, 0);

        ResetGraphicAnimation(shieldGauge, true);
        shieldGauge.AnimationState.SetAnimation(0, "0", false);        
        SetShield();

        shieldCount = 3;

        initCompleted = true;
        Debug.Log(heroSpine);
    }

    public void SetPlayerHero(bool isHuman) {
        string id;
        GameObject hero;
        if (isHuman == true)
            id = PlayMangement.instance.socketHandler.gameState.players.human.hero.id;
        else
            id = PlayMangement.instance.socketHandler.gameState.players.orc.hero.id;
        this.heroID = id;
        hero = Instantiate(AccountManager.Instance.resource.heroSkeleton[id], transform);
        hero.transform.SetAsLastSibling();
        heroSpine = hero.GetComponent<HeroSpine>();

        if (isPlayer) {
            float reverse = hero.transform.localScale.x * -1f;
            hero.transform.localScale = new Vector3(reverse, hero.transform.localScale.y, hero.transform.localScale.z);
            heroSpine.GetComponent<MeshRenderer>().sortingOrder = 55;
            hero.transform.localPosition = new Vector3(0, 1, 0);
            hero.transform.localScale = new Vector3(-1, 1, 1);

            transform.Find("MagicTargetTrigger").localPosition = new Vector3(0, 3.81f, 0);
            transform.Find("ClickableUI").localPosition = new Vector3(0, 3.02f, 0);
        }
        else {
            transform.Find("MagicTargetTrigger").localPosition = new Vector3(0, 2.55f, 0);
            transform.Find("ClickableUI").localPosition = new Vector3(0, 2.07f, 0);

            heroSpine.GetComponent<MeshRenderer>().sortingOrder = 6;
        }
    }
    
    private void SetParticleSize(ParticleSystem particle) {
        particle.transform.position = Camera.main.ScreenToWorldPoint(particle.transform.parent.position);
        particle.transform.position = new Vector3(particle.transform.position.x, particle.transform.position.y, 0);
    }

    private void Start()
    {
        Init();
        CardDropManager.Instance.SetUnitDropPos();
    }


    protected void SetShield() {
        GameObject shield;
        Transform playerTransform = PlayMangement.instance.backGround.transform.Find("PlayerPosition");
        shield = Instantiate(EffectSystem.Instance.effectObject[EffectSystem.EffectType.HERO_SHIELD], transform);


        SkeletonAnimation skeletonAnimation = shield.GetComponent<SkeletonAnimation>();
        Skeleton skeleton = skeletonAnimation.skeleton;
        skeleton.SetSkin((isHuman == true) ? "HUMAN" : "ORC");
        ResetAnimation(skeletonAnimation);

        shield.transform.position = bodyTransform.position;
        shield.transform.localScale = PlayMangement.instance.backGround.transform.localScale;
        shield.name = "shield";
        shield.SetActive(false);
        //heroSpine.defenseFinish += DisableShield;
    }

    
    

    public void SetHP(int amount) {
        HP.Value = amount;
    }



    public void SetPlayerStat() {
        ObserverText();
    }

    private void ObserverText() {
        playerUI.transform.Find("PlayerHealth/HealthText").gameObject.SetActive(true);
        var ObserveHP = HP.TakeWhile(_ => PlayMangement.instance.isGame == true).Subscribe(_ => ChangedHP());
        var ObserveResource = resource.TakeWhile(_ => PlayMangement.instance.isGame == true).Subscribe(_=> ChangedResource());

        var ObserveDead = HP.Where(x => x <= 0)
                              .Subscribe(_ => {
                                  SetState(HeroState.DEAD);
                              })
                              .AddTo(PlayMangement.instance.transform.gameObject);
    }
    

    private void ChangedHP() {
        HPText.text = HP.Value.ToString();
        if (HP.Value < 20)
            HPGauge.localPosition = new Vector3(0, -((20 - HP.Value) * 6), 0);
        else
            HPGauge.localPosition = Vector3.zero;
    }

    private void ChangedResource() {
        costText.text = resource.Value.ToString();
    }

    public void UpdateHealth() {
        HP.Value += 2;
    }
    

    public virtual void PlayerTakeDamage() {
        if (GetComponent<SkillModules.guarded>() != null) return;
        BattleConnector socketHandler = PlayMangement.instance.socketHandler;
        SocketFormat.Player data;
        data = socketHandler.gameState.players.myPlayer(isHuman);
        int shieldGaugeAmount = socketHandler.shieldStack.GetShieldAmount(isHuman);
        int amount = 0;

        if (isHuman == true)
            amount = socketHandler.gameState.players.human.hero.currentHp;
        else
            amount = socketHandler.gameState.players.orc.hero.currentHp;

        amount = HP.Value - amount;
        PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.HERO_UNDER_ATTACK, this, isPlayer);

        if(shieldGaugeAmount != 0) Debug.Log("쉴드게이지!" + shieldGaugeAmount);
        Debug.Log("data.shieldActivate : " + data.hero.shieldActivate);
        Debug.Log("CheckShieldActivate : " + CheckShieldActivate(shieldGaugeAmount));
        if(data.hero.shieldActivate && CheckShieldActivate(shieldGaugeAmount)) {
            //ActiveShield();
        }
        else {
            EffectSystem.Instance.ShowDamageText(bodyTransform.position, -amount);
            if (HP.Value >= amount)
                HP.Value -= amount;
            else
                HP.Value -= HP.Value;

            if (HP.Value > 0)
                SetState(HeroState.HIT);

            if (HP.Value > 0 && HP.Value < 8)
                heroSpine.CriticalFace();


            if (shieldCount > 0) {
                if (shieldGaugeAmount == 0)
                    shieldStack.Value = data.hero.shieldGauge;
                else {
                    EffectSystem.Instance.IncreaseShieldFeedBack(shieldFeedBack.transform.gameObject ,shieldGaugeAmount);
                    ChangeShieldStack(shieldStack.Value, shieldGaugeAmount);
                }
            }
        }

        CustomVibrate.Vibrate(1000);
    }

    /// <summary>
    /// 영웅의 실드 게이지 조정
    /// </summary>
    /// <param name="amount"></param>
    public void PillageEnemyShield(int amount) {
        //const int MaxGage = 8;
        PlayMangement playMangement = PlayMangement.instance;
        PlayerController targetPlayer = (isPlayer == true) ? playMangement.enemyPlayer : playMangement.player;
        int enemyShieldStack = targetPlayer.shieldStack.Value;
        //var enemyShieldStack = isPlayer ? playMangement.enemyPlayer.shieldStack : playMangement.player.
        //내가 뺏어올 수 있는 양 계산
        int availableAmountToGet = 0;
        availableAmountToGet = (enemyShieldStack < amount) ? enemyShieldStack : amount;
        targetPlayer.DiscountShieldStack(targetPlayer.shieldStack.Value, availableAmountToGet);

        //Logger.Log("적 실드 " + targetPlayer.shieldStack.Value + "로 바뀜(약탈)");

        ////내가 채울 수 있는 양 계산
        ChangeShieldStack(shieldStack.Value, availableAmountToGet);
    }
    


    private bool CheckShieldActivate(int shieldData) {
        return (shieldStack.Value + shieldData) >= 8;
    }

    public void ActiveShield() {
        GameObject shield = transform.Find("shield").gameObject;
        shield.SetActive(true);        
        SkeletonAnimation skeletonAnimation = shield.GetComponent<SkeletonAnimation>();
        ResetAnimation(skeletonAnimation, true);
        TrackEntry entry;
        string side = (isPlayer == true) ? "bottom" : "upside";
        entry = skeletonAnimation.AnimationState.SetAnimation(0, side, false);
        entry.Complete += delegate (TrackEntry temp) { shield.SetActive(false); };
        SetState(HeroState.SHIELD);
        HeroCardTimer();
    }

    public void HeroCardTimer() {
        if (isPlayer == false) return;
        if (ScenarioGameManagment.scenarioInstance != null) return;

        var ingameTimer = GetComponent<IngameTimer>();
        ingameTimer.OnTimeout.AddListener(PlayMangement.instance.showCardsHandler.TimeoutShowCards);
        ingameTimer.BeginTimer(20);
        Invoke("OnTimeOut", 20);
        ingameTimer.timerUI.transform.SetParent(PlayMangement.instance.showCardsHandler.timerPos);
        ingameTimer.timerUI.transform.localPosition = Vector3.zero;
    }


    public void OnTimeOut() {
        var ingameTimer = GetComponent<IngameTimer>();
        ingameTimer.timerUI.transform.SetParent(ingameTimer.parent);
        ingameTimer.timerUI.transform.localPosition = new Vector3(0, -440f, 0);
    }

    public void DisableShield() {
        GameObject shield = transform.Find("shield").gameObject;
        shield.SetActive(false);
    }

    public void EffectForPlayer(int amount = 0, string skillId = null) {
        Vector3 position = new Vector3(transform.position.x, transform.position.y + 2.5f, transform.position.z);
        if (amount < 0) {
            if (skillId == "ac10021") {
                EffectSystem.Instance.ShowEffectOnEvent(EffectSystem.EffectType.TREBUCHET, position, delegate() { MagicHit(); }, false, transform);
            }
            else
                EffectSystem.Instance.ShowEffect(EffectSystem.EffectType.EXPLOSION, position);
        }
        else if (amount > 0)
            EffectSystem.Instance.ShowEffect(EffectSystem.EffectType.BUFF, position);
        else
            return;
    }
    

    public void TakeIgnoreShieldDamage(bool isMagic = false, string skillId= null) {
        int amount;
        if (GetComponent<SkillModules.guarded>() != null) {
            amount = 0;
        }

        BattleConnector socketHandler = PlayMangement.instance.socketHandler;
        if (isHuman == true)
            amount = socketHandler.gameState.players.human.hero.currentHp;
        else
            amount = socketHandler.gameState.players.orc.hero.currentHp;

        amount = HP.Value - amount;


        EffectSystem.Instance.ShowDamageText(bodyTransform.position, -amount);
        if (HP.Value >= amount)
            HP.Value -= amount;
        else
            HP.Value -= HP.Value;
        
        if (HP.Value > 0 && HP.Value < 8)
            heroSpine.CriticalFace();

    }

    public void Hit() {
        SetState(HeroState.HIT);
    }

    public void MagicHit() {
        SetState(HeroState.HIT);
        StartCoroutine(PlayMangement.instance.cameraShake(0.8f, 3));
    }

    public void ReleaseTurn() {
        PlayMangement playManagement = PlayMangement.instance;
        if (playManagement.socketHandler.gameState.turn.turnState.CompareTo("play") != 0) return;
        if (myTurn == true && !dragCard) {
            if (isPlayer) {
                playManagement.releaseTurnBtn.gameObject.SetActive(false);
                buttonParticle.SetActive(false);
            }
            SoundManager.Instance.PlayIngameSfx(IngameSfxSound.TURNBUTTON);
            myTurn = false;
            if (isHuman == playManagement.player.isHuman)
                playManagement.SettingMethod(BattleConnector.SendMessageList.turn_over);
        }
    }

    public void ActivePlayer() {
        activeCardMinCost = 100;
        myTurn = true;      
        if(isPlayer == true) {
            for (int i = 0; i < MaximumCardCount; i++) {
                CardHandler card = DeckCard(i);
                if (card != null)
                    card.ActivateCard();
            }
        }
        if (activeCardMinCost == 100) {
            if (isPlayer)
                buttonParticle.SetActive(true);
        }
        if (PlayMangement.instance.currentTurn != TurnType.BATTLE)
            PlayMangement.dragable = true;
    }

    public void ActiveOrcTurn() {
        activeCardMinCost = 100;
        TurnType currentTurn = PlayMangement.instance.currentTurn;
        myTurn = true;
        if (isPlayer == true && currentTurn == TurnType.ORC) {
            for (int i = 0; i < MaximumCardCount; i++) {
                CardHandler card = DeckCard(i);
                if (card != null) {
                    if (card.cardData.type == "unit")
                        card.ActivateCard();
                    else
                        card.DisableCard();
                }

            }
        }
        else if(isPlayer == true && currentTurn == TurnType.SECRET) {
            for (int i = 0; i < MaximumCardCount; i++) {
                CardHandler card = DeckCard(i);
                if (card != null) {
                    if (card.cardData.type == "magic" || card.cardData.type =="tool")
                        card.ActivateCard();
                    else
                        card.DisableCard();
                }

            }
        }
        if (activeCardMinCost == 100) {
            if (isPlayer)
                buttonParticle.SetActive(true);
        }
        if (PlayMangement.instance.currentTurn != TurnType.BATTLE)
            PlayMangement.dragable = true;
    }

    public void DisablePlayer() {
        myTurn = false;
        if (isPlayer == true) {
            for (int i = 0; i < MaximumCardCount; i++) {
                CardHandler card = DeckCard(i);
                if (card != null)
                    card.DisableCard();
            }
        }
        if (isPlayer)
            buttonParticle.SetActive(false);
    }

    public CardHandler DeckCard(int num) {
        if (num < 0 || num > 9) return null;
        Transform cardSlot = cdpm.transform;

        if (cardSlot.GetChild(num).childCount == 0) return null;
        return cardSlot.GetChild(num).GetChild(0).gameObject.GetComponent<CardHandler>();
    }

    public void UpdateCardCount() {
        playerUI.transform.Find("CardNum/Value").gameObject.GetComponent<Text>().text = PlayMangement.instance.socketHandler.gameState.players.enemyPlayer(isHuman).deck.handCards.Length.ToString();
    }


    /// <summary>
    /// 내 핸드에 있는 해당 id의 카드들만 비활성화 시킴
    /// </summary>
    /// <param name="id"></param> 카드의 아이디
    /// <param name="active"></param> true 일시 카드 활성화, false면 비활성화
    //public void SetThisCardAble(string id, bool active) {
    //    if (isPlayer == true) {
    //        Transform cardSlot_1 = playerUI.transform.Find("CardHand").GetChild(0);
    //        Transform cardSlot_2 = playerUI.transform.Find("CardHand").GetChild(1);
    //        for (int i = 0; i < cardSlot_1.childCount; i++) {
    //            if (cardSlot_1.GetChild(i).GetComponent<CardHandler>().cardID == id) {
    //                if(active)
    //                    cardSlot_1.GetChild(i).GetChild(0).GetComponent<CardHandler>().ActivateCard();
    //                else
    //                    cardSlot_1.GetChild(i).GetChild(0).GetComponent<CardHandler>().DisableCard();
    //            }
    //        }
    //        for (int i = 0; i < cardSlot_2.childCount; i++) {
    //            if (cardSlot_2.GetChild(i).GetComponent<CardHandler>().cardID == id) {
    //                if (active)
    //                    cardSlot_2.GetChild(i).GetChild(0).GetComponent<CardHandler>().ActivateCard();
    //                else
    //                    cardSlot_2.GetChild(i).GetChild(0).GetComponent<CardHandler>().DisableCard();
    //            }
    //        }
    //    }
    //}

    public void ChangeShieldStack(int start, int amount) {
        shieldStack.Value = (shieldStack.Value + amount > 8) ? 8 : shieldStack.Value += amount;
        ResetGraphicAnimation(shieldGauge, true);
        TrackEntry entry = new TrackEntry();        
        SoundManager.Instance.PlayShieldChargeCount(amount);

        if (amount > 0) {
            SoundManager.Instance.PlayShieldChargeCount(amount);
            for (int i = 1; i <= amount; i++) {

                if (start + i > 8)
                    break;

                entry = shieldGauge.AnimationState.AddAnimation(0, (start + i).ToString(), false, 0);
            }

            if (shieldStack.Value >= 8)
                entry = shieldGauge.AnimationState.AddAnimation(0, "full", true, 0);
        }
        else {
            if (amount == 0) return;
            int to = Mathf.Abs(amount);
            for (int i = 0; i < to; i++) {

                if (start - i < 0)
                    break;

                entry = shieldGauge.AnimationState.AddAnimation(0, (start - i - 1).ToString(), false, 0);
            }
        }
    }

    public void DiscountShieldStack(int start, int amount) {
        shieldStack.Value = (start >= amount) ? start - amount : 0;
        ResetGraphicAnimation(shieldGauge, true);
        TrackEntry entry = new TrackEntry();

        entry = shieldGauge.AnimationState.SetAnimation(0, shieldStack.Value.ToString(), false);
    }


    public void FullShieldStack(int start) {
        int amount = 8 - start;
        ResetGraphicAnimation(shieldGauge, true);
        TrackEntry entry = new TrackEntry();
        SoundManager.Instance.PlayShieldChargeCount(amount);
        for (int i = 1; i < amount; i++)
            entry = shieldGauge.AnimationState.AddAnimation(0, (start + i).ToString(), false, 0);

        entry = shieldGauge.AnimationState.AddAnimation(0, "full", true, 0);
    }

    public void ConsumeShieldStack() {
        ResetGraphicAnimation(shieldGauge);
        TrackEntry entry;
        entry = shieldGauge.AnimationState.SetAnimation(0, "0", false);
        string aniName = shieldCount == 3 ? "NOANI" : (3 - shieldCount).ToString();
        sheildRemain.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, aniName, false);
    }

    public void SetShieldStack(int val) {
        shieldGauge.Initialize(false);
        shieldGauge.Update(0);
        TrackEntry entry;
        entry = shieldGauge.AnimationState.SetAnimation(0, "0", false);
        string aniName = val == 3 ? "NOANI" : (3 - val).ToString();
        sheildRemain.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, aniName, false);
    }




    public void PlayerUseCard() {
        SetState(HeroState.ATTACK);
    }

    public void PlayerThinking() {
        SetState(HeroState.THINKING);
    }

    public void PlayerThinkFinish() {
        SetState(HeroState.THINKDONE);
    }

    public void PlayerDead() {
        SetState(HeroState.DEAD);
    }

    public void PlayerAddAction(UnityEngine.Events.UnityAction action) {
        heroSpine.afterAction += action;
    }

    private void ResetGraphicAnimation(SkeletonGraphic skeleton, bool resetTrack = false) {
        skeleton.Initialize(false);
        skeleton.Update(0);
        if(resetTrack == true)
            skeleton.AnimationState.ClearTrack(0);
    }

    private void ResetAnimation(SkeletonAnimation skeleton, bool resetTrack = false) {
        skeleton.Initialize(false);
        skeleton.Update(0);
        if (resetTrack == true)
            skeleton.AnimationState.ClearTrack(0);
    }

    

    protected void SetState(HeroState state) {
        if (heroSpine == null) return;

        switch (state) {
            case HeroState.IDLE:
                heroSpine.Idle();
                break;
            case HeroState.HIT:
                heroSpine.Hit();
                break;
            case HeroState.ATTACK:
                heroSpine.Attack();
                break;
            case HeroState.SHIELD:
                heroSpine.Shield();
                break;
            case HeroState.DEAD:
                heroSpine.Dead();
                break;
            case HeroState.THINKING:
                heroSpine.Thinking();
                break;
            case HeroState.THINKDONE:
                heroSpine.ThinkDone();
                break;
        }
    }
}
