using System;
using System.Collections;
using System.Collections.Generic;
using BestHTTP;
using UnityEngine;
using UnityEngine.UI;
using Spine;
using Spine.Unity;

public class DeckHandler : MonoBehaviour
{
    [SerializeField] DeckEditController deckEditCanvas;
    [SerializeField] TemplateMenu templateCanvas;
    private string deckID;
    private bool isBasic = false;
    private bool isHuman;
    public dataModules.Deck templateDeck;

    dataModules.Deck deck;
    public bool ableTemplate = false;

    public string DECKID {
        get { return deckID; }
        set { deckID = value; }
    }

    public void InitDeck() {
        transform.Find("HeroImg").GetComponent<Image>().sprite = AccountManager.Instance.resource.deckPortraite["empty"];
        transform.Find("CardNum").gameObject.SetActive(false);
        transform.Find("DeckName").gameObject.SetActive(false);
    }

    public void SetNewDeck(dataModules.Deck deck) {
        deckID = deck.id;
        isHuman = (deck.camp == "human") ? true : false;
        Transform deckObj = transform.GetChild(0);
        deckObj.Find("RaceFlag/Human").gameObject.SetActive(isHuman);
        deckObj.Find("RaceFlag/Orc").gameObject.SetActive(!isHuman);
        deckObj.Find("HeroImg").gameObject.SetActive(true);
        if (deck.bannerImage == "custom")
            deckObj.Find("HeroImg").GetComponent<Image>().sprite = AccountManager.Instance.resource.deckPortraite[deck.heroId];
        else {
            deckObj.Find("HeroImg").GetComponent<Image>().sprite = AccountManager.Instance.resource.deckPortraite[deck.bannerImage];
        }
        deckObj.Find("CardNum").gameObject.SetActive(true);
        deckObj.Find("CardNum/Value").GetComponent<TMPro.TextMeshProUGUI>().text = deck.totalCardCount.ToString();
        if (deck.totalCardCount < 40) {
            deckObj.Find("CardNum/Value").GetComponent<TMPro.TextMeshProUGUI>().color = Color.red;
            deckObj.Find("HeroImg").GetComponent<Image>().color = new Color(0.235f, 0.235f, 0.235f);
            deckObj.Find("HeroImg").Find("Block").gameObject.SetActive(true);
        }
        else {
            deckObj.Find("CardNum/Value").GetComponent<TMPro.TextMeshProUGUI>().color = Color.white;
            deckObj.Find("HeroImg").GetComponent<Image>().color = Color.white;
            deckObj.Find("HeroImg").Find("Block").gameObject.SetActive(false);
        }
        deckObj.Find("DeckName").gameObject.SetActive(true);
        string deckName = deck.name;
        if (deckName.Contains("sampledeck"))
            deckName = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("SampleDeck", deckName);
        deckObj.Find("DeckName").GetComponent<TMPro.TextMeshProUGUI>().text = deckName;
        StartCoroutine(WaitForHeroInfo(deck.heroId));

        this.deck = deck;
    }

    IEnumerator WaitForHeroInfo(string heroId) {
        yield return new WaitUntil(() => AccountManager.Instance.myHeroInventories != null);
        int heroTier = AccountManager.Instance.myHeroInventories[heroId].tier;
        Transform heroTierTranform = transform.GetChild(0).Find("HeroInfo/HeroTier");
        for (int i = 0; i < 3; i++) {
            heroTierTranform.GetChild(i).GetChild(0).gameObject.SetActive(i < heroTier);
        }
    }

    public void SetNewTemplateDeck(dataModules.Deck deck) {
        templateDeck = deck;
        deckID = deck.id;
        if(deck.bannerImage != null)
            transform.Find("HeroImg").GetComponent<Image>().sprite = AccountManager.Instance.resource.deckPortraite[deck.bannerImage];

        string deckName = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("MainUI", "ui_page_deckedit_sampledeck") + ": " +
            AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("SampleDeck", deck.name);

        transform.Find("DeckName").GetComponent<TMPro.TextMeshProUGUI>().text = deckName;
        int playerCardNum = CheckPlayerCards(deck);
        transform.Find("CardNum/Value").GetComponent<TMPro.TextMeshProUGUI>().text = playerCardNum.ToString() + "/";
        ableTemplate = (playerCardNum == 40);
        //if (deck.totalCardCount < 40)
        //    transform.Find("CardNum/Value").GetComponent<TMPro.TextMeshProUGUI>().color = Color.red;
        //else
        //    transform.Find("CardNum/Value").GetComponent<TMPro.TextMeshProUGUI>().color = Color.white;
        transform.Find("Selected").gameObject.SetActive(false);
    }

    int CheckPlayerCards(dataModules.Deck deck) {
        int cardCount = 0;
        foreach(dataModules.Item card in deck.items) {
            if (AccountManager.Instance.cardPackage.data.ContainsKey(card.cardId))
                cardCount += AccountManager.Instance.cardPackage.data[card.cardId].cardCount;
        }
        return cardCount;
    }

    public void OpenDeckButton() {
        DeckSettingManager deckManager = transform.parent.parent.parent.GetComponent<DeckSettingManager>();
        if (deckManager.isAni) return;
        if (deckManager.selectedDeck == transform) {
            StartCoroutine(deckManager.CloseDeckButtons());
            return;
        }
        deckManager.isAni = true;
        StartCoroutine(deckManager.OpenDeckButtons(transform));
    }

    public void CloseDeckButton() {
        if (isBasic) return;
        GameObject editButtons = transform.Find("DeckInfo/EditButtons").gameObject;
        editButtons.SetActive(false);
    }

    public void SelectTemplateDeck() {
        transform.Find("Selected").GetComponent<SkeletonGraphic>().Initialize(true);
        transform.Find("Selected").gameObject.SetActive(true);
        templateCanvas.SelectDeck(this);
    }

    public void CancelSelect() {
        transform.Find("Selected").gameObject.SetActive(false);
        templateCanvas.selectedDeck = null;
    }

    public void EditCustomDeck() {
        dataModules.Deck customDeck = null;
        if (isHuman) {
            foreach (dataModules.Deck deck in AccountManager.Instance.humanDecks) {
                if (deckID == deck.id) {
                    customDeck = deck;
                    break;
                }
            }
        }
        else {
            foreach (dataModules.Deck deck in AccountManager.Instance.orcDecks) {
                if (deckID == deck.id) {
                    customDeck = deck;
                    break;
                }
            }
        }
        if(customDeck != null)
            deckEditCanvas.SetCustumDeckEdit(customDeck, false);
        deckEditCanvas.gameObject.SetActive(true);
        deckEditCanvas.GetComponent<DeckEditController>().RefreshLine();
        DeckSettingManager deckManager = transform.parent.parent.parent.GetComponent<DeckSettingManager>();
        deckManager.RefreshLine();

        FindObjectOfType<HUDController>().SetHeader(HUDController.Type.HIDE);
    }

    public void DeleteButton() {
        if (AccountManager.Instance == null) return;

        var translator = AccountManager.Instance.GetComponent<Fbl_Translator>();
        string message = translator.GetLocalizedText("UIPopup", "ui_popup_deckedit_questiondeletedeck");
        string header = translator.GetLocalizedText("UIPopup", "ui_popup_check");
        string yesBtn = translator.GetLocalizedText("UIPopup", "ui_popup_yes");
        string noBtn = translator.GetLocalizedText("UIPopup", "ui_popup_no");

        Modal.instantiate(
            message,
            Modal.Type.YESNO, () => {
                DeckSettingManager deckManager = transform.parent.parent.parent.GetComponent<DeckSettingManager>();
                StartCoroutine(deckManager.CloseDeckButtons());
                AccountManager.Instance.RequestDeckRemove(DECKID);
            },
            headerText: header,
            btnTexts: new string[] { yesBtn, noBtn }
        );
    }

    public void StartAIBattle() {
        const int MaxCardNum = 40;
        if (deck.deckValidate) {
            PlayerPrefs.SetString("SelectedDeckId", deckID);
            PlayerPrefs.SetString("selectedHeroId", deck.heroId);

            PlayerPrefs.SetString("SelectedBattleType", "solo");
            string camp;
            if (isHuman) { camp = "human"; }
            else { camp = "orc"; }
            PlayerPrefs.SetString("SelectedRace", camp);

            FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.CONNECT_MATCHING_SCENE);
        }
        else {
            if(deck.totalCardCount != MaxCardNum) {
                var translator = AccountManager.Instance.GetComponent<Fbl_Translator>();

                string message = translator.GetLocalizedText("UIPopup", "ui_popup_cantusedeck");
                string okBtn = translator.GetLocalizedText("UIPopup", "ui_popup_check");
                string header = translator.GetLocalizedText("UIPopup", "ui_popup_check");

                Modal.instantiate(
                    message, Modal.Type.CHECK,
                    btnTexts: new string[] { okBtn },
                    headerText: header
                );
            }
        }
    }

    public async void ChangeHandToEditBtn() {
        BlockerController.blocker.touchBlocker.SetActive(true);
        await System.Threading.Tasks.Task.Delay(150);
        BlockerController.blocker.SetBlocker(transform.Find("DeckObject/Buttons/EditBtn").gameObject);
        transform.Find("DeckObject/Buttons/EditBtn").GetComponent<Button>().onClick.AddListener(RemoveHandFromEditBtn);
        transform.Find("DeckObject/HeroImg").GetComponent<Button>().onClick.RemoveListener(ChangeHandToEditBtn);
    }

    public void RemoveHandFromEditBtn() {
        transform.Find("DeckObject/Buttons/EditBtn").GetComponent<Button>().onClick.RemoveListener(RemoveHandFromEditBtn);
        BlockerController.blocker.gameObject.SetActive(false);
    }
}
