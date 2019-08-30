using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScenarioManager : MonoBehaviour
{
    public static ScenarioManager Instance { get; private set; }
    public ShowSelectRace human, orc;
    public string heroID;
    public string selectStage;
    public bool isHuman;
    public bool isIngameButtonClicked = false;
    public GameObject stageCanvas;
    public GameObject deckContent;


    private GameObject selectedDeckObject = null;
    public dataModules.Deck selectedDeck;

    private string leaderDeckId;
    public string LeaderDeckId {
        get {
            return leaderDeckId;
        }
        set {
            leaderDeckId = value;
            ChangeDeck(leaderDeckId);
        }
    }

    [SerializeField] GameObject orcDeckPrefab;
    [SerializeField] GameObject humanDeckPrefab;

    private void Awake() {
        Instance = this;
        OnHumanButton();
        PlayerPrefs.SetString("SelectedDeckId", "");
    }

    private void OnDestroy() {
        Instance = null;
    }   
    

    public void OnBackButton() {
        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
        FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);
    }

    public void OnHumanButton() {
        orc.raceButton.GetComponent<Image>().sprite = orc.deactiveSprite;
        orc.heroSelect.SetActive(false);
        orc.StageCanvas.SetActive(false);

        heroID = "";
        selectStage = "";

        human.raceButton.GetComponent<Image>().sprite = human.activeSprite;
        human.heroSelect.SetActive(true);
        human.StageCanvas.SetActive(true);
        isHuman = true;
        PlayerPrefs.SetString("SelectedRace", "HUMAN");
    }
    
    public void OnOrcButton() {
        human.raceButton.GetComponent<Image>().sprite = human.deactiveSprite;
        human.heroSelect.SetActive(false);
        human.StageCanvas.SetActive(false);

        heroID = "";
        selectStage = "";

        orc.raceButton.GetComponent<Image>().sprite = orc.activeSprite;
        orc.heroSelect.SetActive(true);
        orc.StageCanvas.SetActive(true);
        isHuman = false;
        PlayerPrefs.SetString("SelectedRace", "ORC");
    }

    public void OnStageCloseBtn() {        
        stageCanvas.SetActive(false);
    }

    private void CreateBasicDeckList(bool isHuman) {
        List<dataModules.Deck> totalDecks = new List<dataModules.Deck>();
        GameObject deckPrefab = humanDeckPrefab;
        AccountManager accountManager = AccountManager.Instance;

        switch (isHuman) {
            case true:
                totalDecks.AddRange(accountManager.humanDecks);
                deckPrefab = humanDeckPrefab;
                break;
            case false:
                totalDecks.AddRange(accountManager.orcDecks);
                deckPrefab = orcDeckPrefab;
                break;
            default:
                totalDecks = null;
                break;
        }

        if (totalDecks == null) return;
        PlayerPrefs.SetString("SelectedDeckId", "");

        int deckIndex = 0;

        for(int i =0; i<totalDecks.Count; i++) {
            GameObject setDeck = Instantiate(deckPrefab, deckContent.transform);
            setDeck.transform.Find("Deck/Name").GetComponent<TextMeshProUGUI>().text = totalDecks[deckIndex].name;

            if(LeaderDeckId == totalDecks[deckIndex].id)
                selectedDeckObject = setDeck;

            setDeck.transform.Find("Deck/Info/Text").GetComponent<TextMeshProUGUI>().text =
                    totalDecks[deckIndex].totalCardCount + "/40";
            setDeck.transform.Find("Deck").GetComponent<dataModules.StringIndex>().Id = totalDecks[deckIndex].id;
            int temp = deckIndex;
            setDeck.transform.Find("Deck").GetComponent<Button>().onClick.AddListener(() => {
                ScenarioManager.Instance.OnDeckSelected(setDeck, totalDecks[temp]);
            });
            deckIndex++;
        }
    }

    public void OnDeckSelected(GameObject selectedDeckObject, dataModules.Deck data) {
        if (this.selectedDeckObject != null) {
            this.selectedDeckObject.transform.Find("Outline").gameObject.SetActive(false);
            this.selectedDeckObject.transform.Find("Deck/Twinkle").gameObject.SetActive(false);
        }
        selectedDeckObject.transform.Find("Outline").gameObject.SetActive(true);

        LeaderDeckId = selectedDeckObject.transform.Find("Deck").GetComponent<dataModules.StringIndex>().Id;
        this.selectedDeckObject = selectedDeckObject;
        GameObject twinkle = selectedDeckObject.transform.Find("Deck/Twinkle").gameObject;
        twinkle.SetActive(true);
        //twinkle.GetComponent<DeckClickSpine>().Click();
        selectedDeck = data;
    }

    private void ClearDeckList() {
        foreach (Transform child in deckContent.transform) {
            Destroy(child.gameObject);
        }
    }

    private void ChangeDeck(string deckID) {
        var msg = string.Format("{0} 선택됨", deckID);
        PlayerPrefs.SetString("SelectedDeckId", deckID);
        int isNum = 0;
        if (int.TryParse(deckID, out isNum)) {
            PlayerPrefs.SetString("SelectedDeckType", "custom");
        }
        else {
            PlayerPrefs.SetString("SelectedDeckType", "basic");
        }
    }

    public void OnCloseBtn() {
        stageCanvas.SetActive(false);
    }

    public void OnHumanTempStage() {
        stageCanvas.SetActive(true);
        ClearDeckList();
        CreateBasicDeckList(true);
    }
    
    public void OnOrcTempStage() {
        stageCanvas.SetActive(true);
        ClearDeckList();
        CreateBasicDeckList(false);
    }

    public void OnStartBtn() {
        if (isIngameButtonClicked) {
            Logger.Log("이미 대전 시작 버튼이 눌려진 상태");
            return;
        }

        PlayerPrefs.SetString("SelectedBattleType", "solo");
        string race = PlayerPrefs.GetString("SelectedRace").ToLower();
        string selectedDeckId = PlayerPrefs.GetString("SelectedDeckId").ToLower();

        if (race != null && !string.IsNullOrEmpty(selectedDeckId)) {
            if (selectedDeck.deckValidate) {
                isIngameButtonClicked = true;
                FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.CONNECT_MATCHING_SCENE);
            }
            else {
                Modal.instantiate("유효하지 않은 덱입니다.", Modal.Type.CHECK);
            }
        }
        else {
            if (race == "none") Logger.Log("종족을 선택해야 합니다.");
            if (string.IsNullOrEmpty(selectedDeckId)) Logger.Log("덱을 선택해야 합니다.");

            if (race == "none") {
                Modal.instantiate("종족을 선택해 주세요.", Modal.Type.CHECK);
            }
            else if (string.IsNullOrEmpty(selectedDeckId)) {
                Modal.instantiate("덱을 선택해 주세요.", Modal.Type.CHECK);
            }
        }
        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
    }




}

[System.Serializable]
public class ShowSelectRace {
    public GameObject raceButton;
    public Sprite activeSprite;
    public Sprite deactiveSprite;
    public GameObject heroSelect;
    public GameObject StageCanvas;
    public GameObject heroContent;
    public GameObject stageContent;
}

public class ScenarioData : MonoBehaviour {
    protected ScenarioManager scenarioManager;

    private void Start() {
        scenarioManager = ScenarioManager.Instance;
    }

    public virtual void OnClicked() {
        return;
    }
}