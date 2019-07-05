using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDictionaryManager : MonoBehaviour {
    [SerializeField] Transform cardList;
    [SerializeField] Transform heroCards;
    public void CloseDictionaryCanvas() {
        gameObject.SetActive(false);
    }

    public void SetToHumanCards() {
        transform.Find("Buttons/OrcSelect").GetChild(0).gameObject.SetActive(false);
        transform.Find("Buttons/HumanSelect").GetChild(0).gameObject.SetActive(true);
        heroCards.Find("OrcHeroList").gameObject.SetActive(false);
        heroCards.Find("HumanHeroList").gameObject.SetActive(true);
        SetCardsInDictionary(true);
    }

    public void SetToOrcCards() {
        transform.Find("Buttons/OrcSelect").GetChild(0).gameObject.SetActive(true);
        transform.Find("Buttons/HumanSelect").GetChild(0).gameObject.SetActive(false);
        heroCards.Find("OrcHeroList").gameObject.SetActive(true);
        heroCards.Find("HumanHeroList").gameObject.SetActive(false);
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
    }
}
