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
    public string heroID;
    private GameObject heroCardGroup;

    private GameObject settingLayout;
    private GameObject ownCardLayout;
    private GameObject UnReleaseCardLayout;

    private GameObject deckNamePanel;

    public GameObject selectCard;
    public bool editing = false;

    Dictionary<string, GameObject> setCardList;
    List<DeckItem> items;

    public string deckID = null;
    bool isHuman;
    int setCardNum = 0;
    int haveCardNum = 0;
    int maxHaveCard = 0;
    int dontHaveCard = 0;
    TMPro.TextMeshProUGUI setCardText;
    TMPro.TextMeshProUGUI haveCardText;
    TMPro.TextMeshProUGUI dontHaveCardText;

    public TemplateMenu templateMenu;

    public MenuSceneController menuSceneController;

    private void Start() {
        SetObject();
    }

    private void InitCanvas() {
        
        setCardList = new Dictionary<string, GameObject>();
        setCardNum = 0;
        haveCardNum = 0;
        dontHaveCard = 0;
        for (int i = 0; i < 40; i++) {
            ownCardLayout.transform.GetChild(i).gameObject.SetActive(false);
            UnReleaseCardLayout.transform.GetChild(i).gameObject.SetActive(false);
            settingLayout.transform.GetChild(i).gameObject.SetActive(false);
            settingLayout.transform.GetChild(i).GetComponent<EditCardHandler>().SETNUM = 0;
        }
        selectCard = null;
        transform.Find("SetDeckLayout").Find("glow").gameObject.SetActive(false);
        transform.Find("ExceptButton").gameObject.SetActive(false);
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

            RequestModifyDeck(form, deckID);
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
        if (card.transform.Find("Disabled").gameObject.activeSelf) return;
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
    

    public void ExpectFromDeck() {
        if (selectCard == null) return;
        EditCardHandler cardHandler = selectCard.GetComponent<EditCardHandler>();
        string id = cardHandler.cardID;
        cardHandler.SETNUM--;
        cardHandler.SetSetNum();
        selectCard.transform.Find("SelectedPanel").gameObject.SetActive(false);
        EditCardHandler beforeCard = cardHandler.beforeObject.GetComponent<EditCardHandler>();
        beforeCard.HAVENUM++;
        if (beforeCard.HAVENUM > 0)
            cardHandler.beforeObject.SetActive(true);
        beforeCard.SetHaveNum();
        if (cardHandler.SETNUM == 0) {
            beforeCard = null;
            selectCard.transform.SetAsLastSibling();
            setCardList.Remove(id);
            selectCard.gameObject.SetActive(false);
        }
        transform.Find("ExceptButton").gameObject.SetActive(false);
        setCardNum--;
        haveCardNum++;
        RefreshLine();
    }

    public void ConfirmSetDeck() {
        if (selectCard == null) return;
        if (setCardNum == 40) return;
        EditCardHandler cardHandler = selectCard.GetComponent<EditCardHandler>();
        string id = cardHandler.cardID;
        if (!setCardList.ContainsKey(id)) {
            GameObject addedCard = settingLayout.transform.GetChild(setCardList.Count).gameObject;
            setCardList.Add(id, addedCard);
            EditCardHandler addCardHandler = addedCard.GetComponent<EditCardHandler>();
            addCardHandler.SETNUM++;
            addCardHandler.DrawCard(id, isHuman);
            addCardHandler.SetSetNum();
            addCardHandler.beforeObject = selectCard;
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
        selectCard.transform.Find("SelectedPanel").gameObject.SetActive(false);
        if (cardHandler.HAVENUM == 0)
            selectCard.SetActive(false);
        selectCard = null;
        transform.Find("SetDeckLayout").Find("glow").gameObject.SetActive(false);
        setCardNum++;
        RefreshLine();
    }


    public void RefreshLine() {
        setCardText.text = setCardNum.ToString() + "/40";
        haveCardText.text = haveCardNum.ToString() + "/" + maxHaveCard.ToString();
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.Find("CardPanel").Find("Viewport").Find("Content").GetComponent<RectTransform>());
    }



    private void SetObject() {
        deckNamePanel = transform.Find("DeckNamePanel").gameObject;
        heroCardGroup = transform.Find("HeroCards").gameObject;
        setCardText = transform.Find("DeckNamePanel/Capacity").GetComponent<TMPro.TextMeshProUGUI>();
        haveCardText = transform.Find("CardPanel/Viewport/Content/OwnCard/Header/Capacity").GetComponent<TMPro.TextMeshProUGUI>();
        dontHaveCardText = transform.Find("CardPanel/Viewport/Content/NotOwingCard/Header/Capacity").GetComponent<TMPro.TextMeshProUGUI>();
        settingLayout = transform.Find("SetDeckLayout").Find("SetDeck").gameObject;
        ownCardLayout = transform.Find("CardPanel/Viewport/Content/OwnCard/Own").gameObject;
        UnReleaseCardLayout = transform.Find("CardPanel").Find("Viewport").Find("Content").Find("NotOwingCard").Find("NotOwing").gameObject;

        transform.Find("ConfirmButton").GetComponent<Button>().onClick.AddListener(delegate () { ConfirmButton(); });
        transform.Find("CancelButton").GetComponent<Button>().onClick.AddListener(delegate () { CancelButton(); });
        gameObject.SetActive(false);
    }
    
    public void SetDeckEdit(string heroId, bool isHuman) {
        editing = false;
        InitCanvas();
        Transform heroCards;
        HeroInventory heroData = null;
        this.isHuman = isHuman;
        deckNamePanel.transform.Find("NameTemplate").GetComponent<InputField>().text = "";
        transform.Find("HeroPortrait").GetComponent<Image>().sprite = AccountManager.Instance.resource.heroPortraite[heroId + "_button"];
        if (isHuman) {
            heroCards = transform.Find("HeroCards/Human");
            transform.Find("HeroCards/Orc").gameObject.SetActive(false);
            foreach(dataModules.Templates data in AccountManager.Instance.humanTemplates) {
                if (data.id == heroId) {
                    heroData = data;
                    break;
                }
            }
        }
        else {
            heroCards = transform.Find("HeroCards/Orc");
            transform.Find("HeroCards/Human").gameObject.SetActive(false);
            foreach (dataModules.Templates data in AccountManager.Instance.orcTemplates) {
                if (data.id == heroId) {
                    heroData = data;
                    break;
                }
            }
        }
        //transform.Find("Class/Class_1/icon").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["class_icon_" + heroData.heroClasses[0]];
        //transform.Find("Class/Class_2/icon").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["class_icon_" + heroData.heroClasses[1]];
        transform.Find("Class/Class_1").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["class_" + heroData.heroClasses[0]];
        transform.Find("Class/Class_2").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["class_" + heroData.heroClasses[1]];
        heroCards.gameObject.SetActive(true);
        for(int i = 0; i < heroData.heroCards.Length; i++)
            heroCards.GetChild(i).GetComponent<MenuCardHandler>().DrawCard(heroData.heroCards[i].cardId, isHuman);
        SetDeckEditCards(isHuman);
    }

    private void SetDeckEditCards(bool isHuman) {
        int ownCount = 0;
        int notOwnCount = 0;
        CardDataPackage myCards = AccountManager.Instance.cardPackage;
        foreach(dataModules.CollectionCard card in AccountManager.Instance.allCards) {
            if (card.isHeroCard) continue;
            string race;
            if (isHuman)
                race = "human";
            else
                race = "orc";
            if (card.camp != race) continue;
            if (myCards.data.ContainsKey(card.id)) {
                ownCardLayout.transform.GetChild(ownCount).GetComponent<EditCardHandler>().HAVENUM = myCards.data[card.id].cardCount;
                haveCardNum += myCards.data[card.id].cardCount;
                ownCardLayout.transform.GetChild(ownCount).GetComponent<EditCardHandler>().DrawCard(card.id, isHuman);
                ownCardLayout.transform.GetChild(ownCount++).gameObject.SetActive(true);
            }
            else {
                UnReleaseCardLayout.transform.GetChild(notOwnCount).GetComponent<EditCardHandler>().DrawCard(card.id, isHuman);
                UnReleaseCardLayout.transform.GetChild(notOwnCount++).gameObject.SetActive(true);
                dontHaveCard++;
            }
        }
        maxHaveCard = haveCardNum;
        dontHaveCardText.text = dontHaveCard.ToString();
        RefreshLine();
    }

    public void SetCustumDeckEdit(dataModules.Deck loadedDeck) {
        editing = true;
        InitCanvas();
        Transform heroCards;
        HeroInventory heroData = null;
        deckID = loadedDeck.id;
        deckNamePanel.transform.Find("NameTemplate").GetComponent<InputField>().text = loadedDeck.name;
        transform.Find("HeroPortrait").GetComponent<Image>().sprite = AccountManager.Instance.resource.heroPortraite[loadedDeck.heroId + "_button"];
        if (loadedDeck.camp == "human") {
            this.isHuman = true;
            heroCards = transform.Find("HeroCards/Human");
            transform.Find("HeroCards/Orc").gameObject.SetActive(false);
            foreach (dataModules.HeroInventory data in AccountManager.Instance.humanTemplates) {
                if (data.id == loadedDeck.heroId) {
                    heroData = data;
                    break;
                }
            }
        }
        else {
            this.isHuman = false;
            heroCards = transform.Find("HeroCards/Orc");
            transform.Find("HeroCards/Human").gameObject.SetActive(false);
            foreach (dataModules.HeroInventory data in AccountManager.Instance.orcTemplates) {
                if (data.id == loadedDeck.heroId) {
                    heroData = data;
                    break;
                }
            }
        }
        //transform.Find("Class/Class_1/icon").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["class_icon_" + heroData.heroClasses[0]];
        //transform.Find("Class/Class_2/icon").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["class_icon_" + heroData.heroClasses[1]];
        transform.Find("Class/Class_1").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["class_" + heroData.heroClasses[0]];
        transform.Find("Class/Class_2").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["class_" + heroData.heroClasses[1]];
        heroCards.gameObject.SetActive(true);
        for (int i = 0; i < heroData.heroCards.Length; i++)
            heroCards.GetChild(i).GetComponent<MenuCardHandler>().DrawCard(heroData.heroCards[i].cardId, isHuman);
        SetCustomDeckEditCards(loadedDeck);
    }

    private void SetCustomDeckEditCards(dataModules.Deck lodedDeck) {
        int ownCount = 0;
        int notOwnCount = 0;
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
            string race;
            if (isHuman)
                race = "human";
            else
                race = "orc";
            if (card.camp != race) continue;
            if (myCards.data.ContainsKey(card.id)) {
                if (!setCardList.ContainsKey(card.id)){
                    ownCardLayout.transform.GetChild(ownCount).GetComponent<EditCardHandler>().HAVENUM = myCards.data[card.id].cardCount;
                    haveCardNum += myCards.data[card.id].cardCount;
                    ownCardLayout.transform.GetChild(ownCount).GetComponent<EditCardHandler>().DrawCard(card.id, isHuman);
                    ownCardLayout.transform.GetChild(ownCount++).gameObject.SetActive(true);
                }
                else {
                    EditCardHandler settedCard = setCardList[card.id].GetComponent<EditCardHandler>();
                    EditCardHandler ownedCard = ownCardLayout.transform.GetChild(ownCount).GetComponent<EditCardHandler>();
                    settedCard.beforeObject = ownedCard.gameObject;
                    ownedCard.HAVENUM = myCards.data[card.id].cardCount - settedCard.SETNUM;
                    ownedCard.DrawCard(card.id, isHuman);
                    if (ownedCard.HAVENUM > 0)
                        ownedCard.gameObject.SetActive(true);
                    ownCount++;
                    haveCardNum += ownedCard.HAVENUM;
                }
            }
            else {
                UnReleaseCardLayout.transform.GetChild(notOwnCount).GetComponent<EditCardHandler>().DrawCard(card.id, isHuman);
                UnReleaseCardLayout.transform.GetChild(notOwnCount++).gameObject.SetActive(true);
                dontHaveCard++;
            }
        }
        maxHaveCard = setCardNum + haveCardNum;
        dontHaveCardText.text = dontHaveCard.ToString();
        RefreshLine();
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
        deckNamePanel.transform.Find("NameTemplate").Find("Text").GetComponent<Text>().text = "";
        heroID = (isHuman == true) ? "h10001" : "h10002";

        formatData.heroId = heroID; //영웅 id
        formatData.items = items.ToArray(); //추가한 카드 정보들
        formatData.name = deckNamePanel.transform.Find("NameTemplate").Find("Text").GetComponent<Text>().text;   //덱 이름
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
