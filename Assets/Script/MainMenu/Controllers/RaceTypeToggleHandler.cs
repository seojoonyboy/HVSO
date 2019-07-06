using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using dataModules;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
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
    int id = 1;

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
    private int pageIndex;
    public int PageIndex {
        get {
            return pageIndex;
        }
        set {
            pageIndex = value;

            if (pageIndex < 0) pageIndex = 0;
            if (pageIndex > MaxPageNum) pageIndex = MaxPageNum;

            var text = string.Format("{0}/{1}", PageIndex + 1, MaxPageNum);
            controller.ChangePageText(text);
            //Logger.Log(pageIndex);
        }
    }

    public int MaxPageNum;

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
        AccountManager accountManager = AccountManager.Instance;
        switch (type) {
            case BattleReadySceneController.RaceType.HUMAN:
                selectedHeroes = accountManager.humanDecks.heros;
                break;
            case BattleReadySceneController.RaceType.ORC:
                selectedHeroes = accountManager.orcDecks.heros;
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
        List<Deck> totalDecks = new List<Deck>();
        GameObject deckPrefab = humanDeckPrefab;
        int pageNum = 0;
        MaxPageNum = 0;
        AccountManager accountManager = AccountManager.Instance;
        switch (type) {
            case BattleReadySceneController.RaceType.HUMAN:
                totalDecks.AddRange(accountManager.humanDecks.basicDecks);
                totalDecks.AddRange(accountManager.humanDecks.customDecks);
                deckPrefab = humanDeckPrefab;

                pageNum = TotalDeckPages(ref totalDecks);
                MaxPageNum = pageNum;
                break;
            case BattleReadySceneController.RaceType.ORC:
                totalDecks.AddRange(accountManager.orcDecks.basicDecks);
                totalDecks.AddRange(accountManager.orcDecks.customDecks);
                deckPrefab = orcDeckPrefab;

                pageNum = TotalDeckPages(ref totalDecks);
                MaxPageNum = pageNum;
                break;
            default:
                totalDecks = null;
                break;
        }

        //test code
        //basicDecks = new List<Deck>();
        //for(int i=0; i<10; i++) {
        //    Deck deck = new Deck();
        //    deck.name = i + "번째 덱";
        //    deck.id = i.ToString();

        //    basicDecks.Add(deck);
        //}
        //end test code
        if ((int)type != id) return;
        if (totalDecks == null) return;

        Logger.Log("Total Deck Count : " + totalDecks.Count);
        int item_index = 0;         //전체 Deck Index
        PageIndex = 0;
        controller.ButtonGlowEffect.SetActive(false);
        Bolt.Variables.Saved.Set("SelectedDeckId", "");

        GameObject lastPage = null;
        for (int i = 0; i < pageNum; i++) {
            GameObject page = Instantiate(deckGroupPrefab, deckParent);
            lastPage = page;
            for (int j = 0; j < DECK_SLOT_NUM_PER_PAGE; j++) {
                if (item_index > totalDecks.Count - 1) break;

                //덱 프리팹 생성
                GameObject _deck = Instantiate(deckPrefab, page.transform);
                _deck.transform.Find("Deck/Name").GetComponent<TextMeshProUGUI>().text = totalDecks[item_index].name;
                if(LeaderDeckId == totalDecks[item_index].id) {
                    selectedDeck = _deck;
                    controller.selectedDeck = totalDecks[item_index];
                }

                _deck.transform.Find("Deck/Info/Text").GetComponent<TextMeshProUGUI>().text = 
                    totalDecks[item_index].cardTotalCount + "/40";

                _deck.transform.Find("Deck").GetComponent<StringIndex>().Id = totalDecks[item_index].id;

                int _index = item_index;
                _deck.transform.Find("Deck").GetComponent<Button>().onClick.AddListener(() => {
                    OnDeckSelected(_deck, totalDecks[_index]);
                });
                item_index++;
            }
            if (pageIndex != i) page.SetActive(false);
        }

        ////해당 페이지에 빈 슬롯이 있는 경우
        //if(lastPage.transform.childCount <= DECK_SLOT_NUM_PER_PAGE) {
        //    GameObject AddDeckButton = Instantiate(AddDeckButtonPrefab, lastPage.transform);
        //}
        ////새로 페이지를 만들어야 함
        //else {
        //    GameObject page = Instantiate(deckGroupPrefab, deckParent);
        //    GameObject AddDeckButton = Instantiate(AddDeckButtonPrefab, page.transform);
        //}


        if(type == BattleReadySceneController.RaceType.HUMAN) {
            if(id == 0) {
                AddButtonListener();
            }
        }
        else if(type == BattleReadySceneController.RaceType.ORC) {
            if (id == 1) {
                AddButtonListener();
            }
        }
    }

    private void AddButtonListener() {
        var Button = deckParent.parent.transform.Find("PrevBtn").GetComponent<Button>();
        Button.onClick.RemoveListener(GoToPrevPage);
        Button.onClick.AddListener(() => {
            GoToPrevPage();
        });

        Button = deckParent.parent.transform.Find("NextBtn").GetComponent<Button>();
        Button.onClick.RemoveListener(GoToNextPage);
        Button.onClick.AddListener(() => {
            GoToNextPage();
        });
    }

    /// <summary>
    /// 대표 덱을 가장 앞으로 나오게 한다.
    /// </summary>
    /// <param name="leaderDeck"></param>
    private void SetLeaderDeckFront(ref GameObject leaderDeck) {
        leaderDeck.transform.SetAsFirstSibling();
    }

    public void OnDeckSelected(GameObject selectedDeck, Deck data) {
        if(this.selectedDeck != null) {
            this.selectedDeck.transform.Find("Outline").gameObject.SetActive(false);
            this.selectedDeck.transform.Find("Shadow").gameObject.SetActive(true);
        }
        selectedDeck.transform.Find("Outline").gameObject.SetActive(true);
        selectedDeck.transform.Find("Shadow").gameObject.SetActive(false);

        LeaderDeckId = selectedDeck.transform.Find("Deck").GetComponent<StringIndex>().Id;
        this.selectedDeck = selectedDeck;
        selectedDeck.transform.Find("Deck/Twinkle").GetComponent<DeckClickSpine>().Click();
        controller.selectedDeck = data;
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

    public void GoToPrevPage() {
        if (PageIndex == 0) return;
        deckParent.GetChild(PageIndex).gameObject.SetActive(false);
        deckParent.GetChild(--PageIndex).gameObject.SetActive(true);
        //horizontalScrollSnap.GoToScreen(--PageIndex);
    }

    public void GoToNextPage() {
        if (PageIndex == MaxPageNum - 1) return;
        deckParent.GetChild(PageIndex).gameObject.SetActive(false);
        deckParent.GetChild(++PageIndex).gameObject.SetActive(true);
        //horizontalScrollSnap.GoToScreen(++PageIndex);
    }
}
