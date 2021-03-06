using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using dataModules;
using System;

public class DeckListHandlerInBattleReady : MonoBehaviour {
    AccountManager accountManager;
    [SerializeField] Transform content;
    public BattleReadySceneController parentController;

    public Sprite[] campImages;
    [SerializeField] GameObject battleStart;

    void Awake() {
        accountManager = AccountManager.Instance;
    }

    void OnEnable() {
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_DECKS_UPDATED, LoadMyDecks);
        accountManager.RequestMyDecks();
    }

    void OnDestroy() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_DECKS_UPDATED, LoadMyDecks);
    }

    private void LoadMyDecks(Enum Event_Type, Component Sender, object Param) {
        BestHTTP.HTTPResponse res = (BestHTTP.HTTPResponse)Param;

        var result = JsonReader.Read<Decks>(res.DataAsText);
        accountManager.orcDecks = result.orc;
        accountManager.humanDecks = result.human;

        var humanDecks = accountManager.humanDecks;
        var orcDecks = accountManager.orcDecks;

        for (int i = 0; i < humanDecks.Count; i++) {
            content.GetChild(i).gameObject.SetActive(true);

            Image heroImg = content.GetChild(i).Find("HeroImg").GetComponent<Image>();
            string banner = humanDecks[i].bannerImage;
            if (banner == "custom") {
                heroImg.sprite = accountManager.resource.deckPortraite["h10001"];
            }
            else {
                heroImg.sprite = accountManager.resource.deckPortraite[banner];
            }

            content.GetChild(i).Find("RaceFlag/Human").gameObject.SetActive(true);
            content.GetChild(i).Find("RaceFlag/Orc").gameObject.SetActive(false);

            var cardNumValue = content.GetChild(i).Find("CardNum/Value").GetComponent<TextMeshProUGUI>();
            cardNumValue.text = humanDecks[i].totalCardCount + "/";
            if (humanDecks[i].totalCardCount < 40) {
                heroImg.transform.Find("Block").gameObject.SetActive(true);
                heroImg.color = new Color32(60, 60, 60, 255);
                cardNumValue.color = new Color32(255, 0, 0, 255);
                //content.GetChild(i).GetComponent<Button>().interactable = false;
            }
            else {
                heroImg.transform.Find("Block").gameObject.SetActive(false);
                heroImg.color = new Color32(255, 255, 255, 255);
                cardNumValue.color = new Color32(255, 255, 255, 255);
                //content.GetChild(i).GetComponent<Button>().interactable = true;
            }
            
            string name = (humanDecks[i].name.Contains("sampledeck")) ? AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("SampleDeck", humanDecks[i].name) : humanDecks[i].name;
            content.GetChild(i).Find("DeckName").GetComponent<TextMeshProUGUI>().text = name;

            GameObject obj = content.GetChild(i).gameObject;
            DeckHandler deckHandler = content.GetChild(i).gameObject.GetComponent<DeckHandler>();
            deckHandler.DECKID = humanDecks[i].id;
            var deck = humanDecks[i];

            Button button = deckHandler.GetComponent<Button>();
            button.onClick.AddListener(() => { OnDeckSelected(deckHandler.DECKID, "human", deck, obj); });
        }

        int index = 0;
        for (int i = humanDecks.Count; i < humanDecks.Count + orcDecks.Count; i++) {
            content.GetChild(i).gameObject.SetActive(true);

            Image heroImg = content.GetChild(i).Find("HeroImg").GetComponent<Image>();
            string banner = orcDecks[index].bannerImage;
            if (banner == "custom") {
                heroImg.sprite = accountManager.resource.deckPortraite["h10002"];
            }
            else {
                heroImg.sprite = accountManager.resource.deckPortraite[banner];
            }

            content.GetChild(i).Find("RaceFlag/Human").gameObject.SetActive(false);
            content.GetChild(i).Find("RaceFlag/Orc").gameObject.SetActive(true);

            var cardNumValue = content.GetChild(i).Find("CardNum/Value").GetComponent<TextMeshProUGUI>();
            cardNumValue.text = orcDecks[index].totalCardCount + "/";
            if (orcDecks[index].totalCardCount < 40) {
                heroImg.transform.Find("Block").gameObject.SetActive(true);
                heroImg.color = new Color32(60, 60, 60, 255);
                cardNumValue.color = new Color32(255, 0, 0, 255);
            }
            else {
                heroImg.transform.Find("Block").gameObject.SetActive(false);
                heroImg.color = new Color32(255, 255, 255, 255);
                cardNumValue.color = new Color32(255, 255, 255, 255);
            }

            string name = (orcDecks[index].name.Contains("sampledeck")) ? AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("SampleDeck", orcDecks[index].name) : orcDecks[index].name;
            content.GetChild(i).Find("DeckName").GetComponent<TextMeshProUGUI>().text = name;

            GameObject obj = content.GetChild(i).gameObject;
            DeckHandler deckHandler = content.GetChild(i).gameObject.GetComponent<DeckHandler>();
            deckHandler.DECKID = orcDecks[index].id;
            var deck = orcDecks[index];

            Button button = deckHandler.GetComponent<Button>();
            button.onClick.AddListener(() => { OnDeckSelected(deckHandler.DECKID, "orc", deck, obj); });
            index++;
        }

        int totalDeckCount = humanDecks.Count + orcDecks.Count;
        transform.Find("Header/NumValue").GetComponent<TextMeshProUGUI>().text = totalDeckCount + "/" + accountManager.userData.maxDeckCount;
    }

    private void Start() {
        ResetMyDecks();
        accountManager.RequestMyDecks();

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
    }

    void OnDisable() {
        battleStart.SetActive(false);

        if(selectedObj != null) {
            selectedObj.transform.Find("FrontEffect").gameObject.SetActive(false);
            selectedObj = null;
        }
    }

    public void ResetMyDecks() {
        foreach(Transform child in content) {
            child.gameObject.SetActive(false);
        }
    }

    GameObject selectedObj = null;

    public void OnDeckSelected(string deckId, string camp, dataModules.Deck deck, GameObject obj) {
        if(selectedObj != null) {
            selectedObj.transform.Find("FrontEffect").gameObject.SetActive(false);
            selectedObj.transform.Find("Glow").gameObject.SetActive(false);
        }

        PlayerPrefs.SetString("SelectedRace", camp);
        PlayerPrefs.SetString("SelectedDeckId", deckId);
        PlayerPrefs.SetString("SelectedBattleType", "league");

        parentController.selectedDeck = deck;
        PlayerPrefs.SetString("selectedHeroId", deck.heroId);

        //Logger.Log(camp + "???" + deckId + "?????? ?????????");

        selectedObj = obj;

        selectedObj.transform.Find("FrontEffect").gameObject.SetActive(true);
        selectedObj.transform.Find("Glow").gameObject.SetActive(true);

        battleStart.SetActive(true);
    }
}
