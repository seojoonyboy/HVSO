using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public bool race;
    public bool isPlayer;
    public int[,] placement = new int[2,5] { {0,0,0,0,0 },
                                              {0,0,0,0,0 }};
    public GameObject card;
    public GameObject playerUI;
    public int HP = 20;
    public int resource = 0;


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
    
    void UpdateResource() {
        playerUI.transform.Find("PlayerResource").GetChild(0).GetComponent<Text>().text = resource.ToString();
    }


}
