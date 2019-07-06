using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckHandler : MonoBehaviour
{
    [SerializeField] DeckEditController deckEditCanvas;
    private string deckID;
    private bool isBasic = false;
    private bool isHuman;
    public string DECKID {
        get { return deckID; }
        set { deckID = value; }
    }

    public void SetDeck(dataModules.Deck deck, bool basic = false) {
        isBasic = basic;
        deckID = deck.id;
        if (deck.camp == "human") isHuman = true;
        else isHuman = false;
        Transform deckInfo = transform.Find("DeckInfo");
        deckInfo.gameObject.SetActive(true);
        deckInfo.Find("Deck/Portrait").GetComponent<Image>().sprite = AccountManager.Instance.resource.heroPortraite[deck._hero.id];
        deckInfo.Find("Deck/Name/Text").GetComponent<TMPro.TextMeshProUGUI>().text = deck.name;
        deckInfo.Find("Deck/Info/Text").GetComponent<TMPro.TextMeshProUGUI>().text = deck.cardTotalCount.ToString() + "/40";
    }

    public void OpenDeckButton() {
        if (isBasic) return;
        GameObject editButtons = transform.Find("DeckInfo/EditButtons").gameObject;
        if (editButtons.activeSelf)
            editButtons.SetActive(false);
        else
            editButtons.SetActive(true);
    }

    public void EditCustomDeck() {
        dataModules.Deck customDeck = null;
        foreach (dataModules.Deck deck in AccountManager.Instance.humanDecks.customDecks) {
            if (deckID == deck.id) {
                customDeck = deck;
                break;
            }
        }
        if(customDeck != null)
            deckEditCanvas.SetCustumDeckEdit(customDeck);
    }
}
