using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioHandManager : CardHandManager
{
    void Start() {
        firstDraw = true;
        cardList = new List<GameObject>();
        firstDrawList = new List<GameObject>();
        clm = PlayMangement.instance.cardInfoCanvas.Find("CardInfoList").GetComponent<CardListManager>();
        //handCardNum = transform.parent.Find("PlayerCardNum/Value").GetComponent<TMPro.TextMeshProUGUI>();
    }
}
