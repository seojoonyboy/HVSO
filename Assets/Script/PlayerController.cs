using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class PlayerController : MonoBehaviour
{
    public bool race;
    public bool isPlayer;
    public int[,] placement = new int[2,5] { {0,0,0,0,0 },
                                              {0,0,0,0,0 }};
    public GameObject card;
    public GameObject playerUI;


    public ReactiveProperty<int> HP;
    public ReactiveProperty<int> resource = new ReactiveProperty<int>(2);    
    private ReactiveProperty<int> shieldStack = new ReactiveProperty<int>(0);
    private int shieldCount = 0;

    private void Start()
    {
        for(int i = 0; i < 5; i++) {
            if(isPlayer == true) {
                GameObject getCard = Instantiate(card);
                getCard.transform.SetParent(playerUI.transform.Find("CardSlot"));
            }
            else {
                GameObject getCard = (race == true) ? Instantiate(PlayMangement.instance.plantBack) : Instantiate(PlayMangement.instance.zombieBack);
                getCard.transform.SetParent(playerUI.transform.Find("CardSlot"));
            }
        }
        playerUI.transform.Find("PlayerResource").GetComponent<Image>().sprite = (race == true) ? PlayMangement.instance.plantResourceIcon : PlayMangement.instance.zombieResourceIcon;
    }

    public void SetPlayerStat(int hp) {
        HP = new ReactiveProperty<int>(hp);
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



}
