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
        controller.ChangeRaceType(type);

        ClearList();
        CreateBasicDeckList(type);
    }
    
    public void SwitchOn() {
        if (GetComponent<BooleanIndex>().isOn) return;

        Init();

        transform.Find("Selected").gameObject.SetActive(true);
        transform.Find("Unselected").gameObject.SetActive(false);
    }

    public void SwitchOff() {
        transform.Find("Selected").gameObject.SetActive(false);
        transform.Find("Unselected").gameObject.SetActive(true);
    }

    private void CreateBasicDeckList(BattleReadySceneController.RaceType type) {
        List<Deck> totalDecks = new List<Deck>();
        GameObject deckPrefab = humanDeckPrefab;
        int pageNum = 0;
        MaxPageNum = 0;
        AccountManager accountManager = AccountManager.Instance;
        switch (type) {
            case BattleReadySceneController.RaceType.HUMAN:
                totalDecks.AddRange(accountManager.humanDecks);
                deckPrefab = humanDeckPrefab;

                pageNum = TotalDeckPages(ref totalDecks);
                MaxPageNum = pageNum;
                break;
            case BattleReadySceneController.RaceType.ORC:
                totalDecks.AddRange(accountManager.orcDecks);
                deckPrefab = orcDeckPrefab;

                pageNum = TotalDeckPages(ref totalDecks);
                MaxPageNum = pageNum;
                break;
            default:
                totalDecks = null;
                break;
        }
        if ((int)type != id) return;
        if (totalDecks == null) return;

        Logger.Log("Total Deck Count : " + totalDecks.Count);
        int item_index = 0;         //전체 Deck Index
        PageIndex = 0;
        controller.ButtonGlowEffect.SetActive(false);
        PlayerPrefs.SetString("SelectedDeckId", "");

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
                    totalDecks[item_index].totalCardCount + "/40";

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
        Button.onClick.RemoveAllListeners();
        Button.onClick.AddListener(() => {
            GoToPrevPage();
        });

        Button = deckParent.parent.transform.Find("NextBtn").GetComponent<Button>();
        Button.onClick.RemoveAllListeners();
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
            this.selectedDeck.transform.Find("Deck/Twinkle").gameObject.SetActive(false);
        }
        selectedDeck.transform.Find("Outline").gameObject.SetActive(true);

        LeaderDeckId = selectedDeck.transform.Find("Deck").GetComponent<StringIndex>().Id;
        this.selectedDeck = selectedDeck;
        GameObject twinkle = selectedDeck.transform.Find("Deck/Twinkle").gameObject;
        twinkle.SetActive(true);
        //twinkle.GetComponent<DeckClickSpine>().Click();
        controller.selectedDeck = data;
    }

    private int TotalPortraitPages(ref List<Hero> heroes) {
        return Mathf.CeilToInt((float)heroes.Count / PORTRAIT_SLOT_NUM_PER_PAGE); 
    }

    private int TotalDeckPages(ref List<Deck> decks) {
        return Mathf.CeilToInt((float)decks.Count / DECK_SLOT_NUM_PER_PAGE);
    }

    public void ClearList() {
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
