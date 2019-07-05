using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine;
using Spine.Unity;

public class CardDictionaryManager : MonoBehaviour {
    [SerializeField] Transform cardList;
    [SerializeField] Transform heroCards;
    [SerializeField] Transform heroInfoWindow;
    bool isHumanDictionary;

    private void Start() {
        gameObject.SetActive(false);
    }

    public void CloseDictionaryCanvas() {
        gameObject.SetActive(false);
    }

    public void SetToHumanCards() {
        isHumanDictionary = true;
        transform.Find("Bars/Orc").gameObject.SetActive(false);
        transform.Find("Bars/Human").gameObject.SetActive(true);
        transform.Find("Buttons/OrcSelect").GetChild(0).gameObject.SetActive(false);
        transform.Find("Buttons/HumanSelect").GetChild(0).gameObject.SetActive(true);
        SetCardsInDictionary();
    }

    public void SetToOrcCards() {
        isHumanDictionary = false;
        transform.Find("Bars/Orc").gameObject.SetActive(true);
        transform.Find("Bars/Human").gameObject.SetActive(false);
        transform.Find("Buttons/OrcSelect").GetChild(0).gameObject.SetActive(true);
        transform.Find("Buttons/HumanSelect").GetChild(0).gameObject.SetActive(false);
        SetCardsInDictionary();
    }

    public void SetCardsInDictionary() {
        for (int i = 0; i < cardList.childCount; i++)
            cardList.GetChild(i).gameObject.SetActive(false);
        int count = 0;
        foreach (dataModules.CollectionCard card in AccountManager.Instance.allCards) {
            if (isHumanDictionary) {
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
        SetHeroButtons();
    }

    public void SetHeroButtons() {
        for (int i = 0; i < 8; i++) {
            heroCards.GetChild(i).Find("Empty").gameObject.SetActive(true);
            heroCards.GetChild(i).Find("Disable").gameObject.SetActive(true);
        }
        int count = 0;

        if (isHumanDictionary) {
            foreach (dataModules.Hero card in AccountManager.Instance.humanDecks.heros) {
                heroCards.GetChild(count).GetComponent<Image>().sprite = AccountManager.Instance.resource.heroPortraite[card.id + "_button"];
                heroCards.GetChild(count).Find("Empty").gameObject.SetActive(false);
                heroCards.GetChild(count).Find("Disable").gameObject.SetActive(false);
                count++;

            }
        }
        else {
            foreach (dataModules.Hero card in AccountManager.Instance.orcDecks.heros) {

                heroCards.GetChild(count).GetComponent<Image>().sprite = AccountManager.Instance.resource.heroPortraite[card.id + "_button"];
                heroCards.GetChild(count).Find("Empty").gameObject.SetActive(false);
                heroCards.GetChild(count).Find("Disable").gameObject.SetActive(false);
                count++;
            }
        }
    }

    public void OpenHeroInfoWIndow(int index) {
        SetHeroInfoWindow(index);
        heroInfoWindow.gameObject.SetActive(true);
    }

    public void CloseHeroInfoWIndow() {
        heroInfoWindow.gameObject.SetActive(false);
    }

    public void SetHeroInfoWindow(int index) {
        dataModules.Hero hero;
        Transform heroCards;
        if (isHumanDictionary) {
            heroInfoWindow.Find("HeroCards/OrcHeroCard").gameObject.SetActive(false);
            heroCards = heroInfoWindow.Find("HeroCards/HumanHeroCard");
            hero = AccountManager.Instance.humanDecks.heros[index];
            heroInfoWindow.Find("Ribon").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["hero_name_human_superrare"];
        }
        else {
            heroInfoWindow.Find("HeroCards/HumanHeroCard").gameObject.SetActive(false);
            heroCards = heroInfoWindow.Find("HeroCards/OrcHeroCard");
            hero = AccountManager.Instance.orcDecks.heros[index];
            heroInfoWindow.Find("Ribon").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["hero_name_orc_superrare"];
        }
        heroInfoWindow.Find("BackGroundImage/Human").gameObject.SetActive(isHumanDictionary);
        heroInfoWindow.Find("BackGroundImage/Orc").gameObject.SetActive(!isHumanDictionary);
        heroInfoWindow.gameObject.SetActive(true);
        heroInfoWindow.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = hero.name;
        heroInfoWindow.Find("HeroSpines").GetChild(0).gameObject.SetActive(false);
        Transform heroSpine = heroInfoWindow.Find("HeroSpines/" + hero.id);
        heroSpine.gameObject.SetActive(true);
        heroSpine.SetAsFirstSibling();
        heroInfoWindow.Find("Class").GetChild(0).GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["class_" + hero.heroClasses[0]];
        heroInfoWindow.Find("Class").GetChild(0).GetChild(0).GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["class_icon_" + hero.heroClasses[0]];
        heroInfoWindow.Find("Class").GetChild(1).GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["class_" + hero.heroClasses[1]];
        heroInfoWindow.Find("Class").GetChild(1).GetChild(0).GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["class_icon_" + hero.heroClasses[1]];
        for (int i = 0; i < hero.heroCards.Count; i++) {
            heroCards.GetChild(i).GetComponent<MenuCardHandler>().DrawCard(hero.heroCards[i].cardId, isHumanDictionary);
        }
        heroCards.gameObject.SetActive(true);
    }
}
