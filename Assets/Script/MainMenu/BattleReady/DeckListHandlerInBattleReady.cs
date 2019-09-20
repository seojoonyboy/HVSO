using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckListHandlerInBattleReady : MonoBehaviour {
    AccountManager accountManager;
    [SerializeField] Transform content;
    [SerializeField] BattleReadySceneController parentController;

    public Sprite[] campImages;
    void Awake() {
        accountManager = AccountManager.Instance;
    }

    void OnEnable() {
        parentController.HudController.SetBackButton(() => {
            gameObject.SetActive(false);
        });
        ResetMyDecks();
        LoadMyDecks();
    }

    void OnDisable() {
        parentController.HudController.SetBackButton(() => {
            parentController.OnBackButton();
        });
    }

    public void LoadMyDecks() {
        var humanDecks = accountManager.humanDecks;
        var orcDecks = accountManager.orcDecks;

        for(int i = 0; i < humanDecks.Count; i++) {
            content.GetChild(i).gameObject.SetActive(true);
            content.GetChild(i).Find("DeckObject/HeroImg").GetComponent<Image>().sprite = campImages[0];

            DeckHandler deckHandler = content.GetChild(i).gameObject.GetComponent<DeckHandler>();
            deckHandler.DECKID = humanDecks[i].id;
            var deck = humanDecks[i];
            deckHandler.deck = deck;

            Button button = deckHandler.GetComponent<Button>();
            button.onClick.AddListener(() => { OnDeckSelected(deckHandler.DECKID, "human", deck); });
        }

        for(int i = humanDecks.Count; i < humanDecks.Count + orcDecks.Count; i++) {
            content.GetChild(i).gameObject.SetActive(true);
            content.GetChild(i).Find("DeckObject/HeroImg").GetComponent<Image>().sprite = campImages[1];

            DeckHandler deckHandler = content.GetChild(i).gameObject.GetComponent<DeckHandler>();
            deckHandler.DECKID = orcDecks[i].id;
            var deck = orcDecks[i];
            deckHandler.deck = deck;

            Button button = deckHandler.GetComponent<Button>();
            button.onClick.AddListener(() => { OnDeckSelected(deckHandler.DECKID, "orc", deck); });
        }
    }

    public void ResetMyDecks() {
        foreach(Transform child in content) {
            child.gameObject.SetActive(false);
        }
    }

    public void OnDeckSelected(string deckId, string camp, dataModules.Deck deck) {
        PlayerPrefs.SetString("SelectedRace", camp);
        PlayerPrefs.SetString("SelectedDeckId", deckId);
        PlayerPrefs.SetString("SelectedBattleType", "multi");

        parentController.selectedDeck = deck;

        Logger.Log(camp + "¿«" + deckId + "µ¶¿Ã º±≈√µ ");
        gameObject.SetActive(false);

        parentController.OnStartButton();
    }
}
