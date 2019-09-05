using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using Tutorial;

public class ScenarioManager : SerializedMonoBehaviour
{
    public static ScenarioManager Instance { get; private set; }
    public ShowSelectRace human, orc;
    public string heroID;
    public bool isIngameButtonClicked = false;
    public GameObject stageCanvas;
    public GameObject deckContent;

    private GameObject selectedDeckObject = null;
    public dataModules.Deck selectedDeck;

    public bool isHuman;

    public Dictionary<int, string> stage_name;
    public List<ChapterData> human_chapterDatas, orc_chapterDatas;
    public ChapterData selectedChapterData;

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
        PlayerPrefs.SetString("SelectedRace", "");
        PlayerPrefs.SetString("SelectedDeckId", "");
        PlayerPrefs.SetString("SelectedDeckType", "");
        PlayerPrefs.SetString("SelectedBattleType", "");
    }   
    

    public void OnBackButton() {
        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
        FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);
    }

    public void OnHumanButton() {
        orc.raceButton.GetComponent<Image>().sprite = orc.deactiveSprite;
        orc.heroSelect.SetActive(false);
        orc.StageCanvas.SetActive(false);

        if (stage_name.Count > 0)
            stage_name.Clear();

        heroID = "";

        stage_name.Add(1, "토벌");


        for(int i = 0; i<stage_name.Count; i++) 
            human.stageContent.transform.GetChild(i).Find("StageName").gameObject.GetComponent<TextMeshProUGUI>().text = 0.ToString() + "-" + (i+1).ToString() + " " + stage_name[i + 1];
        

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

        if (stage_name.Count > 0)
            stage_name.Clear();

        heroID = "";

        
        stage_name.Add(1, "복수");

        for (int i = 0; i < stage_name.Count; i++)
            orc.stageContent.transform.GetChild(i).Find("StageName").gameObject.GetComponent<TextMeshProUGUI>().text = 0.ToString() + "-" + (i + 1).ToString() + " " + stage_name[i + 1];


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

    public void OnClickStage(ChapterData chapterData) {
        stageCanvas.SetActive(true);
        ClearDeckList();
        CreateBasicDeckList((isHuman == true) ? true : false);
        stageCanvas
            .transform
            .Find("StagePanel/TextGroup/StageName")
            .gameObject
            .GetComponent<TextMeshProUGUI>().text = chapterData.chapter.ToString() + "-" + chapterData.stage_number.ToString() + " " + stage_name[chapterData.stage_number];
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
                FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.TUTORIAL);
                ScenarioGameManagment.chapterData = selectedChapterData;
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

namespace Tutorial {
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

    public class ScenarioButton : MonoBehaviour {
        protected ScenarioManager scenarioManager;

        private void Start() {
            scenarioManager = ScenarioManager.Instance;
        }

        public virtual void OnClicked() { }
    }

    public class ChapterData {
        public int chapter;
        public int stage_number;
        public string stage_Name;
        public List<ScriptData> scripts;
    }

    public class ScriptData {
        [MultiLineProperty(10)] public string Print_text;
        public List<Method> methods;

        public bool isExecute;
    }

    public class Method {
        public string name;
        public List<string> args;
    }
}