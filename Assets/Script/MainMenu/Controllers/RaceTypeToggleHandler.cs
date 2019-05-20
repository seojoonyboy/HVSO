using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using dataModules;
using UnityEngine.UI;

public class RaceTypeToggleHandler : MonoBehaviour {
    [SerializeField] Transform heroPortraitParent, deckParent;

    [Space(10)]
    [SerializeField] GameObject portraitPrefab;
    [SerializeField] GameObject deckPrefab;

    [Space(10)]
    [SerializeField] GameObject heroGroupPrefab;
    [SerializeField] GameObject deckGroupPrefab;
    [SerializeField] BattleReadySceneController controller;
    AccountManager accountManager;
    int id;

    private void Awake() {
        accountManager = AccountManager.Instance;
        id = GetComponent<IntergerIndex>().Id;
    }

    public void SwitchOn() {
        transform.Find("Selected").gameObject.SetActive(true);
        var type = (BattleReadySceneController.RaceType)id;
        controller.ChangeRaceType(type);
        var decks = accountManager.myDecks;
        int deckIndex = 0;

        switch (type) {
            case BattleReadySceneController.RaceType.HUMAN:
                deckIndex = 0;
                break;
            case BattleReadySceneController.RaceType.ORC:
                deckIndex = 1;
                break;
        }

        var selectedDeck = decks[deckIndex];
        CreateDummyList(ref selectedDeck);
    }

    public void SwitchOff() {
        transform.Find("Selected").gameObject.SetActive(false);
    }

    private void CreateDummyList(ref AccountManager.Deck deck) {
        ClearList();

        GameObject deckGroup = Instantiate(deckGroupPrefab, deckParent);
        GameObject heroGroup = Instantiate(heroGroupPrefab, heroPortraitParent);

        GameObject _deck = deckGroup.transform.GetChild(0).gameObject;
        GameObject _hero = heroGroup.transform.GetChild(0).gameObject;

        _deck.transform.Find("Deactive").gameObject.SetActive(false);
        _hero.transform.Find("Deactive").gameObject.SetActive(false);

        _deck.transform.Find("Text").GetComponent<Text>().text = deck.deckName + "덱";
        _hero.transform.Find("Name").GetComponent<Text>().text = deck.heroName;

        _deck.GetComponent<Button>().onClick.AddListener(() => { controller.OnClickDeck(_deck); });

        _deck.transform.Find("InfoPanel/InfoButton").GetComponent<Button>().onClick.AddListener(() => {
            //controller.OnClickCardListModal();
        });
    }

    private void CreateList() {
        ClearList();

    }

    private void ClearList() {
        foreach(Transform tf in heroPortraitParent) {
            Destroy(tf.gameObject);
        }
        foreach(Transform tf in deckParent) {
            Destroy(tf.gameObject);
        }
    }
}
