using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using TMPro;
using System;
using Bolt;

public class PlayerController : MonoBehaviour
{
    public bool isHuman;
    public bool isPlayer;
    public bool isMulligan = true;
    private bool myTurn = false;
    public int[,] placement = new int[2,5] { {0,0,0,0,0 },
                                              {0,0,0,0,0 }};
    public GameObject card;
    public GameObject back;
    public GameObject playerUI;
    [SerializeField] public CardHandDeckManager cdpm;

    public GameObject backLine;
    public GameObject frontLine;
    public bool dragCard = false;

    public Vector3 unitClosePosition;
    public Vector3 wallPosition;

    public ReactiveProperty<int> HP;
    public ReactiveProperty<int> resource = new ReactiveProperty<int>(2);
    public ReactiveProperty<bool> isPicking = new ReactiveProperty<bool>(false);
    private ReactiveProperty<int> shieldStack = new ReactiveProperty<int>(0);
    private int shieldCount = 0;

    protected HeroSpine heroSpine;
    public List<Transform> dropableLines;
    public enum HeroState {
        IDLE,
        ATTACK,
        HIT
    }

    public bool getPlayerTurn {
        get { return myTurn; }
    }

    public void Init() {
        string race = Variables.Saved.Get("SelectedRace").ToString();
        if (race == "HUMAN") isHuman = isPlayer;
        else isHuman = !isPlayer;
        if (isHuman) {
            Instantiate(AccountManager.Instance.resource.raceUiPrefabs["HUMAN"][0], playerUI.transform.Find("PlayerHealth"));
            Instantiate(AccountManager.Instance.resource.raceUiPrefabs["HUMAN"][1], playerUI.transform.Find("PlayerResource"));
        }
        else {
            Instantiate(AccountManager.Instance.resource.raceUiPrefabs["ORC"][0], playerUI.transform.Find("PlayerHealth"));
            Instantiate(AccountManager.Instance.resource.raceUiPrefabs["ORC"][1], playerUI.transform.Find("PlayerResource"));
        }

        if(isHuman == true) {
            string heroID = "h10001";
            GameObject hero = Instantiate(AccountManager.Instance.resource.heroSkeleton[heroID], transform);
            hero.transform.SetAsLastSibling();
            heroSpine = hero.GetComponent<HeroSpine>();

            if (isPlayer == true) {
                hero.transform.localScale = new Vector3(-1, 1, 1);
                heroSpine.GetComponent<MeshRenderer>().sortingOrder = 12;
            }
            else
                heroSpine.GetComponent<MeshRenderer>().sortingOrder = 8;

            hero.transform.localScale = PlayMangement.instance.backGround.transform.localScale;
        }
        else {
            string heroID = "h10002";
            GameObject hero = Instantiate(AccountManager.Instance.resource.heroSkeleton[heroID], transform);
            hero.transform.SetAsLastSibling();
            heroSpine = hero.GetComponent<HeroSpine>();
            

            if (isPlayer == true) {
                hero.transform.localScale = new Vector3(-1, 1, 1);
                heroSpine.GetComponent<MeshRenderer>().sortingOrder = 12;
            }
            else
                heroSpine.GetComponent<MeshRenderer>().sortingOrder = 8;

            hero.transform.localScale = PlayMangement.instance.backGround.transform.localScale;
        }


        shieldCount = 3;
        Debug.Log(heroSpine);
    }
    

    private void Start()
    {
        Init();
        CardDropManager.Instance.SetUnitDropPos();
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
        Image shieldImage = playerUI.transform.Find("PlayerHealth").GetChild(0).GetChild(0).GetChild(1).GetComponent<Image>();

        var ObserveHP = HP.Subscribe(_=> ChangedHP()).AddTo(PlayMangement.instance.transform.gameObject);
        var ObserveResource = resource.Subscribe(_=> ChangedResource()).AddTo(PlayMangement.instance.transform.gameObject);
        var ObserveShield = shieldStack.Subscribe(_ => shieldImage.fillAmount = (float)shieldStack.Value / 8).AddTo(PlayMangement.instance.transform.gameObject);
        var gameOverDispose = HP.Where(x => x <= 0)
                              .Subscribe(_ => {
                                               PlayMangement.instance.GetBattleResult();
                                               ObserveHP.Dispose();
                                               ObserveResource.Dispose();
                                               ObserveShield.Dispose(); })
                              .AddTo(PlayMangement.instance.transform.gameObject);        
    }

    private void ChangedHP() {
        TextMeshProUGUI HPText = playerUI.transform.Find("PlayerHealth").GetChild(0).GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
        HPText.text = HP.Value.ToString();
    }

    private void ChangedResource() {
        TextMeshProUGUI resourceText = playerUI.transform.Find("PlayerResource").GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        resourceText.text = resource.Value.ToString();
    }

    public void UpdateHealth() {
        HP.Value += 2;
    }

    public void PlayerTakeDamage(int amount) {
        Queue<SocketFormat.Hero> heroShildData = isHuman ? PlayMangement.instance.socketHandler.humanData : PlayMangement.instance.socketHandler.orcData;
        SocketFormat.Hero data;
        if(heroShildData.Count != 0) data = heroShildData.Peek();
        else data = PlayMangement.instance.socketHandler.gameState.players.myPlayer(isHuman).hero;

        if (shieldStack.Value < 7) {
            if (HP.Value >= amount) {
                HP.Value -= amount;
                SetState(HeroState.HIT);
                if (shieldCount > 0) shieldStack.Value = data.shildGauge;
            }
            else HP.Value = 0;
        }
        else if(shieldCount != data.shildCount) {
            shieldStack.Value = 8;
            PlayMangement.instance.heroShieldActive = true;
            StartCoroutine(PlayMangement.instance.DrawSpecialCard(isHuman));
            shieldStack.Value = 0;
            shieldCount--;
        }
    }

    public void ReleaseTurn() {
        if (myTurn == true && !dragCard) {
            PlayMangement.instance.GetPlayerTurnRelease();
            if(isHuman == PlayMangement.instance.player.isHuman)
                PlayMangement.instance.socketHandler.TurnOver();
        }
    }

    public void ActivePlayer() {
        myTurn = true;      
        if(isPlayer == true) {
            Transform cardSlot_1 = playerUI.transform.Find("CardHand").GetChild(0);
            Transform cardSlot_2 = playerUI.transform.Find("CardHand").GetChild(1);
            for (int i = 0; i< cardSlot_1.childCount; i++) {
                if (cardSlot_1.GetChild(i).gameObject.activeSelf)
                    cardSlot_1.GetChild(i).GetChild(0).GetComponent<CardHandler>().ActivateCard();
            }
            for (int i = 0; i < cardSlot_2.childCount; i++) {
                if (cardSlot_2.GetChild(i).gameObject.activeSelf)
                    cardSlot_2.GetChild(i).GetChild(0).GetComponent<CardHandler>().ActivateCard();
            }
        }
    }

    public void ActiveOrcTurn() {
        myTurn = true;
        if (isPlayer == true) {
            Transform cardSlot_1 = playerUI.transform.Find("CardHand").GetChild(0);
            Transform cardSlot_2 = playerUI.transform.Find("CardHand").GetChild(1);
            for (int i = 0; i < cardSlot_1.childCount; i++) {
                if (cardSlot_1.GetChild(i).gameObject.activeSelf && cardSlot_1.GetChild(i).GetChild(0).GetComponent<CardHandler>().cardData.type == "unit")
                    cardSlot_1.GetChild(i).GetChild(0).GetComponent<CardHandler>().ActivateCard();
            }
            for (int i = 0; i < cardSlot_2.childCount; i++) {
                if (cardSlot_2.GetChild(i).gameObject.activeSelf && cardSlot_2.GetChild(i).GetChild(0).GetComponent<CardHandler>().cardData.type == "unit")
                    cardSlot_2.GetChild(i).GetChild(0).GetComponent<CardHandler>().ActivateCard();
            }
        }
    }

    public void ActiveOrcSpecTurn() {
        myTurn = true;
        if (isPlayer == true) {
            Transform cardSlot_1 = playerUI.transform.Find("CardHand").GetChild(0);
            Transform cardSlot_2 = playerUI.transform.Find("CardHand").GetChild(1);
            for (int i = 0; i < cardSlot_1.childCount; i++) {
                if (cardSlot_1.GetChild(i).gameObject.activeSelf && cardSlot_1.GetChild(i).GetChild(0).GetComponent<CardHandler>().cardData.type == "magic")
                    cardSlot_1.GetChild(i).GetChild(0).GetComponent<CardHandler>().ActivateCard();
            }
            for (int i = 0; i < cardSlot_2.childCount; i++) {
                if (cardSlot_2.GetChild(i).gameObject.activeSelf && cardSlot_2.GetChild(i).GetChild(0).GetComponent<CardHandler>().cardData.type == "magic")
                    cardSlot_2.GetChild(i).GetChild(0).GetComponent<CardHandler>().ActivateCard();
            }
        }
    }

    public void DisablePlayer() {
        myTurn = false;
        if (isPlayer == true) {
            Transform cardSlot_1 = playerUI.transform.Find("CardHand").GetChild(0);
            Transform cardSlot_2 = playerUI.transform.Find("CardHand").GetChild(1);
            for (int i = 0; i < cardSlot_1.childCount; i++) {
                if(cardSlot_1.GetChild(i).gameObject.activeSelf)
                    cardSlot_1.GetChild(i).GetChild(0).GetComponent<CardHandler>().DisableCard();
            }
            for (int i = 0; i < cardSlot_2.childCount; i++) {
                if (cardSlot_2.GetChild(i).gameObject.activeSelf)
                    cardSlot_2.GetChild(i).GetChild(0).GetComponent<CardHandler>().DisableCard();
            }
        }
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
        }
    }
}
