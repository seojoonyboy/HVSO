using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckListHandlerInBattleReady : MonoBehaviour {
    AccountManager accountManager;
    [SerializeField] Transform content;
    [SerializeField] BattleReadySceneController parentController;

    public Sprite[] campImages;
    [SerializeField] GameObject battleStart;

    void Awake() {
        accountManager = AccountManager.Instance;
    }

    void OnEnable() {
        parentController.HudController.SetBackButton(() => {
            gameObject.SetActive(false);
        });
        ResetMyDecks();
        LoadMyDecks();

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
    }

    void OnDisable() {
        parentController.HudController.SetBackButton(() => {
            parentController.OnBackButton();
        });
        battleStart.SetActive(false);

        if(selectedObj != null) {
            selectedObj.transform.Find("BackEffect").gameObject.SetActive(false);
            selectedObj.transform.Find("FrontEffect").gameObject.SetActive(false);

            selectedObj = null;
        }
    }

    public void LoadMyDecks() {
        var humanDecks = accountManager.humanDecks;
        var orcDecks = accountManager.orcDecks;

        for(int i = 0; i < humanDecks.Count; i++) {
            content.GetChild(i).gameObject.SetActive(true);
            content.GetChild(i).Find("HeroImg").GetComponent<Image>().sprite = campImages[0];
            content.GetChild(i).Find("CardNum/Value").GetComponent<TextMeshProUGUI>().text = humanDecks[i].totalCardCount + "/";
            content.GetChild(i).Find("DeckName").GetComponent<TextMeshProUGUI>().text = humanDecks[i].name;

            GameObject obj = content.GetChild(i).gameObject;
            DeckHandler deckHandler = content.GetChild(i).gameObject.GetComponent<DeckHandler>();
            deckHandler.DECKID = humanDecks[i].id;
            var deck = humanDecks[i];

            Button button = deckHandler.GetComponent<Button>();
            button.onClick.AddListener(() => { OnDeckSelected(deckHandler.DECKID, "HUMAN", deck, obj); });
        }

        int index = 0;
        for(int i = humanDecks.Count; i < humanDecks.Count + orcDecks.Count; i++) {
            content.GetChild(i).gameObject.SetActive(true);
            content.GetChild(i).Find("HeroImg").GetComponent<Image>().sprite = campImages[1];
            content.GetChild(i).Find("CardNum/Value").GetComponent<TextMeshProUGUI>().text = orcDecks[index].totalCardCount + "/";
            content.GetChild(i).Find("DeckName").GetComponent<TextMeshProUGUI>().text = orcDecks[index].name;

            GameObject obj = content.GetChild(i).gameObject;
            DeckHandler deckHandler = content.GetChild(i).gameObject.GetComponent<DeckHandler>();
            deckHandler.DECKID = orcDecks[index].id;
            var deck = orcDecks[index];

            Button button = deckHandler.GetComponent<Button>();
            button.onClick.AddListener(() => { OnDeckSelected(deckHandler.DECKID, "ORC", deck, obj); });
            index++;
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
        PlayerPrefs.SetString("SelectedBattleType", "multi");

        parentController.selectedDeck = deck;

        Logger.Log(camp + "의" + deckId + "덱이 선택됨");

        selectedObj = obj;

        selectedObj.transform.Find("BackEffect").gameObject.SetActive(true);
        selectedObj.transform.Find("FrontEffect").gameObject.SetActive(true);

        battleStart.SetActive(true);
    }
}
