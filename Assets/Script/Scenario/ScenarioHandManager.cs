using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioHandManager : CardHandManager
{
    public override IEnumerator FirstDraw() {
        bool race = PlayMangement.instance.player.isHuman;        
        GameObject card;
        SocketFormat.Card socketCard = new SocketFormat.Card();

        socketCard.id = "ac10001";
        socketCard.type = "unit";

        if (socketCard.type == "unit")
            card = cardStorage.Find("UnitCards").GetChild(0).gameObject;
        else {
            card = cardStorage.Find("MagicCards").GetChild(0).gameObject;
        }

        card.transform.SetParent(firstDrawParent);
        card.SetActive(true);
        card.GetComponent<CardHandler>().DrawCard(socketCard.id, socketCard.itemId, true);

        if (socketCard.type == "magic") {
            card.transform.Find("Name/Text").GetComponent<TMPro.TextMeshProUGUI>().text = socketCard.name;
            AddMagicAttribute(ref card);
        }

        iTween.MoveTo(card, firstDrawParent.GetChild(firstDrawList.Count).position, 0.5f);
        iTween.RotateTo(card, new Vector3(0, 0, 0), 0.5f);
        firstDrawList.Add(card);
        //card.transform.localScale = new Vector3(1.1f, 1.1f, 1);
        yield return new WaitForSeconds(0.5f);
        card.transform.Find("ChangeButton").gameObject.SetActive(true);
        if (firstDrawList.Count == 4) {
            yield return new WaitForSeconds(0.5f);
            firstDrawParent.parent.Find("FinishButton").gameObject.SetActive(true);
        }

    }
}
