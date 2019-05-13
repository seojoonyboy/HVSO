using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayMangement : MonoBehaviour
{
    public PlayerController player, enemyPlayer;

    public GameObject plantCard, plantBack;
    public GameObject zombieCard, zombieBack;


    Camera cam;
    public GameObject uiSlot;

    public static PlayMangement instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }
    private void OnDestroy()
    {
        instance = null; 
    }

    

    private void Start()
    {
        cam = Camera.main;
        player.card = (player.race == true) ? plantCard : zombieCard;
        enemyPlayer.card = (enemyPlayer.race == true) ? plantCard : zombieCard;
        

        for(int i = 0; i< player.transform.childCount; i++)
        {
            for(int j = 0; j < player.transform.GetChild(i).childCount; j++)
            {
                GameObject slot = Instantiate(uiSlot);
                slot.transform.SetParent(player.playerUI.transform.parent.Find("IngamePanel").Find("PlayerSlot").GetChild(i));
                slot.transform.position = cam.WorldToScreenPoint(player.transform.GetChild(i).GetChild(j).position);
            }
        }



    }




}
