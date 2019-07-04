using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckHandler : MonoBehaviour
{
    private int deckID;

    public int DECKID {
        get { return deckID; }
        set { deckID = value; }
    }

    public void SetDeck(dataModules.Deck deck) {
        Transform deckInfo = transform.Find("DeckInfo");
        deckInfo.gameObject.SetActive(true);
        deckInfo.Find("Deck/Portrait").GetComponent<Image>().sprite = AccountManager.Instance.resource.heroPortraite[deck._hero.id];
        deckInfo.Find("Deck/Name/Text").GetComponent<TMPro.TextMeshProUGUI>().text = deck.name;
        deckInfo.Find("Deck/Info/Text").GetComponent<TMPro.TextMeshProUGUI>().text = deck.cardTotalCount.ToString() + "/40";
    }

    public void OpenDeckButton() {
        GameObject editButtons = transform.Find("DeckInfo/EditButtons").gameObject;
        if (editButtons.activeSelf)
            editButtons.SetActive(false);
        else
            editButtons.SetActive(true);
    }
}
