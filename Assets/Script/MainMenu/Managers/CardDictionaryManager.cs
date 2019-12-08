using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Spine;
using Spine.Unity;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CardDictionaryManager : MonoBehaviour {
    [SerializeField] Transform cardList;
    [SerializeField] Transform heroCards;
    [SerializeField] Transform cardStorage;
    [SerializeField] Transform sortingModal;
    [SerializeField] TMPro.TextMeshProUGUI cardNum;

    bool isHumanDictionary;
    float clickTime;
    bool standby;
    SortingOptions selectedSortOption;
    SortingOptions beforeSortOption;

    Transform selectedHero;
    string selectedHeroId;
    bool isHeroDic;
    bool isAni = false;

    public static CardDictionaryManager cardDictionaryManager;
    Dictionary<string, int> classHumanCard;
    Dictionary<string, int> classOrcCard;
    List<DictionaryCard> dicCards;

    private void Start() {
        cardDictionaryManager = this;
    }

    public void SetCardDictionary() {
        isHeroDic = false;
        gameObject.SetActive(true);
        transform.Find("HeroDictionary").gameObject.SetActive(false);
        selectedHero = null;
        selectedHeroId = null;
        Transform classList = cardList.Find("CardsByCost");
        selectedSortOption = SortingOptions.CLASS;
        transform.Find("UIbar/Crystal/Value").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userResource.crystal.ToString();
        if (AccountManager.Instance.dicInfo.isHuman) {
            isHumanDictionary = true;
            SetToHumanCards();
            transform.Find("BackgroundImg").GetComponent<Image>().sprite = AccountManager.Instance.resource.campBackgrounds["human"];
        }
        else {
            isHumanDictionary = false;
            SetToOrcCards();
            transform.Find("BackgroundImg").GetComponent<Image>().sprite = AccountManager.Instance.resource.campBackgrounds["orc"];
        }
        RefreshLine();
    }

    public void SetHeroDictionary() {
        isHeroDic = true;
        gameObject.SetActive(true);
        transform.Find("HeroDictionary").gameObject.SetActive(true);
        transform.Find("CardDictionary").gameObject.SetActive(false);
        transform.Find("UIbar/SortBtn").gameObject.SetActive(false);
        selectedHero = null;
        selectedHeroId = null;
        selectedSortOption = SortingOptions.CLASS;
        transform.Find("UIbar/Crystal/Value").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userResource.crystal.ToString();
        if (AccountManager.Instance.dicInfo.isHuman) {
            isHumanDictionary = true;
            SetToHumanHeroes();
            transform.Find("BackgroundImg").GetComponent<Image>().sprite = AccountManager.Instance.resource.campBackgrounds["human"];
        }
        else {
            isHumanDictionary = false;
            SetToOrcHeroes();
            transform.Find("BackgroundImg").GetComponent<Image>().sprite = AccountManager.Instance.resource.campBackgrounds["orc"];
        }
        RefreshLine();
    }

    public void SetToHumanCards() {        
        transform.Find("CardDictionary").gameObject.SetActive(true);
        transform.Find("UIbar/SortBtn").gameObject.SetActive(true);
        GetCard();
        SetCardsByClass();
    }

    public void SetToOrcCards() {
        transform.Find("CardDictionary").gameObject.SetActive(true);
        transform.Find("UIbar/SortBtn").gameObject.SetActive(true);
        GetCard();
        SetCardsByClass();
    }

    public void GetCard() {
        if (dicCards != null) dicCards.Clear();
        dicCards = new List<DictionaryCard>();
        int count = 0;
        foreach (dataModules.CollectionCard card in AccountManager.Instance.allCards) {
            if (card.isHeroCard) continue;
            if (card.unownable) continue;
            if ((card.camp == "human") == isHumanDictionary) {
                int rarelityValue = 0;
                switch (card.rarelity) {
                    case "common":
                        rarelityValue = 0;
                        break;
                    case "uncommon":
                        rarelityValue = 1;
                        break;
                    case "rare":
                        rarelityValue = 2;
                        break;
                    case "superrare":
                        rarelityValue = 3;
                        break;
                    case "legend":
                        rarelityValue = 4;
                        break;
                }
                dicCards.Add(new DictionaryCard { 
                    cardObject = cardStorage.GetChild(count).gameObject, 
                    cardId = card.id, 
                    cardClass = card.cardClasses[0], 
                    rareOrder = rarelityValue, 
                    costOrder = card.cost });
                count++;
            }
        }
        dicCards = SortDicCard(dicCards).ToList();
    }

    IEnumerable<DictionaryCard> SortDicCard(List<DictionaryCard> cards) {
        return cards.OrderBy(n => n.rareOrder).ThenBy(m => m.costOrder).ThenBy(o => o.cardId);
    }

    public void SetToHumanHeroes() {
        Transform heroParent = transform.Find("HeroDictionary/HeroSelect");
        heroParent.gameObject.SetActive(true);
        for (int i = 0; i < heroParent.childCount; i++) {
            heroParent.GetChild(i).GetChild(0).localPosition = Vector3.zero;
            heroParent.GetChild(i).GetChild(0).Find("Buttons").localPosition = new Vector3(0, -10, 0);
            heroParent.GetChild(i).gameObject.SetActive(false);
            heroParent.GetChild(i).Find("HeroObject/HeroExp/OrcGauge").gameObject.SetActive(false);
            for (int j = 0; j < 3; j++) 
                heroParent.GetChild(i).Find("HeroObject/HeroLevel").GetChild(j).gameObject.SetActive(false);
        }
        RefreshHumanHero();
    }

    public void RefreshHumanHero() {
        CountHumanCardClass();
        Transform heroParent = transform.Find("HeroDictionary/HeroSelect");
        int count = 0;
        for (int i = 0; i < AccountManager.Instance.allHeroes.Count; i++) {
            dataModules.HeroInventory heroData = AccountManager.Instance.allHeroes[i];
            if (heroData.camp == "human") {
                Transform heroSlot = heroParent.GetChild(count);
                heroSlot.Find("HeroObject/HeroImg").GetComponent<Image>().sprite = AccountManager.Instance.resource.heroPortraite[heroData.id + "_dic"];
                heroSlot.Find("HeroObject/CardNum/Value").GetComponent<TMPro.TextMeshProUGUI>().text =
                    (classHumanCard[heroData.heroClasses[0]] + classHumanCard[heroData.heroClasses[1]]).ToString();
                heroSlot.Find("HeroObject/CardNum/AllCardNum").GetComponent<TMPro.TextMeshProUGUI>().text = "/" +
                    (classHumanCard["all_" + heroData.heroClasses[0]] + classHumanCard["all_" + heroData.heroClasses[1]]).ToString();
                heroSlot.gameObject.SetActive(true);
                heroSlot.gameObject.name = heroData.id;
                if (AccountManager.Instance.myHeroInventories.ContainsKey(heroData.id)) {
                    dataModules.HeroInventory myHeroData = AccountManager.Instance.myHeroInventories[heroData.id];
                    heroSlot.Find("HeroObject/HeroExp/HumanGauge").gameObject.SetActive(true);
                    if (myHeroData.next_level != null) {
                        heroSlot.Find("HeroObject/HeroExp/HumanGauge").GetComponent<Image>().fillAmount = (float)myHeroData.piece / myHeroData.next_level.piece;
                        heroSlot.Find("HeroObject/HeroExp/Value").GetComponent<TMPro.TextMeshProUGUI>().text = myHeroData.piece + "/" + myHeroData.next_level.piece;
                    }
                    else {
                        heroSlot.Find("HeroObject/HeroExp/HumanGauge").GetComponent<Image>().fillAmount = 1;
                        heroSlot.Find("HeroObject/HeroExp/Value").GetComponent<TMPro.TextMeshProUGUI>().text = "MAX";
                    }
                    for (int j = 0; j < myHeroData.tier; j++)
                        heroSlot.Find("HeroObject/HeroLevel").GetChild(j).gameObject.SetActive(true);
                }
                else {
                    heroSlot.Find("HeroObject/HeroExp/HumanGauge").gameObject.SetActive(true);
                    heroSlot.Find("HeroObject/HeroExp/HumanGauge").GetComponent<Image>().fillAmount = 0;
                    heroSlot.Find("HeroObject/HeroExp/Value").GetComponent<TMPro.TextMeshProUGUI>().text = "0/10";
                }
                count++;
            }
        }
    }

    public void SetToOrcHeroes() {
        Transform heroParent = transform.Find("HeroDictionary/HeroSelect");
        heroParent.gameObject.SetActive(true);
        for (int i = 0; i < heroParent.childCount; i++) {
            heroParent.GetChild(i).GetChild(0).localPosition = Vector3.zero;
            heroParent.GetChild(i).GetChild(0).Find("Buttons").localPosition = new Vector3(0, -10, 0);
            heroParent.GetChild(i).gameObject.SetActive(false);
            heroParent.GetChild(i).Find("HeroObject/HeroExp/HumanGauge").gameObject.SetActive(false);
            for (int j = 0; j < 3; j++)
                heroParent.GetChild(i).Find("HeroObject/HeroLevel").GetChild(j).gameObject.SetActive(false);
        }
        RefreshOrcHero();
    }

    public void RefreshOrcHero() {
        CountOrcCardClass();
        int count = 0;
        Transform heroParent = transform.Find("HeroDictionary/HeroSelect");
        for (int i = 0; i < AccountManager.Instance.allHeroes.Count; i++) {
            dataModules.HeroInventory heroData = AccountManager.Instance.allHeroes[i];
            if (heroData.camp == "orc") {
                Transform heroSlot = heroParent.GetChild(count);
                heroSlot.Find("HeroObject/HeroImg").GetComponent<Image>().sprite = AccountManager.Instance.resource.heroPortraite[heroData.id + "_dic"];
                heroSlot.Find("HeroObject/CardNum/Value").GetComponent<TMPro.TextMeshProUGUI>().text =
                    (classOrcCard[heroData.heroClasses[0]] + classOrcCard[heroData.heroClasses[1]]).ToString();
                heroSlot.Find("HeroObject/CardNum/AllCardNum").GetComponent<TMPro.TextMeshProUGUI>().text = "/" +
                    (classOrcCard["all_" + heroData.heroClasses[0]] + classOrcCard["all_" + heroData.heroClasses[1]]).ToString();
                heroSlot.gameObject.SetActive(true);
                heroSlot.gameObject.name = heroData.id;

                if (AccountManager.Instance.myHeroInventories.ContainsKey(heroData.id)) {
                    dataModules.HeroInventory myHeroData = AccountManager.Instance.myHeroInventories[heroData.id];
                    heroSlot.Find("HeroObject/HeroExp/OrcGauge").gameObject.SetActive(true);
                    if (myHeroData.next_level != null) {
                        heroSlot.Find("HeroObject/HeroExp/OrcGauge").GetComponent<Image>().fillAmount = (float)myHeroData.piece / myHeroData.next_level.piece;
                        heroSlot.Find("HeroObject/HeroExp/Value").GetComponent<TMPro.TextMeshProUGUI>().text = myHeroData.piece + "/" + myHeroData.next_level.piece;
                    }
                    else {
                        heroSlot.Find("HeroObject/HeroExp/OrcGauge").GetComponent<Image>().fillAmount = 1;
                        heroSlot.Find("HeroObject/HeroExp/Value").GetComponent<TMPro.TextMeshProUGUI>().text = "MAX";
                    }
                    for (int j = 0; j < myHeroData.tier; j++)
                        heroSlot.Find("HeroObject/HeroLevel").GetChild(j).gameObject.SetActive(true);
                }
                else {
                    heroSlot.Find("HeroObject/HeroExp/OrcGauge").gameObject.SetActive(true);
                    heroSlot.Find("HeroObject/HeroExp/OrcGauge").GetComponent<Image>().fillAmount = 0;
                    heroSlot.Find("HeroObject/HeroExp/Value").GetComponent<TMPro.TextMeshProUGUI>().text = "0/10";
                }
                count++;
            }
        }
    }

    public void OpenHeroButton(Transform target) {
        if (isAni) return;
        if (target == selectedHero) {
            StartCoroutine(CloseHeroButtons());
            return;
        }
        isAni = true;
        StartCoroutine(OpenHeroButtons(target));
    }


    public IEnumerator OpenHeroButtons(Transform hero) {
        if (selectedHero != null)
            yield return CloseHeroButtons();
        selectedHero = hero;
        int deckIndex = hero.GetSiblingIndex();
        iTween.MoveTo(selectedHero.GetChild(0).Find("Buttons").gameObject, iTween.Hash("y", -175, "islocal", true, "time", 0.3f));
        Transform heroParent = transform.Find("HeroDictionary/HeroSelect");
        for (int i = deckIndex + 1; i < heroParent.childCount; i++) {
            if (heroParent.GetChild(i).gameObject.activeSelf) {
                iTween.MoveTo(heroParent.GetChild(i).GetChild(0).gameObject, iTween.Hash("y", -175, "islocal", true, "time", 0.3f));
            }
        }
        selectedHeroId = hero.gameObject.name;
        yield return new WaitForSeconds(0.25f);
        isAni = false;
    }

    public IEnumerator CloseHeroButtons() {
        int deckIndex = 0;
        if (selectedHero != null) {
            deckIndex = selectedHero.GetSiblingIndex();
            iTween.MoveTo(selectedHero.GetChild(0).Find("Buttons").gameObject, iTween.Hash("y", -10, "islocal", true, "time", 0.1f));
        }
        Transform heroParent = transform.Find("HeroDictionary/HeroSelect");
        for (int i = deckIndex + 1; i < heroParent.childCount; i++) {
            if (heroParent.GetChild(i).gameObject.activeSelf) {
                iTween.MoveTo(heroParent.GetChild(i).GetChild(0).gameObject, iTween.Hash("y", 0, "islocal", true, "time", 0.1f));
            }
        }
        yield return new WaitForSeconds(0.2f);
        selectedHero = null;
    }

    public void CountHumanCardClass() {
        if(classHumanCard != null) classHumanCard.Clear();
        classHumanCard = new Dictionary<string, int>();
        foreach (dataModules.CollectionCard card in AccountManager.Instance.allCards) {
            if (card.unownable) continue;
            if(card.camp == "human" && !card.isHeroCard) {
                if (!classHumanCard.ContainsKey(card.cardClasses[0])) {
                    classHumanCard.Add("all_" + card.cardClasses[0], 0);
                    classHumanCard.Add(card.cardClasses[0], 0);
                }
                classHumanCard["all_" + card.cardClasses[0]]++;
                if (AccountManager.Instance.cardPackage.data.ContainsKey(card.id)) {
                    classHumanCard[card.cardClasses[0]]++;
                }
            }
        }
    }

    public void CountOrcCardClass() {
        if (classOrcCard != null) classOrcCard.Clear();
        classOrcCard = new Dictionary<string, int>();
        foreach (dataModules.CollectionCard card in AccountManager.Instance.allCards) {
            if (card.unownable) continue;
            if (card.camp == "orc" && !card.isHeroCard) {
                if (!classOrcCard.ContainsKey(card.cardClasses[0])) {
                    classOrcCard.Add("all_" + card.cardClasses[0], 0);
                    classOrcCard.Add(card.cardClasses[0], 0);
                }
                classOrcCard["all_" + card.cardClasses[0]]++;
                if (AccountManager.Instance.cardPackage.data.ContainsKey(card.id)) {
                    classOrcCard[card.cardClasses[0]]++;
                }
            }
        }
    }

    public void RefreshCardCount() {
        if (isHumanDictionary) {
            CountHumanCardClass();
            Transform heroParent = transform.Find("HeroDictionary/HeroSelect");
            int count = 0;
            for (int i = 0; i < heroParent.childCount; i++) {
                dataModules.HeroInventory heroData = AccountManager.Instance.allHeroes[i];
                if (heroData.id == heroParent.GetChild(count).name) {
                    heroParent.GetChild(count).Find("HeroObject/CardNum/Value").GetComponent<TMPro.TextMeshProUGUI>().text =
                        (classHumanCard[heroData.heroClasses[0]] + classHumanCard[heroData.heroClasses[1]]).ToString();
                    count++;
                }
            }
        }
        else {
            CountOrcCardClass();
            Transform heroParent = transform.Find("HeroDictionary/HeroSelect");
            int count = 0;
            for (int i = 0; i < heroParent.childCount; i++) {
                dataModules.HeroInventory heroData = AccountManager.Instance.allHeroes[i];
                if (heroData.id == heroParent.GetChild(count).name) {
                    heroParent.GetChild(count).Find("HeroObject/CardNum/Value").GetComponent<TMPro.TextMeshProUGUI>().text =
                        (classOrcCard[heroData.heroClasses[0]] + classOrcCard[heroData.heroClasses[1]]).ToString();
                    count++;
                }
            }
        }
    }


    public void OpenSortModal() {
        SoundManager.Instance.PlaySound(UISfxSound.BUTTON1);
        sortingModal.gameObject.SetActive(true);
        for (int i = 0; i < 5; i++)
            sortingModal.Find("Buttons").GetChild(i).GetChild(0).gameObject.SetActive(true);
        switch (selectedSortOption) {
            case SortingOptions.CLASS:
                sortingModal.Find("Buttons/Class").GetChild(0).gameObject.SetActive(false);
                break;
            case SortingOptions.COST_ASCEND:
                sortingModal.Find("Buttons/Cost").GetChild(0).gameObject.SetActive(false);
                sortingModal.Find("Buttons/Ascending").GetChild(0).gameObject.SetActive(false);
                break;
            case SortingOptions.COST_DESCEND:
                sortingModal.Find("Buttons/Cost").GetChild(0).gameObject.SetActive(false);
                sortingModal.Find("Buttons/Descending").GetChild(0).gameObject.SetActive(false);
                break;
            case SortingOptions.RARELITY_ASCEND:
                sortingModal.Find("Buttons/Rarelity").GetChild(0).gameObject.SetActive(false);
                sortingModal.Find("Buttons/Ascending").GetChild(0).gameObject.SetActive(false);
                break;
            case SortingOptions.RARELITY_DESCEND:
                sortingModal.Find("Buttons/Rarelity").GetChild(0).gameObject.SetActive(false);
                sortingModal.Find("Buttons/Descending").GetChild(0).gameObject.SetActive(false);
                break;
        }
        EscapeKeyController.escapeKeyCtrl.AddEscape(CloseSortModal);
    }

    public void CloseSortModal() {
        selectedSortOption = beforeSortOption;
        sortingModal.gameObject.SetActive(false);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CloseSortModal);
    }

    public void ClickClassButton() {
        for (int i = 0; i < 5; i++)
            sortingModal.Find("Buttons").GetChild(i).GetChild(0).gameObject.SetActive(true);
        sortingModal.Find("Buttons/Class").GetChild(0).gameObject.SetActive(false);
        beforeSortOption = selectedSortOption;
        selectedSortOption = SortingOptions.CLASS;
    }

    public void ClickRarelityButton() {
        for (int i = 0; i < 5; i++)
            sortingModal.Find("Buttons").GetChild(i).GetChild(0).gameObject.SetActive(true);
        sortingModal.Find("Buttons/Rarelity").GetChild(0).gameObject.SetActive(false);
        sortingModal.Find("Buttons/Descending").GetChild(0).gameObject.SetActive(false);
        beforeSortOption = selectedSortOption;
        selectedSortOption = SortingOptions.RARELITY_DESCEND;
    }

    public void ClickCostButton() {
        for (int i = 0; i < 5; i++)
            sortingModal.Find("Buttons").GetChild(i).GetChild(0).gameObject.SetActive(true);
        sortingModal.Find("Buttons/Cost").GetChild(0).gameObject.SetActive(false);
        sortingModal.Find("Buttons/Descending").GetChild(0).gameObject.SetActive(false);
        beforeSortOption = selectedSortOption;
        selectedSortOption = SortingOptions.COST_DESCEND;
    }

    public void ClickAscendingButton() {
        if (selectedSortOption == SortingOptions.CLASS) return;
        if (!sortingModal.Find("Buttons/Ascending").GetChild(0).gameObject.activeSelf) return;
        beforeSortOption = selectedSortOption;
        if (selectedSortOption == SortingOptions.RARELITY_DESCEND) {
            sortingModal.Find("Buttons/Ascending").GetChild(0).gameObject.SetActive(false);
            sortingModal.Find("Buttons/Descending").GetChild(0).gameObject.SetActive(true);
            selectedSortOption = SortingOptions.RARELITY_ASCEND;
        }
        if (selectedSortOption == SortingOptions.COST_DESCEND) {
            sortingModal.Find("Buttons/Ascending").GetChild(0).gameObject.SetActive(false);
            sortingModal.Find("Buttons/Descending").GetChild(0).gameObject.SetActive(true);
            selectedSortOption = SortingOptions.COST_ASCEND;
        }
    }

    public void ClickDescendingButton() {
        if (selectedSortOption == SortingOptions.CLASS) return;
        if (!sortingModal.Find("Buttons/Descending").GetChild(0).gameObject.activeSelf) return;
        beforeSortOption = selectedSortOption;
        if (selectedSortOption == SortingOptions.RARELITY_ASCEND) {
            sortingModal.Find("Buttons/Ascending").GetChild(0).gameObject.SetActive(true);
            sortingModal.Find("Buttons/Descending").GetChild(0).gameObject.SetActive(false);
            selectedSortOption = SortingOptions.RARELITY_DESCEND;
        }
        if (selectedSortOption == SortingOptions.COST_ASCEND) {
            sortingModal.Find("Buttons/Ascending").GetChild(0).gameObject.SetActive(true);
            sortingModal.Find("Buttons/Descending").GetChild(0).gameObject.SetActive(false);
            selectedSortOption = SortingOptions.COST_DESCEND;
        }
    }

    public void ApplySortting() {
        SoundManager.Instance.PlaySound(UISfxSound.BUTTON1);
        AccountManager.Instance.dicInfo.sortingState = selectedSortOption;
        switch (selectedSortOption) {
            case SortingOptions.CLASS:
                SetCardsByClass();
                break;
            case SortingOptions.COST_ASCEND:
                SetCardsByCost(false);
                break;
            case SortingOptions.COST_DESCEND:
                SetCardsByCost(true);
                break;
            case SortingOptions.RARELITY_ASCEND:
                SetCardsByRarelity(false);
                break;
            case SortingOptions.RARELITY_DESCEND:
                SetCardsByRarelity(true);
                break;
        }
        beforeSortOption = selectedSortOption;
        CloseSortModal();
        if (selectedHero != null)
            SortHeroCards();
    }

    void InitDictionary() {
        for (int i = 0; i < cardList.childCount; i++) {
            for (int j = 0; j < cardList.GetChild(i).childCount; j++) {
                Transform cardSet = cardList.GetChild(i).GetChild(j).Find("Grid");
                while (cardSet.childCount != 0) {
                    cardSet.GetChild(0).GetChild(0).gameObject.SetActive(false);
                    cardSet.GetChild(0).GetChild(1).gameObject.SetActive(false);
                    cardSet.GetChild(0).gameObject.SetActive(false);
                    cardSet.GetChild(0).SetParent(cardStorage);
                }
                cardList.GetChild(i).GetChild(j).gameObject.SetActive(false);
            }
            cardList.GetChild(i).gameObject.SetActive(false);
            cardList.GetChild(i).localPosition = Vector3.zero;
        }
    }

    public void SetCardsByClass() {
        InitDictionary();
        int totalCount = 0;
        int haveCount = 0;
        Transform classList = cardList.Find("CardsByClass");
        classList.gameObject.SetActive(true);

        foreach(DictionaryCard card in dicCards) {
            Transform cardObj = cardStorage.GetChild(0);
            Transform classSet = classList.Find(card.cardClass).Find("Grid");
            card.cardObject.transform.SetParent(classSet);
            card.cardObject.GetComponent<MenuCardHandler>().DrawCard(card.cardId, isHumanDictionary);
            if (AccountManager.Instance.cardPackage.data.ContainsKey(card.cardId)) haveCount++;
            card.cardObject.SetActive(true);
            totalCount++;
        }

        //foreach (dataModules.CollectionCard card in AccountManager.Instance.allCards) {
        //    if (card.isHeroCard) continue;
        //    if (isHumanDictionary && card.camp != "human") continue;
        //    if (!isHumanDictionary && card.camp != "orc") continue;
        //    Transform cardObj = cardStorage.GetChild(0);
        //    Transform classSet = classList.Find(card.cardClasses[0]).Find("Grid");
        //    cardObj.SetParent(classSet);
        //    cardObj.GetComponent<MenuCardHandler>().DrawCard(card.id, isHumanDictionary);
        //    if (AccountManager.Instance.cardPackage.data.ContainsKey(card.id)) haveCount++;
        //    cardObj.gameObject.SetActive(true);
        //    totalCount++;
        //}

        for (int i = 0; i < classList.childCount; i++) {
            Transform cardSet = classList.GetChild(i).Find("Grid");
            if (cardSet.childCount > 0)
                classList.GetChild(i).gameObject.SetActive(true);
        }
        cardNum.text = haveCount.ToString() + "/" + totalCount.ToString();
        RefreshLine();
        //SetHeroButtons();
    }

    public void SetCardsByRarelity(bool descending) {
        InitDictionary();
        int totalCount = 0;
        int haveCount = 0;
        Transform rarelityList = cardList.Find("CardsByRarelity");
        rarelityList.gameObject.SetActive(true);
        if (descending) {
            rarelityList.Find("legend").SetSiblingIndex(0);
            rarelityList.Find("superrare").SetSiblingIndex(1);
            rarelityList.Find("rare").SetSiblingIndex(2);
            rarelityList.Find("uncommon").SetSiblingIndex(3);
            rarelityList.Find("common").SetSiblingIndex(4);
        }
        else {
            rarelityList.Find("common").SetSiblingIndex(0);
            rarelityList.Find("uncommon").SetSiblingIndex(1);
            rarelityList.Find("rare").SetSiblingIndex(2);
            rarelityList.Find("superrare").SetSiblingIndex(3);
            rarelityList.Find("legend").SetSiblingIndex(4);
        }

        string rarelity = string.Empty;
        foreach (DictionaryCard card in dicCards) {
            Transform cardObj = cardStorage.GetChild(0);
            switch (card.rareOrder) {
                case 0:
                    rarelity = "common";
                    break;
                case 1:
                    rarelity = "uncommon";
                    break;
                case 2:
                    rarelity = "rare";
                    break;
                case 3:
                    rarelity = "superrare";
                    break;
                case 4:
                    rarelity = "legend";
                    break;
            }
            Transform classSet = rarelityList.Find(rarelity).Find("Grid");
            card.cardObject.transform.SetParent(classSet);
            card.cardObject.GetComponent<MenuCardHandler>().DrawCard(card.cardId, isHumanDictionary);
            if (AccountManager.Instance.cardPackage.data.ContainsKey(card.cardId)) haveCount++;
            card.cardObject.SetActive(true);
            totalCount++;
        }

        //foreach (dataModules.CollectionCard card in AccountManager.Instance.allCards) {
        //    if (card.isHeroCard) continue;
        //    if (isHumanDictionary && card.camp != "human") continue;
        //    if (!isHumanDictionary && card.camp != "orc") continue;
        //    Transform cardObj = cardStorage.GetChild(0);
        //    Transform classSet = rarelityList.Find(card.rarelity).Find("Grid");
        //    cardObj.SetParent(classSet);
        //    cardObj.GetComponent<MenuCardHandler>().DrawCard(card.id, isHumanDictionary);
        //    if (AccountManager.Instance.cardPackage.data.ContainsKey(card.id)) haveCount++;
        //    cardObj.gameObject.SetActive(true);
        //    totalCount++;
        //}


        for (int i = 0; i < rarelityList.childCount; i++) {
            Transform cardSet = rarelityList.GetChild(i).Find("Grid");
            if (cardSet.childCount > 0)
                rarelityList.GetChild(i).gameObject.SetActive(true);
        }
        cardNum.text = haveCount.ToString() + "/" + totalCount.ToString();
        RefreshLine();
    }

    public void SetCardsByCost(bool descending) {
        InitDictionary();
        int totalCount = 0;
        int haveCount = 0;
        Transform costList = cardList.Find("CardsByCost");
        costList.gameObject.SetActive(true);
        if (descending) {
            for (int i = 0; i < costList.childCount; i++)
                costList.Find(i.ToString()).SetSiblingIndex(costList.childCount - (1 + i));
        }
        else {
            for (int i = 0; i < costList.childCount; i++)
                costList.Find(i.ToString()).SetSiblingIndex(i);
        }

        foreach (DictionaryCard card in dicCards) {
            Transform cardObj = cardStorage.GetChild(0);
            Transform classSet = costList.Find(card.costOrder.ToString()).Find("Grid");
            card.cardObject.transform.SetParent(classSet);
            card.cardObject.GetComponent<MenuCardHandler>().DrawCard(card.cardId, isHumanDictionary);
            if (AccountManager.Instance.cardPackage.data.ContainsKey(card.cardId)) haveCount++;
            card.cardObject.SetActive(true);
            totalCount++;
        }

        //foreach (dataModules.CollectionCard card in AccountManager.Instance.allCards) {
        //    if (card.isHeroCard) continue;
        //    if (isHumanDictionary && card.camp != "human") continue;
        //    if (!isHumanDictionary && card.camp != "orc") continue;
        //    Transform cardObj = cardStorage.GetChild(0);
        //    Transform classSet = costList.Find(card.cost.ToString()).Find("Grid");
        //    cardObj.SetParent(classSet);
        //    cardObj.GetComponent<MenuCardHandler>().DrawCard(card.id, isHumanDictionary);
        //    if (AccountManager.Instance.cardPackage.data.ContainsKey(card.id)) haveCount++;
        //    cardObj.gameObject.SetActive(true);
        //    totalCount++;
        //}

        for (int i = 0; i < costList.childCount; i++) {
            Transform cardSet = costList.GetChild(i).Find("Grid");
            if (cardSet.childCount > 0)
                costList.GetChild(i).gameObject.SetActive(true);
        }
        cardNum.text = haveCount.ToString() + "/" + totalCount.ToString();
        RefreshLine();
    }

    public void OpenHeroImage() {
        if (selectedHero == null) return;
        transform.Find("HeroDictionary/HeroSelect").gameObject.SetActive(false);
        transform.Find("HeroDictionary/HeroImage").gameObject.SetActive(true);
        transform.Find("HeroDictionary/HeroImage/HeroStory/HumanBG").gameObject.SetActive(isHumanDictionary);
        transform.Find("HeroDictionary/HeroImage/HeroStory/OrcBG").gameObject.SetActive(!isHumanDictionary);
        transform.Find("HeroDictionary/HeroImage/HeroSpine").Find(selectedHeroId).gameObject.SetActive(true);
        transform.Find("HeroDictionary/HeroImage/HeroSpine").Find(selectedHeroId).SetAsFirstSibling();
        foreach(dataModules.HeroInventory hero in AccountManager.Instance.allHeroes) {
            if (hero.id == selectedHeroId) {
                transform.Find("HeroDictionary/HeroImage/HeroStory/HeroName").GetComponent<TMPro.TextMeshProUGUI>().text = hero.name;
                break;
            }
        }
        EscapeKeyController.escapeKeyCtrl.AddEscape(ExitDictionaryScene);
    }

    public void OpenHeroInfo() {
        OpenHeroInfoWIndow(selectedHeroId);
    }


    public void OpenHeroCardDic() {
        if (selectedHero == null) return;        
        selectedSortOption = SortingOptions.CLASS;
        transform.Find("HeroDictionary").gameObject.SetActive(false);
        if (isHumanDictionary)
            SetToHumanCards();
        else
            SetToOrcCards();
        SortHeroCards();
        EscapeKeyController.escapeKeyCtrl.AddEscape(ExitDictionaryScene);
    }

    public void SortHeroCards() {
        switch (selectedSortOption) {
            case SortingOptions.CLASS:
                SetHeroesCardByClass();
                break;
            case SortingOptions.COST_ASCEND:
            case SortingOptions.COST_DESCEND:
                SetHeroesCardByCost();
                break;
            case SortingOptions.RARELITY_ASCEND:
            case SortingOptions.RARELITY_DESCEND:
                SetHeroesCardsByRarelity();
                break;
        }
        RefreshLine();
    }

    public void SetHeroesCardByClass() {
        Transform classList = cardList.Find("CardsByClass");
        string heroClass1 = "";
        string heroClass2 = "";
        foreach (dataModules.HeroInventory hero in AccountManager.Instance.allHeroes) {
            if (hero.id == selectedHeroId) {
                heroClass1 = hero.heroClasses[0];
                heroClass2 = hero.heroClasses[1];
            }
        }
        for (int i = 0; i < classList.childCount; i++) {
            if (classList.GetChild(i).name == heroClass1 || classList.GetChild(i).name == heroClass2)
                classList.GetChild(i).gameObject.SetActive(true);
            else
                classList.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void SetHeroesCardByCost() {
        Transform classList = cardList.Find("CardsByCost");
        string heroClass1 = "";
        string heroClass2 = "";
        foreach (dataModules.HeroInventory hero in AccountManager.Instance.allHeroes) {
            if (hero.id == selectedHeroId) {
                heroClass1 = hero.heroClasses[0];
                heroClass2 = hero.heroClasses[1];
            }
        }
        for (int i = 0; i < classList.childCount; i++) {
            Transform cardParent = classList.GetChild(i).Find("Grid");
            int countActiveChild = 0;
            for (int j = 0; j < cardParent.childCount; j++) {
                string cardId = cardParent.GetChild(j).GetComponent<MenuCardHandler>().cardID;
                if (AccountManager.Instance.allCardsDic[cardId].cardClasses[0] == heroClass1 || AccountManager.Instance.allCardsDic[cardId].cardClasses[0] == heroClass2) {
                    cardParent.GetChild(j).gameObject.SetActive(true);
                    countActiveChild++;
                }
                else
                    cardParent.GetChild(j).gameObject.SetActive(false);
            }
            classList.GetChild(i).gameObject.SetActive(countActiveChild != 0);
        }
    }

    public void SetHeroesCardsByRarelity() {
        Transform classList = cardList.Find("CardsByRarelity");
        string heroClass1 = "";
        string heroClass2 = "";
        foreach (dataModules.HeroInventory hero in AccountManager.Instance.allHeroes) {
            if (hero.id == selectedHeroId) {
                heroClass1 = hero.heroClasses[0];
                heroClass2 = hero.heroClasses[1];
            }
        }
        for (int i = 0; i < classList.childCount; i++) {
            Transform cardParent = classList.GetChild(i).Find("Grid");
            int countActiveChild = 0;
            for (int j = 0; j < cardParent.childCount; j++) {
                string cardId = cardParent.GetChild(j).GetComponent<MenuCardHandler>().cardID;
                if (AccountManager.Instance.allCardsDic[cardId].cardClasses[0] == heroClass1 || AccountManager.Instance.allCardsDic[cardId].cardClasses[0] == heroClass2) {
                    cardParent.GetChild(j).gameObject.SetActive(true);
                    countActiveChild++;
                }
                else
                    cardParent.GetChild(j).gameObject.SetActive(false);
            }
            classList.GetChild(i).gameObject.SetActive(countActiveChild != 0);
        }
    }

    public void OpenHeroInfoWIndow(string heroId) {
        SoundManager.Instance.PlaySound(UISfxSound.BUTTON1);
        MenuHeroInfo.heroInfoWindow.SetHeroInfoWindow(heroId);
        MenuHeroInfo.heroInfoWindow.transform.parent.gameObject.SetActive(true);
        MenuHeroInfo.heroInfoWindow.gameObject.SetActive(true);
    }

    public void CloseHeroInfoWIndow() {
        MenuHeroInfo.heroInfoWindow.gameObject.SetActive(false);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CloseHeroInfoWIndow);
    }

    public void ExitDictionaryScene() {
        SoundManager.Instance.PlaySound(UISfxSound.BUTTON1);
        if (!isHeroDic) {
            ExitDictionaryCanvas();
        }
        else {
            if (transform.Find("CardDictionary").gameObject.activeSelf) {
                EscapeKeyController.escapeKeyCtrl.RemoveEscape(ExitDictionaryScene);
                transform.Find("CardDictionary").gameObject.SetActive(false);
                transform.Find("HeroDictionary").gameObject.SetActive(true);
                transform.Find("UIbar/SortBtn").gameObject.SetActive(false);
                RefreshCardCount();
            }
            else if (transform.Find("HeroDictionary/HeroImage").gameObject.activeSelf) {
                EscapeKeyController.escapeKeyCtrl.RemoveEscape(ExitDictionaryScene);
                transform.Find("HeroDictionary/HeroImage").gameObject.SetActive(false);
                transform.Find("HeroDictionary/HeroImage/HeroSpine").GetChild(0).gameObject.SetActive(false);
                transform.Find("HeroDictionary/HeroSelect").gameObject.SetActive(true);
            }
            else {
                ExitDictionaryCanvas();
            }
        }
        if(tutorialHand != null) {
            tutorialHand.transform.parent.transform.GetComponent<Button>().onClick.RemoveListener(tutoAction);
            Destroy(tutorialHand);
        }
    }

    public void ExitDictionaryCanvas() {
        selectedHero = null;
        selectedHeroId = "";
        transform.Find("CardDictionary").gameObject.SetActive(false);
        transform.Find("HeroDictionary").gameObject.SetActive(false);
        selectedSortOption = SortingOptions.CLASS;
        MenuSceneController.menuSceneController.CloseDictionary();
    }


    public void RefreshLine() {
        Canvas.ForceUpdateCanvases();
        for (int i = 0; i < 3; i++) {
            LayoutRebuilder.ForceRebuildLayoutImmediate(cardList.GetChild(i).GetComponent<RectTransform>());
        }

        Invoke("UpdateContentHeight", 0.1f);
    }

    private void UpdateContentHeight() {
        float tmp = 512f;
        Transform activatedTf = null;
        foreach (Transform tf in cardList) {
            if (tf.gameObject.activeSelf) {
                activatedTf = tf;
            }
        }

        if (activatedTf == null) return;
        float height = activatedTf.GetComponent<RectTransform>().rect.height;
        float result = height + tmp;
        //Debug.Log("result : " + (result).ToString());
        transform.Find("CardDictionary").GetComponent<RectTransform>().sizeDelta = new Vector2(1080, result);
        activatedTf.GetComponent<RectTransform>().anchoredPosition = new Vector2(activatedTf.GetComponent<RectTransform>().anchoredPosition.x, 0);
        GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
    }

    GameObject tutorialHand;
    UnityAction tutoAction;

    public void cardShowHand(Quest.QuestContentController quest, string[] args) {
        Transform beforeHand = transform.Find("tutorialHand");
        if(beforeHand != null) Destroy(beforeHand.gameObject);
        DictionaryCard card = dicCards.Find(x=>x.cardId == args[0]);
        tutorialHand = Instantiate(quest.manager.handSpinePrefab, card.cardObject.transform, false);
        tutorialHand.name = "tutorialHand";
        tutoAction = () => MenuCardInfo.cardInfoWindow.makeShowHand(quest);
        card.cardObject.GetComponent<Button>().onClick.AddListener(tutoAction);
    }
}

public enum SortingOptions {
    CLASS,
    RARELITY_ASCEND,
    RARELITY_DESCEND,
    COST_ASCEND,
    COST_DESCEND,
}

public class DictionaryInfo {
    public bool isHuman;
    public SortingOptions sortingState;
    public bool inDic;
}

public class DictionaryCard {
    public GameObject cardObject;
    public string cardId;
    public string cardClass;
    public int rareOrder;
    public int costOrder;
}