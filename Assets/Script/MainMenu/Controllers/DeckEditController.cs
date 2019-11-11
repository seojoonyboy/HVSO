using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using dataModules;
using TMPro;
using UnityEngine.EventSystems;
using BestHTTP;
using System;
using System.Linq;

public class DeckEditController : MonoBehaviour
{
    [SerializeField] public Transform cardStorage;
    [SerializeField] public GameObject deckNamePanel;
    [SerializeField] public Transform settingLayout;
    [SerializeField] public Transform ownCardLayout;
    [SerializeField] public TMPro.TextMeshProUGUI pagenumText;
    [SerializeField] public TMPro.TextMeshProUGUI setCardText;
    [SerializeField] public Transform buttons;

    public string heroID;
    HeroInventory heroData;    

    public GameObject selectCard;
    public bool editing = false;

    public Dictionary<string, GameObject> setCardList;
    List<DeckItem> items;
    List<EditCard> editCards;

    public string deckID = null;
    bool isHuman;
    public int setCardNum = 0;
    int haveCardNum = 0;
    int maxHaveCard = 0;
    int dontHaveCard = 0;
    
    
    string maxPage;
    int lastPage;

    public TemplateMenu templateMenu;

    public MenuSceneController menuSceneController;
    bool isTemplate = false;
    int currentPage;
    int[] cardMana;

    GameObject cancelModal;

    void Awake() {
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_DECK_MODIFIED, OnDeckModified);
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_DECK_CREATED, OnMakeNewDeckFinished);
    }

    void OnDestroy() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_DECK_MODIFIED, OnDeckModified);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_DECK_CREATED, OnMakeNewDeckFinished);
    }

    private void OnMakeNewDeckFinished(Enum Event_Type, Component Sender, object Param) {
        gameObject.SetActive(false);
        if (templateMenu != null) {
            templateMenu.transform.gameObject.SetActive(false);
            templateMenu = null;
        }
        deckNamePanel.transform.Find("NameTemplate").GetComponent<TMPro.TMP_InputField>().text = "";
    }

    private void OnDeckModified(Enum Event_Type, Component Sender, object Param) {
        menuSceneController.decksLoader.Load();
        gameObject.SetActive(false);
    }

    private void InitCanvas() {
        EscapeKeyController.escapeKeyCtrl.AddEscape(CancelButton);
        setCardList = new Dictionary<string, GameObject>();
        if (editCards != null) editCards.Clear();
        editCards = GetCards();
        setCardNum = 0;
        haveCardNum = 0;
        dontHaveCard = 0;
        currentPage = 0;        
        EditCardHandler.dragable = true;
        settingLayout.localPosition = new Vector3(0, settingLayout.localPosition.y, 0);
        while (cardStorage.childCount > 0) {
            string cardRoot = cardStorage.GetChild(0).GetComponent<EditCardHandler>().editBookRoot;
            cardStorage.GetChild(0).SetParent(ownCardLayout.Find(cardRoot));
        }
        for (int i = 0; i < 40; i++) {
            EditCardHandler cardHandler = settingLayout.GetChild(i).GetComponent<EditCardHandler>();
            cardHandler.InitEditCard();
            cardHandler.deckEditController = this;
        }

        Transform cardStore = transform.Find("InnerCanvas/CardStore");
        for(int i = 0; i < cardStore.childCount; i++) {
            cardStore.GetChild(i).GetComponent<EditCardHandler>().deckEditController = this;
        }
        for (int i = 0; i < ownCardLayout.transform.childCount; i++) {
            for (int j = 0; j < 9; j++) {
                if (ownCardLayout.GetChild(i).GetChild(j).childCount > 0) {
                    EditCardHandler cardHandler = ownCardLayout.GetChild(i).GetChild(j).GetChild(0).GetComponent<EditCardHandler>();
                    cardHandler.InitEditCard();
                    cardHandler.deckEditController = this;
                    cardHandler.transform.SetParent(cardStore);
                    cardHandler.transform.localPosition = Vector3.zero;
                }
            }
        }

        ownCardLayout.GetChild(currentPage).gameObject.SetActive(true);
        for (int i = 1; i < ownCardLayout.childCount; i++)
            ownCardLayout.GetChild(i).gameObject.SetActive(false);
        buttons.Find("NextPageButton").gameObject.SetActive(true);
        buttons.Find("PrevPageButton").gameObject.SetActive(false);
        pagenumText.text = "1/" + ownCardLayout.childCount.ToString();
        selectCard = null;
        transform.Find("InnerCanvas/ShowAllClass/Selected").gameObject.SetActive(true);
        transform.Find("InnerCanvas/SortToClass1/Selected").gameObject.SetActive(false);
        transform.Find("InnerCanvas/SortToClass2/Selected").gameObject.SetActive(false);
        transform.Find("InnerCanvas/ShowAllCards/Selected").gameObject.SetActive(false);
    }

    public List<EditCard> GetCards() {
        List<EditCard> cards = new List<EditCard>();
        Transform cardStore = transform.Find("InnerCanvas/CardStore");
        int count = 0;

        foreach (dataModules.CollectionCard card in AccountManager.Instance.allCards) {
            if (isHuman == (card.camp == "human")) {
                if (card.isHeroCard) continue;
                if (card.unownable) continue;
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
                cards.Add(new EditCard { cardObject = cardStore.GetChild(count).gameObject, cardId = card.id, cardClass = card.cardClasses[0], costOrder = card.cost, rareOrder = rarelityValue });
                count++;
            }
        }  

        return SortCardListByCost(cards).ToList();
    }

    IEnumerable<EditCard> SortCardListByCost(List<EditCard> cards) {
        return cards.OrderBy(n => n.costOrder).ThenBy(m => m.rareOrder);
    }

    public void ConfirmButton() {
        if(editing == true) {
            NetworkManager.ModifyDeckReqFormat form = new NetworkManager.ModifyDeckReqFormat();
            NetworkManager.ModifyDeckReqArgs field = new NetworkManager.ModifyDeckReqArgs();

            field.fieldName = NetworkManager.ModifyDeckReqField.NAME;

            string inputNameVal = deckNamePanel.transform.Find("NameTemplate").GetComponent<TMPro.TMP_InputField>().text;
            
            
            if (string.IsNullOrEmpty(inputNameVal)) {
                Modal.instantiate("원하는 이름을 적어주세요", Modal.Type.CHECK);
                return;
            }
            field.value = inputNameVal;
            form.parms.Add(field);

            //template을 통한 덱 생성도 새로운 생성 요청으로 취급 해야함.
            if (isTemplate) RequestNewDeck();
            else {
                RequestModifyDeck(form, deckID);
                if (inputNameVal.Contains(" ")) {
                    Modal.instantiate("덱 이름의 빈 칸은 제거됩니다.", Modal.Type.CHECK);
                    inputNameVal = inputNameVal.Replace(" ", string.Empty);
                }
            };
        }
        else {
            RequestNewDeck();
        }
        FindObjectOfType<HUDController>().SetHeader(HUDController.Type.SHOW_USER_INFO);        
    }

    public void CancelButton() {
        cancelModal = Modal.instantiate("편집을 취소 하시겠습니까?", Modal.Type.YESNO, () => {
            CancelEdit();
        }, ResumeEdit);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CancelButton);
        //EscapeKeyController.escapeKeyCtrl.AddEscape(ResumeEdit);
    }

    public void CancelEdit() {
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CancelButton);
        setCardList = null;
        if (templateMenu != null) {
            templateMenu.transform.gameObject.SetActive(true);
            templateMenu = null;
        }

        deckNamePanel.transform.Find("NameTemplate").GetComponent<TMPro.TMP_InputField>().text = "";

        if(isTemplate)
            FindObjectOfType<HUDController>().SetHeader(HUDController.Type.RESOURCE_ONLY_WITH_BACKBUTTON);
        else
            FindObjectOfType<HUDController>().SetHeader(HUDController.Type.SHOW_USER_INFO);

        isTemplate = false;
        gameObject.SetActive(false);
    }

    public void ResumeEdit() {
        DestroyImmediate(cancelModal, true);
        EscapeKeyController.escapeKeyCtrl.AddEscape(CancelButton);
        //EscapeKeyController.escapeKeyCtrl.RemoveEscape(ResumeEdit);
    }
   
    public void OnTouchCard(GameObject card) {
        if (card.GetComponent<EditCardHandler>().cardObject.Find("Disabled").gameObject.activeSelf) return;
        if (selectCard == null)
            selectCard = card;
        else {
            if (card != selectCard) {
                selectCard.transform.Find("SelectedPanel").gameObject.SetActive(false);
                if (selectCard.transform.parent.name == "Own")
                    transform.Find("SetDeckLayout").Find("glow").gameObject.SetActive(false);
                selectCard = null;
                selectCard = card;
            }
        }
        if (selectCard.transform.parent.name == "Own")
            transform.Find("InnerCanvas/SetDeckLayout").Find("glow").gameObject.SetActive(true);

        if (selectCard.transform.parent.name == "SetDeck") {
            transform.Find("InnerCanvas/ExceptButton").gameObject.SetActive(true);
        }
        selectCard.transform.Find("SelectedPanel").gameObject.SetActive(true);
    }

    public void OpenHeroInfo() {
        SetManaCurve();
        transform.Find("InnerCanvas/HeroInfoWindow").gameObject.SetActive(true);
        EscapeKeyController.escapeKeyCtrl.AddEscape(CloseHeroInfo);
    }

    public void CloseHeroInfo() {
        transform.Find("InnerCanvas/HeroInfoWindow").gameObject.SetActive(false);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CloseHeroInfo);
    }

    public void SetManaCurve() {
        Transform cardList = transform.Find("InnerCanvas/HandDeck/Mask/SettedDeck");
        for (int i = 0; i < 8; i++)
            cardMana[i] = 0;
        for (int i = 0; i < cardList.childCount; i++) {
            if (!cardList.GetChild(i).gameObject.activeSelf) continue;
            int cost = cardList.GetChild(i).GetComponent<EditCardDragHandler>().cardData.cost;
            int num = cardList.GetChild(i).GetComponent<EditCardDragHandler>().SETNUM;
            if (cost >= 8)
                cardMana[7] += num;
            else
                cardMana[cost] += num;
        }
        Transform manaCorveParent = transform.Find("InnerCanvas/HeroInfoWindow/ManaCurve/ManaSliderParent");
        for(int i = 0; i < 8; i++) {
            if (cardMana[i] > 30) cardMana[i] = 30;
            manaCorveParent.GetChild(i).Find("SliderValue").GetComponent<Image>().fillAmount = (float)cardMana[i] / 30.0f;
            manaCorveParent.GetChild(i).Find("CardNum").GetComponent<TMPro.TextMeshProUGUI>().text = cardMana[i].ToString();
        }
    }

    public void NextPageButton() {
        ownCardLayout.GetChild(currentPage).gameObject.SetActive(false);
        currentPage++;
        ownCardLayout.GetChild(currentPage).gameObject.SetActive(true);
        buttons.Find("PrevPageButton").gameObject.SetActive(true);
        pagenumText.text = (currentPage + 1).ToString() + maxPage;
        if (currentPage + 1 == lastPage) {
            buttons.Find("NextPageButton").gameObject.SetActive(false);
            return;
        }
    }

    public void PrevPageButton() {
        ownCardLayout.GetChild(currentPage).gameObject.SetActive(false);
        currentPage--;
        ownCardLayout.GetChild(currentPage).gameObject.SetActive(true);
        buttons.Find("NextPageButton").gameObject.SetActive(true);
        if (currentPage - 1 < 0) {
            buttons.Find("PrevPageButton").gameObject.SetActive(false);
            return;
        }
    }

    public void ExpectFromDeck() {
        if (EditCardHandler.onAnimation) return;
        string cardId = MenuCardInfo.cardInfoWindow.editCard.GetComponent<EditCardHandler>().cardID;
        if (!setCardList.ContainsKey(cardId)) return;
        EditCardHandler handCardHandler = setCardList[cardId].GetComponent<EditCardHandler>();
        handCardHandler.SETNUM--;
        handCardHandler.SetSetNum();
        EditCardHandler beforeCard = handCardHandler.beforeObject.GetComponent<EditCardHandler>();
        beforeCard.HAVENUM++;
        beforeCard.SetHaveNum(true);
        if (handCardHandler.SETNUM == 0) {
            handCardHandler.beforeObject = null;
            handCardHandler.transform.SetAsLastSibling();
            handCardHandler.transform.localScale = Vector3.zero;
            StartCoroutine(handCardHandler.SortHandPos());
            setCardList.Remove(cardId);
        }
        setCardNum--;
        haveCardNum++;
        ownCardLayout.GetChild(currentPage).gameObject.SetActive(false);
        currentPage = beforeCard.transform.parent.parent.GetSiblingIndex();
        ownCardLayout.GetChild(currentPage).gameObject.SetActive(true);
        if (currentPage > 0) buttons.Find("PrevPageButton").gameObject.SetActive(true);
        else buttons.Find("PrevPageButton").gameObject.SetActive(false);
        if (currentPage + 1 == lastPage) buttons.Find("NextPageButton").gameObject.SetActive(false);
        else buttons.Find("NextPageButton").gameObject.SetActive(true);
        MenuCardInfo.cardInfoWindow.SetEditCardInfo(beforeCard.HAVENUM, setCardNum);
        RefreshLine();
    }

    public void ExpectFromDeck(string id, GameObject card) {
        EditCardHandler cardHandler = card.GetComponent<EditCardHandler>();
        cardHandler.SETNUM--;
        cardHandler.SetSetNum();
        EditCardHandler beforeCard = cardHandler.beforeObject.GetComponent<EditCardHandler>();
        beforeCard.HAVENUM++;
        beforeCard.SetHaveNum(true);
        if (cardHandler.SETNUM == 0) {
            cardHandler.beforeObject = null;
            setCardList.Remove(id);
        }
        setCardNum--;
        haveCardNum++;
        ownCardLayout.GetChild(currentPage).gameObject.SetActive(false);
        currentPage = beforeCard.transform.parent.parent.GetSiblingIndex();
        ownCardLayout.GetChild(currentPage).gameObject.SetActive(true);
        if (currentPage > 0) buttons.Find("PrevPageButton").gameObject.SetActive(true);
        else buttons.Find("PrevPageButton").gameObject.SetActive(false);
        if (currentPage + 1 == lastPage) buttons.Find("NextPageButton").gameObject.SetActive(false);
        else buttons.Find("NextPageButton").gameObject.SetActive(true);
        RefreshLine();
    }

    public void ConfirmSetDeck() {
        if (setCardNum == 40) return;
        if (EditCardHandler.onAnimation) return;
        EditCardHandler cardHandler = MenuCardInfo.cardInfoWindow.editCard.GetComponent<EditCardHandler>();
        string cardId = cardHandler.cardID;
        StartCoroutine(cardHandler.ShowAddedCardPos());
        if (!setCardList.ContainsKey(MenuCardInfo.cardInfoWindow.editCard.GetComponent<EditCardHandler>().cardID)) {
            GameObject addedCard = settingLayout.transform.GetChild(setCardList.Count).gameObject;
            setCardList.Add(cardId, addedCard);
            EditCardHandler addCardHandler = addedCard.GetComponent<EditCardHandler>();
            addCardHandler.SETNUM++;
            addCardHandler.DrawCard(cardId, isHuman);
            addCardHandler.SetSetNum(true);
            addCardHandler.beforeObject = cardHandler.gameObject;
            addedCard.SetActive(true);
        }
        else {
            EditCardHandler addCardHandler = setCardList[cardId].GetComponent<EditCardHandler>();
            addCardHandler.SETNUM++;
            addCardHandler.SetSetNum(true);
        }
        cardHandler.HAVENUM--;
        haveCardNum--;
        cardHandler.SetHaveNum();
        setCardNum++;
        MenuCardInfo.cardInfoWindow.SetEditCardInfo(cardHandler.HAVENUM, setCardNum);
        RefreshLine();
    }

    public void ConfirmSetDeck(string id, GameObject card) {
        if (setCardNum == 40) return;
        EditCardHandler cardHandler = card.GetComponent<EditCardHandler>();
        if (!setCardList.ContainsKey(id)) {
            GameObject addedCard = settingLayout.transform.GetChild(setCardList.Count).gameObject;
            setCardList.Add(id, addedCard);
            EditCardHandler addCardHandler = addedCard.GetComponent<EditCardHandler>();
            addCardHandler.SETNUM++;
            addCardHandler.DrawCard(id, isHuman);
            addCardHandler.SetSetNum(true);
            addCardHandler.beforeObject = cardHandler.gameObject;
            addedCard.SetActive(true);
        }
        else {
            EditCardHandler addCardHandler = setCardList[id].GetComponent<EditCardHandler>();
            addCardHandler.SETNUM++;
            addCardHandler.SetSetNum(true);
        }
        cardHandler.HAVENUM--;
        haveCardNum--;
        cardHandler.SetHaveNum();
        setCardNum++;
        RefreshLine();
    }


    public void RefreshLine() {
        setCardText.text = setCardNum.ToString() + "/40";
        pagenumText.text = (currentPage + 1).ToString() + maxPage;
        if (setCardNum == 0)
            transform.Find("InnerCanvas/DeckImage/EmptyHand").gameObject.SetActive(true);
        else
            transform.Find("InnerCanvas/DeckImage/EmptyHand").gameObject.SetActive(false);
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(settingLayout.GetComponent<RectTransform>());
    }

    public void SetHeroInfo(string heroId) {
        dataModules.HeroInventory hero = new dataModules.HeroInventory();
        foreach (dataModules.HeroInventory heroes in AccountManager.Instance.allHeroes) {
            if (heroes.id == heroId) {
                hero = heroes;
                break;
            }
        }
        
        Transform heroWindow = transform.Find("InnerCanvas/HeroInfoWindow");
        heroWindow.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = hero.name;
        Transform heroSpine = heroWindow.Find("HeroSpines");
        heroSpine.GetChild(0).gameObject.SetActive(false);
        heroSpine.Find(heroId).gameObject.SetActive(true);
        heroSpine.Find(heroId).SetAsFirstSibling();
        Transform skillWindow = heroWindow.Find("SkillInfo");
        skillWindow.Find("Card1/Card").GetComponent<MenuCardHandler>().DrawCard(hero.heroCards[0].id);
        skillWindow.Find("Card1/CardName").GetComponent<TMPro.TextMeshProUGUI>().text = hero.heroCards[0].name;
        skillWindow.Find("Card1/CardInfo").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.GetComponent<Translator>().DialogSetRichText(hero.heroCards[0].skills[0].desc);
        skillWindow.Find("Card2/Card").GetComponent<MenuCardHandler>().DrawCard(hero.heroCards[1].id);
        skillWindow.Find("Card2/CardName").GetComponent<TMPro.TextMeshProUGUI>().text = hero.heroCards[1].name;
        skillWindow.Find("Card2/CardInfo").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.GetComponent<Translator>().DialogSetRichText(hero.heroCards[1].skills[0].desc);
        cardMana = new int[8];
        for (int i = 0; i < 8; i++)
            cardMana[i] = 0;
    }

    
    public void SetDeckEdit(string heroId, bool isHuman) {
        editing = false;
        this.isHuman = isHuman;
        isTemplate = true;
        InitCanvas();
        heroData = null;
        heroID = heroId;

        deckNamePanel.transform.Find("NameTemplate").GetComponent<TMPro.TMP_InputField>().text = "";
        transform.Find("InnerCanvas/DeckNamePanel/PlaceHolder").gameObject.SetActive(true);    
        SetHeroInfo(heroId);
        
        foreach (dataModules.HeroInventory heroes in AccountManager.Instance.allHeroes) {
            if (heroes.id == heroId) {
                heroData = heroes;
                break;
            }
        }

        Dictionary<string, Sprite> classSprite = AccountManager.Instance.resource.classImage;
        transform.Find("InnerCanvas/SortToClass1").GetComponent<Image>().sprite = classSprite[heroData.heroClasses[0] + "_sortbtnOff"];
        transform.Find("InnerCanvas/SortToClass1/Selected").GetComponent<Image>().sprite = classSprite[heroData.heroClasses[0] + "_sortbtnOn"];
        transform.Find("InnerCanvas/SortToClass2").GetComponent<Image>().sprite = classSprite[heroData.heroClasses[1] + "_sortbtnOff"];
        transform.Find("InnerCanvas/SortToClass2/Selected").GetComponent<Image>().sprite = classSprite[heroData.heroClasses[1] + "_sortbtnOn"];

        if (heroData.heroClasses != null && heroData.heroClasses.Length >= 2) {
            transform.Find("InnerCanvas/ShowAllClass").GetComponent<Image>().sprite = GetAllSortImage(heroData.heroClasses[0], heroData.heroClasses[1], false);
            transform.Find("InnerCanvas/ShowAllClass/Selected").GetComponent<Image>().sprite = GetAllSortImage(heroData.heroClasses[0], heroData.heroClasses[1], true);
        }

        SetDeckEditCards(isHuman, heroData);
    }

    private void SetDeckEditCards(bool isHuman, HeroInventory heroData) {
        int ownCount = -1;
        CardDataPackage myCards = AccountManager.Instance.cardPackage;

        foreach (EditCard card in editCards) {
            if (card.cardClass != heroData.heroClasses[0] && card.cardClass != heroData.heroClasses[1]) continue;
            ownCount++;
            int page = ownCount / 9;
            card.cardObject.transform.SetParent(ownCardLayout.GetChild(page).GetChild(ownCount - (page * 9)));
            card.cardObject.transform.localScale = Vector3.one;
            card.cardObject.transform.localPosition = Vector3.zero;
            EditCardHandler ownedCard = card.cardObject.GetComponent<EditCardHandler>();
            ownedCard.gameObject.SetActive(true);
            if (myCards.data.ContainsKey(card.cardId)) {
                ownedCard.HAVENUM = myCards.data[card.cardId].cardCount;
                haveCardNum += myCards.data[card.cardId].cardCount;
            }
            ownedCard.DrawCard(card.cardId, isHuman);
            maxHaveCard = haveCardNum;
        }

        lastPage = (ownCount / 9) + 1;
        maxPage = "/" + lastPage.ToString();
        currentPage = 0;
        pagenumText.text = (currentPage + 1).ToString() + maxPage;
        RefreshLine();
    }

    public void SetCustumDeckEdit(Deck loadedDeck, bool isTemplate = false) {
        this.isTemplate = isTemplate;
        editing = true;
        heroData = null;
        heroID = loadedDeck.heroId;
        if (loadedDeck.camp == "human")
            isHuman = true;
        else
            isHuman = false;
        InitCanvas();
        if (!isTemplate) deckID = loadedDeck.id;
        deckID = loadedDeck.id;
        

        deckNamePanel.transform.Find("NameTemplate").GetComponent<TMPro.TMP_InputField>().text = loadedDeck.name;
        transform.Find("InnerCanvas/DeckNamePanel/PlaceHolder").gameObject.SetActive(string.IsNullOrEmpty(deckNamePanel.transform.Find("NameTemplate").GetComponent<TMPro.TMP_InputField>().text));
        SetHeroInfo(loadedDeck.heroId);

        if (isHuman) {
            if (isTemplate) {
                foreach (dataModules.Templates data in AccountManager.Instance.humanTemplates) {
                    foreach (dataModules.Deck template in data.templates) {
                        if (template.id == loadedDeck.id) {
                            heroData = data;
                            break;
                        }
                    }
                }
            }
            else {
                foreach(dataModules.Deck data in AccountManager.Instance.humanDecks) {
                    if (data.id == loadedDeck.id) {
                        foreach(dataModules.HeroInventory heroInventory in AccountManager.Instance.humanTemplates) {
                            if(loadedDeck.heroId == heroInventory.id) {
                                heroData = heroInventory;
                                break;
                            }
                        }
                    }
                }
            }
        }
        else {
            if (isTemplate) {
                foreach (dataModules.Templates data in AccountManager.Instance.orcTemplates) {
                    foreach (dataModules.Deck template in data.templates) {
                        if (template.id == loadedDeck.id) {
                            heroData = data;
                            break;
                        }
                    }
                }
            }
            else {
                foreach (dataModules.Deck data in AccountManager.Instance.orcDecks) {
                    if (data.id == loadedDeck.id) {
                        foreach (dataModules.HeroInventory heroInventory in AccountManager.Instance.orcTemplates) {
                            if (loadedDeck.heroId == heroInventory.id) {
                                heroData = heroInventory;
                                break;
                            }
                        }
                    }
                }
            }
        }
        Dictionary<string, Sprite> classSprite = AccountManager.Instance.resource.classImage;
        transform.Find("InnerCanvas/SortToClass1").GetComponent<Image>().sprite = classSprite[heroData.heroClasses[0] + "_sortbtnOff"];
        transform.Find("InnerCanvas/SortToClass1/Selected").GetComponent<Image>().sprite = classSprite[heroData.heroClasses[0] + "_sortbtnOn"];
        transform.Find("InnerCanvas/SortToClass2").GetComponent<Image>().sprite = classSprite[heroData.heroClasses[1] + "_sortbtnOff"];
        transform.Find("InnerCanvas/SortToClass2/Selected").GetComponent<Image>().sprite = classSprite[heroData.heroClasses[1] + "_sortbtnOn"];

        if(heroData.heroClasses != null && heroData.heroClasses.Length >= 2) {
            transform.Find("InnerCanvas/ShowAllClass").GetComponent<Image>().sprite = GetAllSortImage(heroData.heroClasses[0], heroData.heroClasses[1], false);
            transform.Find("InnerCanvas/ShowAllClass/Selected").GetComponent<Image>().sprite = GetAllSortImage(heroData.heroClasses[0], heroData.heroClasses[1], true);
        }

        SetCustomDeckEditCards(loadedDeck, heroData);
    }

    public Sprite GetAllSortImage(string keyword1, string keyword2, bool isOn) {
        Dictionary<string, Sprite> classSprite = AccountManager.Instance.resource.classImage;
        var keys = classSprite.Keys;
        string selectedKey = string.Empty;
        foreach (string key in keys) {
            if(key.Contains(keyword1) && key.Contains(keyword2)) {
                if(isOn && key.Contains("sortbtnOn"))
                    selectedKey = key;
                if(!isOn && key.Contains("sortbtnOff"))
                    selectedKey = key;
            }
        }
        if (!string.IsNullOrEmpty(selectedKey)) {
            return classSprite[selectedKey];
        }
        else {
            return null;
        }
    }

    private void SetCustomDeckEditCards(dataModules.Deck lodedDeck, HeroInventory heroData) {
        int ownCount = -1;
        int settedCardNum = 0;
        CardDataPackage myCards = AccountManager.Instance.cardPackage;

        foreach (dataModules.Item card in lodedDeck.items) {
            if (myCards.data.ContainsKey(card.id)) {
                EditCardHandler settedCard = settingLayout.transform.GetChild(settedCardNum).GetComponent<EditCardHandler>();
                if (myCards.data[card.id].cardCount >= card.cardCount)
                    settedCard.SETNUM = card.cardCount;
                else
                    settedCard.SETNUM = myCards.data[card.id].cardCount;
                settedCard.DrawCard(card.cardId, isHuman);
                settedCard.SetSetNum();
                setCardNum += settedCard.SETNUM;
                setCardList.Add(card.cardId, settedCard.gameObject);
                settedCardNum++;
                settedCard.gameObject.SetActive(true);
            }
        }

        foreach (EditCard card in editCards) {
            if (card.cardClass != heroData.heroClasses[0] && card.cardClass != heroData.heroClasses[1]) continue;
            ownCount++;
            int page = ownCount / 9;
            card.cardObject.transform.SetParent(ownCardLayout.GetChild(page).GetChild(ownCount - (page * 9)));
            card.cardObject.transform.localScale = Vector3.one;
            card.cardObject.transform.localPosition = Vector3.zero;
            EditCardHandler ownedCard = card.cardObject.GetComponent<EditCardHandler>();
            ownedCard.gameObject.SetActive(true);

            if (myCards.data.ContainsKey(card.cardId)) {
                if (!setCardList.ContainsKey(card.cardId)) {
                    ownedCard.HAVENUM = myCards.data[card.cardId].cardCount;
                    haveCardNum += myCards.data[card.cardId].cardCount;
                    ownedCard.DrawCard(card.cardId, isHuman);
                    maxHaveCard = haveCardNum;
                }
                else {
                    EditCardHandler settedCard = setCardList[card.cardId].GetComponent<EditCardHandler>();
                    settedCard.beforeObject = ownedCard.gameObject;
                    ownedCard.HAVENUM = myCards.data[card.cardId].cardCount - settedCard.SETNUM;
                    ownedCard.DrawCard(card.cardId, isHuman);
                    if (ownedCard.HAVENUM == 0) {
                        ownedCard.DisableCard(true);
                    }
                    haveCardNum += ownedCard.HAVENUM;
                }
            }
            else {
                ownedCard.HAVENUM = 0;
                ownedCard.DrawCard(card.cardId, isHuman);
            }
            maxHaveCard = setCardNum + haveCardNum;
        }

        lastPage = (ownCount / 9) + 1;
        maxPage = "/" + lastPage.ToString();
        pagenumText.text = (currentPage + 1).ToString() + maxPage;
        RefreshLine();
    }

    public void SortToAll() {
        int ownCount = -1;
        for (int i = 0; i < ownCardLayout.transform.childCount; i++) {
            for (int j = 0; j < 9; j++) {
                if (ownCardLayout.GetChild(i).GetChild(j).childCount > 0) {
                    EditCardHandler cardHandler = ownCardLayout.GetChild(i).GetChild(j).GetChild(0).GetComponent<EditCardHandler>();
                    cardHandler.transform.SetParent(transform.Find("InnerCanvas/CardStore"));
                }
            }
        }
        List<EditCard> cards = SortCardListByCost(editCards).ToList();
        foreach (EditCard card in cards) {
            if (card.cardClass != heroData.heroClasses[0] && card.cardClass != heroData.heroClasses[1]) continue;
            ownCount++;
            int page = ownCount / 9;
            card.cardObject.transform.SetParent(ownCardLayout.GetChild(page).GetChild(ownCount - (page * 9)));
            card.cardObject.transform.localScale = Vector3.one;
            card.cardObject.transform.localPosition = Vector3.zero;
        }

        ownCardLayout.GetChild(currentPage).gameObject.SetActive(false);
        currentPage = 0;
        ownCardLayout.GetChild(currentPage).gameObject.SetActive(true);
        lastPage = (ownCount / 9) + 1;
        maxPage = "/" + ((ownCount / 9) + 1).ToString();
        pagenumText.text = "1" + maxPage;
        if (lastPage == 1) {
            buttons.Find("NextPageButton").gameObject.SetActive(false);
        }            
        else
            buttons.Find("NextPageButton").gameObject.SetActive(true);
        buttons.Find("PrevPageButton").gameObject.SetActive(false);
        RefreshLine();
        transform.Find("InnerCanvas/ShowAllClass/Selected").gameObject.SetActive(true);
        transform.Find("InnerCanvas/SortToClass1/Selected").gameObject.SetActive(false);
        transform.Find("InnerCanvas/SortToClass2/Selected").gameObject.SetActive(false);
        transform.Find("InnerCanvas/ShowAllCards/Selected").gameObject.SetActive(false);
    }

    public void SortToClass(int classNum) {
        int ownCount = -1;
        string className = heroData.heroClasses[classNum];
        for (int i = 0; i < ownCardLayout.transform.childCount; i++) {
            for (int j = 0; j < 9; j++) {
                if (ownCardLayout.GetChild(i).GetChild(j).childCount > 0) {
                    EditCardHandler cardHandler = ownCardLayout.GetChild(i).GetChild(j).GetChild(0).GetComponent<EditCardHandler>();
                    cardHandler.transform.SetParent(transform.Find("InnerCanvas/CardStore"));
                }
            }
        }
        List<EditCard> cards = SortCardListByCost(editCards).ToList();
        foreach (EditCard card in cards) {
            if (card.cardClass == className) {
                ownCount++;
                int page = ownCount / 9;
                card.cardObject.transform.SetParent(ownCardLayout.GetChild(page).GetChild(ownCount - (page * 9)));
                card.cardObject.transform.localScale = Vector3.one;
                card.cardObject.transform.localPosition = Vector3.zero;
            }
        }

        //List<GameObject> targetCards = new List<GameObject>();
        //for (int i = 0; i < ownCardLayout.transform.childCount; i++) {
        //    for (int j = 0; j < 9; j++) {
        //        if (ownCardLayout.GetChild(i).GetChild(j).childCount > 0) {
        //            EditCardHandler cardHandler = ownCardLayout.GetChild(i).GetChild(j).GetChild(0).GetComponent<EditCardHandler>();
        //            if (cardHandler.editBookRoot != "") {
        //                cardHandler.transform.SetParent(cardStorage);
        //            }
        //        }
        //    }
        //}
        //for (int i = 0; i < cardStorage.childCount; i++) {
        //    EditCardHandler cardHandler = cardStorage.GetChild(i).GetComponent<EditCardHandler>();
        //    if (cardHandler.cardData.cardClasses[0] == className)
        //        targetCards.Add(cardHandler.gameObject);
        //}
        //foreach(GameObject cards in targetCards) {
        //    ownCount++;
        //    int page = ownCount / 9;
        //    cards.transform.SetParent(ownCardLayout.GetChild(page).GetChild(ownCount - (page * 9)));
        //    cards.transform.localPosition = Vector3.zero;
        //}
        ownCardLayout.GetChild(currentPage).gameObject.SetActive(false);
        currentPage = 0;
        ownCardLayout.GetChild(currentPage).gameObject.SetActive(true);
        lastPage = (ownCount / 9) + 1;
        maxPage = "/" + lastPage.ToString();
        pagenumText.text = "1" + maxPage;
        if (lastPage == 1) {
            buttons.Find("NextPageButton").gameObject.SetActive(false);
        }
        else
            buttons.Find("NextPageButton").gameObject.SetActive(true);
        buttons.Find("PrevPageButton").gameObject.SetActive(false);
        RefreshLine();
        transform.Find("InnerCanvas/ShowAllClass/Selected").gameObject.SetActive(false);
        if (classNum == 0) {
            transform.Find("InnerCanvas/SortToClass1/Selected").gameObject.SetActive(true);
            transform.Find("InnerCanvas/SortToClass2/Selected").gameObject.SetActive(false);
        }
        else {
            transform.Find("InnerCanvas/SortToClass1/Selected").gameObject.SetActive(false);
            transform.Find("InnerCanvas/SortToClass2/Selected").gameObject.SetActive(true);
        }
        transform.Find("InnerCanvas/ShowAllCards/Selected").gameObject.SetActive(false);
    }

    public void ShowAllClassCards() {
        int ownCount = -1;
        for (int i = 0; i < ownCardLayout.transform.childCount; i++) {
            for (int j = 0; j < 9; j++) {
                if (ownCardLayout.GetChild(i).GetChild(j).childCount > 0) {
                    EditCardHandler cardHandler = ownCardLayout.GetChild(i).GetChild(j).GetChild(0).GetComponent<EditCardHandler>();
                    cardHandler.transform.SetParent(transform.Find("InnerCanvas/CardStore"));
                }
            }
        }
        List<EditCard> cards = SortCardListByCost(editCards).ToList();
        foreach (EditCard card in cards) {
            ownCount++;
            int page = ownCount / 9;
            card.cardObject.transform.SetParent(ownCardLayout.GetChild(page).GetChild(ownCount - (page * 9)));
            card.cardObject.transform.localScale = Vector3.one;
            card.cardObject.transform.localPosition = Vector3.zero;
            if(card.cardClass != heroData.heroClasses[0] && card.cardClass != heroData.heroClasses[1]) {
                card.cardObject.SetActive(true);
                card.cardObject.GetComponent<EditCardHandler>().HAVENUM = 0;
                card.cardObject.GetComponent<EditCardHandler>().DrawCard(card.cardId, isHuman);
            }
        }
        ownCardLayout.GetChild(currentPage).gameObject.SetActive(false);
        currentPage = 0;
        ownCardLayout.GetChild(currentPage).gameObject.SetActive(true);
        lastPage = (ownCount / 9) + 1;
        maxPage = "/" + lastPage.ToString();
        pagenumText.text = "1" + maxPage;
        if (lastPage == 1) {
            buttons.Find("NextPageButton").gameObject.SetActive(false);
        }
        else
            buttons.Find("NextPageButton").gameObject.SetActive(true);
        buttons.Find("PrevPageButton").gameObject.SetActive(false);
        RefreshLine();
        transform.Find("InnerCanvas/ShowAllClass/Selected").gameObject.SetActive(false);
        transform.Find("InnerCanvas/SortToClass1/Selected").gameObject.SetActive(false);
        transform.Find("InnerCanvas/SortToClass2/Selected").gameObject.SetActive(false);
        transform.Find("InnerCanvas/ShowAllCards/Selected").gameObject.SetActive(true);
    }

    public void SelectInputName() {
        transform.Find("InnerCanvas/DeckNamePanel/PlaceHolder").gameObject.SetActive(false);
    }

    public void EndInputName() {
        if(string.IsNullOrEmpty(deckNamePanel.transform.Find("NameTemplate").GetComponent<TMPro.TMP_InputField>().text))
            transform.Find("InnerCanvas/DeckNamePanel/PlaceHolder").gameObject.SetActive(true);
    }




    /// <summary>
    /// Server에게 덱 수정 요청
    /// </summary>
    /// <param name="data"></param>
    /// <param name="deckId"></param>
    public void RequestModifyDeck(NetworkManager.ModifyDeckReqFormat formatData, string deckId) {
        var fields = new List<NetworkManager.ModifyDeckReqArgs>();
        NetworkManager.ModifyDeckReqArgs field = new NetworkManager.ModifyDeckReqArgs();
        DeckItem data;
        items = new List<DeckItem>();

        foreach (var pairs in setCardList) {
            int count = pairs.Value.GetComponent<EditCardHandler>().SETNUM;
            if (items.Exists(x => x.cardId == pairs.Key)) {
                var itemCount = items.Find(x => x.cardId == pairs.Key);
                itemCount.cardCount = count;
            }
            else {
                data = new DeckItem();
                data.cardId = pairs.Key;
                data.cardCount = count;
                items.Add(data);
            }
        }        

        //덱 이름
        field.fieldName = NetworkManager.ModifyDeckReqField.ITEMS;  
        field.value = items.ToArray();
        formatData.parms.Add(field);

        AccountManager.Instance.RequestDeckModify(formatData, deckId);
    }

    /// <summary>
    /// Server에게 덱 새로 추가 요청(커스텀 덱)
    /// </summary>
    public void RequestNewDeck() {
        if (string.IsNullOrEmpty(deckNamePanel.transform.Find("NameTemplate").GetComponent<TMPro.TMP_InputField>().text) == true) {
            Modal.instantiate("덱 이름을 입력해 주세요.", Modal.Type.CHECK);
            return;
        }
        NetworkManager.AddCustomDeckReqFormat formatData = new NetworkManager.AddCustomDeckReqFormat();
        items = new List<DeckItem>();
        foreach (var pairs in setCardList) {
            int count = pairs.Value.GetComponent<EditCardHandler>().SETNUM;
            DeckItem data;

            if (items.Exists(x => x.cardId == pairs.Key)) {
                var itemCount = items.Find(x => x.cardId == pairs.Key);
                itemCount.cardCount = count;
            }
            else {
                data = new DeckItem();
                data.cardId = pairs.Key;
                data.cardCount = count;
                items.Add(data);
            }
        }
        var nameVal = deckNamePanel.transform.Find("NameTemplate").GetComponent<TMPro.TMP_InputField>().text;

        formatData.heroId = heroID; //영웅 id
        formatData.items = items.ToArray(); //추가한 카드 정보들
        formatData.name = nameVal.Replace(" ", string.Empty);
        formatData.camp = (isHuman == true) ? "human" : "orc";
        formatData.bannerImage = "custom";

        AccountManager.Instance.RequestDeckMake(formatData);
    }

    [System.Serializable]
    public class DeckItem {
        public string cardId;
        public int cardCount;
        public DeckItem() { }

        public DeckItem(string cardId, int cardCount) {
            this.cardId = cardId;
            this.cardCount = cardCount;
        }
    }
}

public class EditCard {
    public GameObject cardObject;
    public string cardId;
    public string cardClass;
    public int costOrder;
    public int rareOrder;
}
