using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using Tutorial;
using UnityEngine.Events;

public class ScenarioManager : SerializedMonoBehaviour
{
    public static ScenarioManager Instance { get; private set; }
    public ShowSelectRace human, orc;
    public string heroID;
    public bool isIngameButtonClicked = false;
    public GameObject stageCanvas;
    public GameObject deckContent;

    private GameObject selectedDeckObject = null;
    public object selectedDeck;


    public GameObject headerMenu;
    public bool isHuman;
    public List<ChapterData> human_chapterDatas, orc_chapterDatas;
    public ChapterData selectedChapterData;

    [SerializeField] GameObject orcDeckPrefab;
    [SerializeField] GameObject humanDeckPrefab;


    public static UnityEvent OnLobbySceneLoaded = new UnityEvent();
    private void Awake() {
        Instance = this;
        OnHumanCategories();
        OnLobbySceneLoaded.Invoke();
        isIngameButtonClicked = false;
        //PlayerPrefs.SetString("SelectedDeckId", "");
#if !UNITY_EDITOR
        ScenarioMask.Instance.transform.parent.Find("DebugText").gameObject.SetActive(false);
#endif
    }

    private void OnDestroy() {
        Instance = null;
        //PlayerPrefs.SetString("SelectedRace", "");
        //PlayerPrefs.SetString("SelectedDeckId", "");
        //PlayerPrefs.SetString("SelectedDeckType", "");
        //PlayerPrefs.SetString("SelectedBattleType", "");
    }

    [SerializeField] HUDController HUDController;
    void OnEnable() {
        HUDController.SetHeader(HUDController.Type.RESOURCE_ONLY_WITH_BACKBUTTON);
        HUDController.SetBackButton(() => {
            OnBackButton();
        });
    }


    public void OnBackButton() {
        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
        PlayerPrefs.SetString("SelectedRace", "");
        PlayerPrefs.SetString("SelectedDeckId", "");
        PlayerPrefs.SetString("SelectedDeckType", "");
        PlayerPrefs.SetString("SelectedBattleType", "");
        //FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);        
        HUDController.SetHeader(HUDController.Type.SHOW_USER_INFO);

        gameObject.SetActive(false);
    }

    public void OnHumanCategories() {
        //heroID = "";
        isHuman = true;
        PlayerPrefs.SetString("SelectedRace", "HUMAN");
        ToggleUI();
        SetStoryListInfo();
    }
    
    public void OnOrcCategories() {
        //heroID = "";
        isHuman = false;
        PlayerPrefs.SetString("SelectedRace", "ORC");
        ToggleUI();
        SetStoryListInfo();
    }

    /// <summary>
    /// 종족 선택시 UI 세팅
    /// </summary>
    private void ToggleUI() {
        OffPrevStoryList();
        SetStoryListInfo();

        if (isHuman) {
            orc.raceButton.GetComponent<Image>().sprite = orc.deactiveSprite;
            orc.heroSelect.SetActive(false);
            orc.StageCanvas.SetActive(false);

            human.raceButton.GetComponent<Image>().sprite = human.activeSprite;
            human.heroSelect.SetActive(true);
            human.StageCanvas.SetActive(true);
        }
        else {
            human.raceButton.GetComponent<Image>().sprite = human.deactiveSprite;
            human.heroSelect.SetActive(false);
            human.StageCanvas.SetActive(false);

            orc.raceButton.GetComponent<Image>().sprite = orc.activeSprite;
            orc.heroSelect.SetActive(true);
            orc.StageCanvas.SetActive(true);
        }
    }

    private void SetStoryListInfo() {
        Transform canvas;
        List<ChapterData> selectedList;
        if (isHuman) {
            canvas = human.StageCanvas.transform;
            selectedList = human_chapterDatas;
        }
        else {
            canvas = orc.StageCanvas.transform;
            selectedList = orc_chapterDatas;
        }

        foreach (Transform child in canvas.transform) {
            child.gameObject.SetActive(true);
        }

        Transform content = canvas.Find("HUD/StageSelect/Viewport/Content");
        for(int i=0; i < selectedList.Count; i++) {
            GameObject item = content.GetChild(i).gameObject;
            item.SetActive(true);
            string str = string.Format("{0}-{1} {2}", selectedList[i].chapter, selectedList[i].stage_number, selectedList[i].stage_Name);
            item.transform.Find("StageName").GetComponent<TextMeshProUGUI>().text = str;

            StageButton stageButtonComp = item.GetComponent<StageButton>();
            stageButtonComp.Init(selectedList[i].chapter, selectedList[i].stage_number, isHuman);

            //item.transform.Find("StageScript").GetComponent<TextMeshProUGUI>().text = selectedList[i].description;
        }
    }

    private void OffPrevStoryList() {
        if (isHuman) {
            Transform stageSelectContent = orc.StageCanvas.transform.Find("HUD/StageSelect/Viewport/Content");
            foreach(Transform child in stageSelectContent) {
                child.gameObject.SetActive(false);
            }

            foreach (Transform child in orc.StageCanvas.transform) {
                child.gameObject.SetActive(false);
            }
        }
        else {
            Transform stageSelectContent = orc.StageCanvas.transform.Find("HUD/StageSelect/Viewport/Content");
            foreach (Transform child in stageSelectContent) {
                child.gameObject.SetActive(false);
            }

            foreach (Transform child in human.StageCanvas.transform) {
                child.gameObject.SetActive(false);
            }
        }
    }

    public void OnStageCloseBtn() {        
        stageCanvas.SetActive(false);
    }

    bool isTutorialSelected = false;

    private void CreateTutorialDeck(bool isHuman) {
        GameObject deckPrefab;
        string deckName = "";
        if (isHuman) {
            deckPrefab = humanDeckPrefab;
            deckName = "튜토리얼 덱";
        }
        else {
            deckPrefab = orcDeckPrefab;
            deckName = "튜토리얼 덱";
        }
        GameObject setDeck = Instantiate(deckPrefab, deckContent.transform);
        dataModules.Deck dummyDeck = new dataModules.Deck();
        dummyDeck.deckValidate = true;
        
        setDeck.transform.Find("Deck").GetComponent<Button>().onClick.AddListener(() => {
            OnDeckSelected(setDeck, dummyDeck, true);
        });
        setDeck.transform.Find("Deck/Name").GetComponent<TextMeshProUGUI>().text = deckName;
    }

    private void LoadMyDecks(bool isHuman) {
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

        for (int i = 0; i < totalDecks.Count; i++) {
            GameObject setDeck = Instantiate(deckPrefab, deckContent.transform);
            setDeck.transform.Find("Deck/Name").GetComponent<TextMeshProUGUI>().text = totalDecks[deckIndex].name;

            setDeck.transform.Find("Deck/Info/Text").GetComponent<TextMeshProUGUI>().text =
                    totalDecks[deckIndex].totalCardCount + "/40";
            setDeck.transform.Find("Deck").GetComponent<dataModules.StringIndex>().Id = totalDecks[deckIndex].id;
            int temp = deckIndex;
            setDeck.transform.Find("Deck").GetComponent<Button>().onClick.AddListener(() => {
                Instance.OnDeckSelected(setDeck, totalDecks[temp], false);
            });
            deckIndex++;
        }
    }

    public void OnDeckSelected(GameObject selectedDeckObject, dataModules.Deck data, bool isTutorial) {
        if (this.selectedDeckObject != null) {
            this.selectedDeckObject.transform.Find("SelectedBack").gameObject.SetActive(false);
            this.selectedDeckObject.transform.Find("Selected").gameObject.SetActive(false);
        }
        //selectedDeckObject.transform.Find("Outline").gameObject.SetActive(true);
        this.selectedDeckObject = selectedDeckObject;
        this.selectedDeckObject.transform.Find("SelectedBack").gameObject.SetActive(true);
        this.selectedDeckObject.transform.Find("Selected").gameObject.SetActive(true);
        //GameObject twinkle = selectedDeckObject.transform.Find("Deck/Twinkle").gameObject;
        //twinkle.SetActive(true);
        //twinkle.GetComponent<DeckClickSpine>().Click();
        object[] selectedInfo = new object[] { isTutorial, data };
        PlayerPrefs.SetString("SelectedDeckId", data.id);
        selectedDeck = selectedInfo;
    }

    private void ClearDeckList() {
        foreach (Transform child in deckContent.transform) {
            Destroy(child.gameObject);
        }
    }

    public void OnCloseBtn() {
        stageCanvas.SetActive(false);
        HUDController.gameObject.SetActive(true);
    }

    public void OnClickStage(ChapterData chapterData, bool isTutorial) {
        stageCanvas.SetActive(true);
        ClearDeckList();
        isTutorialSelected = isTutorial;
        HUDController.gameObject.SetActive(false);

        if (isTutorial) {
            CreateTutorialDeck(isHuman);

            stageCanvas
                .transform
                .Find("HUD/StagePanel/TextGroup/StageName")
                .gameObject
                .GetComponent<TextMeshProUGUI>().text = chapterData.chapter.ToString() + "-" + chapterData.stage_number.ToString();


            stageCanvas
                .transform
                .Find("HUD/StagePanel/TextGroup/StageName")
                .gameObject
                .GetComponent<TextMeshProUGUI>().text = chapterData.stage_Name;


            stageCanvas
                .transform
                .Find("HUD/StagePanel/TextGroup/StageScript")
                .gameObject
                .GetComponent<Text>().text = chapterData.description;

        }
        else {
            LoadMyDecks(isHuman);
            stageCanvas
                .transform
                .Find("HUD/StagePanel/TextGroup/StageName")
                .gameObject
                .GetComponent<TextMeshProUGUI>().text = chapterData.chapter.ToString() + "-" + chapterData.stage_number.ToString();


            stageCanvas
                .transform
                .Find("HUD/StagePanel/TextGroup/StageName")
                .gameObject
                .GetComponent<TextMeshProUGUI>().text = chapterData.stage_Name;


            stageCanvas
                .transform
                .Find("HUD/StagePanel/TextGroup/StageScript")
                .gameObject
                .GetComponent<Text>().text = chapterData.description;
        }
    }

    public void OnStartBtn() {
        if (isIngameButtonClicked) {
            Logger.Log("이미 대전 시작 버튼이 눌려진 상태");
            return;
        }
        PlayerPrefs.SetString("SelectedBattleType", "story");
        string race = PlayerPrefs.GetString("SelectedRace").ToLower();

        if(selectedDeck == null) {
            Modal.instantiate("유효하지 않은 덱입니다.", Modal.Type.CHECK);
            SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
            return;
        }

        object[] selectedDeckInfo = (object[])selectedDeck;
        bool isTutorial = (bool)selectedDeckInfo[0];
        if (isTutorial) {
            FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.CONNECT_MATCHING_SCENE);
            ScenarioGameManagment.chapterData = selectedChapterData;
        }
        else {
            string selectedDeckId = PlayerPrefs.GetString("SelectedDeckId").ToLower();
            dataModules.Deck selectedDeck = (dataModules.Deck)selectedDeckInfo[1];

            if (race != null && !string.IsNullOrEmpty(selectedDeckId)) {
                if (selectedDeck.deckValidate) {
                    isIngameButtonClicked = true;

                    FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.CONNECT_MATCHING_SCENE);
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
        public string match_type;
        [MultiLineProperty(10)] public string description;
        [MultiLineProperty(5)] public string specialRule;
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