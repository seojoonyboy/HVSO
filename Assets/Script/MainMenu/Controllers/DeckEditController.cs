using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using dataModules;
using TMPro;
using UnityEngine.EventSystems;

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
    bool isHuman;
    


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
            ownCardLayout.GetComponent<Image>().enabled = true;
            ownCardLayout.GetComponent<Button>().enabled = true;
        }
        selectCard.transform.Find("SelectedPanel").gameObject.SetActive(true);
    }
    

    public void ExpectFromDeck() {
        if (selectCard == null) return;
        string id = selectCard.GetComponent<EditCardHandler>().cardID;
        selectCard.transform.SetAsLastSibling();
        selectCard.transform.Find("SelectedPanel").gameObject.SetActive(false);
        selectCard.gameObject.SetActive(false);
        setCardList.Remove(id);
        selectCard.GetComponent<EditCardHandler>().beforeObject.SetActive(true);
        ownCardLayout.GetComponent<Image>().enabled = false;
        ownCardLayout.GetComponent<Button>().enabled = false;
        
    }

    public void ConfirmSetDeck() {
        if (selectCard == null) return;
        string id = selectCard.GetComponent<EditCardHandler>().cardID;
        if (!setCardList.ContainsKey(id)) {
            GameObject addedCard = settingLayout.transform.GetChild(setCardList.Count).gameObject;
            setCardList.Add(selectCard.GetComponent<EditCardHandler>().cardID, addedCard);
            addedCard.GetComponent<EditCardHandler>().DrawCard(id, isHuman);
            addedCard.GetComponent<EditCardHandler>().beforeObject = selectCard;
            addedCard.SetActive(true);
            selectCard.transform.Find("SelectedPanel").gameObject.SetActive(false);
            selectCard.SetActive(false);
            selectCard = null;
        }
        transform.Find("SetDeckLayout").Find("glow").gameObject.SetActive(false);
        Canvas.ForceUpdateCanvases();
        ownCardLayout.GetComponent<ContentSizeFitter>().enabled = false;
        ownCardLayout.GetComponent<ContentSizeFitter>().enabled = true;
        ownCardLayout.GetComponent<GridLayoutGroup>().enabled = false;
        ownCardLayout.GetComponent<GridLayoutGroup>().enabled = true;
        RefreshLine();
        LayoutRebuilder.ForceRebuildLayoutImmediate(ownCardLayout.GetComponent<RectTransform>());
    }


    private void RefreshLine() {
        transform.Find("CardPanel").Find("Viewport").Find("Content").GetComponent<VerticalLayoutGroup>().spacing = 1;
        transform.Find("CardPanel").Find("Viewport").Find("Content").GetComponent<VerticalLayoutGroup>().spacing = 0;
    }



    private void SetObject() {
        deckNamePanel = transform.Find("DeckNamePanel").gameObject;
        heroCardGroup = transform.Find("HeroCards").gameObject;

        settingLayout = transform.Find("SetDeckLayout").Find("SetDeck").gameObject;
        ownCardLayout = transform.Find("CardPanel").Find("Viewport").Find("Content").Find("OwnCard").Find("Own").gameObject;
        UnReleaseCardLayout = transform.Find("CardPanel").Find("Viewport").Find("Content").Find("NotOwingCard").Find("NotOwing").gameObject;

        transform.Find("ConfirmButton").GetComponent<Button>().onClick.AddListener(delegate () { ConfirmButton(); });
        transform.Find("CancelButton").GetComponent<Button>().onClick.AddListener(delegate () { CancelButton(); });
        gameObject.SetActive(false);
    }

    
    public void SetUnitCard() {
        foreach(Transform child in settingLayout.transform) {
            child.gameObject.SetActive(true);
            child.GetComponent<EditCardHandler>().deckEditController = this;
            child.GetComponent<EditCardHandler>().cardgroup.card = child.gameObject;
            child.GetComponent<EditCardHandler>().cardgroup.CardLocation = child.transform.parent.gameObject;
        }

        foreach(Transform child in ownCardLayout.transform) {
            child.gameObject.SetActive(true);
            child.GetComponent<EditCardHandler>().deckEditController = this;
            child.GetComponent<EditCardHandler>().cardgroup.card = child.gameObject;
            child.GetComponent<EditCardHandler>().cardgroup.CardLocation = child.transform.parent.gameObject;
        }

        RefreshLine();
    }
    
    public void SetDeckEdit(string heroId, bool isHuman) {
        setCardList = new Dictionary<string, GameObject>();
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
                ownCardLayout.transform.GetChild(ownCount).GetComponent<EditCardHandler>().DrawCard(card.id, isHuman);
                ownCardLayout.transform.GetChild(ownCount++).gameObject.SetActive(true);
            }
            else {
                UnReleaseCardLayout.transform.GetChild(notOwnCount).GetComponent<EditCardHandler>().DrawCard(card.id, isHuman);
                ownCardLayout.transform.GetChild(notOwnCount++).gameObject.SetActive(true);
            }
        }
    }
}
[System.Serializable]
public class SelectCard {
    public GameObject CardLocation;
    public GameObject card;
}
