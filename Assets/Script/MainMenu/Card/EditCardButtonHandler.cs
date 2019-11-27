using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditCardButtonHandler : MonoBehaviour {
    
    [SerializeField] Transform handDeckArea;
    [SerializeField] Transform cardBookArea;
    [SerializeField] Transform deckEditCanvas;

    Transform card;
    dataModules.CollectionCard cardData;
    bool isHandCard;

    public void CheckDragging() {
        if (!gameObject.activeSelf) return;
        CloseCardButtons();
    }

    public void SetCardButtons(Transform card, bool isHandCard, int cardNum) {
        this.card = card;
        cardData = card.GetComponent<EditCardHandler>().cardData;
        this.isHandCard = isHandCard;
        gameObject.SetActive(true);
        transform.position = card.position;
        if (isHandCard) 
            transform.SetParent(handDeckArea);
        else 
            transform.SetParent(cardBookArea);

        transform.GetChild(0).Find("AddCard").gameObject.SetActive(!isHandCard);
        transform.GetChild(0).Find("ExceptCard").gameObject.SetActive(isHandCard);
        if(!isHandCard)
            transform.GetChild(0).Find("AddCard").GetComponent<Button>().interactable = (cardNum == 0) ? false : true;
        SetCardImage(card.GetComponent<EditCardHandler>());
        if (gameObject.activeSelf) {
            handDeckArea.GetComponent<ScrollRect>().enabled = true;
            cardBookArea.GetComponent<ScrollRect>().enabled = true;
        }
        if (deckEditCanvas.GetComponent<DeckEditController>().setCardNum == 40) {
            transform.GetChild(0).Find("AddCard").GetComponent<Button>().interactable = false;
            transform.GetChild(0).Find("AddCard/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "FULL";
        }
        else
            transform.GetChild(0).Find("AddCard/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "추가";
    }

    public void SetCardImage(EditCardHandler card) {
        EditCardHandler cardHandler = transform.GetChild(0).Find("CardImage").GetComponent<EditCardHandler>();
        cardHandler.InitEditCard();
        if (isHandCard)
            cardHandler.HAVENUM = card.SETNUM;
        else
            cardHandler.HAVENUM = card.HAVENUM;
        cardHandler.DrawCard(cardData.id, cardData.camp == "human");
        cardHandler.gameObject.SetActive(true);
    }

    public void CloseCardButtons() {
        if (!gameObject.activeSelf) return;
        EditCardHandler cardHandler = transform.GetChild(0).Find("CardImage").GetComponent<EditCardHandler>();
        cardHandler.InitEditCard();
        card = null;
        cardData = null;
        gameObject.SetActive(false);
    }

    public void OpenCardInfo() {
        MenuCardInfo.cardInfoWindow.transform.parent.gameObject.SetActive(true);
        MenuCardInfo.cardInfoWindow.gameObject.SetActive(true);
        MenuCardInfo.cardInfoWindow.SetCardInfo(cardData, cardData.camp == "human", card);
        MenuCardInfo.cardInfoWindow.transform.Find("Flavor").gameObject.SetActive(false);
        if (transform.parent.name == "HandDeckArea") {
            MenuCardInfo.cardInfoWindow.transform.Find("CreateCard/BreakBtn/DisableInHand").gameObject.SetActive(true);
        }
        else
            MenuCardInfo.cardInfoWindow.bookHaveNum = card.GetComponent<EditCardHandler>().HAVENUM;
        EscapeKeyController.escapeKeyCtrl.AddEscape(MenuCardInfo.cardInfoWindow.CloseInfo);
    }

    public void AddCardInDeck() {
        DeckEditController deckEdit = deckEditCanvas.GetComponent<DeckEditController>();
        if (deckEdit.setCardNum == 40) return;
        EditCardHandler cardHandler = transform.GetChild(0).Find("CardImage").GetComponent<EditCardHandler>();
        deckEdit.ConfirmSetDeck(card.gameObject, cardData.id);
        cardHandler.DrawCard(cardData.id, cardData.camp == "human");
        cardHandler.HAVENUM--;
        cardHandler.SetHaveNum();
        if (cardHandler.HAVENUM == 0) CloseCardButtons();
        if (deckEdit.setCardNum == 40) {
            transform.GetChild(0).Find("AddCard").GetComponent<Button>().interactable = false;
            transform.GetChild(0).Find("AddCard/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "FULL";
        }
        else
            transform.GetChild(0).Find("AddCard/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "추가";
    }

    public void ExceptCardFromDeck() {
        EditCardHandler cardHandler = transform.GetChild(0).Find("CardImage").GetComponent<EditCardHandler>();
        deckEditCanvas.GetComponent<DeckEditController>().ExceptFromDeck(card.gameObject, cardData.id);
        cardHandler.DrawCard(cardData.id, cardData.camp == "human");
        cardHandler.HAVENUM--;
        cardHandler.SetHaveNum();
        if (cardHandler.HAVENUM == 0) CloseCardButtons();
    }

    public void MakeCard(Transform cardObj, bool makeCard) {
        deckEditCanvas.GetComponent<DeckEditController>().AddMadeCard(cardObj, makeCard);
    }
}
