using System;
using System.Collections;
using System.Collections.Generic;
using BestHTTP;
using UnityEngine;
using UnityEngine.UI;
using Spine;
using Spine.Unity;

public class DeckHandler : MonoBehaviour
{
    [SerializeField] DeckEditController deckEditCanvas;
    [SerializeField] TemplateMenu templateCanvas;
    private string deckID;
    private bool isBasic = false;
    private bool isHuman;
    public dataModules.Deck templateDeck;

    dataModules.Deck deck;
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
        Transform deckObj = transform.GetChild(0);
        deckObj.Find("HeroImg").gameObject.SetActive(true);
        deckObj.Find("HeroImg").GetComponent<Image>().sprite = AccountManager.Instance.resource.deckPortraite[deck.heroId];
        deckObj.Find("CardNum").gameObject.SetActive(true);
        deckObj.Find("CardNum/Value").GetComponent<TMPro.TextMeshProUGUI>().text = deck.totalCardCount.ToString() + "/";
        deckObj.Find("DeckName").gameObject.SetActive(true);
        deckObj.Find("DeckName").GetComponent<TMPro.TextMeshProUGUI>().text = deck.name.ToString();

        this.deck = deck;
    }

    public void SetNewTemplateDeck(dataModules.Deck deck) {
        templateDeck = deck;
        deckID = deck.id;
        transform.Find("HeroImg").GetComponent<Image>().sprite = AccountManager.Instance.resource.deckPortraite[deck.heroId];
        transform.Find("DeckName").GetComponent<TMPro.TextMeshProUGUI>().text = deck.name.ToString();
        transform.Find("CardNum/Value").GetComponent<TMPro.TextMeshProUGUI>().text = deck.totalCardCount.ToString() + "/";
        transform.Find("Selected").gameObject.SetActive(false);
        transform.Find("SelectedBack").gameObject.SetActive(false);
    }

    public void OpenDeckButton() {
        DeckSettingManager deckManager = transform.parent.parent.parent.GetComponent<DeckSettingManager>();
        if (deckManager.selectedDeck == transform) {
            StartCoroutine(deckManager.CloseDeckButtons());
            return;
        }
        StartCoroutine(deckManager.OpenDeckButtons(transform));
    }

    public void CloseDeckButton() {
        if (isBasic) return;
        GameObject editButtons = transform.Find("DeckInfo/EditButtons").gameObject;
        editButtons.SetActive(false);
    }

    public void SelectTemplateDeck() {
        transform.Find("Selected").GetComponent<SkeletonGraphic>().Initialize(true);
        transform.Find("Selected").gameObject.SetActive(true);
        transform.Find("SelectedBack").GetComponent<SkeletonGraphic>().Initialize(true);
        transform.Find("SelectedBack").gameObject.SetActive(true);
        templateCanvas.SelectDeck(this);
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
            deckEditCanvas.SetCustumDeckEdit(customDeck, false);
        deckEditCanvas.gameObject.SetActive(true);
        deckEditCanvas.GetComponent<DeckEditController>().RefreshLine();
        DeckSettingManager deckManager = transform.parent.parent.parent.GetComponent<DeckSettingManager>();
        deckManager.RefreshLine();

        FindObjectOfType<HUDController>().SetHeader(HUDController.Type.HIDE);
    }

    public void DeleteButton() {
        if (AccountManager.Instance == null) return;
        DeckSettingManager deckManager = transform.parent.parent.parent.GetComponent<DeckSettingManager>();
        StartCoroutine(deckManager.CloseDeckButtons());
        //transform.GetChild(0).Find("Buttons").localPosition = new Vector3(-5, 0, 0);
        AccountManager.Instance.RequestDeckRemove(DECKID, OnRemoved);
        transform.SetAsLastSibling();
        gameObject.SetActive(false);
    }

    private void OnRemoved(HTTPRequest originalRequest, HTTPResponse response) {
        if (response.StatusCode == 200) {
            transform.GetComponentInParent<DeckSettingManager>()
                .menuSceneController
                .decksLoader
                .Load();
        }
    }

    public void StartAIBattle() {
        const int MaxCardNum = 40;
        if (deck.deckValidate) {
            PlayerPrefs.SetString("SelectedDeckId", deckID);
            PlayerPrefs.SetString("SelectedBattleType", "solo");
            string camp;
            if (isHuman) { camp = "HUMAN"; }
            else { camp = "ORC"; }
            PlayerPrefs.SetString("SelectedRace", camp);

            FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.CONNECT_MATCHING_SCENE);
        }
        else {
            if(deck.totalCardCount != MaxCardNum) {
                Modal.instantiate("유효하지 않은 덱입니다. 카드 부족", Modal.Type.CHECK);
            }
        }
    }
}
