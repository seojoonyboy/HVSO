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

    Dictionary<string, GameObject> setCardList;
    List<NetworkManager.DeckItem> items;

    bool isHuman;
    int setCardNum = 0;
    int haveCardNum = 0;
    int maxHaveCard = 0;
    int dontHaveCard = 0;
    TMPro.TextMeshProUGUI setCardText;
    TMPro.TextMeshProUGUI haveCardText;
    TMPro.TextMeshProUGUI dontHaveCardText;


    private void Start() {
        SetObject();
    }

    public void NewDeck() {

    }

    public void LoadEditDeck() {

    }
    
    public void ConfirmButton() {

    }

    public void CancelButton() {
        setCardList = null;
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


    private void RefreshLine() {
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

    
    public void SetUnitCard() {
        foreach(Transform child in settingLayout.transform) {
            child.gameObject.SetActive(true);
            child.GetComponent<EditCardHandler>().deckEditController = this;
            //child.GetComponent<EditCardHandler>().cardgroup.card = child.gameObject;
            //child.GetComponent<EditCardHandler>().cardgroup.CardLocation = child.transform.parent.gameObject;
        }

        foreach(Transform child in ownCardLayout.transform) {
            child.gameObject.SetActive(true);
            child.GetComponent<EditCardHandler>().deckEditController = this;
            //child.GetComponent<EditCardHandler>().cardgroup.card = child.gameObject;
            //child.GetComponent<EditCardHandler>().cardgroup.CardLocation = child.transform.parent.gameObject;
        }

        RefreshLine();
    }
    
    public void SetDeckEdit(string heroId, bool isHuman) {
        setCardList = new Dictionary<string, GameObject>();
        setCardNum = 0;
        haveCardNum = 0;
        Transform heroCards;
        Hero heroData = null;
        this.isHuman = isHuman;
        transform.Find("HeroPortrait").GetComponent<Image>().sprite = AccountManager.Instance.resource.heroPortraite[heroId + "_button"];
        if (isHuman) {
            heroCards = transform.Find("HeroCards/Human");
            transform.Find("HeroCards/Orc").gameObject.SetActive(false);
            foreach(dataModules.Hero data in AccountManager.Instance.humanDecks.heros) {
                if (data.id == heroId) {
                    heroData = data;
                    break;
                }
            }
        }
        else {
            heroCards = transform.Find("HeroCards/Orc");
            transform.Find("HeroCards/Human").gameObject.SetActive(false);
            foreach (dataModules.Hero data in AccountManager.Instance.orcDecks.heros) {
                if (data.id == heroId) {
                    heroData = data;
                    break;
                }
            }
        }
        heroCards.gameObject.SetActive(true);
        for(int i = 0; i < heroData.heroCards.Count; i++)
            heroCards.GetChild(i).GetComponent<MenuCardHandler>().DrawCard(heroData.heroCards[i].cardId, isHuman);
        SetDeckEditCards(isHuman);
    }

    private void SetDeckEditCards(bool isHuman) {
        for(int i = 0; i < 40; i++) {
            ownCardLayout.transform.GetChild(i).gameObject.SetActive(false);
            UnReleaseCardLayout.transform.GetChild(i).gameObject.SetActive(false);
            settingLayout.transform.GetChild(i).gameObject.SetActive(false);
        }
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



    /// <summary>
    /// Server에게 덱 수정 요청
    /// </summary>
    /// <param name="data"></param>
    /// <param name="deckId"></param>
    void RequestModifyDeck(NetworkManager.ModifyDeckReqFormat formatData, int deckId) {
        var fields = new List<NetworkManager.ModifyDeckReqArgs>();
        NetworkManager.ModifyDeckReqArgs field = new NetworkManager.ModifyDeckReqArgs();

        foreach (var pairs in setCardList) {
            //int count = pairs.Value.GetComponent<EditCardHandler>().setNum;
            NetworkManager.DeckItem data = new NetworkManager.DeckItem();
            data.cardId = pairs.Key;
            data.cardCount = 0;
            items.Add(data);
        }

        field.fieldName = NetworkManager.ModifyDeckReqField.NAME;
        field.value = deckNamePanel.transform.Find("InputField").Find("Text").GetComponent<Text>().text;   //덱 이름
        fields.Add(field);

        field = new NetworkManager.ModifyDeckReqArgs();
        field.fieldName = NetworkManager.ModifyDeckReqField.ITEMS;  //추가한 카드정보들
        field.value = items.ToArray();

        AccountManager.Instance.RequestDeckModify(formatData, deckId, OnDeckModifyFinished);
    }

    private void OnDeckModifyFinished(HTTPRequest originalRequest, HTTPResponse response) {
        //덱 수정 요청 완료
        if (response.StatusCode == 200) {
            Logger.Log("덱 편집완료 완료");
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Server에게 덱 새로 추가 요청(커스텀 덱)
    /// </summary>
    void RequestNewDeck() {
        NetworkManager.AddCustomDeckReqFormat formatData = new NetworkManager.AddCustomDeckReqFormat();

        foreach(var pairs in setCardList) {
            //int count = pairs.Value.GetComponent<EditCardHandler>().setNum;
            NetworkManager.DeckItem data = new NetworkManager.DeckItem();
            data.cardId = pairs.Key;
            data.cardCount = 0;
            items.Add(data);
        }



        formatData.heroId = heroID; //영웅 id
        formatData.items = items.ToArray(); //추가한 카드 정보들
        formatData.name = deckNamePanel.transform.Find("InputField").Find("Text").GetComponent<Text>().text;   //덱 이름

        AccountManager.Instance.RequestDeckMake(formatData, OnMakeNewDeckFinished);
    }

    private void OnMakeNewDeckFinished(HTTPRequest originalRequest, HTTPResponse response) {
        //덱 새로 생성 완료
        if (response.StatusCode == 200) {
            Logger.Log("덱 생성 완료");
            gameObject.SetActive(false);
        }

    }

}
