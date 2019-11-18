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

    private void Update() {
        if (transform.position.y != card.position.y) {
            if (isHandCard) {
                RectTransform handRect = handDeckArea.GetChild(0).GetComponent<RectTransform>();
                if (transform.position.y < card.position.y) {
                    if (handRect.anchoredPosition.y > 0 || handRect.anchoredPosition.y < -10) {
                        CloseCardButtons();
                    }
                }
                else if (handRect.anchoredPosition.y < handRect.sizeDelta.y - handDeckArea.GetComponent<RectTransform>().sizeDelta.y
                    || handRect.anchoredPosition.y > handRect.sizeDelta.y - handDeckArea.GetComponent<RectTransform>().sizeDelta.y + 10) {
                    CloseCardButtons();
                }
            }
            if (!isHandCard) {
                RectTransform bookRect = cardBookArea.GetChild(0).GetComponent<RectTransform>();
                if (transform.position.y < card.position.y) {
                    if (bookRect.anchoredPosition.y > 0 || bookRect.anchoredPosition.y < -10) {
                        CloseCardButtons();
                    }
                }
                else if (bookRect.anchoredPosition.y + 100 < (bookRect.sizeDelta.y - cardBookArea.GetComponent<RectTransform>().sizeDelta.y) * deckEditCanvas.localScale.x
                    || bookRect.anchoredPosition.y > bookRect.sizeDelta.y - cardBookArea.GetComponent<RectTransform>().sizeDelta.y + 10) {
                    CloseCardButtons();
                }
            }
        }
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
        EditCardHandler cardHandler = transform.GetChild(0).Find("CardImage").GetComponent<EditCardHandler>();
        cardHandler.InitEditCard();
        cardHandler.HAVENUM = cardNum;
        cardHandler.DrawCard(cardData.id, cardData.camp == "human");
        cardHandler.gameObject.SetActive(true);
        if (gameObject.activeSelf) {
            handDeckArea.GetComponent<ScrollRect>().enabled = true;
            cardBookArea.GetComponent<ScrollRect>().enabled = true;
        }
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
        MenuCardInfo.cardInfoWindow.SetCardInfo(cardData, cardData.camp == "human", null);
        MenuCardInfo.cardInfoWindow.transform.Find("CreateCard").gameObject.SetActive(true);
        MenuCardInfo.cardInfoWindow.transform.Find("EditCardUI").gameObject.SetActive(false);
        MenuCardInfo.cardInfoWindow.transform.Find("Flavor").gameObject.SetActive(false);
        EscapeKeyController.escapeKeyCtrl.AddEscape(MenuCardInfo.cardInfoWindow.CloseInfo);
    }

    public void AddCardInDeck() {
        EditCardHandler cardHandler = transform.GetChild(0).Find("CardImage").GetComponent<EditCardHandler>();
        deckEditCanvas.GetComponent<DeckEditController>().ConfirmSetDeck(card.gameObject, cardData.id);
        cardHandler.DrawCard(cardData.id, cardData.camp == "human");
        cardHandler.HAVENUM--;
        cardHandler.SetHaveNum();
        if (cardHandler.HAVENUM == 0) CloseCardButtons();
    }

    public void ExceptCardFromDeck() {
        EditCardHandler cardHandler = transform.GetChild(0).Find("CardImage").GetComponent<EditCardHandler>();
        deckEditCanvas.GetComponent<DeckEditController>().ExceptFromDeck(card.gameObject, cardData.id);
        cardHandler.DrawCard(cardData.id, cardData.camp == "human");
        cardHandler.HAVENUM--;
        cardHandler.SetHaveNum();
        if (cardHandler.HAVENUM == 0) CloseCardButtons();
    }
}
