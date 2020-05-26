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
            int idx = i;
            GameObject deckObject = content.GetChild(i).gameObject;
            deckObject.SetActive(true);
            var deckHandler = deckObject.GetComponent<DeckHandler>();
            deckHandler.SetNewDeck(humanDecks[i]);
            
            Button button = deckObject.transform.GetChild(0).Find("HeroImg").GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { OnDeckSelected(deckHandler.DECKID, "human", humanDecks[idx], deckObject); });
        }
        int index = 0;
        for (int i = humanDecks.Count; i < humanDecks.Count + orcDecks.Count; i++) {
            int idx = index;
            GameObject deckObject = content.GetChild(i).gameObject;
            deckObject.SetActive(true);
            var deckHandler = deckObject.GetComponent<DeckHandler>();
            deckHandler.SetNewDeck(orcDecks[index]);
            
            Button button = deckObject.transform.GetChild(0).Find("HeroImg").GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { OnDeckSelected(deckHandler.DECKID, "orc", orcDecks[idx], deckObject); });
            
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
            selectedObj.transform.GetChild(0).Find("FrontEffect").gameObject.SetActive(false);
            selectedObj = null;
        }
    }

    public void ResetMyDecks() {
        foreach(Transform child in content) {
            child.gameObject.SetActive(false);
        }
    }

    GameObject selectedObj = null;

    public void OnDeckSelected(string deckId, string camp, Deck deck, GameObject obj) {
        if(selectedObj != null) {
            selectedObj.transform.GetChild(0).Find("FrontEffect").gameObject.SetActive(false);
            selectedObj.transform.GetChild(0).Find("Glow").gameObject.SetActive(false);
        }

        PlayerPrefs.SetString("SelectedRace", camp);
        PlayerPrefs.SetString("SelectedDeckId", deckId);
        PlayerPrefs.SetString("SelectedBattleType", "league");

        parentController.selectedDeck = deck;
        PlayerPrefs.SetString("selectedHeroId", deck.heroId);

        //Logger.Log(camp + "의" + deckId + "덱이 선택됨");

        selectedObj = obj;

        selectedObj.transform.GetChild(0).Find("FrontEffect").gameObject.SetActive(true);
        selectedObj.transform.GetChild(0).Find("Glow").gameObject.SetActive(true);

        battleStart.SetActive(true);
    }
}
