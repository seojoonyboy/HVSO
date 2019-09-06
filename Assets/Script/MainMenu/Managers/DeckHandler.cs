using System;
using System.Collections;
using System.Collections.Generic;
using BestHTTP;
using UnityEngine;
using UnityEngine.UI;

public class DeckHandler : MonoBehaviour
{
    [SerializeField] DeckEditController deckEditCanvas;
    [SerializeField] TemplateMenu templateCanvas;
    private string deckID;
    private bool isBasic = false;
    private bool isHuman;
    public dataModules.Deck templateDeck;
    public string DECKID {
        get { return deckID; }
        set { deckID = value; }
    }

    public void InitDeck() {
        transform.Find("HeroImg").GetComponent<Image>().sprite = AccountManager.Instance.resource.deckPortraite["empty"];
        transform.Find("CardNum").gameObject.SetActive(false);
        transform.Find("DeckName").gameObject.SetActive(false);
    }

    public void SetNewDeck(dataModules.Deck deck) {
        deckID = deck.id;
        if (deck.camp == "human") isHuman = true;
        else isHuman = false;
        transform.Find("HeroImg").GetComponent<Image>().sprite = AccountManager.Instance.resource.deckPortraite[deck.heroId];
        transform.Find("CardNum").gameObject.SetActive(true);
        transform.Find("CardNum/Value").GetComponent<TMPro.TextMeshProUGUI>().text = deck.totalCardCount.ToString() + "/";
        transform.Find("DeckName").gameObject.SetActive(true);
        transform.Find("DeckName").GetComponent<TMPro.TextMeshProUGUI>().text = deck.name.ToString() + "/";
    }

    public void SetDeck(dataModules.Deck deck, bool basic = false) {
        isBasic = basic;
        deckID = deck.id;
        if (deck.camp == "human") isHuman = true;
        else isHuman = false;
        Transform deckInfo = transform.Find("DeckInfo");
        deckInfo.gameObject.SetActive(true);
        deckInfo.Find("Deck/Portrait").GetComponent<Image>().sprite = AccountManager.Instance.resource.heroPortraite[deck.heroId];
        deckInfo.Find("Deck/Name/Text").GetComponent<TMPro.TextMeshProUGUI>().text = deck.name;
        deckInfo.Find("Deck/Info/Text").GetComponent<TMPro.TextMeshProUGUI>().text = deck.totalCardCount.ToString() + "/40";
    }

    public void SetTemplateDeck(dataModules.Deck deck) {
        templateDeck = deck;
        deckID = deck.id;
        if (deck.camp == "human") isHuman = true;
        else isHuman = false;
        Transform deckInfo = transform.Find("DeckInfo");
        deckInfo.Find("Portrait").GetComponent<Image>().sprite = AccountManager.Instance.resource.heroPortraite[deck.heroId];
        deckInfo.Find("DeckName").GetComponent<TMPro.TextMeshProUGUI>().text = deck.name;
        deckInfo.Find("Capacity").GetComponent<TMPro.TextMeshProUGUI>().text = deck.totalCardCount.ToString() + "/40";
    }

    public void OpenDeckButton() {
        if (isBasic) return;
        GameObject editButtons = transform.Find("DeckInfo/EditButtons").gameObject;
        if (editButtons.activeSelf)
            editButtons.SetActive(false);
        else
            editButtons.SetActive(true);
    }

    public void CloseDeckButton() {
        if (isBasic) return;
        GameObject editButtons = transform.Find("DeckInfo/EditButtons").gameObject;
        editButtons.SetActive(false);
    }

    public void SelectTemplateDeck() {
        transform.Find("Selected").gameObject.SetActive(true);
        templateCanvas.selectedDeck = this;
        templateCanvas.transform.Find("CancelSelect").gameObject.SetActive(true);
        templateCanvas.transform.Find("DeckEditBtn").gameObject.SetActive(true);
    }

    public void CancelSelect() {
        transform.Find("Selected").gameObject.SetActive(false);
        templateCanvas.selectedDeck = null;
    }

    public void EditCustomDeck() {
        dataModules.Deck customDeck = null;
        if (isHuman) {
            foreach (dataModules.Deck deck in AccountManager.Instance.humanDecks) {
                if (deckID == deck.id) {
                    customDeck = deck;
                    break;
                }
            }
        }
        else {
            foreach (dataModules.Deck deck in AccountManager.Instance.orcDecks) {
                if (deckID == deck.id) {
                    customDeck = deck;
                    break;
                }
            }
        }
        if(customDeck != null)
            deckEditCanvas.SetCustumDeckEdit(customDeck);
        deckEditCanvas.gameObject.SetActive(true);
        deckEditCanvas.GetComponent<DeckEditController>().RefreshLine();
        transform.Find("DeckInfo/EditButtons").gameObject.SetActive(false);

        FindObjectOfType<HUDController>().SetHeader(HUDController.Type.HIDE);
    }

    public void DeleteButton() {
        if (AccountManager.Instance == null) return;
        AccountManager.Instance.RequestDeckRemove(DECKID, OnRemoved);
        transform.Find("DeckInfo/EditButtons").gameObject.SetActive(false);
        Transform deckInfo = transform.Find("DeckInfo");
        deckInfo.gameObject.SetActive(false);
        transform.SetAsLastSibling();
        gameObject.SetActive(false);

        if (transform.parent.GetChild(2).gameObject.activeSelf && transform.parent.GetChild(2).Find("DeckInfo").gameObject.activeSelf)
            gameObject.SetActive(true);
    }

    private void OnRemoved(HTTPRequest originalRequest, HTTPResponse response) {
        if (response.StatusCode == 200) {
            transform.GetComponentInParent<DeckSettingManager>()
                .menuSceneController
                .decksLoader
                .Load();
        }
    }
}
