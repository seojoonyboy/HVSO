using dataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Bolt;
using System;
using TMPro;

public class BattleReadySceneController : MonoBehaviour {
    public Toggle[] battleTypeToggles;
    public Button[] raceTypeButtons;

    BattleType selectedBattleType;
    RaceType raceType;

    List<GameObject> allDeckObjects = new List<GameObject>();
    public GameObject CardListModal, CardInfoModal;
    GameObject loadingModal;

    public HumanDecks humanDecks;
    public OrcDecks orcDecks;
    bool isIngameButtonClicked;

    [SerializeField] TextMeshProUGUI pageText;
    [SerializeField] public GameObject EmptyMsgShowPanel;
    [SerializeField] public GameObject ButtonGlowEffect;

    IEnumerator Start() {
        yield return null;
        DataLoad();
        isIngameButtonClicked = false;
    }

    private void OnEnable() {
        ChangeBattleType(0);
        Variables.Saved.Set("SelectedDeckId", "");
        Variables.Saved.Set("SelectedRace", RaceType.NONE);
    }

    public void ChangePageText(string msg) {
        pageText.text = msg;
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
            //raceTypeButtons[0].onClick.Invoke();
        }
        else {
            Modal.instantiate("데이터를 정상적으로 불러오지 못했습니다.\n다시 요청합니까?", Modal.Type.YESNO, () => {
                DataLoad();
            });
        }
    }

    public void OnStartButton() {
        if (isIngameButtonClicked) {
            Logger.Log("이미 대전 시작 버튼이 눌려진 상태");
            return;
        }

        string race = Variables.Saved.Get("SelectedRace").ToString().ToLower();
        string selectedDeckId = Variables.Saved.Get("SelectedDeckId").ToString().ToLower();
        if (race != null && !string.IsNullOrEmpty(selectedDeckId)) {
            isIngameButtonClicked = true;
            SceneManager.Instance.LoadScene(SceneManager.Scene.CONNECT_MATCHING_SCENE);
        }
        else {
            if (race == "none") Logger.Log("종족을 선택해야 합니다.");
            if (string.IsNullOrEmpty(selectedDeckId)) Logger.Log("덱을 선택해야 합니다.");

            if(race == "none") {
                Modal.instantiate("종족을 선택해 주세요.", Modal.Type.CHECK);
            }
            else if(string.IsNullOrEmpty(selectedDeckId)) {
                Modal.instantiate("덱을 선택해 주세요.", Modal.Type.CHECK);
            }
        }
        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
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
        Logger.Log(type);
        raceType = type;

        if (EmptyMsgShowPanel.activeSelf) EmptyMsgShowPanel.SetActive(false);
    }

    public void ChangeDeck(string deckId) {
        var msg = string.Format("{0} 선택됨", deckId);
        Logger.Log(msg);
        Variables.Saved.Set("SelectedDeckId", deckId);

        ButtonGlowEffect.SetActive(true);
    }

    public void ChangeBattleType(int pageIndex) {
        string type = "multi";
        switch (pageIndex) {
            case 0:
            default:
                type = "multi";
                break;
            case 1:
                type = "solo";
                break;
        }
        Variables.Saved.Set("SelectedBattleType", type);
    }

    public enum BattleType {
        AI = 0,
        CASUAL = 1,
        RANK = 2
    }

    public enum RaceType {
        HUMAN = 0,
        ORC = 1,
        NONE = 2
    }

    public void OnClickModifyButton(GameObject target) {

    }

    public void OnClickCardListModal() {
        CardListModal.SetActive(true);
    }

    public void OffCardListModal() {
        CardListModal.SetActive(false);
    }
}
