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
    public string heroID;

    private Transform buttons;
    private Transform settingLayout;
    private Transform ownCardLayout;
    private Transform heroInfoWindow;

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

    public TemplateMenu templateMenu;

    public MenuSceneController menuSceneController;
    bool isTemplate = false;
    int currentPage;

    private void Start() {
        SetObject();
    }

    private void SetObject() {
        deckNamePanel = transform.Find("DeckNamePanel").gameObject;        
        settingLayout = transform.Find("HandDeck/Mask/SettedDeck");
        ownCardLayout = transform.Find("CardBook");
        heroInfoWindow = transform.Find("HeroInfoWindow");
        pagenumText = transform.Find("PageNumber/Capacity").GetComponent<TMPro.TextMeshProUGUI>();
        setCardText = transform.Find("HandDeck/Capacity").GetComponent<TMPro.TextMeshProUGUI>();
        buttons = transform.Find("Buttons");

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
        for (int i = 0; i < 40; i++) {
            EditCardHandler cardHandler = settingLayout.GetChild(i).GetComponent<EditCardHandler>();
            cardHandler.InitEditCard();
            cardHandler.deckEditController = this;
            cardHandler.menuCardInfo = this.menuCardInfo;
        }
        for(int i = 0; i < ownCardLayout.transform.childCount; i++) {
            for (int j = 0; j < 9; j++) {
                EditCardHandler cardHandler = ownCardLayout.GetChild(i).GetChild(j).GetChild(0).GetComponent<EditCardHandler>();
                cardHandler.InitEditCard();
                cardHandler.deckEditController = this;
                cardHandler.menuCardInfo = this.menuCardInfo;
            }
        }
        ownCardLayout.GetChild(currentPage).gameObject.SetActive(true);
        for (int i = 1; i < ownCardLayout.childCount; i++)
            ownCardLayout.GetChild(i).gameObject.SetActive(false);
        buttons.Find("NextPageButton").gameObject.SetActive(true);
        buttons.Find("PrevPageButton").gameObject.SetActive(false);
        pagenumText.text = "1/" + ownCardLayout.childCount.ToString();
        selectCard = null;
        //transform.Find("SetDeckLayout").Find("glow").gameObject.SetActive(false);
        //transform.Find("ExceptButton").gameObject.SetActive(false);
    }

    public void ConfirmButton() {

        if(editing == true) {
            NetworkManager.ModifyDeckReqFormat form = new NetworkManager.ModifyDeckReqFormat();
            NetworkManager.ModifyDeckReqArgs field = new NetworkManager.ModifyDeckReqArgs();

            field.fieldName = NetworkManager.ModifyDeckReqField.NAME;

            string inputNameVal = deckNamePanel.transform.Find("NameTemplate").Find("Text").GetComponent<Text>().text;
            
            
            if (string.IsNullOrEmpty(inputNameVal)) {
                Modal.instantiate("원하는 이름을 적어주세요", Modal.Type.CHECK);
                return;
            }
            if (inputNameVal.Contains(" ")) {
                Modal.instantiate("덱 이름의 빈 칸은 제거됩니다.", Modal.Type.CHECK);
                inputNameVal = inputNameVal.Replace(" ", string.Empty);
            }
            field.value = inputNameVal;
            form.parms.Add(field);

            //template을 통한 덱 생성도 새로운 생성 요청으로 취급 해야함.
            if (isTemplate) RequestNewDeck();
            else RequestModifyDeck(form, deckID);
        }
        else {
            RequestNewDeck();
        }


    }

    public void CancelButton() {
        setCardList = null;
        gameObject.SetActive(false);
        if (templateMenu != null)
            templateMenu.transform.gameObject.SetActive(false);

        deckNamePanel.transform.Find("NameTemplate").Find("Text").GetComponent<Text>().text = "";
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
            transform.Find("SetDeckLayout").Find("glow").gameObject.SetActive(true);

        if (selectCard.transform.parent.name == "SetDeck") {
            transform.Find("ExceptButton").gameObject.SetActive(true);
        }
        selectCard.transform.Find("SelectedPanel").gameObject.SetActive(true);
    }

    public void OpenHeroInfo() {
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
        if (!ownCardLayout.GetChild(currentPage + 1).GetChild(0).gameObject.activeSelf || currentPage + 1 < ownCardLayout.childCount) {
            buttons.Find("NextPageButton").gameObject.SetActive(false);
            return;
        }
    }

    public void PrevPageButton() {
        ownCardLayout.GetChild(currentPage).gameObject.SetActive(false);
        currentPage--;
        ownCardLayout.GetChild(currentPage).gameObject.SetActive(true);
        buttons.Find("NextPageButton").gameObject.SetActive(true);
        pagenumText.text = (currentPage + 1).ToString() + maxPage;
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
        beforeCard.SetHaveNum();
        if (cardHandler.SETNUM == 0) {
            cardHandler.beforeObject = null;
            setCardList.Remove(id);
        }
        setCardNum--;
        haveCardNum++;
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
            addCardHandler.SetSetNum();
            addCardHandler.beforeObject = cardHandler.gameObject;
            addedCard.SetActive(true);
        }
        else {
            EditCardHandler addCardHandler = setCardList[id].GetComponent<EditCardHandler>();
            addCardHandler.SETNUM++;
            addCardHandler.SetSetNum();
        }
        cardHandler.HAVENUM--;
        haveCardNum--;
        cardHandler.SetHaveNum();
        setCardNum++;
        RefreshLine();
    }


    public void RefreshLine() {
        setCardText.text = setCardNum.ToString() + "/40";
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(settingLayout.GetComponent<RectTransform>());
    }



    
    
    public void SetDeckEdit(string heroId, bool isHuman) {
        editing = false;
        InitCanvas();
        Transform heroCards;
        HeroInventory heroData = null;
        this.isHuman = isHuman;
        deckNamePanel.transform.Find("NameTemplate").GetComponent<InputField>().text = "";

        Transform heroSpines = heroInfoWindow.Find("HeroSpines");
        for (int i = 0; i < heroSpines.childCount; i++) 
            heroSpines.GetChild(i).gameObject.SetActive(false);
        heroSpines.Find(heroId).gameObject.SetActive(true);

        if (isHuman) {
            heroInfoWindow.Find("BackGroundImage/Human").gameObject.SetActive(true);
            heroInfoWindow.Find("BackGroundImage/Orc").gameObject.SetActive(false);
            heroCards = heroInfoWindow.Find("HeroCards/Human");
            heroInfoWindow.Find("HeroCards/Orc").gameObject.SetActive(false);
            foreach(dataModules.Templates data in AccountManager.Instance.humanTemplates) {
                if (data.id == heroId) {
                    heroData = data;
                    break;
                }
            }
        }
        else {
            heroInfoWindow.Find("BackGroundImage/Human").gameObject.SetActive(false);
            heroInfoWindow.Find("BackGroundImage/Orc").gameObject.SetActive(true);
            heroCards = heroInfoWindow.Find("HeroCards/Orc");
            heroInfoWindow.Find("HeroCards/Human").gameObject.SetActive(false);
            foreach (dataModules.Templates data in AccountManager.Instance.orcTemplates) {
                if (data.id == heroId) {
                    heroData = data;
                    break;
                }
            }
        }
        heroInfoWindow.Find("Class/Class_1").GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[heroData.heroClasses[0]];
        heroInfoWindow.Find("Class/Class_2").GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[heroData.heroClasses[1]];
        heroCards.gameObject.SetActive(true);
        for(int i = 0; i < heroData.heroCards.Length; i++)
            heroCards.GetChild(i).GetComponent<MenuCardHandler>().DrawCard(heroData.heroCards[i].cardId, isHuman);
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
            if (myCards.data.ContainsKey(card.id)) {
                ownCount++;
                int page = ownCount / 9;
                EditCardHandler ownedCard = ownCardLayout.GetChild(page).GetChild(ownCount - (page * 9)).GetChild(0).GetComponent<EditCardHandler>();
                ownedCard.gameObject.SetActive(true);
                ownedCard.HAVENUM = myCards.data[card.id].cardCount;
                haveCardNum += myCards.data[card.id].cardCount;
                ownedCard.DrawCard(card.id, isHuman);
                maxHaveCard = haveCardNum;
                RefreshLine();
            }
        }
        maxPage = "/" + ((ownCount / 9) + 1).ToString();
        pagenumText.text = (currentPage + 1).ToString() + maxPage;
    }

    public void SetCustumDeckEdit(Deck loadedDeck, bool isTemplate = false) {
        this.isTemplate = isTemplate;
        editing = true;
        InitCanvas();
        Transform heroCards;
        HeroInventory heroData = null;
        if(!isTemplate) deckID = loadedDeck.id;
        deckID = loadedDeck.id;
        if (loadedDeck.camp == "human")
            isHuman = true;
        else
            isHuman = false;
        deckNamePanel.transform.Find("NameTemplate").GetComponent<InputField>().text = loadedDeck.name;

        Transform heroSpines = heroInfoWindow.Find("HeroSpines");
        for (int i = 0; i < heroSpines.childCount; i++)
            heroSpines.GetChild(i).gameObject.SetActive(false);
        heroSpines.Find(loadedDeck.heroId).gameObject.SetActive(true);

        if (isHuman) {
            heroInfoWindow.Find("BackGroundImage/Human").gameObject.SetActive(true);
            heroInfoWindow.Find("BackGroundImage/Orc").gameObject.SetActive(false);
            heroCards = heroInfoWindow.Find("HeroCards/Human");
            heroInfoWindow.Find("HeroCards/Orc").gameObject.SetActive(false);
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
            heroInfoWindow.Find("BackGroundImage/Human").gameObject.SetActive(false);
            heroInfoWindow.Find("BackGroundImage/Orc").gameObject.SetActive(true);
            heroCards = heroInfoWindow.Find("HeroCards/Orc");
            heroInfoWindow.Find("HeroCards/Human").gameObject.SetActive(false);
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
        heroInfoWindow.Find("Class/Class_1").GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[heroData.heroClasses[0]];
        heroInfoWindow.Find("Class/Class_2").GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[heroData.heroClasses[1]];
        heroCards.gameObject.SetActive(true);
        for (int i = 0; i < heroData.heroCards.Length; i++)
            heroCards.GetChild(i).GetComponent<MenuCardHandler>().DrawCard(heroData.heroCards[i].cardId, isHuman);
        SetCustomDeckEditCards(loadedDeck, heroData);
    }

    private void SetCustomDeckEditCards(dataModules.Deck lodedDeck, HeroInventory heroData) {
        int ownCount = -1;
        int settedCardNum = 0;
        CardDataPackage myCards = AccountManager.Instance.cardPackage;
        foreach(dataModules.Item card in lodedDeck.items) {
            EditCardHandler settedCard = settingLayout.transform.GetChild(settedCardNum).GetComponent<EditCardHandler>();
            settedCard.SETNUM = card.cardCount;
            settedCard.DrawCard(card.cardId, isHuman);
            setCardNum += card.cardCount;
            setCardList.Add(card.cardId, settedCard.gameObject);
            settedCardNum++;
            settedCard.gameObject.SetActive(true);
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
            if (myCards.data.ContainsKey(card.id)) {
                if (!setCardList.ContainsKey(card.id)){
                    ownCount++;
                    int page = ownCount / 9;
                    EditCardHandler ownedCard = ownCardLayout.GetChild(page).GetChild(ownCount - (page * 9)).GetChild(0).GetComponent<EditCardHandler>();
                    ownedCard.gameObject.SetActive(true);
                    ownedCard.HAVENUM = myCards.data[card.id].cardCount;
                    haveCardNum += myCards.data[card.id].cardCount;
                    ownedCard.DrawCard(card.id, isHuman);
                    maxHaveCard = haveCardNum;
                }
                else {
                    ownCount++;
                    int page = ownCount / 9;
                    EditCardHandler settedCard = setCardList[card.id].GetComponent<EditCardHandler>();
                    EditCardHandler ownedCard = ownCardLayout.GetChild(page).GetChild(ownCount - (page * 9)).GetChild(0).GetComponent<EditCardHandler>();
                    ownedCard.gameObject.SetActive(true);
                    settedCard.beforeObject = ownedCard.gameObject;
                    ownedCard.HAVENUM = myCards.data[card.id].cardCount - settedCard.SETNUM;
                    ownedCard.DrawCard(card.id, isHuman);
                    if (ownedCard.HAVENUM == 0) {
                        ownedCard.DisableCard(true);
                    }                    
                    haveCardNum += ownedCard.HAVENUM;
                }
            }
            maxHaveCard = setCardNum + haveCardNum;
            RefreshLine();
        }
        maxPage = "/" + ((ownCount / 9) + 1).ToString();
        pagenumText.text = (currentPage + 1).ToString() + maxPage;
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
        if (string.IsNullOrEmpty(deckNamePanel.transform.Find("NameTemplate").Find("Text").GetComponent<Text>().text) == true) {
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
        var nameVal = deckNamePanel.transform.Find("NameTemplate").Find("Text").GetComponent<Text>().text;
        heroID = (isHuman == true) ? "h10001" : "h10002";

        formatData.heroId = heroID; //영웅 id
        formatData.items = items.ToArray(); //추가한 카드 정보들
        formatData.name = nameVal.Replace(" ", string.Empty);
        formatData.camp = (isHuman == true) ? "human" : "orc";

        AccountManager.Instance.RequestDeckMake(formatData, OnMakeNewDeckFinished);
    }

    private void OnMakeNewDeckFinished(HTTPRequest originalRequest, HTTPResponse response) {
        //덱 새로 생성 완료
        if (response.StatusCode == 200) {
            Logger.Log("덱 생성 완료");
            menuSceneController.decksLoader.Load();
            gameObject.SetActive(false);
            templateMenu.transform.gameObject.SetActive(false);
            deckNamePanel.transform.Find("NameTemplate").Find("Text").GetComponent<Text>().text = "";
        }

    }

    [System.Serializable]
    public class DeckItem {
        public string cardId;
        public int cardCount;
    }
}
