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

    public GameObject backLine;
    public GameObject frontLine;


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
        UnitDropManager.Instance.SetUnitDropPos();
    }

    public IEnumerator GenerateCard() {
        int i = 0;
        while(i < 4) {
            yield return new WaitForSeconds(0.3f);
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

        cdpm.AddCard();

        GameObject enemyCard = Instantiate(PlayMangement.instance.enemyPlayer.back);
        enemyCard.transform.SetParent(PlayMangement.instance.enemyPlayer.playerUI.transform.Find("CardSlot"));
        enemyCard.transform.localScale = new Vector3(1, 1, 1);
        enemyCard.SetActive(true);
    }

    public void DrawPlayerCard(GameObject card) {
        cdpm.AddCard();
        if (race == true)
            card.GetComponent<CardHandler>().DrawCard("ac1000" + Random.Range(1,5));
        else
            card.GetComponent<CardHandler>().DrawCard("ac10012");
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
        var ObserveShield = shieldStack.Subscribe(_ => shieldImage.fillAmount = (float)shieldStack.Value / 8).AddTo(PlayMangement.instance.transform.gameObject);

        var gameOverDispose = HP.Where(x => x <= 0)
                              .Subscribe(_ => {
                                               PlayMangement.instance.GetBattleResult();
                                               ObserveHP.Dispose();
                                               ObserveResource.Dispose();
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
    

    



}
