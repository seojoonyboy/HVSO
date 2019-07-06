using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using dataModules;
using TMPro;
using UnityEngine.EventSystems;
using BestHTTP;
using System;

public class DeckEditController : MonoBehaviour
{

    public string heroID;
    private GameObject heroInfo;
    private GameObject heroCardGroup;
    private GameObject heroProperty;
    private GameObject settingLayout;
    private GameObject ownCardLayout;
    private GameObject UnReleaseCardLayout;
    private GameObject deckNamePanel;

    public bool editing = false;
    public SelectCard selectCard;
    AccountManager accountManager;
    List<NetworkManager.DeckItem> items;
    public int deckId = -1;
    
    public List<NetworkManager.DeckItem> pairs = new List<NetworkManager.DeckItem>();
    private void Awake() {
        SetObject();
        //SetHeroData();
        //SettingCard();
        

        accountManager = AccountManager.Instance;
        SetUnitCard();
    }

    void Start() {
        //items = new List<NetworkManager.DeckItem>();
        

    }

    private void Update() {
    }


    public void NewDeck() {
        editing = false;
    }

    public void LoadEditDeck(Deck deck) {
        editing = true;
    }
    
    public void ConfirmButton() {
        switch (editing) {
            case true:
                SaveEditDeck();
                break;
            case false:
                SaveNewDeck();
                break;
        }
    }

    public void SaveNewDeck() {
        RequestNewDeck();
    }

    public void SaveEditDeck() {
        NetworkManager.ModifyDeckReqFormat form = new NetworkManager.ModifyDeckReqFormat();
        NetworkManager.ModifyDeckReqArgs field = new NetworkManager.ModifyDeckReqArgs();

        field.fieldName = NetworkManager.ModifyDeckReqField.NAME;
        field.value = "Modified";
        form.parms.Add(field);

        RequestModifyDeck(form, 5);
    }


    public void CancelButton() {

    }

    private void SettingCard() {
        AccountManager accountManager = AccountManager.Instance;
        HeroInventory hero = accountManager.myHeroInventories[heroID];

        List<CollectionCard> cardKeyList = accountManager.allCards;

        int count = 0;
        foreach(CollectionCard card in cardKeyList) { 
            if (card.isHeroCard == true) continue;
            if (card.camp != hero.camp) continue;


            count++;
        }
    }

    public void OnTouchCard(SelectCard card) {
        if (card != selectCard) {

            if(selectCard.card != null) {
                selectCard.card.transform.Find("DeletePanel").gameObject.SetActive(false);

                if (selectCard.CardLocation.name.Contains("Own") == true)
                    transform.Find("SetDeckLayout").Find("glow").gameObject.SetActive(false);
            }
            

            selectCard = null;
            selectCard = card;
        }
        //Vector3 origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //Ray2D ray = new Ray2D(origin, Vector2.zero);
        //RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        string location = selectCard.CardLocation.name;

        if (location.Contains("Own") == true)
            transform.Find("SetDeckLayout").Find("glow").gameObject.SetActive(true);

        if (location.Contains("SetDeck") == true)
            selectCard.card.transform.Find("DeletePanel").gameObject.SetActive(true);
    }
    

    public void ExpectFromDeck() {
        if (selectCard == null) return;
        selectCard.card.transform.SetParent(ownCardLayout.transform);
        selectCard.card.GetComponent<EditCardHandler>().cardgroup.CardLocation = ownCardLayout;

        selectCard.card.transform.Find("DeletePanel").gameObject.SetActive(false);
        transform.Find("SetDeckLayout").Find("glow").gameObject.SetActive(false);

        NetworkManager.DeckItem deckItem = new NetworkManager.DeckItem();

        deckItem.cardId = selectCard.card.GetComponent<EditCardHandler>().cardID;
        deckItem.cardCount = 1;

        if (pairs.Exists(x=>x.cardId == deckItem.cardId)) {
            var _item = pairs.Find(x => x.cardId == deckItem.cardId);
            _item.cardCount--;

            if (_item.cardCount <= 0)
                pairs.Remove(_item);
        }
        RefreshLine();
    }

    public void ConfirmSetDeck() {
        if (selectCard == null) return;
        selectCard.card.transform.Find("DeletePanel").gameObject.SetActive(false);
        transform.Find("SetDeckLayout").Find("glow").gameObject.SetActive(false);


       

        NetworkManager.DeckItem deckItem = new NetworkManager.DeckItem();
        deckItem.cardId = selectCard.card.GetComponent<EditCardHandler>().cardID;
        deckItem.cardCount = 1;
        
        var _item = pairs.Find(x => x.cardId == deckItem.cardId);

        

        if (pairs.Exists(x => x == _item)) {         
            if (_item.cardCount < 4) {
                
                _item.cardCount++;
            }
        }
        else {
            selectCard.card.transform.SetParent(settingLayout.transform);
            selectCard.card.GetComponent<EditCardHandler>().cardgroup.CardLocation = settingLayout;
            pairs.Add(deckItem);
        }
        

        
        RefreshLine();
    }


    private void RefreshLine() {
        transform.Find("CardPanel").Find("Viewport").Find("Content").GetComponent<VerticalLayoutGroup>().spacing = 1;
        transform.Find("CardPanel").Find("Viewport").Find("Content").GetComponent<VerticalLayoutGroup>().spacing = 0;
    }



    private void SetObject() {
        deckNamePanel = transform.Find("DeckNamePanel").gameObject;
        heroInfo = transform.Find("HeroStatus").gameObject;
        heroCardGroup = transform.Find("HeroCard").gameObject;
        heroProperty = transform.Find("HeroProperty").gameObject;

        settingLayout = transform.Find("SetDeckLayout").Find("SetDeck").gameObject;
        ownCardLayout = transform.Find("CardPanel").Find("Viewport").Find("Content").Find("OwnCard").Find("Own").gameObject;
        UnReleaseCardLayout = transform.Find("CardPanel").Find("Viewport").Find("Content").Find("NotOwingCard").Find("NotOwing").gameObject;

        transform.Find("ConfirmButton").GetComponent<Button>().onClick.AddListener(delegate () { ConfirmButton(); });
        transform.Find("CancelButton").GetComponent<Button>().onClick.AddListener(delegate () { CancelButton(); });
    }

    public void SetHeroData() {
        if (AccountManager.Instance == null) return;
        ResourceManager resource = AccountManager.Instance.resource;

        HeroInventory heroData = AccountManager.Instance.myHeroInventories["h10001"];
        int cardCount = 0;

        heroInfo.transform.Find("Streamer").Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = heroData.name;

        foreach (HeroCard card in heroData.heroCards) {
            Transform heroCardObject = heroCardGroup.transform.GetChild(cardCount);

            heroCardObject.Find("NameTemplate").Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = card.name;
            heroCardObject.Find("Cost").Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = card.cost.ToString();
            cardCount++;
        }

        int childcount = 0;
        foreach (Transform child in heroProperty.transform) {
            child.gameObject.GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[heroData.heroClasses[childcount]];
            child.GetChild(0).GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["class_icon_" + heroData.heroClasses[childcount]];

            childcount++;
        }
    }

    public void SetUnitCard() {
        //deck = AccountManager.Instance.humanDecks.customDecks[0];

        //foreach(Item cards in deck.items) {
            //child.gameObject.SetActive(true);
            //child.GetComponent<EditCardHandler>().deckEditController = this;
            //child.GetComponent<EditCardHandler>().cardgroup.card = child.gameObject;
            //child.GetComponent<EditCardHandler>().cardgroup.CardLocation = child.transform.parent.gameObject;
        //}
        /*
        foreach(Transform child in ownCardLayout.transform) {
            child.gameObject.SetActive(true);
            child.GetComponent<EditCardHandler>().deckEditController = this;
            child.GetComponent<EditCardHandler>().cardgroup.card = child.gameObject;
            child.GetComponent<EditCardHandler>().cardgroup.CardLocation = child.transform.parent.gameObject;
        }
        */
        RefreshLine();
    }
}
[System.Serializable]
public class SelectCard {
    public GameObject CardLocation;
    public GameObject card;
}


/*
public interface LoadEdit {
    void LoadCardData();
}


public class NewDeck : MonoBehaviour, LoadEdit {

    public void LoadCardData() {

    }
}

public class EditDeck : MonoBehaviour, LoadEdit {

    public void LoadCardData() {

    }
}

public class GetDeck : MonoBehaviour {

    private LoadEdit loadEdit;    

    public void Execute(LoadEdit deck) {
        loadEdit = deck;

        if (loadEdit == null)
            Logger.Log("에러!");
        else
            loadEdit.LoadCardData();
    }
}
*/
