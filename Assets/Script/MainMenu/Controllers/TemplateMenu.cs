using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using Spine;
using dataModules;
using TMPro;

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

    private string previewID;
    public bool isHuman;
    public string heroID;
    public DeckHandler selectedDeck;
    bool newDeck;

    private void Awake() {
        //Transform upper = transform.Find("InnerCanvas/Upper");
        //Transform footer = transform.Find("InnerCanvas/Footer");
        //Transform heroSelect = upper.Find("HeroSelect");

        //heroButtonLayout = transform.Find("InnerCanvas/HeroButton/HeroBtnLayout").gameObject;
        //heroPortrait = heroSelect.Find("Portrait").gameObject;
        //heroName = heroSelect.Find("NameTamplate").GetComponentInChildren<TextMeshProUGUI>();
        //heroProperty = heroSelect.Find("HeroProperty").gameObject;
        //heroCardGroup = upper.Find("HeroCard").gameObject;
        //deckLayout = footer.Find("DeckBtnLayout").gameObject;
        //gameObject.SetActive(false);
        //SetHeroBtnID();
    }


    public void SetHeroBtnID() {
        if (AccountManager.Instance == null) return;
        ResourceManager resource = AccountManager.Instance.resource;

        isHuman = (gameObject.name.Contains("Human") == true) ? true : false;
        Transform btnLayout = heroButtonLayout.transform;
        int count = 0;

        foreach (Transform child in btnLayout) {
            TemplateHeroBtn templateHeroBtn = child.gameObject.AddComponent<TemplateHeroBtn>();
            Button button = child.gameObject.GetComponent<Button>();

            if (isHuman == true)
                templateHeroBtn.heroID = (child.GetSiblingIndex() == 0) ? "h10001" : "h10003";
            else
                templateHeroBtn.heroID = (child.GetSiblingIndex() == 0) ? "h10002" : "h10004";

            templateHeroBtn.menu = this;



            if (button != null)
                button.onClick.AddListener(delegate () { templateHeroBtn.HeroSelectBtn(); });

            if (count < 2) {
                GameObject skeletonData = resource.heroPreview[templateHeroBtn.heroID].gameObject;
                GameObject preview = Instantiate(skeletonData, heroPortrait.transform);
                preview.transform.position = heroPortrait.transform.position;
                preview.SetActive((count == 0) ? true : false);
                preview.name = templateHeroBtn.heroID;
            }

            if (count > 0)
                button.enabled = false;

            count++;
        }
        previewID = (isHuman == true) ? "h10001" : "h10002";
    }

    public void SetTemplateDecks() {
        for(int i = 0; i < 3; i++) {
            transform.Find("DeckList").GetChild(i).gameObject.SetActive(false);
        }
        List<Templates> templates;
        if (isHuman) 
            templates = AccountManager.Instance.humanTemplates;
        else
            templates = AccountManager.Instance.orcTemplates;
        foreach(Templates heros in templates) {
            if(heros.id == heroID) {
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
    }

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
    }

    public void ChangeHeroID(string ID) {
        string id = ID;
        heroID = ID;
        ChangeHeroSkeleton(id);
        ChangeHeroData(id);
        SetTemplateDecks();
    }

    private void ChangeHeroSkeleton(string heroID) {
        heroPortrait.transform.Find(previewID).gameObject.SetActive(false);
        heroPortrait.transform.Find(heroID).gameObject.SetActive(true);
        previewID = heroID;
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
            selectedDeck.transform.Find("Selected").gameObject.SetActive(false);
            if (selectedDeck == deck) {
                selectedDeck = null;
                return;
            }
        }
        if (newDeck) {
            transform.Find("DeckList/NewDeck/Selected").gameObject.SetActive(false);
            newDeck = false;
        }
        selectedDeck = deck;
        
    }

    public void SelectNewDeck() {
        if (selectedDeck != null)
            selectedDeck.transform.Find("Selected").gameObject.SetActive(false);
        if (newDeck) {
            transform.Find("DeckList/NewDeck/Selected").gameObject.SetActive(false);
            newDeck = false;
            return;
        }
        selectedDeck = null;
        newDeck = true;
        transform.Find("DeckList/NewDeck/Selected").gameObject.SetActive(true);
    }

    public void CancelSelectDeck() {
        if (selectedDeck != null) {
            selectedDeck.transform.Find("Selected").gameObject.SetActive(false);
            selectedDeck = null;
        }
        if (newDeck) {
            transform.Find("DeckList/NewDeck/Selected").gameObject.SetActive(false);
            newDeck = false;
        }
    }


    public void ReturnToMenu() {
        gameObject.SetActive(false);

        FindObjectOfType<HUDController>().SetHeader(HUDController.Type.SHOW_USER_INFO);
    }
}
