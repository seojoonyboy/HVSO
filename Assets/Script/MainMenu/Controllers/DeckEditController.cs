using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using dataModules;
using TMPro;
using UnityEngine.EventSystems;
using BestHTTP;

public class DeckEditController : MonoBehaviour
{
    [SerializeField] MenuCardInfo menuCardInfo;
    [SerializeField] Transform cardStorage;
    [SerializeField] MenuHeroInfo heroInfoWindow;
    public string heroID;
    HeroInventory heroData;

    private Transform buttons;
    public Transform settingLayout;
    private Transform ownCardLayout;

    private GameObject deckNamePanel;

    public GameObject selectCard;
    public bool editing = false;

    public Dictionary<string, GameObject> setCardList;
    List<DeckItem> items;

    public string deckID = null;
    bool isHuman;
    int setCardNum = 0;
    int haveCardNum = 0;
    int maxHaveCard = 0;
    int dontHaveCard = 0;
    public TMPro.TextMeshProUGUI setCardText;
    TMPro.TextMeshProUGUI pagenumText;
    string maxPage;
    int lastPage;

    public TemplateMenu templateMenu;

    public MenuSceneController menuSceneController;
    bool isTemplate = false;
    int currentPage;

    private void Start() {
        SetObject();
    }

    private void SetObject() {
        deckNamePanel = transform.Find("InnerCanvas/DeckNamePanel").gameObject;        
        settingLayout = transform.Find("InnerCanvas/HandDeck/Mask/SettedDeck");
        ownCardLayout = transform.Find("InnerCanvas/CardBook");
        pagenumText = transform.Find("InnerCanvas/PageNumber/Capacity").GetComponent<TMPro.TextMeshProUGUI>();
        setCardText = transform.Find("InnerCanvas/HandDeck/Capacity").GetComponent<TMPro.TextMeshProUGUI>();
        buttons = transform.Find("InnerCanvas/Buttons");

        buttons.Find("SaveDeckButton").GetComponent<Button>().onClick.AddListener(delegate () { ConfirmButton(); });
        buttons.Find("CancelButton").GetComponent<Button>().onClick.AddListener(delegate () { CancelButton(); });
        gameObject.SetActive(false);
    }

    private void InitCanvas() {
        setCardList = new Dictionary<string, GameObject>();
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
            cardHandler.menuCardInfo = this.menuCardInfo;
        }
        for(int i = 0; i < ownCardLayout.transform.childCount; i++) {
            for (int j = 0; j < 9; j++) {
                EditCardHandler cardHandler = ownCardLayout.GetChild(i).GetChild(j).GetChild(0).GetComponent<EditCardHandler>();
                if (cardHandler.editBookRoot != "")
                    cardHandler.transform.SetParent(ownCardLayout.Find(cardHandler.editBookRoot));
                cardHandler.InitEditCard();
                cardHandler.deckEditController = this;
                cardHandler.menuCardInfo = this.menuCardInfo;
                cardHandler.transform.localPosition = Vector3.zero;
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
        transform.Find("InnerCanvas/CancelWindow").gameObject.SetActive(true);
        //if(isTemplate)
    }

    public void CancelEdit() {
        transform.Find("InnerCanvas/CancelWindow").gameObject.SetActive(false);
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
        transform.Find("InnerCanvas/CancelWindow").gameObject.SetActive(false);
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
        heroInfoWindow.transform.parent.gameObject.SetActive(true);
        heroInfoWindow.gameObject.SetActive(true);
    }
    public void CloseHeroInfo() {
        heroInfoWindow.gameObject.SetActive(false);
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



    
    
    public void SetDeckEdit(string heroId, bool isHuman) {
        editing = false;
        InitCanvas();
        //Transform heroCards;
        heroData = null;
        heroID = heroId;
        this.isHuman = isHuman;
        isTemplate = true;

        deckNamePanel.transform.Find("NameTemplate").GetComponent<TMPro.TMP_InputField>().text = "";
        transform.Find("InnerCanvas/DeckNamePanel/PlaceHolder").gameObject.SetActive(true);    
        heroInfoWindow.SetHeroInfoWindow(heroId);
        
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
        foreach(dataModules.CollectionCard card in AccountManager.Instance.allCards) {
            if (card.isHeroCard) continue;
            if (card.cardClasses[0] != heroData.heroClasses[0] && card.cardClasses[0] != heroData.heroClasses[1]) continue;
            string race;
            if (isHuman)
                race = "human";
            else
                race = "orc";
            if (card.camp != race) continue;

            ownCount++;
            int page = ownCount / 9;
            EditCardHandler ownedCard = ownCardLayout.GetChild(page).GetChild(ownCount - (page * 9)).GetChild(0).GetComponent<EditCardHandler>();
            ownedCard.editBookRoot = ownedCard.transform.parent.parent.name + "/" + ownedCard.transform.parent.name;
            ownedCard.gameObject.SetActive(true);
            if (myCards.data.ContainsKey(card.id)) {
                ownedCard.HAVENUM = myCards.data[card.id].cardCount;
                haveCardNum += myCards.data[card.id].cardCount;
            }
            ownedCard.DrawCard(card.id, isHuman);
            maxHaveCard = haveCardNum;
            RefreshLine();
        }
        lastPage = (ownCount / 9) + 1;
        maxPage = "/" + lastPage.ToString();
        currentPage = 0;
        pagenumText.text = (currentPage + 1).ToString() + maxPage;
    }

    public void SetCustumDeckEdit(Deck loadedDeck, bool isTemplate = false) {
        this.isTemplate = isTemplate;
        editing = true;
        InitCanvas();
        heroData = null;
        heroID = loadedDeck.heroId;
        if (!isTemplate) deckID = loadedDeck.id;
        deckID = loadedDeck.id;
        if (loadedDeck.camp == "human")
            isHuman = true;
        else
            isHuman = false;

        deckNamePanel.transform.Find("NameTemplate").GetComponent<TMPro.TMP_InputField>().text = loadedDeck.name;
        transform.Find("InnerCanvas/DeckNamePanel/PlaceHolder").gameObject.SetActive(string.IsNullOrEmpty(deckNamePanel.transform.Find("NameTemplate").GetComponent<TMPro.TMP_InputField>().text));
        heroInfoWindow.SetHeroInfoWindow(loadedDeck.heroId);

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
        foreach(dataModules.Item card in lodedDeck.items) {
            if (myCards.data.ContainsKey(card.id)) {
                EditCardHandler settedCard = settingLayout.transform.GetChild(settedCardNum).GetComponent<EditCardHandler>();
                if(myCards.data[card.id].cardCount >= card.cardCount)
                    settedCard.SETNUM = card.cardCount;
                else
                    settedCard.SETNUM = myCards.data[card.id].cardCount;
                settedCard.DrawCard(card.cardId, isHuman);
                setCardNum += settedCard.SETNUM;
                setCardList.Add(card.cardId, settedCard.gameObject);
                settedCardNum++;
                settedCard.gameObject.SetActive(true);
            }
        }

        foreach (dataModules.CollectionCard card in AccountManager.Instance.allCards) {
            if (card.isHeroCard) continue;
            if (card.cardClasses[0] != heroData.heroClasses[0] && card.cardClasses[0] != heroData.heroClasses[1]) continue;
            string race;
            if (isHuman)
                race = "human";
            else
                race = "orc";
            if (card.camp != race) continue;
            ownCount++;
            int page = ownCount / 9;
            EditCardHandler ownedCard = ownCardLayout.GetChild(page).GetChild(ownCount - (page * 9)).GetChild(0).GetComponent<EditCardHandler>();
            ownedCard.gameObject.SetActive(true);
            ownedCard.editBookRoot = ownedCard.transform.parent.parent.name + "/" + ownedCard.transform.parent.name;
            if (myCards.data.ContainsKey(card.id)) {
                if (!setCardList.ContainsKey(card.id)) {
                    ownedCard.HAVENUM = myCards.data[card.id].cardCount;
                    haveCardNum += myCards.data[card.id].cardCount;
                    ownedCard.DrawCard(card.id, isHuman);
                    maxHaveCard = haveCardNum;
                }
                else {
                    EditCardHandler settedCard = setCardList[card.id].GetComponent<EditCardHandler>();
                    settedCard.beforeObject = ownedCard.gameObject;
                    ownedCard.HAVENUM = myCards.data[card.id].cardCount - settedCard.SETNUM;
                    ownedCard.DrawCard(card.id, isHuman);
                    if (ownedCard.HAVENUM == 0) {
                        ownedCard.DisableCard(true);
                    }
                    haveCardNum += ownedCard.HAVENUM;
                }
            }
            else {
                ownedCard.HAVENUM = 0;
                ownedCard.DrawCard(card.id, isHuman);
            }
            maxHaveCard = setCardNum + haveCardNum;
            RefreshLine();
        }
        lastPage = (ownCount / 9) + 1;
        maxPage = "/" + lastPage.ToString();
        pagenumText.text = (currentPage + 1).ToString() + maxPage;
    }

    public void SortToAll() {
        int ownCount = -1;
        List<GameObject> targetCards = new List<GameObject>();
        for (int i = 0; i < ownCardLayout.transform.childCount; i++) {
            for (int j = 0; j < 9; j++) {
                if (ownCardLayout.GetChild(i).GetChild(j).childCount > 0) {
                    EditCardHandler cardHandler = ownCardLayout.GetChild(i).GetChild(j).GetChild(0).GetComponent<EditCardHandler>();
                    if (cardHandler.editBookRoot != "") {
                        cardHandler.transform.SetParent(cardStorage);
                    }
                }
            }
        }
        while(cardStorage.childCount > 0) {
            EditCardHandler cardHandler = cardStorage.GetChild(0).GetComponent<EditCardHandler>();
            cardHandler.transform.SetParent(ownCardLayout.Find(cardHandler.editBookRoot));
            cardHandler.transform.localPosition = Vector3.zero;
            ownCount++;
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
    }

    public void SortToClass(int classNum) {
        int ownCount = -1;
        string className = heroData.heroClasses[classNum];
        List<GameObject> targetCards = new List<GameObject>();
        for (int i = 0; i < ownCardLayout.transform.childCount; i++) {
            for (int j = 0; j < 9; j++) {
                if (ownCardLayout.GetChild(i).GetChild(j).childCount > 0) {
                    EditCardHandler cardHandler = ownCardLayout.GetChild(i).GetChild(j).GetChild(0).GetComponent<EditCardHandler>();
                    if (cardHandler.editBookRoot != "") {
                        cardHandler.transform.SetParent(cardStorage);
                    }
                }
            }
        }
        for (int i = 0; i < cardStorage.childCount; i++) {
            EditCardHandler cardHandler = cardStorage.GetChild(i).GetComponent<EditCardHandler>();
            if (cardHandler.cardData.cardClasses[0] == className)
                targetCards.Add(cardHandler.gameObject);
        }
        foreach(GameObject cards in targetCards) {
            ownCount++;
            int page = ownCount / 9;
            cards.transform.SetParent(ownCardLayout.GetChild(page).GetChild(ownCount - (page * 9)));
            cards.transform.localPosition = Vector3.zero;
        }
        ownCardLayout.GetChild(currentPage).gameObject.SetActive(false);
        currentPage = 0;
        ownCardLayout.GetChild(currentPage).gameObject.SetActive(true);
        int lastPage = (ownCount / 9) + 1;
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

        AccountManager.Instance.RequestDeckModify(formatData, deckId, OnDeckModifyFinished);
    }

    private void OnDeckModifyFinished(HTTPRequest originalRequest, HTTPResponse response) {
        //덱 수정 요청 완료
        if (response.StatusCode == 200) {
            Logger.Log("덱 편집완료 완료");
            menuSceneController.decksLoader.Load();
            gameObject.SetActive(false);
        }
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

        AccountManager.Instance.RequestDeckMake(formatData, OnMakeNewDeckFinished);
    }

    private void OnMakeNewDeckFinished(HTTPRequest originalRequest, HTTPResponse response) {
        //덱 새로 생성 완료
        if (response.StatusCode == 200) {
            Logger.Log("덱 생성 완료");
            menuSceneController.decksLoader.Load();
            gameObject.SetActive(false);
            if (templateMenu != null) {
                templateMenu.transform.gameObject.SetActive(false);
                templateMenu = null;
            }
            deckNamePanel.transform.Find("NameTemplate").GetComponent<TMPro.TMP_InputField>().text = "";
        }

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
