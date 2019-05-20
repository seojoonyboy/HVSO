using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class PlayerController : MonoBehaviour
{
    public bool race;
    public bool isPlayer;
    private bool myTurn = false;
    public int[,] placement = new int[2,5] { {0,0,0,0,0 },
                                              {0,0,0,0,0 }};
    public GameObject card;
    public GameObject back;
    public GameObject playerUI;
    [SerializeField] public CardDeckPositionManager cdpm;

    public ReactiveProperty<int> HP;
    public ReactiveProperty<int> resource = new ReactiveProperty<int>(2);
    public ReactiveProperty<bool> isPicking = new ReactiveProperty<bool>(false);
    private ReactiveProperty<int> shieldStack = new ReactiveProperty<int>(0);
    private int shieldCount = 0;

    public bool getPlayerTurn {
        get { return myTurn; }
    }

    private void Start()
    {
        SetUnitSlot();
    }

    private void SetUnitSlot() {
        for(int i = 0; i< transform.childCount; i++) {
            for(int j = 0; j<transform.GetChild(i).childCount; j++) {                
                transform.GetChild(i).GetChild(j).position = new Vector3(PlayMangement.instance.backGround.transform.GetChild(j).position.x, transform.GetChild(i).GetChild(j).position.y, 0);

                if(isPlayer == true) {
                    GameObject slot = Instantiate(PlayMangement.instance.uiSlot);
                    slot.transform.SetParent(playerUI.transform.parent.Find("IngamePanel").Find("PlayerSlot").GetChild(i));
                    slot.transform.position = Camera.main.WorldToScreenPoint(transform.GetChild(i).GetChild(j).position);
                }                
            }
        }
    }

    public IEnumerator GenerateCard() {
        int i = 0;
        while(i < 10) {
            yield return new WaitForSeconds(0.5f);
            GameObject setCard = Instantiate(card);
            DrawPlayerCard(setCard);

            GameObject enemyCard = Instantiate(PlayMangement.instance.enemyPlayer.back);
            enemyCard.transform.SetParent(PlayMangement.instance.enemyPlayer.playerUI.transform.Find("CardSlot"));
            enemyCard.SetActive(true);
            i++;            
        }
        StartCoroutine(PlayMangement.instance.WaitSecond());
    }

    public void EndTurnDraw() {
        if (PlayMangement.instance.isGame == false) return;

        GameObject setCard = Instantiate(card);
        DrawPlayerCard(setCard);
        setCard.GetComponent<CardHandler>().DisableCard();

        GameObject enemyCard = Instantiate(PlayMangement.instance.enemyPlayer.back);
        enemyCard.transform.SetParent(PlayMangement.instance.enemyPlayer.playerUI.transform.Find("CardSlot"));
        enemyCard.SetActive(true);
    }

    public void DrawPlayerCard(GameObject card) {
        cdpm.AddCard(card);
        if (race == true)
            card.GetComponent<CardHandler>().DrawCard("ac10009");
        else
            card.GetComponent<CardHandler>().DrawCard("ac10014");
        card.SetActive(true);
    }


    public void SetPlayerStat(int hp) {
        HP = new ReactiveProperty<int>(hp);
        ObserverText();
    }

    private void ObserverText() {
        Text HPText = playerUI.transform.Find("PlayerHealth/Health/Text").GetComponent<Text>();
        Text resourceText = playerUI.transform.Find("PlayerResource/Text").GetComponent<Text>();
        Image shieldImage = playerUI.transform.Find("PlayerHealth/Shield/Gage").GetComponent<Image>();

        var ObserveHP = HP.SubscribeToText(HPText).AddTo(PlayMangement.instance.transform.gameObject);
        var ObserveResource = resource.SubscribeToText(resourceText).AddTo(PlayMangement.instance.transform.gameObject);
        var ObserveCardPick = isPicking.Subscribe(_ => HighLightCardSlot()).AddTo(PlayMangement.instance.transform.gameObject);
        var ObserveShield = shieldStack.Subscribe(_ => shieldImage.fillAmount = (float)shieldStack.Value / 8).AddTo(PlayMangement.instance.transform.gameObject);

        var gameOverDispose = HP.Where(x => x <= 0)
                              .Subscribe(_ => {
                                               PlayerDefeat();
                                               ObserveHP.Dispose();
                                               ObserveResource.Dispose();
                                               ObserveCardPick.Dispose();
                                               ObserveShield.Dispose(); })
                              .AddTo(PlayMangement.instance.transform.gameObject);
    }

    public void UpdateHealth() {
        HP.Value += 2;
    }
    


    public void PlayerTakeDamage(int amount) {
        if (shieldStack.Value < 8) {
            if (HP.Value >= amount) {
                HP.Value -= amount;
                shieldStack.Value++;
            }
            else
                HP.Value = 0;
        }
        else {
            HP.Value += 2;
            DrawSpeicalCard();
            shieldStack.Value = 0;
        } 
    }

    public void DrawSpeicalCard() {

    }

    public void ReleaseTurn() {
        if (myTurn == true)
            PlayMangement.instance.GetPlayerTurnRelease();
    }


    public void ActivePlayer() {
        myTurn = true;      
        if(isPlayer == true) {
            Transform cardSlot_1 = playerUI.transform.Find("CardHand").GetChild(0);
            Transform cardSlot_2 = playerUI.transform.Find("CardHand").GetChild(1);
            for (int i = 0; i< cardSlot_1.childCount; i++) {
                cardSlot_1.GetChild(i).GetComponent<CardHandler>().ActivateCard();
            }
            for (int i = 0; i < cardSlot_2.childCount; i++) {
                cardSlot_2.GetChild(i).GetComponent<CardHandler>().ActivateCard();
            }
        }
    }

    public void DisablePlayer() {
        myTurn = false;
        if (isPlayer == true) {
            Transform cardSlot_1 = playerUI.transform.Find("CardHand").GetChild(0);
            Transform cardSlot_2 = playerUI.transform.Find("CardHand").GetChild(1);
            for (int i = 0; i < cardSlot_1.childCount; i++) {
                cardSlot_1.GetChild(i).GetComponent<CardHandler>().DisableCard();
            }
            for (int i = 0; i < cardSlot_2.childCount; i++) {
                cardSlot_2.GetChild(i).GetComponent<CardHandler>().DisableCard();
            }
        }
    }

    public void PlayerDefeat() {
        PlayMangement.instance.isGame = false;
    }


    public void HighLightCardSlot() {
        Transform slotToUI = playerUI.transform.parent.Find("IngamePanel").Find("PlayerSlot");

        if (myTurn == true) {
            if (isPicking.Value == true) {
                for (int i = 0; i < slotToUI.GetChild(0).childCount; i++) {
                    if (transform.GetChild(0).GetChild(i).childCount == 0) {
                        slotToUI.GetChild(0).GetChild(i).GetComponent<Image>().enabled = true;
                    }
                }
            }
            else  {
                for (int i = 0; i < slotToUI.GetChild(0).childCount; i++) {
                    if (transform.GetChild(0).GetChild(i).childCount == 0) {
                        slotToUI.GetChild(0).GetChild(i).GetComponent<Image>().enabled = false;
                    }
                }
            }
        }

    }



}
