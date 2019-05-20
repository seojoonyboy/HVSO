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
        //if(isPlayer == true && race == true) {
        //    playerUI.transform.Find("ReleaseTurnBtn").GetComponent<Image>().sprite = PlayMangement.instance.humanBtn;
        //    playerUI.transform.Find("PlayerResource").GetComponent<Image>().sprite = PlayMangement.instance.plantResourceIcon;
        //}
        //else if(isPlayer == true && race == false) {
        //    playerUI.transform.Find("ReleaseTurnBtn").GetComponent<Image>().sprite = PlayMangement.instance.orcBtn;
        //    playerUI.transform.Find("PlayerResource").GetComponent<Image>().sprite = PlayMangement.instance.zombieResourceIcon;
        //}

        //if(isPlayer == false && race == true) {
        //    playerUI.transform.Find("PlayerResource").GetComponent<Image>().sprite = PlayMangement.instance.plantResourceIcon;
        //}
        //else if (isPlayer == false && race == false) {
        //    playerUI.transform.Find("PlayerResource").GetComponent<Image>().sprite = PlayMangement.instance.zombieResourceIcon;
        //}

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

        HP.SubscribeToText(HPText).AddTo(PlayMangement.instance.transform.gameObject);
        resource.SubscribeToText(resourceText).AddTo(PlayMangement.instance.transform.gameObject);
        isPicking.Subscribe(_ => HighLightCardSlot()).AddTo(PlayMangement.instance.transform.gameObject);
        shieldStack.Subscribe(_ => shieldImage.fillAmount = (float)shieldStack.Value / 8 ).AddTo(PlayMangement.instance.transform.gameObject);
    }

    public void UpdateHealth() {
        HP.Value += 2;
    }

    public void PlayerTakeDamage(int amount) {
        if (shieldStack.Value < 8) {
            HP.Value -= amount;
            shieldStack.Value++;
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
