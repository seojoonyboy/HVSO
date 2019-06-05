using dataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Bolt;
using System;

public class BattleReadySceneController : MonoBehaviour {
    public Toggle[] battleTypeToggles;
    public Button[] raceTypeButtons;

    BattleType selectedBattleType;
    RaceType raceType;

    List<GameObject> allDeckObjects = new List<GameObject>();
    public GameObject selectedDeck { get; private set; }

    public GameObject CardListModal, CardInfoModal;
    GameObject loadingModal;

    public HumanDecks humanDecks;
    public OrcDecks orcDecks;
    bool isIngameButtonClicked;

    void Start() {
        DataLoad();
        isIngameButtonClicked = false;
    }

    public void DataLoad() {
        AccountManager.Instance.RequestHumanDecks(OnReqHumanDecks, OnRetryReq);
        loadingModal = LoadingModal.instantiate();
        loadingModal.transform.Find("Panel/AdditionalMessage").GetComponent<Text>().text = "덱 정보를 불러오는중...Human Decks";
    }

    private void OnRetryReq(string msg) {
        loadingModal.transform.GetChild(0).GetComponent<UIModule.LoadingTextEffect>().AddAdditionalMsg(msg);
    }

    private void OnReqHumanDecks(HttpResponse response) {
        if(response.responseCode == 200) {
            humanDecks = JsonReader.Read<HumanDecks>(response.data);

            AccountManager.Instance.RequestOrcDecks(OnReqOrcDecks, OnRetryReq);
            loadingModal.transform.Find("Panel/AdditionalMessage").GetComponent<Text>().text = "덱 정보를 불러오는중...Orc Decks";
        }
        else {
            Modal.instantiate("데이터를 정상적으로 불러오지 못했습니다.\n다시 요청합니까?", Modal.Type.YESNO, ()=> {
                DataLoad();
            });
        }
    }

    private void OnReqOrcDecks(HttpResponse response) {
        Destroy(loadingModal);
        if (response.responseCode == 200) {
            orcDecks = JsonReader.Read<OrcDecks>(response.data);

            //battleTypeToggles[0].GetComponent<Toggle>().isOn = true;
            //battleTypeToggles[0].GetComponent<ToggleHandler>().OnValueChanged();
            raceTypeButtons[0].onClick.Invoke();
        }
        else {
            Modal.instantiate("데이터를 정상적으로 불러오지 못했습니다.\n다시 요청합니까?", Modal.Type.YESNO, () => {
                DataLoad();
            });
        }
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            //GraphicRaycaster gr = this.GetComponent<GraphicRaycaster>();
            //PointerEventData ped = new PointerEventData(null);
            //ped.position = Input.mousePosition;
            //List<RaycastResult> results = new List<RaycastResult>();
            //gr.Raycast(ped, results);

            //if (selectedDeck != null && isNoneDeckClicked(results)) {
            //    OffClickDeck();
            //}
        }
    }

    private bool isNoneDeckClicked(List<RaycastResult> results) {
        foreach(RaycastResult result in results) {
            if(result.gameObject == selectedDeck){
                return false;
            }
        }
        return true;
    }

    public void OnStartButton() {
        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);

        if (isIngameButtonClicked) {
            Debug.Log("이미 대전 시작 버튼이 눌려진 상태");
            return;
        }

        isIngameButtonClicked = true;
        SceneManager.Instance.LoadScene(SceneManager.Scene.CONNECT_MATCHING_SCENE);
        //SceneManager.Instance.LoadScene(SceneManager.Scene.MAIN_SCENE);
        //SceneManager.Instance.LoadScene(SceneManager.Scene.MISSION_INGAME);
    }

    public void OnBackButton() {
        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
        SceneManager.Instance.LoadScene(SceneManager.Scene.MAIN_SCENE);
    }

    public void OnDeckListButton() {
        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
        SceneManager.Instance.LoadScene(SceneManager.Scene.DECK_LIST_SCNE);
    }

    public void ChangeBattleType(BattleType type) {
        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
        selectedBattleType = type;
    }

    public void ChangeRaceType(RaceType type) {
        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
        Variables.Saved.Set("SelectedRace", type);
        Debug.Log(type);
        raceType = type;
    }

    public enum BattleType {
        AI = 0,
        CASUAL = 1,
        RANK = 2
    }

    public enum RaceType {
        HUMAN = 0,
        ORC = 1
    }

    public void OnClickModifyButton(GameObject target) {

    }

    public void OffClickDeck() {
        if (selectedDeck != null) {
            if (selectedDeck.GetComponent<BooleanIndex>().isOn) {
                EasyTween itween = selectedDeck.transform.Find("Animations/OnClose").GetComponent<EasyTween>();
                itween.OpenCloseObjectAnimation();

                selectedDeck.GetComponent<BooleanIndex>().isOn = false;
            }
            selectedDeck = null;
        }
    }

    public void OnClickCardListModal() {
        CardListModal.SetActive(true);
    }

    public void OffCardListModal() {
        CardListModal.SetActive(false);
    }
}
