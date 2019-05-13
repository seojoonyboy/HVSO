using Bolt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool race;
    public int[,] placement = new int[2, 5] { {0,0,0,0,0 },
                                              {0,0,0,0,0 }};


    public GameObject card;


    // Start is called before the first frame update
    IEnumerator Start() {
        yield return new WaitForSeconds(3.0f);
        CustomEvent.Trigger(phaseManager, "EndTurn");
        yield return new WaitForSeconds(3.0f);
        CustomEvent.Trigger(phaseManager, "EndTurn");
        yield return new WaitForSeconds(3.0f);
        CustomEvent.Trigger(phaseManager, "EndTurn");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
