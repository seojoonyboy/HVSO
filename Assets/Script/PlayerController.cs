using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using TMPro;
using System;
using Spine;
using Spine.Unity;

public class PlayerController : MonoBehaviour
{
    public bool isHuman;
    public bool isPlayer;
    private bool myTurn = false;
    public int[,] placement = new int[2,5] { {0,0,0,0,0 },
                                              {0,0,0,0,0 }};
    public GameObject card;
    public GameObject back;
    public GameObject playerUI;
    public SkeletonGraphic shieldFeedBack;
    public SkeletonGraphic shieldGauge;
    protected Transform sheildRemain;
    [SerializeField] public CardHandManager cdpm;

    public GameObject backLine;
    public GameObject frontLine;
    protected TextMeshProUGUI costText;
    protected TextMeshProUGUI HPText;
    protected Transform HPGauge;
    public GameObject buttonParticle;
    public bool dragCard = false;

    public Vector3 unitClosePosition;
    public Vector3 wallPosition;

    public ReactiveProperty<int> HP;
    public ReactiveProperty<int> resource = new ReactiveProperty<int>(2);
    public ReactiveProperty<bool> isPicking = new ReactiveProperty<bool>(false);
    public ReactiveProperty<int> shieldStack = new ReactiveProperty<int>(0);
    protected int shieldCount = 0;

    protected HeroSpine heroSpine;
    public static int activeCardMinCost;
    public string heroID;

    public EffectSystem.ActionDelegate actionCall;    

    public GameObject effectObject;
    public enum HeroState {
        IDLE,
        ATTACK,
        HIT,
        DEAD,
        THINKING,
        THINKDONE
    }

    public bool getPlayerTurn {
        get { return myTurn; }
    }

    public virtual void Init() {
        string race = PlayerPrefs.GetString("SelectedRace");
        if (race == "HUMAN") isHuman = isPlayer;
        else isHuman = !isPlayer;
        costText = playerUI.transform.Find("PlayerResource").GetChild(0).Find("Text").GetComponent<TextMeshProUGUI>();
        HPText = playerUI.transform.Find("PlayerHealth/HealthText").GetComponent<TextMeshProUGUI>();
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
        shieldGauge.Initialize(false);
        shieldGauge.Update(0);
        shieldGauge.Skeleton.SetSlotsToSetupPose();
        shieldGauge.AnimationState.SetAnimation(0, "0", false);        
        SetShield();

        shieldCount = 3;
        Debug.Log(heroSpine);
    }

    public void SetPlayerHero(bool isHuman, string heroID = "") {
        string id;
        GameObject hero;
        if(isHuman == true) 
            id = (string.IsNullOrEmpty(heroID)) ? "h10001" : heroID;        
        else 
            id = (string.IsNullOrEmpty(heroID)) ? "h10002" : heroID;
        this.heroID = id;

        hero = Instantiate(AccountManager.Instance.resource.heroSkeleton[id], transform);
        hero.transform.SetAsLastSibling();
        heroSpine = hero.GetComponent<HeroSpine>();

        if (isPlayer == true) {
            float reverse = hero.transform.localScale.x * -1f;
            hero.transform.localScale = new Vector3(reverse, hero.transform.localScale.y, hero.transform.localScale.z);
            heroSpine.GetComponent<MeshRenderer>().sortingOrder = 19;
            hero.transform.localPosition = new Vector3(0, 1, 0);
            hero.transform.localScale = new Vector3(-1, 1, 1);
        }
        else
            heroSpine.GetComponent<MeshRenderer>().sortingOrder = 7;
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
        Transform positionTransform = PlayMangement.instance.backGround.transform.Find("PlayerPosition");
        shield = (isHuman == true) ? Instantiate(PlayMangement.instance.humanShield, transform) : Instantiate(PlayMangement.instance.orcShield, transform);
        shield.transform.position = (isPlayer == true) ? positionTransform.Find("Player_1Wall").position : positionTransform.Find("Player_2Wall").position;
        shield.transform.localScale = PlayMangement.instance.backGround.transform.localScale;
        shield.name = "shield";
        shield.SetActive(false);
        heroSpine.defenseFinish += DisableShield;
    }



    public void DrawPlayerCard(GameObject card) {
        cdpm.AddCard();
        string cardID;
        if (isHuman == true) {
            cardID = "ac1000";
            card.GetComponent<CardHandler>().DrawCard(cardID + UnityEngine.Random.Range(1, 5));
        }
        else {
            cardID = "ac1001";
            card.GetComponent<CardHandler>().DrawCard(cardID + UnityEngine.Random.Range(1, 5));
        }
        card.SetActive(true);
    }
    



    public void SetPlayerStat(int hp) {
        HP = new ReactiveProperty<int>(hp);
        ObserverText();
    }

    private void ObserverText() {       
        

        var ObserveHP = HP.Subscribe(_=> ChangedHP()).AddTo(PlayMangement.instance.transform.gameObject);
        var ObserveResource = resource.Subscribe(_=> ChangedResource()).AddTo(PlayMangement.instance.transform.gameObject);
        //var ObserveShield = shieldStack.Subscribe(_ => shieldGauge.fillAmount = (float)shieldStack.Value / 8).AddTo(PlayMangement.instance.transform.gameObject);
        //var heroDown = HP.Where(x => x <= 0).Subscribe(_ => ).AddTo(PlayMangement.instance.transform.gameObject);

        var gameOverDispose = HP.Where(x => x <= 0)
                              .Subscribe(_ => {
                                               SetState(HeroState.DEAD);
                                               //PlayMangement.instance.GetBattleResult();
                                               ObserveHP.Dispose();
                                               ObserveResource.Dispose(); })
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

    public virtual void PlayerTakeDamage(int amount) {
        BattleConnector socketHandler = PlayMangement.instance.socketHandler;
        Queue<SocketFormat.Player> heroshieldData = isHuman ? socketHandler.humanData : socketHandler.orcData;
        SocketFormat.Player data;
        if(heroshieldData.Count != 0) data = heroshieldData.Peek();
        else data = socketHandler.gameState.players.myPlayer(isHuman);
        SocketFormat.ShieldCharge shieldData = GetShieldData();

        Debug.Log("쉴드게이지!" + shieldData.shieldCount);

        if(data.shieldActivate && CheckShieldActivate(shieldData)) {
            ActiveShield();
        }
        else {
            if (GetComponent<SkillModules.guarded>() != null) {
                amount = 0;
            }

            if (HP.Value >= amount)
                HP.Value -= amount;
            else
                HP.Value -= HP.Value;

            if (HP.Value > 0)
                SetState(HeroState.HIT);

            if (shieldCount > 0) {
                if (shieldData == null)
                    shieldStack.Value = data.hero.shieldGauge;
                else {
                    EffectSystem.Instance.IncreaseShieldFeedBack(shieldFeedBack.transform.gameObject ,shieldData.shieldCount);
                    ChangeShieldStack(shieldStack.Value, shieldData.shieldCount);
                }
            }
        }
    }

    /// <summary>
    /// 영웅의 실드 게이지 조정
    /// </summary>
    /// <param name="amount"></param>
    public void ChangeShieldCount(int amount) {
        Logger.Log("ChangeShieldCount Method 호출");
        int newVal = shieldStack.Value + amount;
        if (newVal < 0) newVal = 0;
        else shieldStack.Value = newVal;
    }
    


    private bool CheckShieldActivate(SocketFormat.ShieldCharge shieldData) {
        if(shieldData == null) return true;
        if(shieldData.shieldCount == 0) return true;
        return (shieldStack.Value + shieldData.shieldCount) >= 8;
    }

    private SocketFormat.ShieldCharge GetShieldData() {
        BattleConnector socketHandler = PlayMangement.instance.socketHandler;
        
        if(socketHandler.shieldChargeQueue.Count != 0) {
            SocketFormat.ShieldCharge shieldData = socketHandler.shieldChargeQueue.Dequeue();
            string camp = isHuman ? "human" : "orc";
            if(shieldData.camp.CompareTo(camp) != 0) 
                Debug.LogError("서버에서 온 쉴드의 종족이 다릅니다.");
            Debug.Log("쉴드 게이지 발동 : " + JsonUtility.ToJson(shieldData));
            return shieldData;
        }
        else
            Debug.LogError("서버에서 온 쉴드 게이지가 없습니다!");
        return null;
    }

    public void ActiveShield() {
        GameObject shield = transform.Find("shield").gameObject;
        shield.SetActive(true);
        SetState(HeroState.ATTACK);
        if(PlayMangement.instance.heroShieldActive) return;
        PlayMangement.instance.heroShieldActive = true;
        FullShieldStack(shieldStack.Value);
        StartCoroutine(PlayMangement.instance.DrawSpecialCard(isHuman));
        shieldStack.Value = 0;
        shieldCount--;
        
    }

    public void DisableShield() {
        GameObject shield = transform.Find("shield").gameObject;
        shield.SetActive(false);
    }

    public void EffectForPlayer(int amount = 0, string skillId = null) {
        Vector3 position = new Vector3(transform.position.x, transform.position.y + 2.5f, transform.position.z);
        if (amount < 0) {
            if (skillId == "ac10021") {
                actionCall += MagicHit;
                EffectSystem.Instance.ShowEffectOnEvent(EffectSystem.EffectType.TREBUCHET, position, actionCall, transform);
                actionCall -= actionCall;
            }
            else
                EffectSystem.Instance.ShowEffect(EffectSystem.EffectType.EXPLOSION, position);
        }
        else if (amount > 0)
            EffectSystem.Instance.ShowEffect(EffectSystem.EffectType.BUFF, position);
        else
            return;
    }
    

    public void TakeIgnoreShieldDamage(int amount, bool isMagic = false, string skillId= null) {

        if (HP.Value >= amount)
            HP.Value -= amount;
        else
            HP.Value -= HP.Value;

        if (isMagic == true)
            EffectForPlayer(-amount, skillId);
        else
            Hit();
    }

    private void Hit() {
        SetState(HeroState.HIT);
    }

    private void MagicHit() {
        SetState(HeroState.HIT);
        StartCoroutine(PlayMangement.instance.cameraShake(0.8f, 3));
    }

    public void ReleaseTurn() {
        //if (isPlayer == true && PlayMangement.instance.skillAction == true) return;
        if (myTurn == true && !dragCard) {
            if (isPlayer) {
                PlayMangement.instance.releaseTurnBtn.gameObject.SetActive(false);
                buttonParticle.SetActive(false);
            }
            myTurn = false;
            PlayMangement.instance.GetPlayerTurnRelease();
            if(isHuman == PlayMangement.instance.player.isHuman)
                PlayMangement.instance.socketHandler.TurnOver();
        }
    }

    public void ActivePlayer() {
        activeCardMinCost = 100;
        myTurn = true;      
        if(isPlayer == true) {
            Transform cardSlot_1 = cdpm.transform;
            for (int i = 0; i < cardSlot_1.childCount; i++) {
                if (cardSlot_1.GetChild(i).childCount != 0) 
                    cardSlot_1.GetChild(i).GetChild(0).GetComponent<CardHandler>().ActivateCard();
            }
        }
        if (activeCardMinCost == 100) {
            if (isPlayer)
                buttonParticle.SetActive(true);
        }
        if (PlayMangement.instance.currentTurn != "BATTLE")
            PlayMangement.dragable = true;
    }

    public void ActiveOrcTurn() {
        activeCardMinCost = 100;
        string currentTurn = PlayMangement.instance.currentTurn;
        myTurn = true;
        if (isPlayer == true && currentTurn == "ORC") {
            Transform cardSlot_1 = cdpm.transform;
            for (int i = 0; i < cardSlot_1.childCount; i++) {
                if (cardSlot_1.GetChild(i).childCount != 0) {
                    if (cardSlot_1.GetChild(i).GetChild(0).GetComponent<CardHandler>().cardData.type == "unit")
                        cardSlot_1.GetChild(i).GetChild(0).GetComponent<CardHandler>().ActivateCard();
                    else
                        cardSlot_1.GetChild(i).GetChild(0).GetComponent<CardHandler>().DisableCard();
                }

            }
        }
        else if(isPlayer == true && currentTurn == "SECRET") {
            Transform cardSlot_1 = cdpm.transform;
            for (int i = 0; i < cardSlot_1.childCount; i++) {
                if (cardSlot_1.GetChild(i).childCount != 0) {
                    if (cardSlot_1.GetChild(i).GetChild(0).GetComponent<CardHandler>().cardData.type == "magic")
                        cardSlot_1.GetChild(i).GetChild(0).GetComponent<CardHandler>().ActivateCard();
                    else
                        cardSlot_1.GetChild(i).GetChild(0).GetComponent<CardHandler>().DisableCard();
                }

            }
        }
        if (activeCardMinCost == 100) {
            if (isPlayer)
                buttonParticle.SetActive(true);
        }
        if (PlayMangement.instance.currentTurn != "BATTLE")
            PlayMangement.dragable = true;
    }

    public void DisablePlayer() {
        myTurn = false;
        if (isPlayer == true) {
            Transform cardSlot_1 = cdpm.transform;

            for (int i = 0; i < cardSlot_1.childCount; i++) {
                if (cardSlot_1.GetChild(i).childCount != 0)
                    cardSlot_1.GetChild(i).GetChild(0).GetComponent<CardHandler>().DisableCard();
            }
        }
        if (isPlayer)
            buttonParticle.SetActive(false);
    }

    /// <summary>
    /// 내 핸드에 있는 해당 id의 카드들만 비활성화 시킴
    /// </summary>
    /// <param name="id"></param> 카드의 아이디
    /// <param name="active"></param> true 일시 카드 활성화, false면 비활성화
    public void SetThisCardAble(string id, bool active) {
        if (isPlayer == true) {
            Transform cardSlot_1 = playerUI.transform.Find("CardHand").GetChild(0);
            Transform cardSlot_2 = playerUI.transform.Find("CardHand").GetChild(1);
            for (int i = 0; i < cardSlot_1.childCount; i++) {
                if (cardSlot_1.GetChild(i).GetComponent<CardHandler>().cardID == id) {
                    if(active)
                        cardSlot_1.GetChild(i).GetChild(0).GetComponent<CardHandler>().ActivateCard();
                    else
                        cardSlot_1.GetChild(i).GetChild(0).GetComponent<CardHandler>().DisableCard();
                }
            }
            for (int i = 0; i < cardSlot_2.childCount; i++) {
                if (cardSlot_2.GetChild(i).GetComponent<CardHandler>().cardID == id) {
                    if (active)
                        cardSlot_2.GetChild(i).GetChild(0).GetComponent<CardHandler>().ActivateCard();
                    else
                        cardSlot_2.GetChild(i).GetChild(0).GetComponent<CardHandler>().DisableCard();
                }
            }
        }
    }

    public void ChangeShieldStack(int start, int amount) {
        shieldStack.Value += amount;

        if (shieldStack.Value > 8)
            shieldStack.Value = 8;

        shieldGauge.Initialize(false);
        shieldGauge.Update(0);
        //shieldGauge.Skeleton.SetSlotsToSetupPose();
        shieldGauge.AnimationState.ClearTrack(0);
        TrackEntry entry = new TrackEntry();
        

        for (int i = 1; i <= amount; i++)
            entry = shieldGauge.AnimationState.AddAnimation(0, (start + i).ToString(), false, 0);

       // entry.Complete += delegate (TrackEntry trackEntry) {  };       
    }

    public void FullShieldStack(int start) {
        int amount = 8 - start;
        shieldGauge.Initialize(false);
        shieldGauge.Update(0);
        shieldGauge.AnimationState.ClearTrack(0);
        TrackEntry entry = new TrackEntry();

        for (int i = 1; i < amount; i++)
            entry = shieldGauge.AnimationState.AddAnimation(0, (start + i).ToString(), false, 0);

        entry = shieldGauge.AnimationState.AddAnimation(0, "full", true, 0);
    }

    public void ConsumeShieldStack() {
        shieldGauge.Initialize(false);
        shieldGauge.Update(0);
        TrackEntry entry;
        entry = shieldGauge.AnimationState.SetAnimation(0, "0", false);
        sheildRemain.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, (3 - shieldCount).ToString(), false);
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
