using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDictionaryManager : MonoBehaviour {
    [SerializeField] Transform cardList;
    [SerializeField] Transform heroCards;
    public void CloseDictionaryCanvas() {
        gameObject.SetActive(false);
    }

    public void SetToHumanCards() {
        transform.Find("Buttons/OrcSelect").GetChild(0).gameObject.SetActive(false);
        transform.Find("Buttons/HumanSelect").GetChild(0).gameObject.SetActive(true);
        SetCardsInDictionary(true);
    }

    public void SetToOrcCards() {
        transform.Find("Buttons/OrcSelect").GetChild(0).gameObject.SetActive(true);
        transform.Find("Buttons/HumanSelect").GetChild(0).gameObject.SetActive(false);
        SetCardsInDictionary(false);
    }

    public void SetCardsInDictionary(bool isHuman) {
        for (int i = 0; i < cardList.childCount; i++)
            cardList.GetChild(i).gameObject.SetActive(false);
        int count = 0;
        foreach (dataModules.CollectionCard card in AccountManager.Instance.allCards) {
            if (isHuman) {
                if (card.camp == "human" && !card.isHeroCard) {
                    cardList.GetChild(count).gameObject.SetActive(true);
                    cardList.GetChild(count).GetComponent<MenuCardHandler>().DrawCard(card.id, true);
                    count++;
                }
            }
            else {
                if (card.camp == "orc" && !card.isHeroCard) {
                    cardList.GetChild(count).gameObject.SetActive(true);
                    cardList.GetChild(count).GetComponent<MenuCardHandler>().DrawCard(card.id, false);
                    count++;
                }
            }
        }
        SetHeroButtons(isHuman);
    }

    public void SetHeroButtons(bool isHuman) {
        for (int i = 0; i < 8; i++) {
            heroCards.GetChild(i).Find("Empty").gameObject.SetActive(true);
            heroCards.GetChild(i).Find("Disable").gameObject.SetActive(true);
        }
        int count = 0;
        foreach (dataModules.CollectionCard card in AccountManager.Instance.allCards) {
            if (isHuman) {
                if (card.camp == "human" && card.isHeroCard) {
                    heroCards.GetChild(count).GetComponent<Image>().sprite = AccountManager.Instance.resource.heroPortraite[card.id + "_button"];
                    heroCards.GetChild(count).Find("Empty").gameObject.SetActive(false);
                    heroCards.GetChild(count).Find("Disable").gameObject.SetActive(false);
                    count++;
                }
            }
            else {
                if (card.camp == "orc" && card.isHeroCard) {
                    heroCards.GetChild(count).GetComponent<Image>().sprite = AccountManager.Instance.resource.heroPortraite[card.id + "_button"];
                    heroCards.GetChild(count).Find("Empty").gameObject.SetActive(false);
                    heroCards.GetChild(count).Find("Disable").gameObject.SetActive(false);
                    count++;
                }
            }
        }
    }
}
