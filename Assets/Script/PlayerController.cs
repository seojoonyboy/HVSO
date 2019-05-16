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


    public ReactiveProperty<int> HP;
    public ReactiveProperty<int> resource = new ReactiveProperty<int>(2);    
    private ReactiveProperty<int> shieldStack = new ReactiveProperty<int>(0);
    private int shieldCount = 0;

    public bool getPlayerTurn {
        get { return myTurn; }
    }

    private void Start()
    {
        if(isPlayer == true && race == true) {
            playerUI.transform.Find("ReleaseTurnBtn").GetComponent<Image>().sprite = PlayMangement.instance.humanBtn;
            playerUI.transform.Find("PlayerResource").GetComponent<Image>().sprite = PlayMangement.instance.plantResourceIcon;
        }
        else if(isPlayer == true && race == false) {
            playerUI.transform.Find("ReleaseTurnBtn").GetComponent<Image>().sprite = PlayMangement.instance.orcBtn;
            playerUI.transform.Find("PlayerResource").GetComponent<Image>().sprite = PlayMangement.instance.zombieResourceIcon;
        }
        
        if(isPlayer == false && race == true) {
            playerUI.transform.Find("PlayerResource").GetComponent<Image>().sprite = PlayMangement.instance.plantResourceIcon;
        }
        else if (isPlayer == false && race == false) {
            playerUI.transform.Find("PlayerResource").GetComponent<Image>().sprite = PlayMangement.instance.zombieResourceIcon;
        }

    }

    public IEnumerator GenerateCard() {
        int i = 0;
        while(i < 5) {
            yield return new WaitForSeconds(0.5f);
            GameObject setCard = Instantiate(card);
            setCard.transform.SetParent(playerUI.transform.Find("CardSlot"));
            setCard.SetActive(true);

            GameObject enemyCard = Instantiate(PlayMangement.instance.enemyPlayer.back);
            enemyCard.transform.SetParent(PlayMangement.instance.enemyPlayer.playerUI.transform.Find("CardSlot"));
            enemyCard.SetActive(true);
            i++;            
        }
        StartCoroutine(PlayMangement.instance.WaitSecond());
    }


    public void SetPlayerStat(int hp) {
        HP = new ReactiveProperty<int>(hp);

        if(race == true) {

        }
        else {

        }

        ObserverText();
    }

    private void ObserverText() {
        Text HPText = playerUI.transform.Find("PlayerHealth").Find("Health").Find("Text").GetComponent<Text>();
        Text resourceText = playerUI.transform.Find("PlayerResource").Find("Text").GetComponent<Text>();
        Image shieldImage = playerUI.transform.Find("PlayerHealth").Find("Shield").GetComponent<Image>();

        HP.SubscribeToText(HPText).AddTo(PlayMangement.instance.transform.gameObject);
        resource.SubscribeToText(resourceText).AddTo(PlayMangement.instance.transform.gameObject);
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
            Transform cardSlot = playerUI.transform.Find("CardSlot");
            for (int i = 0; i<cardSlot.childCount; i++) {
                cardSlot.GetChild(i).GetComponent<CardHandler>().ActivateCard();
            }            
        }
    }

    public void DisablePlayer() {
        myTurn = false;      
        if (isPlayer == true) {
            Transform cardSlot = playerUI.transform.Find("CardSlot");
            for (int i = 0; i < cardSlot.childCount; i++) {
                cardSlot.GetChild(i).GetComponent<CardHandler>().DisableCard();
            }
        }
    }



}
