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
    [SerializeField] MenuHeroInfo heroInfoWindow;
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

    public static CardDictionaryManager cardDictionaryManager;

    public void AttachDecksLoader(ref MyDecksLoader decksLoader) {
        this.decksLoader = decksLoader;
        this.decksLoader.OnInvenLoadFinished.AddListener(() => { SetToHumanCards(); });
    }

    private void Start() {
        cardDictionaryManager = this;
    }

    //private void Start() {
    //    Transform classList = cardList.Find("CardsByCost");
    //    //for (int i = 0; i < classList.childCount; i++)
    //    //    classList.GetChild(i).Find("Header/Info/Image").GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = i.ToString();
    //    selectedSortOption = SortingOptions.CLASS;
    //    transform.Find("UIbar/Crystal/Value").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userResource.crystal.ToString();
    //    if (AccountManager.Instance.dicInfo.isHuman)
    //        SetToHumanCards();
    //    else
    //        SetToOrcCards();
    //    RefreshLine();
    //}

    public void SetDictionaryScene() {
        selectedHero = null;
        selectedHeroId = null;
        Transform classList = cardList.Find("CardsByCost");
        //for (int i = 0; i < classList.childCount; i++)
        //    classList.GetChild(i).Find("Header/Info/Image").GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = i.ToString();
        selectedSortOption = SortingOptions.CLASS;
        transform.Find("UIbar/Crystal/Value").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userResource.crystal.ToString();
        if (AccountManager.Instance.dicInfo.isHuman)
            SetToHumanCards();
        else
            SetToOrcCards();
        RefreshLine();
    }

    public void SetToHumanCards() {
        SoundManager.Instance.PlaySound("button_1");
        isHumanDictionary = true;
        transform.Find("BackgroundImg").GetComponent<Image>().sprite = AccountManager.Instance.resource.campBackgrounds["human"];
        SetCardsByClass();
    }

    public void SetToOrcCards() {
        SoundManager.Instance.PlaySound("button_1");
        isHumanDictionary = false;
        transform.Find("BackgroundImg").GetComponent<Image>().sprite = AccountManager.Instance.resource.campBackgrounds["orc"];
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
        SoundManager.Instance.PlaySound("button_1");
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
        SoundManager.Instance.PlaySound("button_1");
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
            hero.transform.Find("SelectBack").gameObject.SetActive(false);
            hero.transform.Find("SelectFront").gameObject.SetActive(false);
            List<EventTrigger.Entry> entriesToRemove = new List<EventTrigger.Entry>();
            foreach (var entry in hero.GetComponent<EventTrigger>().triggers) {
                if (entry.eventID == EventTriggerType.PointerDown || entry.eventID == EventTriggerType.PointerUp)
                    entriesToRemove.Add(entry);
            }
            foreach (var entry in entriesToRemove)
                hero.GetComponent<EventTrigger>().triggers.Remove(entry);
            EventTrigger.Entry onBtn = new EventTrigger.Entry();
            onBtn.eventID = EventTriggerType.PointerDown;
            onBtn.callback.AddListener((EventData) => StartClick(heroes.id));
            hero.GetComponent<EventTrigger>().triggers.Add(onBtn);
            EventTrigger.Entry upBtn = new EventTrigger.Entry();
            upBtn.eventID = EventTriggerType.PointerUp;
            upBtn.callback.AddListener((EventData) => EndClick(hero.gameObject, heroes.id));
            hero.GetComponent<EventTrigger>().triggers.Add(upBtn);
            hero.Find("Portrait").GetComponent<Image>().sprite = AccountManager.Instance.resource.heroPortraite[heroes.id + "_button"];
            bool haveHero = AccountManager.Instance.myHeroInventories.ContainsKey(heroes.id);
            hero.Find("HeroLevel").gameObject.SetActive(haveHero);
            hero.Find("Empty").gameObject.SetActive(!haveHero);
            count++;
        }
    }

    public void StartClick(string heroId) {
        SoundManager.Instance.PlaySound("button_1");
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
        btn.transform.Find("SelectBack").GetComponent<SkeletonGraphic>().Initialize(true);
        btn.transform.Find("SelectBack").GetComponent<SkeletonGraphic>().Update(0);
        btn.transform.Find("SelectFront").GetComponent<SkeletonGraphic>().Initialize(true);
        btn.transform.Find("SelectFront").GetComponent<SkeletonGraphic>().Update(0);
        if (selectedHero == null) {
            selectedHero = btn;
            selectedHeroId = heroId;
            btn.transform.Find("SelectBack").gameObject.SetActive(true);
            btn.transform.Find("SelectFront").gameObject.SetActive(true);
            SortHeroCards();
        }
        else if (selectedHero == btn) {
            selectedHero = null;
            selectedHeroId = "";
            btn.transform.Find("SelectBack").gameObject.SetActive(false);
            btn.transform.Find("SelectFront").gameObject.SetActive(false);
            ApplySortting();
        }
        else {
            selectedHero.transform.Find("SelectBack").gameObject.SetActive(false);
            selectedHero.transform.Find("SelectFront").gameObject.SetActive(false);
            selectedHero = btn;
            selectedHeroId = heroId;
            btn.transform.Find("SelectBack").gameObject.SetActive(true);
            btn.transform.Find("SelectFront").gameObject.SetActive(true);
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
        SoundManager.Instance.PlaySound("button_1");
        heroInfoWindow.SetHeroInfoWindow(heroId);
        heroInfoWindow.transform.parent.gameObject.SetActive(true);
        heroInfoWindow.gameObject.SetActive(true);
    }

    public void CloseHeroInfoWIndow() {
        heroInfoWindow.gameObject.SetActive(false);
    }

    public void ExitDictionaryScene() {
        SoundManager.Instance.PlaySound("button_1");
        MenuSceneController.menuSceneController.CloseDictionary();
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