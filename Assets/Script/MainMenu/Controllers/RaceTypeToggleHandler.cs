using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using dataModules;
using UnityEngine.UI;

public class RaceTypeToggleHandler : ToggleHandler {
    [SerializeField] Transform heroPortraitParent, deckParent;

    [Space(10)]
    [SerializeField] GameObject portraitPrefab;
    [SerializeField] GameObject deckPrefab;

    [Space(10)]
    [SerializeField] GameObject heroGroupPrefab;
    [SerializeField] GameObject deckGroupPrefab;

    public override void OnValueChanged() {
        base.OnValueChanged();

        if (toggle.isOn) {
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
    }

    private void CreateDummyList(ref AccountManager.Deck deck) {
        ClearList();

        GameObject deckGroup = Instantiate(deckGroupPrefab, deckParent);
        GameObject heroGroup = Instantiate(heroGroupPrefab, heroPortraitParent);

        GameObject _deck = deckGroup.transform.GetChild(0).gameObject;
        GameObject _hero = heroGroup.transform.GetChild(0).gameObject;

        _deck.transform.Find("Deactive").gameObject.SetActive(false);
        _hero.transform.Find("Deactive").gameObject.SetActive(false);

        _deck.transform.Find("Text").GetComponent<Text>().text = deck.heroName + "덱";
        _hero.transform.Find("Name").GetComponent<Text>().text = deck.heroName;

        _deck.GetComponent<Button>().onClick.AddListener(() => { controller.OnClickDeck(_deck); });
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
