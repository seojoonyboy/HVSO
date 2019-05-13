using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool race;
    public int[,] placement = new int[2,5] { {0,0,0,0,0 },
                                              {0,0,0,0,0 }};
    public GameObject card;
    public GameObject playerUI;
    public int deckCount = 0;
    public int HP = 20;


    private void Start()
    {
        
    }    
}
