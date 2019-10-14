using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using Spine;
using dataModules;
using TMPro;
using BestHTTP;
using System;

public class TemplateMenu : MonoBehaviour {
    [SerializeField]
    private GameObject heroButtonLayout;
    private GameObject heroPortrait;
    private TextMeshProUGUI heroName;
    private GameObject heroProperty;
    private GameObject heroCardGroup;

    [SerializeField]
    private GameObject deckLayout;

    [SerializeField] Canvas deckSettingCanves;
    [SerializeField] GameObject quickDeckMakeBtn;
    [SerializeField] MenuSceneController menuSceneController;

    private string previewID;
    public bool isHuman;
    public string heroID;
    public DeckHandler selectedDeck;
    bool newDeck;

    public void SetTemplateNewDecks(string heroId, bool isHuman) {
        heroID = heroId;
        this.isHuman = isHuman;
        for (int i = 0; i < 3; i++) {
            transform.Find("DeckList").GetChild(i).gameObject.SetActive(false);
        }
        List<Templates> templates;
        if (isHuman)
            templates = AccountManager.Instance.humanTemplates;
        else
            templates = AccountManager.Instance.orcTemplates;
        foreach (Templates heros in templates) {
            if (heros.id == heroId) {
                int count = 0;
                foreach (Deck deck in heros.templates) {
                    GameObject templateDeck = transform.Find("DeckList").GetChild(count).gameObject;
                    templateDeck.SetActive(true);
                    templateDeck.GetComponent<DeckHandler>().SetNewTemplateDeck(deck);
                    count++;
                }
                break;
            }
        }
        transform.Find("DeckList").Find("NewDeck/Selected").gameObject.SetActive(false);
        transform.Find("DeckList").Find("NewDeck/SelectedBack").gameObject.SetActive(false);

        //.decksLoader.Load();
        quickDeckMakeBtn.SetActive(false);
    }



    private void ChangeHeroData(string heroID) {
        HeroInventory heroData = AccountManager.Instance.myHeroInventories[heroID];
        int cardCount = 0;

        foreach (HeroCard card in heroData.heroCards) {
            Transform heroCardObject = heroCardGroup.transform.GetChild(cardCount);
            heroCardObject.GetComponent<MenuCardHandler>().DrawCard(card.cardId, isHuman);
            cardCount++;
        }

        int childcount = 0;
        foreach (Transform child in heroProperty.transform) {
            child.gameObject.GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[heroData.heroClasses[childcount]];
            //child.GetChild(0).GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["class_icon_" + heroData.heroClasses[childcount]];

            childcount++;
        }
    }

    public void SelectDeckBtn() {

    }

    public void StartEditBtn() {
        DeckEditController deckEditCtrl = deckSettingCanves.GetComponent<DeckEditController>();
        if (selectedDeck == null && newDeck) {
            deckEditCtrl.SetDeckEdit(heroID, isHuman);
        }
        else {
            if (selectedDeck == null) return;
            deckEditCtrl.SetCustumDeckEdit(selectedDeck.templateDeck, true);
        }
        deckEditCtrl.gameObject.SetActive(true);
        deckEditCtrl.RefreshLine();
        deckEditCtrl.templateMenu = this;
        CancelSelectDeck();
        FindObjectOfType<HUDController>().SetHeader(HUDController.Type.HIDE);
    }

    public void SelectDeck(DeckHandler deck) {
        if (selectedDeck != null) {
            if (selectedDeck == deck) 
                return;
            selectedDeck.transform.Find("Selected").gameObject.SetActive(false);
            selectedDeck.transform.Find("SelectedBack").gameObject.SetActive(false);
        }
        if (newDeck) {
            transform.Find("DeckList/NewDeck/Selected").gameObject.SetActive(false);
            transform.Find("DeckList/NewDeck/SelectedBack").gameObject.SetActive(false);
            newDeck = false;
        }
        selectedDeck = deck;
        transform.Find("Buttons/StartEditBtn").gameObject.SetActive(true);

        quickDeckMakeBtn.SetActive(true);
    }

    public void SelectNewDeck() {
        Transform newDeckSelected = transform.Find("DeckList/NewDeck/Selected");
        Transform newDeckSelectedBack = transform.Find("DeckList/NewDeck/SelectedBack");
        if (selectedDeck != null) {
            selectedDeck.transform.Find("Selected").gameObject.SetActive(false);
            selectedDeck.transform.Find("SelectedBack").gameObject.SetActive(false);
        }
        selectedDeck = null;
        newDeck = true;
        newDeckSelected.GetComponent<SkeletonGraphic>().Initialize(true);
        newDeckSelected.GetComponent<SkeletonGraphic>().Update(0);
        newDeckSelected.gameObject.SetActive(true);
        newDeckSelectedBack.GetComponent<SkeletonGraphic>().Initialize(true);
        newDeckSelectedBack.GetComponent<SkeletonGraphic>().Update(0);
        newDeckSelectedBack.gameObject.SetActive(true);
        transform.Find("Buttons/StartEditBtn").gameObject.SetActive(true);

        quickDeckMakeBtn.SetActive(false);
    }

    public void CancelSelectDeck() {
        if (selectedDeck != null) {
            selectedDeck.transform.Find("Selected").gameObject.SetActive(false);
            selectedDeck.transform.Find("SelectedBack").gameObject.SetActive(false);
            selectedDeck = null;
        }
        if (newDeck) {
            transform.Find("DeckList/NewDeck/Selected").gameObject.SetActive(false);
            transform.Find("DeckList/NewDeck/SelectedBack").gameObject.SetActive(false);
            newDeck = false;
        }
        transform.Find("Buttons/StartEditBtn").gameObject.SetActive(false);
    }


    public void ReturnToMenu() {
        gameObject.SetActive(false);
        transform.Find("Buttons/StartEditBtn").gameObject.SetActive(false);
        FindObjectOfType<HUDController>().SetHeader(HUDController.Type.SHOW_USER_INFO);
    }

    /// <summary>
    /// 바로 저장 버튼
    /// </summary>
    public void QuickDeckMake() {
        if (selectedDeck == null) return;

        Deck deck = selectedDeck.templateDeck;
        NetworkManager.AddCustomDeckReqFormat format = new NetworkManager.AddCustomDeckReqFormat();
        format.heroId = deck.heroId;
        var items = new List<DeckEditController.DeckItem>();
        foreach(Item item in deck.items) {
            items.Add(new DeckEditController.DeckItem(item.cardId, item.cardCount));
        }
        format.items = items.ToArray();
        format.name = deck.name;
        format.camp = deck.camp;

        AccountManager.Instance.RequestDeckMake(format, (HTTPRequest originalRequest, HTTPResponse response) => {
            if(response.StatusCode == 200) {
                Modal.instantiate("템플릿 덱을 생성하였습니다.", Modal.Type.CHECK, () => {
                    menuSceneController.decksLoader.Load();
                    ReturnToMenu();
                });
            }
        });
    }
}
