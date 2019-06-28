using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using dataModules;
using UnityEngine.UI;
using TMPro;

public class RaceTypeToggleHandler : MonoBehaviour {
    [SerializeField] Transform heroPortraitParent, deckParent;

    [Space(10)]
    [SerializeField] GameObject portraitPrefab;

    [Space(10)]
    [SerializeField] GameObject heroGroupPrefab;
    [SerializeField] GameObject deckGroupPrefab;
    [SerializeField] BattleReadySceneController controller;

    [Space(10)]
    [SerializeField] GameObject orcDeckPrefab;
    [SerializeField] GameObject humanDeckPrefab;
    [SerializeField] GameObject AddDeckButtonPrefab;

    AccountManager accountManager;
    int id;

    //TODO : Server와 연동
    private string leaderDeckId;
    public string LeaderDeckId {
        get {
            return leaderDeckId;
        }
        set {
            leaderDeckId = value;
            controller.ChangeDeck(leaderDeckId);
        }
    }

    private GameObject selectedDeck = null;

    private const int PORTRAIT_SLOT_NUM_PER_PAGE = 8;
    private const int DECK_SLOT_NUM_PER_PAGE = 3;

    private void Awake() {
        accountManager = AccountManager.Instance;
        id = GetComponent<IntergerIndex>().Id;
    }

    public void Init() {
        var type = (BattleReadySceneController.RaceType)id;
        controller.ChangeRaceType(type);        //Send To Machine Variables

        ClearList();
        //CreateHeroList(type);
        CreateBasicDeckList(type);
    }

    //Called By Bolt Machine
    public void SwitchOn() {
        Init();

        transform.Find("Selected").gameObject.SetActive(true);
        transform.Find("Unselected").gameObject.SetActive(false);
    }

    public void SwitchOff() {
        transform.Find("Selected").gameObject.SetActive(false);
        transform.Find("Unselected").gameObject.SetActive(true);
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
        GameObject deckPrefab = humanDeckPrefab;
        switch (type) {
            case BattleReadySceneController.RaceType.HUMAN:
                basicDecks = controller.humanDecks.basicDecks;
                deckPrefab = humanDeckPrefab;
                break;
            case BattleReadySceneController.RaceType.ORC:
                basicDecks = controller.orcDecks.basicDecks;
                deckPrefab = orcDeckPrefab;
                break;
            default:
                basicDecks = null;
                break;
        }

        if (basicDecks == null) return;

        var pageNum = TotalDeckPages(ref basicDecks);
        //TODO Server와 연동
        LeaderDeckId = basicDecks[0].id;

        int item_index = 0;         //전체 Deck Index
        GameObject lastPage = null;
        for (int i = 0; i < pageNum; i++) {
            GameObject page = Instantiate(deckGroupPrefab, deckParent);
            lastPage = page;
            for (int j = 0; j < DECK_SLOT_NUM_PER_PAGE; j++) {
                if (item_index > basicDecks.Count - 1) break;

                //덱 프리팹 생성
                GameObject _deck = Instantiate(deckPrefab, page.transform);
                _deck.transform.Find("Deck/Name").GetComponent<TextMeshProUGUI>().text = basicDecks[item_index].name;
                if(LeaderDeckId == basicDecks[item_index].id) {
                    selectedDeck = _deck;
                    _deck.transform.Find("Outline").GetComponent<Image>().enabled = true;
                }

                _deck.transform.Find("Deck").GetComponent<StringIndex>().Id = basicDecks[item_index].id;

                _deck.transform.Find("Deck").GetComponent<Button>().onClick.AddListener(() => {
                    OnDeckSelected(_deck);
                });

                item_index++;
            }
        }
        GameObject AddDeckButton = Instantiate(AddDeckButtonPrefab, lastPage.transform);
    }

    /// <summary>
    /// 대표 덱을 가장 앞으로 나오게 한다.
    /// </summary>
    /// <param name="leaderDeck"></param>
    private void SetLeaderDeckFront(ref GameObject leaderDeck) {
        leaderDeck.transform.SetAsFirstSibling();
    }

    public void OnDeckSelected(GameObject selectedDeck) {
        this.selectedDeck.transform.Find("Outline").GetComponent<Image>().enabled = false;
        selectedDeck.transform.Find("Outline").GetComponent<Image>().enabled = true;
        LeaderDeckId = selectedDeck.transform.Find("Deck").GetComponent<StringIndex>().Id;
        this.selectedDeck = selectedDeck;
    }

    private int TotalPortraitPages(ref List<Hero> heroes) {
        return Mathf.CeilToInt((float)heroes.Count / PORTRAIT_SLOT_NUM_PER_PAGE); 
    }

    private int TotalDeckPages(ref List<Deck> decks) {
        return Mathf.CeilToInt((float)decks.Count / DECK_SLOT_NUM_PER_PAGE + 1);
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
