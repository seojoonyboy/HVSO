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

public class DeckEditController : MonoBehaviour {
    [SerializeField] public Transform cardStorage;
    [SerializeField] public GameObject deckNamePanel;
    [SerializeField] public Transform settingLayout;
    [SerializeField] public Transform ownCardLayout;
    [SerializeField] public Transform handDeckHeader;
    [SerializeField] public TMPro.TextMeshProUGUI pagenumText;
    [SerializeField] public TMPro.TextMeshProUGUI setCardText;
    [SerializeField] public Transform buttons;
    [SerializeField] public EditCardButtonHandler cardButtons;
    [SerializeField] public EditBarDragHandler editBar;

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
    int bookLength;
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

    public void RefreshLine2() {
        setCardText.text = setCardNum.ToString() + "/40";
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(settingLayout.GetChild(0).GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(ownCardLayout.parent.GetComponent<RectTransform>());

        //Invoke("UpdateContentHeight", 0.1f);
    }

    private void UpdateContentHeight() {
        Transform handDeck = ownCardLayout.GetChild(0);
        Transform cardBook = settingLayout.GetChild(0);
        handDeck.GetComponent<RectTransform>().anchoredPosition = new Vector2(handDeck.GetComponent<RectTransform>().anchoredPosition.x, 0);
        cardBook.GetComponent<RectTransform>().anchoredPosition = new Vector2(cardBook.GetComponent<RectTransform>().anchoredPosition.x, 0);
        handDeck.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
        cardBook.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
    }

    public void AddCardBookLine(bool add) {
        if (add && bookLength == 3) return;
        if (!add && bookLength == 1) return;
        if (add) bookLength++;
        else bookLength--;
        ownCardLayout.GetComponent<RectTransform>().sizeDelta = new Vector2(1080, bookLength * 310);
        ownCardLayout.transform.localPosition = new Vector3(0, (4 - bookLength) * 155);
        settingLayout.GetComponent<RectTransform>().sizeDelta = new Vector2(1080, (4 - bookLength) * 310);
        settingLayout.transform.localPosition = new Vector3(0, -475 + (155 * (2 - bookLength)));
        handDeckHeader.transform.localPosition = new Vector3(0, -85 + (310 * (2 - bookLength)));
        RefreshLine2();
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
        transform.Find("InnerCanvas/BackGroundPatern/Human").gameObject.SetActive(isHuman);
        transform.Find("InnerCanvas/BackGroundPatern/Orc").gameObject.SetActive(!isHuman);
        if (editCards != null) editCards.Clear();
        editCards = GetCards();
        
        setCardNum = 0;
        haveCardNum = 0;
        dontHaveCard = 0;
        currentPage = 0;
        EditCardHandler.dragable = true;
        while (cardStorage.childCount > 0) {
            string cardRoot = cardStorage.GetChild(0).GetComponent<EditCardHandler>().editBookRoot;
            cardStorage.GetChild(0).SetParent(ownCardLayout.Find(cardRoot));
        }
        for (int i = 0; i < 40; i++) {
            EditCardHandler cardHandler = settingLayout.GetChild(0).GetChild(i).GetComponent<EditCardHandler>();
            cardHandler.InitEditCard();
            cardHandler.deckEditController = this;
        }

        Transform cardStore = transform.Find("InnerCanvas/CardStore");
        for (int i = 0; i < cardStore.childCount; i++) {
            cardStore.GetChild(i).GetComponent<EditCardHandler>().deckEditController = this;
        }
        for (int i = 0; i < 3; i++) {
            while (ownCardLayout.GetChild(i).Find("Grid").childCount > 0) {
                EditCardHandler cardHandler = ownCardLayout.GetChild(i).Find("Grid").GetChild(0).GetComponent<EditCardHandler>();
                cardHandler.InitEditCard();
                cardHandler.deckEditController = this;
                cardHandler.transform.SetParent(cardStore);
                cardHandler.transform.localPosition = Vector3.zero;
            }
        }
        transform.Find("InnerCanvas/Buttons/SortToClass1/Selected").gameObject.SetActive(true);
        transform.Find("InnerCanvas/Buttons/SortToClass2/Selected").gameObject.SetActive(true);
    }

    public List<EditCard> GetCards() {
        List<EditCard> cards = new List<EditCard>();
        Transform cardStore = transform.Find("InnerCanvas/CardStore");
        int count = 0;
        string heroClass1 = heroData.heroClasses[0];
        string heroClass2 = heroData.heroClasses[1];
        ownCardLayout.GetChild(0).name = heroClass1;
        ownCardLayout.GetChild(0).Find("Header/Info/Image").GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[heroClass1];
        ownCardLayout.GetChild(0).Find("Header/Info/Name").GetComponent<TMPro.TextMeshProUGUI>().text = heroClass1;
        ownCardLayout.GetChild(1).name = heroClass2;
        ownCardLayout.GetChild(1).Find("Header/Info/Image").GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[heroClass2];
        ownCardLayout.GetChild(1).Find("Header/Info/Name").GetComponent<TMPro.TextMeshProUGUI>().text = heroClass2;


        foreach (dataModules.CollectionCard card in AccountManager.Instance.allCards) {
            if (isHuman == (card.camp == "human")) {
                if (card.isHeroCard) continue;
                if (card.unownable) continue;
                if (card.cardClasses[0] != heroClass1 && card.cardClasses[0] != heroClass2) continue;
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
                cards.Add(new EditCard {
                    cardObject = cardStore.GetChild(count).gameObject,
                    cardId = card.id,
                    cardClass = card.cardClasses[0],
                    costOrder = card.cost,
                    rareOrder = rarelityValue
                });
                count++;
            }
        }
        return SortCardListByCost(cards).ToList();
    }

    IEnumerable<EditCard> SortCardListByCost(List<EditCard> cards) {
        return cards.OrderBy(n => n.costOrder).ThenBy(m => m.rareOrder);
    }

    public void ConfirmButton() {
        if (editing == true) {
            NetworkManager.ModifyDeckReqFormat form = new NetworkManager.ModifyDeckReqFormat();
            NetworkManager.ModifyDeckReqArgs field = new NetworkManager.ModifyDeckReqArgs();

            field.fieldName = NetworkManager.ModifyDeckReqField.NAME;

            string inputNameVal = deckNamePanel.transform.Find("NameTemplate").GetComponent<TMPro.TMP_InputField>().text;


            if (string.IsNullOrEmpty(inputNameVal)) {
                Modal.instantiate("변경하실 닉네임을 입력해주세요.", Modal.Type.CHECK);
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

        if (isTemplate)
            FindObjectOfType<HUDController>().SetHeader(HUDController.Type.RESOURCE_ONLY_WITH_BACKBUTTON);
        else
            FindObjectOfType<HUDController>().SetHeader(HUDController.Type.SHOW_USER_INFO);

        isTemplate = false;
        gameObject.SetActive(false);
        cardButtons.gameObject.SetActive(false);
    }

    public void ResumeEdit() {
        DestroyImmediate(cancelModal, true);
        EscapeKeyController.escapeKeyCtrl.AddEscape(CancelButton);
        //EscapeKeyController.escapeKeyCtrl.RemoveEscape(ResumeEdit);
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
        Transform cardList = transform.Find("InnerCanvas/HandDeckArea/SettedDeck");
        for (int i = 0; i < 8; i++)
            cardMana[i] = 0;
        for (int i = 0; i < cardList.childCount; i++) {
            if (!cardList.GetChild(i).gameObject.activeSelf) continue;
            int cost = cardList.GetChild(i).GetComponent<EditCardHandler>().cardData.cost;
            int num = cardList.GetChild(i).GetComponent<EditCardHandler>().SETNUM;
            if (cost >= 8)
                cardMana[7] += num;
            else
                cardMana[cost] += num;
        }
        Transform manaCorveParent = transform.Find("InnerCanvas/HeroInfoWindow/ManaCurve/ManaSliderParent");
        for (int i = 0; i < 8; i++) {
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

    public void ExceptFromDeck(GameObject cardObj, string cardId) {
        if (EditCardHandler.onAnimation) return;
        if (!setCardList.ContainsKey(cardId)) return;
        EditCardHandler handCardHandler = cardObj.GetComponent<EditCardHandler>();
        handCardHandler.SETNUM--;
        handCardHandler.SetSetNum();
        if (!handCardHandler.beforeObject.activeSelf) handCardHandler.beforeObject.gameObject.SetActive(true);

        float ditance1 = handCardHandler.beforeObject.transform.position.y + (142 * transform.localScale.x) - editBar.transform.Find("CardBookBottom").position.y;
        float ditance2 = handCardHandler.beforeObject.transform.position.y - (142 * transform.localScale.x) - editBar.bookBottomLimit.position.y;
        if (ditance1 > 0) {
            editBar.cardBookArea.GetChild(0).position = new Vector3(editBar.cardBookArea.GetChild(0).position.x, editBar.cardBookArea.GetChild(0).position.y - ditance1);
        }
        else if (ditance2 < 0) {
            editBar.cardBookArea.GetChild(0).position = new Vector3(editBar.cardBookArea.GetChild(0).position.x, editBar.cardBookArea.GetChild(0).position.y - ditance2);
        }

        EditCardHandler beforeCard = handCardHandler.beforeObject.GetComponent<EditCardHandler>();
        beforeCard.HAVENUM++;
        beforeCard.SetHaveNum(true);
        if (handCardHandler.SETNUM == 0) {
            handCardHandler.beforeObject = null;
            handCardHandler.transform.SetAsLastSibling();
            handCardHandler.transform.localScale = Vector3.zero;
            handCardHandler.gameObject.SetActive(false);
            setCardList.Remove(cardId);
        }
        setCardNum--;
        haveCardNum++;
        RefreshLine2();
    }









    public void AutoSetDeck() {
        List<GameObject>[,] bookCards = new List<GameObject>[8, 5];
        int[] setNumsByCost = new int[8];
        foreach (KeyValuePair<string, GameObject> obj in setCardList) {
            int cost = obj.Value.GetComponent<EditCardHandler>().cardData.cost;
            if (cost > 6) cost = 7;
            setNumsByCost[cost] += obj.Value.GetComponent<EditCardHandler>().SETNUM;
        }
        for(int i = 0; i < 8; i++) {
            for(int j = 0; j < 5; j++) {
                bookCards[i,j] = new List<GameObject>();
            }
        }
        for(int i = 0; i < 2; i++) {
            for (int j = 0; j < ownCardLayout.GetChild(i).GetChild(1).childCount; j++) {
                GameObject cardObj = ownCardLayout.GetChild(i).GetChild(1).GetChild(j).gameObject;
                if (cardObj.activeSelf) {
                    dataModules.CollectionCard cardDate = cardObj.GetComponent<EditCardHandler>().cardData;
                    int cost = cardDate.cost;
                    if (cost > 6) cost = 7;
                    switch (cardDate.rarelity) {
                        case "common":
                            bookCards[cost, 4].Add(cardObj);
                            break;
                        case "uncommon":
                            bookCards[cost, 3].Add(cardObj);
                            break;
                        case "rare":
                            bookCards[cost, 2].Add(cardObj);
                            break;
                        case "superrare":
                            bookCards[cost, 1].Add(cardObj);
                            break;
                        case "legend":
                            bookCards[cost, 0].Add(cardObj);
                            break;
                    }
                }
            }
        }
        System.Random rand = new System.Random();
        int costNum = 0;
        while (costNum < 8) {
            int maxNumber = 0;
            switch (costNum) {
                case 0:
                    maxNumber = 4;
                    break;
                case 1:
                    maxNumber = 8;
                    break;
                case 2:
                    maxNumber = 9;
                    break;
                case 3:
                    maxNumber = 7;
                    break;
                case 4:
                    maxNumber = 5;
                    break;
                case 5:
                    maxNumber = 3;
                    break;
                case 6:
                case 7:
                    maxNumber = 2;
                    break;
            }

            while (setNumsByCost[costNum] < maxNumber) {
                for (int i = 0; i < 5; i++) {
                    while (bookCards[costNum, i].Count > 0) {
                        int num = rand.Next(0, bookCards[costNum, i].Count);
                        string cardId = bookCards[costNum, i][num].GetComponent<EditCardHandler>().cardID;
                        EditCardHandler addCardHandler;
                        if (!setCardList.ContainsKey(cardId)) {
                            GameObject addedCard = settingLayout.GetChild(0).GetChild(setCardList.Count).gameObject;
                            addedCard.SetActive(true);
                            setCardList.Add(cardId, addedCard);
                            addCardHandler = addedCard.GetComponent<EditCardHandler>();
                            addCardHandler.SETNUM++;
                            addCardHandler.DrawCard(cardId, isHuman);
                            //addCardHandler.SetSetNum(true);
                            addCardHandler.beforeObject = bookCards[costNum, i][num];
                        }
                        else {
                            addCardHandler = setCardList[cardId].GetComponent<EditCardHandler>();
                            addCardHandler.SETNUM++;
                            //addCardHandler.SetSetNum(true);
                        }
                        setNumsByCost[costNum]++;
                        bookCards[costNum, i][num].GetComponent<EditCardHandler>().HAVENUM--;
                        if (bookCards[costNum, i][num].GetComponent<EditCardHandler>().HAVENUM == 0)
                            bookCards[costNum, i].Remove(bookCards[costNum, i][num]);
                        haveCardNum--;
                        setCardNum++;
                        if (setNumsByCost[costNum] == maxNumber || setCardNum == 40) break;
                    }
                    if (setNumsByCost[costNum] == maxNumber || setCardNum == 40) break;
                }
                break;
            }
            costNum++;
            if (setCardNum == 40) break;
            if (costNum == 8 && setCardNum != 40) {
                costNum = 0;
                setNumsByCost = new int[8];
            }
        }
        SetCardNums();
        SortHandCards();
        RefreshLine2();
    }

    public void SetCardNums() {
        for (int i = 0; i < 40; i++) {
            EditCardHandler cardHandler = settingLayout.GetChild(0).GetChild(i).GetComponent<EditCardHandler>();
            cardHandler.SetSetNum();
        }

        Transform cardStore = transform.Find("InnerCanvas/CardStore");
        for (int i = 0; i < cardStore.childCount; i++) {
            cardStore.GetChild(i).GetComponent<EditCardHandler>().deckEditController = this;
        }
        for (int i = 0; i < 2; i++) {
            for(int j = 0; j < ownCardLayout.GetChild(i).Find("Grid").childCount; j++) { 
                EditCardHandler cardHandler = ownCardLayout.GetChild(i).Find("Grid").GetChild(j).GetComponent<EditCardHandler>();
                cardHandler.SetHaveNum();
                if (cardHandler.HAVENUM == 0) cardHandler.gameObject.SetActive(false);
            }
        }
    }








    public void ConfirmSetDeck(GameObject cardObj, string cardId) {
        if (setCardNum == 40) return;
        if (EditCardHandler.onAnimation) return;
        EditCardHandler cardHandler = cardObj.GetComponent<EditCardHandler>();
        EditCardHandler addCardHandler;
        if (!setCardList.ContainsKey(cardId)) {
            GameObject addedCard = settingLayout.GetChild(0).GetChild(setCardList.Count).gameObject;
            addedCard.SetActive(true);
            setCardList.Add(cardId, addedCard);
            addCardHandler = addedCard.GetComponent<EditCardHandler>();
            addCardHandler.SETNUM++;
            addCardHandler.DrawCard(cardId, isHuman);
            addCardHandler.SetSetNum(true);
            addCardHandler.beforeObject = cardObj;
            SortHandCards();
            RefreshLine2();
        }
        else {
            addCardHandler = setCardList[cardId].GetComponent<EditCardHandler>();
            addCardHandler.SETNUM++;
            addCardHandler.SetSetNum(true);
        }
        float ditance1 = addCardHandler.transform.position.y + (142 * transform.localScale.x) - editBar.handTopPos.position.y;
        float ditance2 = addCardHandler.transform.position.y - (142 * transform.localScale.x) - editBar.transform.Find("CardBookTop").position.y;
        if (ditance1 > 0) {
            editBar.handDeckArea.GetChild(0).position = new Vector3(editBar.handDeckArea.GetChild(0).position.x, editBar.handDeckArea.GetChild(0).position.y - ditance1);
        }
        else if (ditance2 < 0) {
            editBar.handDeckArea.GetChild(0).position = new Vector3(editBar.handDeckArea.GetChild(0).position.x, editBar.handDeckArea.GetChild(0).position.y - ditance2);
        }
        cardHandler.HAVENUM--;
        haveCardNum--;
        cardHandler.SetHaveNum();
        if (cardHandler.HAVENUM == 0) cardObj.SetActive(false);
        setCardNum++;
    }

    public void SortHandCards() {
        List<EditCard> cards = new List<EditCard>();
        int count = 0;
        for (int i = 0; i < settingLayout.GetChild(0).childCount; i++) {
            if (!settingLayout.GetChild(0).GetChild(i).gameObject.activeSelf) break;
            dataModules.CollectionCard cardData = settingLayout.GetChild(0).GetChild(i).GetComponent<EditCardHandler>().cardData;
            int rarelityValue = 0;
            switch (cardData.rarelity) {
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
            cards.Add(new EditCard {
                cardObject = settingLayout.GetChild(0).GetChild(i).gameObject,
                cardId = cardData.id,
                cardClass = cardData.cardClasses[0],
                costOrder = cardData.cost,
                rareOrder = rarelityValue
            });
            count++;
        }
        List<EditCard> sortedCards = SortCardListByCost(cards).ToList();
        for (int i = 0; i < sortedCards.Count; i++) {
            sortedCards[i].cardObject.transform.SetSiblingIndex(i);
        }
    }

    public void AddMadeCard(Transform cardObj, bool makeCard) {
        Transform cardStore = transform.Find("InnerCanvas/CardStore");
        for (int i = 0; i < 3; i++) {
            while (ownCardLayout.GetChild(i).Find("Grid").childCount > 0) {
                EditCardHandler cardHandler = ownCardLayout.GetChild(i).Find("Grid").GetChild(0).GetComponent<EditCardHandler>();
                cardHandler.InitEditCard();
                cardHandler.deckEditController = this;
                cardHandler.transform.SetParent(cardStore);
                cardHandler.transform.localPosition = Vector3.zero;
            }
        }
        CardDataPackage myCards = AccountManager.Instance.cardPackage;
        foreach (EditCard card in editCards) {
            if (myCards.data.ContainsKey(card.cardId))
                card.cardObject.transform.SetParent(ownCardLayout.Find(card.cardClass).Find("Grid"));
            else
                card.cardObject.transform.SetParent(ownCardLayout.GetChild(2).Find("Grid"));
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
                        ownedCard.gameObject.SetActive(false);
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
        if (makeCard) {
            if (cardObj.GetComponent<EditCardHandler>().HAVENUM == 1) {
                cardButtons.CloseCardButtons();
                editBar.InitCanvas();
                RefreshLine2();
                float ditance1 = cardObj.position.y + (142 * transform.localScale.x) - editBar.transform.Find("CardBookBottom").position.y;
                if (Mathf.Abs(ditance1) < editBar.cardBookArea.GetComponent<RectTransform>().sizeDelta.y && ditance1 < 0)
                    editBar.cardBookArea.GetChild(0).position = new Vector3(editBar.cardBookArea.GetChild(0).position.x, editBar.cardBookArea.GetChild(0).position.y - ditance1);
                StartCoroutine(OpenCardButtonLate(cardObj));

            }
        }
        else {
            if (cardObj.GetComponent<EditCardHandler>().HAVENUM == 0) {
                cardButtons.CloseCardButtons();
                if (!setCardList.ContainsKey(cardObj.GetComponent<EditCardHandler>().cardID)) {
                    editBar.InitCanvas();
                    RefreshLine2();
                    float ditance2 = cardObj.position.y - (343 * transform.localScale.x) - editBar.bookBottomLimit.position.y;
                    if (ditance2 < 0)
                        editBar.cardBookArea.GetChild(0).position = new Vector3(editBar.cardBookArea.GetChild(0).position.x, editBar.cardBookArea.GetChild(0).position.y - ditance2);
                    StartCoroutine(OpenCardButtonLate(cardObj));
                }
            }
        }
    }

    IEnumerator OpenCardButtonLate(Transform cardObj) {
        yield return new WaitForSeconds(0.1f);
        EditCardHandler baseCardHandler = cardObj.GetComponent<EditCardHandler>();
        cardButtons.SetCardButtons(cardObj, false, baseCardHandler.HAVENUM);
        if (baseCardHandler.HAVENUM > 0) {
            EditCardHandler cardHandler = cardButtons.transform.GetChild(0).Find("CardImage").GetComponent<EditCardHandler>();
            cardHandler.DrawCard(baseCardHandler.cardID, baseCardHandler.cardData.camp == "human");
            cardHandler.HAVENUM = baseCardHandler.HAVENUM;
            cardHandler.SetHaveNum();
        }
    }




    public void RefreshLine() {
        setCardText.text = setCardNum.ToString() + "/40";
        editBar.InitCanvas();
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
        heroID = heroId;
        heroData = null;
        foreach (dataModules.HeroInventory heroes in AccountManager.Instance.allHeroes) {
            if (heroes.id == heroId) {
                heroData = heroes;
                break;
            }
        }
        InitCanvas();

        deckNamePanel.transform.Find("NameTemplate").GetComponent<TMPro.TMP_InputField>().text = "";
        handDeckHeader.Find("DeckNamePanel").gameObject.SetActive(true);
        SetHeroInfo(heroId);



        Dictionary<string, Sprite> classSprite = AccountManager.Instance.resource.classImage;
        transform.Find("InnerCanvas/Buttons/SortToClass1").GetComponent<Image>().sprite = classSprite[heroData.heroClasses[0]];
        transform.Find("InnerCanvas/Buttons/SortToClass1/Selected").GetComponent<Image>().sprite = classSprite[heroData.heroClasses[0]];
        transform.Find("InnerCanvas/Buttons/SortToClass2").GetComponent<Image>().sprite = classSprite[heroData.heroClasses[1]];
        transform.Find("InnerCanvas/Buttons/SortToClass2/Selected").GetComponent<Image>().sprite = classSprite[heroData.heroClasses[1]];

        SetDeckEditCards(isHuman, heroData);
    }

    private void SetDeckEditCards(bool isHuman, HeroInventory heroData) {
        CardDataPackage myCards = AccountManager.Instance.cardPackage;

        foreach (EditCard card in editCards) {
            EditCardHandler ownedCard = card.cardObject.GetComponent<EditCardHandler>();
            if (myCards.data.ContainsKey(card.cardId)) {
                card.cardObject.transform.SetParent(ownCardLayout.Find(card.cardClass).Find("Grid"));
                ownedCard.HAVENUM = myCards.data[card.cardId].cardCount;
                haveCardNum += myCards.data[card.cardId].cardCount;
            }
            else
                card.cardObject.transform.SetParent(ownCardLayout.GetChild(2).Find("Grid"));
            card.cardObject.transform.localScale = Vector3.one;
            card.cardObject.transform.localPosition = Vector3.zero;
            ownedCard.gameObject.SetActive(true);
            ownedCard.DrawCard(card.cardId, isHuman);
            maxHaveCard = haveCardNum;
        }
        RefreshLine2();
        editBar.InitCanvas();
        //RefreshLine();
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
                foreach (dataModules.Deck data in AccountManager.Instance.humanDecks) {
                    if (data.id == loadedDeck.id) {
                        foreach (dataModules.HeroInventory heroInventory in AccountManager.Instance.humanTemplates) {
                            if (loadedDeck.heroId == heroInventory.id) {
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
        InitCanvas();
        if (!isTemplate) deckID = loadedDeck.id;
        deckID = loadedDeck.id;

        deckNamePanel.transform.Find("NameTemplate").GetComponent<TMPro.TMP_InputField>().text = loadedDeck.name;
        handDeckHeader.Find("DeckNamePanel").gameObject.SetActive(string.IsNullOrEmpty(deckNamePanel.transform.Find("NameTemplate").GetComponent<TMPro.TMP_InputField>().text));
        SetHeroInfo(loadedDeck.heroId);

        Dictionary<string, Sprite> classSprite = AccountManager.Instance.resource.classImage;
        transform.Find("InnerCanvas/Buttons/SortToClass1").GetComponent<Image>().sprite = classSprite[heroData.heroClasses[0]];
        transform.Find("InnerCanvas/Buttons/SortToClass1/Selected").GetComponent<Image>().sprite = classSprite[heroData.heroClasses[0]];
        transform.Find("InnerCanvas/Buttons/SortToClass2").GetComponent<Image>().sprite = classSprite[heroData.heroClasses[1]];
        transform.Find("InnerCanvas/Buttons/SortToClass2/Selected").GetComponent<Image>().sprite = classSprite[heroData.heroClasses[1]];

        SetCustomDeckEditCards(loadedDeck, heroData);
    }

    public Sprite GetAllSortImage(string keyword1, string keyword2, bool isOn) {
        Dictionary<string, Sprite> classSprite = AccountManager.Instance.resource.classImage;
        var keys = classSprite.Keys;
        string selectedKey = string.Empty;
        foreach (string key in keys) {
            if (key.Contains(keyword1) && key.Contains(keyword2)) {
                if (isOn && key.Contains("sortbtnOn"))
                    selectedKey = key;
                if (!isOn && key.Contains("sortbtnOff"))
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
        int settedCardNum = 0;
        CardDataPackage myCards = AccountManager.Instance.cardPackage;

        foreach (dataModules.Item card in lodedDeck.items) {
            if (myCards.data.ContainsKey(card.id)) {
                EditCardHandler settedCard = settingLayout.GetChild(0).GetChild(settedCardNum).GetComponent<EditCardHandler>();
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
            if (myCards.data.ContainsKey(card.cardId))
                card.cardObject.transform.SetParent(ownCardLayout.Find(card.cardClass).Find("Grid"));
            else
                card.cardObject.transform.SetParent(ownCardLayout.GetChild(2).Find("Grid"));
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
                        ownedCard.gameObject.SetActive(false);
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
        SortHandCards();
        RefreshLine2();
        editBar.InitCanvas();
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
        transform.Find("InnerCanvas/Buttons/SortToClass1/Selected").gameObject.SetActive(false);
        transform.Find("InnerCanvas/Buttons/SortToClass2/Selected").gameObject.SetActive(false);
        transform.Find("InnerCanvas/ShowAllCards/Selected").gameObject.SetActive(false);
    }

    public void SortToClass(int classNum) {
        List<EditCard> cards = SortCardListByCost(editCards).ToList();
        if (transform.Find("InnerCanvas/Buttons/SortToClass" + (classNum + 1).ToString() + "/Selected").gameObject.activeSelf) {
            int otherClassNum = (classNum == 0) ? 1 : 0;
            if (!transform.Find("InnerCanvas/Buttons/SortToClass" + (otherClassNum + 1).ToString() + "/Selected").gameObject.activeSelf) return;
            while (ownCardLayout.GetChild(0).childCount > 0) {
                EditCardHandler cardHandler = ownCardLayout.GetChild(0).GetChild(0).GetComponent<EditCardHandler>();
                cardHandler.transform.SetParent(transform.Find("InnerCanvas/CardStore"));
            }
            transform.Find("InnerCanvas/Buttons/SortToClass" + (classNum + 1).ToString() + "/Selected").gameObject.SetActive(false);
            string className = heroData.heroClasses[otherClassNum];
            foreach (EditCard card in cards) {
                if (card.cardClass == className) {
                    card.cardObject.transform.SetParent(ownCardLayout.GetChild(0));
                    card.cardObject.transform.localScale = Vector3.one;
                    card.cardObject.transform.localPosition = Vector3.zero;
                }
            }
        }
        else {
            transform.Find("InnerCanvas/Buttons/SortToClass" + (classNum + 1).ToString() + "/Selected").gameObject.SetActive(true);
            while (ownCardLayout.GetChild(0).childCount > 0) {
                EditCardHandler cardHandler = ownCardLayout.GetChild(0).GetChild(0).GetComponent<EditCardHandler>();
                cardHandler.transform.SetParent(transform.Find("InnerCanvas/CardStore"));
            }
            string className1 = heroData.heroClasses[0];
            string className2 = heroData.heroClasses[1];
            foreach (EditCard card in cards) {
                if (card.cardClass == className1 || card.cardClass == className2) {
                    card.cardObject.transform.SetParent(ownCardLayout.GetChild(0));
                    card.cardObject.transform.localScale = Vector3.one;
                    card.cardObject.transform.localPosition = Vector3.zero;
                }
            }
        }
        RefreshLine();
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
            if (card.cardClass != heroData.heroClasses[0] && card.cardClass != heroData.heroClasses[1]) {
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
        transform.Find("InnerCanvas/Buttons/SortToClass1/Selected").gameObject.SetActive(false);
        transform.Find("InnerCanvas/Buttons/SortToClass2/Selected").gameObject.SetActive(false);
        transform.Find("InnerCanvas/ShowAllCards/Selected").gameObject.SetActive(true);
    }

    public void SelectInputName() {
        handDeckHeader.Find("DeckNamePanel/PlaceHolder").gameObject.SetActive(false);
    }

    public void EndInputName() {
        if (string.IsNullOrEmpty(deckNamePanel.transform.Find("NameTemplate").GetComponent<TMPro.TMP_InputField>().text))
            handDeckHeader.Find("DeckNamePanel/PlaceHolder").gameObject.SetActive(true);
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
