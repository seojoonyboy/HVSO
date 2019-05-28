using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using TMPro;
using System;

public class PlayerController : MonoBehaviour
{
    public bool race;
    public bool isPlayer;
    public bool isMulligan = true;
    private bool myTurn = false;
    public int[,] placement = new int[2,5] { {0,0,0,0,0 },
                                              {0,0,0,0,0 }};
    public GameObject card;
    public GameObject back;
    public GameObject playerUI;
    [SerializeField] public CardDeckPositionManager cdpm;

    public GameObject backLine;
    public GameObject frontLine;
    public bool dragCard = false;


    public ReactiveProperty<int> HP;
    public ReactiveProperty<int> resource = new ReactiveProperty<int>(2);
    public ReactiveProperty<bool> isPicking = new ReactiveProperty<bool>(false);
    private ReactiveProperty<int> shieldStack = new ReactiveProperty<int>(0);
    private int shieldCount = 0;

    protected HeroSpine heroSpine;

    public enum HeroState {
        IDLE,
        ATTACK,
        HIT
    }

    public bool getPlayerTurn {
        get { return myTurn; }
    }

    public void Init() {
        if (transform.childCount > 2)
            heroSpine = transform.Find("HeroSkeleton").GetComponent<HeroSpine>();

        Debug.Log(Camera.main.aspect);
    }

    private void Start()
    {
        Init();
        UnitDropManager.Instance.SetUnitDropPos();
    }

    public IEnumerator GenerateCard() {
        int i = 0;
        while (i < 5) {
            yield return new WaitForSeconds(0.3f);
            if(i < 4)
                StartCoroutine(cdpm.FirstDraw());
            

            GameObject enemyCard = Instantiate(PlayMangement.instance.enemyPlayer.back);
            enemyCard.transform.SetParent(PlayMangement.instance.enemyPlayer.playerUI.transform.Find("CardSlot"));
            enemyCard.transform.localScale = new Vector3(1, 1, 1);
            enemyCard.SetActive(true);
            i++;            
        }
    }

    public void EndTurnDraw() {
        if (PlayMangement.instance.isGame == false) return;

        //cdpm.AddCard();

        GameObject enemyCard = Instantiate(PlayMangement.instance.enemyPlayer.back);
        enemyCard.transform.SetParent(PlayMangement.instance.enemyPlayer.playerUI.transform.Find("CardSlot"));
        enemyCard.transform.localScale = new Vector3(1, 1, 1);
        enemyCard.SetActive(true);
    }

    public void DrawPlayerCard(GameObject card) {
        cdpm.AddCard();
        string cardID;
        if (race == true) {
            cardID = "ac1000";
            card.GetComponent<CardHandler>().DrawCard(cardID + UnityEngine.Random.Range(1, 5));
        }
        else {
            cardID = "ac10012";
            card.GetComponent<CardHandler>().DrawCard(cardID);
        }
        card.SetActive(true);
    }


    public void SetPlayerStat(int hp) {
        HP = new ReactiveProperty<int>(hp);
        ObserverText();
    }

    private void ObserverText() {       
        Image shieldImage = playerUI.transform.Find("PlayerHealth/Shield/Gage").GetComponent<Image>();

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
        TextMeshProUGUI HPText = playerUI.transform.Find("PlayerHealth/Health/Text").GetComponent<TextMeshProUGUI>();
        HPText.text = HP.Value.ToString();
    }

    private void ChangedResource() {
        TextMeshProUGUI resourceText = playerUI.transform.Find("PlayerResource/Text").GetComponent<TextMeshProUGUI>();
        resourceText.text = resource.Value.ToString();
    }





    public void UpdateHealth() {
        HP.Value += 2;
    }
    


    public void PlayerTakeDamage(int amount) {
        if (shieldStack.Value < 7) {
            if (HP.Value >= amount) {
                HP.Value -= amount;
                SetState(HeroState.HIT);
                shieldStack.Value++;
            }
            else
                HP.Value = 0;
        }
        else {
            DrawSpeicalCard();
            shieldStack.Value = 0;
        } 
    }

    public void DrawSpeicalCard() {

    }

    public void ReleaseTurn() {
        if (myTurn == true && !dragCard)
            PlayMangement.instance.GetPlayerTurnRelease();
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
