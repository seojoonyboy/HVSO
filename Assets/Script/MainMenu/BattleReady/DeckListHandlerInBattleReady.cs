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

        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_DECKS_UPDATED, LoadMyDecks);
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

            content.GetChild(i).Find("CardNum/Value").GetComponent<TextMeshProUGUI>().text = humanDecks[i].totalCardCount + "/";
            content.GetChild(i).Find("DeckName").GetComponent<TextMeshProUGUI>().text = humanDecks[i].name;

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

            content.GetChild(i).Find("CardNum/Value").GetComponent<TextMeshProUGUI>().text = orcDecks[index].totalCardCount + "/";
            content.GetChild(i).Find("DeckName").GetComponent<TextMeshProUGUI>().text = orcDecks[index].name;

            GameObject obj = content.GetChild(i).gameObject;
            DeckHandler deckHandler = content.GetChild(i).gameObject.GetComponent<DeckHandler>();
            deckHandler.DECKID = orcDecks[index].id;
            var deck = orcDecks[index];

            Button button = deckHandler.GetComponent<Button>();
            button.onClick.AddListener(() => { OnDeckSelected(deckHandler.DECKID, "orc", deck, obj); });
            index++;
        }
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
            selectedObj.transform.Find("BackEffect").gameObject.SetActive(false);
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
            selectedObj.transform.Find("BackEffect").gameObject.SetActive(false);
            selectedObj.transform.Find("FrontEffect").gameObject.SetActive(false);
        }

        PlayerPrefs.SetString("SelectedRace", camp);
        PlayerPrefs.SetString("SelectedDeckId", deckId);
        PlayerPrefs.SetString("SelectedBattleType", "league");

        parentController.selectedDeck = deck;

        Logger.Log(camp + "의" + deckId + "덱이 선택됨");

        selectedObj = obj;

        selectedObj.transform.Find("BackEffect").gameObject.SetActive(true);
        selectedObj.transform.Find("FrontEffect").gameObject.SetActive(true);

        battleStart.SetActive(true);
    }
}
