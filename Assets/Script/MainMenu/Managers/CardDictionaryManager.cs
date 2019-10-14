using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] Transform heroInfoWindow;
    [SerializeField] Transform cardStorage;
    [SerializeField] Transform sortingModal;
    [SerializeField] TMPro.TextMeshProUGUI cardNum;
    [SerializeField] MyDecksLoader myDecksLoader;

    [SerializeField] Sprite orcPanelBg, humanPanelBg;
    MyDecksLoader decksLoader;

    bool isHumanDictionary;
    float clickTime;
    bool standby;
    SortingOptions selectedSortOption;
    SortingOptions beforeSortOption;

    public UnityEvent SetCardsFinished = new UnityEvent();
    GameObject selectedHero;
    string selectedHeroId;

    public void AttachDecksLoader(ref MyDecksLoader decksLoader) {
        this.decksLoader = decksLoader;
        this.decksLoader.OnInvenLoadFinished.AddListener(() => { SetToHumanCards(); });
    }



    private void Start() {
        Transform classList = cardList.Find("CardsByCost");
        //for (int i = 0; i < classList.childCount; i++)
        //    classList.GetChild(i).Find("Header/Info/Image").GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = i.ToString();
        selectedSortOption = SortingOptions.CLASS;
        transform.Find("UIbar/Crystal/Value").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userResource.crystal.ToString();
        if (AccountManager.Instance.dicInfo.isHuman)
            SetToHumanCards();
        else
            SetToOrcCards();
    }

    public void SetToHumanCards() {
        isHumanDictionary = true;
        SetCardsByClass();
    }

    public void SetToOrcCards() {
        isHumanDictionary = false;
        SetCardsByClass();
    }

    public void RefreshLine() {
        Canvas.ForceUpdateCanvases();
        for (int i = 0; i < 3; i++) {
            LayoutRebuilder.ForceRebuildLayoutImmediate(cardList.GetChild(i).GetComponent<RectTransform>());
        }

        Invoke("UpdateContentHeight", 0.1f);
    }

    private void UpdateContentHeight() {
        float tmp = 1028f; //영웅 슬롯이 2줄이 될 시 300+ 해주면 됨
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
        transform.Find("Content").GetComponent<RectTransform>().sizeDelta = new Vector2(1080, result);
        activatedTf.GetComponent<RectTransform>().anchoredPosition = new Vector2(activatedTf.GetComponent<RectTransform>().anchoredPosition.x, 0);
        GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
    }

    public void OpenSortModal() {
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
    }

    public void CloseSortModal() {
        selectedSortOption = beforeSortOption;
        sortingModal.gameObject.SetActive(false);
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
        foreach (dataModules.CollectionCard card in AccountManager.Instance.allCards) {
            if (card.isHeroCard) continue;
            if (isHumanDictionary && card.camp != "human") continue;
            if (!isHumanDictionary && card.camp != "orc") continue;
            Transform cardObj = cardStorage.GetChild(0);
            Transform classSet = classList.Find(card.cardClasses[0]).Find("Grid");
            cardObj.SetParent(classSet);
            cardObj.GetComponent<MenuCardHandler>().DrawCard(card.id, isHumanDictionary);
            if (AccountManager.Instance.cardPackage.data.ContainsKey(card.id)) haveCount++;
            cardObj.gameObject.SetActive(true);
            totalCount++;
        }
        for (int i = 0; i < classList.childCount; i++) {
            Transform cardSet = classList.GetChild(i).Find("Grid");
            if (cardSet.childCount > 0)
                classList.GetChild(i).gameObject.SetActive(true);
        }
        cardNum.text = haveCount.ToString() + "/" + totalCount.ToString();
        RefreshLine();
        SetHeroButtons();
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
        foreach (dataModules.CollectionCard card in AccountManager.Instance.allCards) {
            if (card.isHeroCard) continue;
            if (isHumanDictionary && card.camp != "human") continue;
            if (!isHumanDictionary && card.camp != "orc") continue;
            Transform cardObj = cardStorage.GetChild(0);
            Transform classSet = rarelityList.Find(card.rarelity).Find("Grid");
            cardObj.SetParent(classSet);
            cardObj.GetComponent<MenuCardHandler>().DrawCard(card.id, isHumanDictionary);
            if (AccountManager.Instance.cardPackage.data.ContainsKey(card.id)) haveCount++;
            cardObj.gameObject.SetActive(true);
            totalCount++;
        }
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
        foreach (dataModules.CollectionCard card in AccountManager.Instance.allCards) {
            if (card.isHeroCard) continue;
            if (isHumanDictionary && card.camp != "human") continue;
            if (!isHumanDictionary && card.camp != "orc") continue;
            Transform cardObj = cardStorage.GetChild(0);
            Transform classSet = costList.Find(card.cost.ToString()).Find("Grid");
            cardObj.SetParent(classSet);
            cardObj.GetComponent<MenuCardHandler>().DrawCard(card.id, isHumanDictionary);
            if (AccountManager.Instance.cardPackage.data.ContainsKey(card.id)) haveCount++;
            cardObj.gameObject.SetActive(true);
            totalCount++;
        }
        for (int i = 0; i < costList.childCount; i++) {
            Transform cardSet = costList.GetChild(i).Find("Grid");
            if (cardSet.childCount > 0)
                costList.GetChild(i).gameObject.SetActive(true);
        }
        cardNum.text = haveCount.ToString() + "/" + totalCount.ToString();
        RefreshLine();
    }

   

    public void SetHeroButtons() {
        for (int i = 0; i < 8; i++) {
            heroCards.GetChild(i).gameObject.SetActive(false);
        }

        int count = 0;
        
        foreach (dataModules.HeroInventory heroes in AccountManager.Instance.allHeroes) {
            if (heroes.camp == "human" && !isHumanDictionary) continue;
            if (heroes.camp == "orc" && isHumanDictionary) continue;
            Transform hero = heroCards.GetChild(count);
            hero.gameObject.SetActive(true);
            EventTrigger.Entry onBtn = new EventTrigger.Entry();
            onBtn.eventID = EventTriggerType.PointerDown;
            onBtn.callback.AddListener((EventData) => StartClick(heroes.id));
            hero.GetComponent<EventTrigger>().triggers.Add(onBtn);
            EventTrigger.Entry upBtn = new EventTrigger.Entry();
            upBtn.eventID = EventTriggerType.PointerUp;
            upBtn.callback.AddListener((EventData) => EndClick(hero.gameObject, heroes.id));
            hero.GetComponent<EventTrigger>().triggers.Add(upBtn);
            hero.GetComponent<Image>().sprite = AccountManager.Instance.resource.heroPortraite[heroes.id + "_button"];
            bool haveHero = AccountManager.Instance.myHeroInventories.ContainsKey(heroes.id);
            hero.Find("HeroLevel").gameObject.SetActive(haveHero);
            hero.Find("Empty").gameObject.SetActive(!haveHero);
            count++;
        }
    }

    public void StartClick(string heroId) {
        clickTime = Time.time;
        StartCoroutine(WaitForOpenInfo(heroId));
    }

    IEnumerator WaitForOpenInfo(string heroId) {
        standby = true;
        while (standby) {
            yield return new WaitForSeconds(0.1f);
            if (Time.time - clickTime >= 0.5f) {
                OpenHeroInfoWIndow(heroId);
                standby = false;
            }
        }
    }
    public void EndClick(GameObject btn, string heroId) {
        if (!standby) return;
        if (selectedHero == null) {
            selectedHero = btn;
            selectedHeroId = heroId;
            btn.transform.Find("Selected").gameObject.SetActive(true);
            SortHeroCards();
        }
        else if (selectedHero == btn) {
            selectedHero = null;
            selectedHeroId = "";
            btn.transform.Find("Selected").gameObject.SetActive(false);
            ApplySortting();
        }
        else {
            selectedHero.transform.Find("Selected").gameObject.SetActive(false);
            selectedHero = btn;
            selectedHeroId = heroId;
            btn.transform.Find("Selected").gameObject.SetActive(true);
            SortHeroCards();
        }
        standby = false;
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
            if(hero.id == selectedHeroId) {
                heroClass1 = hero.heroClasses[0];
                heroClass2 = hero.heroClasses[1];
            }
        }
        for(int i = 0; i < classList.childCount; i++) {
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
        SetHeroInfoWindow(heroId);
        heroInfoWindow.parent.gameObject.SetActive(true);
        heroInfoWindow.gameObject.SetActive(true);
    }

    public void CloseHeroInfoWIndow() {
        heroInfoWindow.gameObject.SetActive(false);
    }

    public void SetHeroInfoWindow(string heroId) {
        dataModules.Templates hero = new dataModules.Templates();
        Transform heroCards;
        if (isHumanDictionary) {
            heroInfoWindow.Find("HeroCards/Orc").gameObject.SetActive(false);
            heroCards = heroInfoWindow.Find("HeroCards/Human");
            foreach (dataModules.Templates heroes in AccountManager.Instance.humanTemplates) {
                if (heroes.id == heroId) {
                    hero = heroes;
                    break;
                }
            }
            //heroInfoWindow.Find("Ribon").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["hero_name_human_superrare"];
        }
        else {
            heroInfoWindow.Find("HeroCards/Human").gameObject.SetActive(false);
            heroCards = heroInfoWindow.Find("HeroCards/Orc");
            foreach (dataModules.Templates heroes in AccountManager.Instance.orcTemplates) {
                if (heroes.id == heroId) {
                    hero = heroes;
                    break;
                }
            }
            //heroInfoWindow.Find("Ribon").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["hero_name_orc_superrare"];
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

    public void ExitDictionaryScene() { 
        FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);
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