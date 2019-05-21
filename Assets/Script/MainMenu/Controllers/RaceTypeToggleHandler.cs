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

    private const int PORTRAIT_SLOT_NUM_PER_PAGE = 8;
    private const int DECK_SLOT_NUM_PER_PAGE = 8;

    private void Awake() {
        accountManager = AccountManager.Instance;
        id = GetComponent<IntergerIndex>().Id;
    }

    public void Init() {
        transform.Find("Selected").gameObject.SetActive(true);
        var type = (BattleReadySceneController.RaceType)id;
        controller.ChangeRaceType(type);        //Send To Machine Variables

        ClearList();
        CreateHeroList(type);
        CreateBasicDeckList(type);
    }

    //Called By Bolt Machine
    public void SwitchOn() {
        Init();
        //var selectedDeck = decks[deckIndex];
        //CreateDummyList(ref selectedDeck);
    }

    public void SwitchOff() {
        transform.Find("Selected").gameObject.SetActive(false);
    }

    private void CreateHeroList(BattleReadySceneController.RaceType type) {
        List<Hero> selectedHeroes;
        switch (type) {
            case BattleReadySceneController.RaceType.HUMAN:
                selectedHeroes = controller.humanDecks.heros;
                break;
            case BattleReadySceneController.RaceType.ORC:
                selectedHeroes = controller.orcDecks.heros;
                break;
            default:
                selectedHeroes = null;
                break;
        }
        if (selectedHeroes == null) return;

        var pageNum = TotalPortraitPages(ref selectedHeroes);

        int item_count = 0;
        int slot_count = 0;

        for(int i=0; i<pageNum; i++) {
            GameObject page = Instantiate(heroGroupPrefab, heroPortraitParent);
            if (slot_count == PORTRAIT_SLOT_NUM_PER_PAGE) {
                slot_count = 0;
                continue;
            }

            for(int j=0; j<PORTRAIT_SLOT_NUM_PER_PAGE; j++) {
                if (item_count > selectedHeroes.Count - 1) break;
                Transform target_portrait = page.transform.GetChild(slot_count);

                target_portrait.transform.Find("Deactive").gameObject.SetActive(false);
                target_portrait.transform.Find("Name").GetComponent<Text>().text = selectedHeroes[item_count].name;

                target_portrait.GetComponent<Data>().data = selectedHeroes[item_count];
                target_portrait.GetComponent<IntergerIndex>().Id = item_count;

                item_count++;
                slot_count++;
            }
        }
    }

    private void CreateBasicDeckList(BattleReadySceneController.RaceType type) {
        List<Deck> basicDecks;
        switch (type) {
            case BattleReadySceneController.RaceType.HUMAN:
                basicDecks = controller.humanDecks.basicDecks;
                break;
            case BattleReadySceneController.RaceType.ORC:
                basicDecks = controller.orcDecks.basicDecks;
                break;
            default:
                basicDecks = null;
                break;
        }

        if (basicDecks == null) return;

        var pageNum = TotalDeckPages(ref basicDecks);

        int item_count = 0;
        int slot_count = 0;

        for (int i = 0; i < pageNum; i++) {
            GameObject page = Instantiate(deckGroupPrefab, deckParent);
            if (slot_count == PORTRAIT_SLOT_NUM_PER_PAGE) {
                slot_count = 0;
                continue;
            }

            for (int j = 0; j < PORTRAIT_SLOT_NUM_PER_PAGE; j++) {
                if (item_count > basicDecks.Count - 1) break;
                Transform target_deck_slot = page.transform.GetChild(slot_count);

                target_deck_slot.transform.Find("Deactive").gameObject.SetActive(false);
                target_deck_slot.transform.Find("Name").GetComponent<Text>().text = basicDecks[item_count].name;

                target_deck_slot.GetComponent<Data>().data = basicDecks[item_count];
                target_deck_slot.GetComponent<IntergerIndex>().Id = item_count;

                item_count++;
                slot_count++;
            }
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

        _deck.transform.Find("Text").GetComponent<Text>().text = deck.deckName + "덱";
        _hero.transform.Find("Name").GetComponent<Text>().text = deck.heroName;

        _deck.transform.Find("InfoPanel/InfoButton").GetComponent<Button>().onClick.AddListener(() => {
            //controller.OnClickCardListModal();
        });
    }

    private int TotalPortraitPages(ref List<Hero> heroes) {
        return Mathf.CeilToInt((float)heroes.Count / PORTRAIT_SLOT_NUM_PER_PAGE); 
    }

    private int TotalDeckPages(ref List<Deck> decks) {
        return Mathf.CeilToInt((float)decks.Count / DECK_SLOT_NUM_PER_PAGE);
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
