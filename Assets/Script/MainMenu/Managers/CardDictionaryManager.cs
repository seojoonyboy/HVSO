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

    [SerializeField] Sprite orcPanelBg, humanPanelBg;

    bool isHumanDictionary;

    private void Start() {
        gameObject.SetActive(false);
    }

    public void CloseDictionaryCanvas() {
        gameObject.SetActive(false);
    }

    public void SetToHumanCards() {
        isHumanDictionary = true;
        transform.Find("Buttons/OrcSelect").GetChild(0).gameObject.SetActive(false);
        transform.Find("Buttons/HumanSelect").GetChild(0).gameObject.SetActive(true);
        heroCards.parent.Find("Background").GetComponent<Image>().sprite = humanPanelBg;

        SetCardsInDictionary();
    }

    public void SetToOrcCards() {
        isHumanDictionary = false;
        transform.Find("Buttons/OrcSelect").GetChild(0).gameObject.SetActive(true);
        transform.Find("Buttons/HumanSelect").GetChild(0).gameObject.SetActive(false);
        heroCards.parent.Find("Background").GetComponent<Image>().sprite = orcPanelBg;

        SetCardsInDictionary();
    }

    public void SetCardsInDictionary() {
        //for (int i = 0; i < cardList.childCount; i++)
        //    cardList.GetChild(i).gameObject.SetActive(false);
        //int count = 0;
        //foreach (dataModules.CollectionCard card in AccountManager.Instance.allCards) {
        //    if (isHumanDictionary) {
        //        if (card.camp == "human" && !card.isHeroCard) {
        //            cardList.GetChild(count).gameObject.SetActive(true);
        //            cardList.GetChild(count).GetComponent<MenuCardHandler>().DrawCard(card.id, true);
        //            count++;
        //        }
        //    }
        //    else {
        //        if (card.camp == "orc" && !card.isHeroCard) {
        //            cardList.GetChild(count).gameObject.SetActive(true);
        //            cardList.GetChild(count).GetComponent<MenuCardHandler>().DrawCard(card.id, false);
        //            count++;
        //        }
        //    }
        //}
        SetHeroButtons();
    }

    public void SetHeroButtons() {
        for(int i = 0; i < 4; i++) {
            for (int j = 0; j < 3; j++) {
                heroCards.GetChild(i).GetChild(j).GetComponent<Image>().color = new Color(82, 80, 80, 255);
            }
        }
        
        int count = 0;

        List<dataModules.Templates> selectedTemplates;
        if (isHumanDictionary) {
            
            selectedTemplates = AccountManager.Instance.humanTemplates;
        }
        else {
            selectedTemplates = AccountManager.Instance.orcTemplates;
        }

        int pageIndex = 0;
        int slotIndex = 0;
        foreach (dataModules.Templates card in selectedTemplates) {
            Transform hero = heroCards.GetChild(pageIndex).GetChild(count);
            hero.gameObject.SetActive(true);
            hero.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = card.name;
            hero.GetComponent<Image>().sprite = AccountManager.Instance.resource.heroPortraite[card.id];

            slotIndex++;
            if (slotIndex == 3) {
                slotIndex = 0;
                pageIndex++;
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
        dataModules.Templates hero;
        Transform heroCards;
        if (isHumanDictionary) {
            heroInfoWindow.Find("HeroCards/OrcHeroCard").gameObject.SetActive(false);
            heroCards = heroInfoWindow.Find("HeroCards/HumanHeroCard");
            hero = AccountManager.Instance.humanTemplates[index];
            heroInfoWindow.Find("Ribon").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["hero_name_human_superrare"];
        }
        else {
            heroInfoWindow.Find("HeroCards/HumanHeroCard").gameObject.SetActive(false);
            heroCards = heroInfoWindow.Find("HeroCards/OrcHeroCard");
            hero = AccountManager.Instance.orcTemplates[index];
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
        heroInfoWindow.Find("Class").GetChild(0).GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[hero.heroClasses[0]];
        heroInfoWindow.Find("Class").GetChild(1).GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[hero.heroClasses[1]];
        for (int i = 0; i < hero.heroCards.Length; i++) {
            heroCards.GetChild(i).GetComponent<MenuCardHandler>().DrawCard(hero.heroCards[i].cardId, isHumanDictionary);
        }
        heroCards.gameObject.SetActive(true);
    }
}
