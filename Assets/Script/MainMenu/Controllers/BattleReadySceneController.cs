using dataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Bolt;

public class BattleReadySceneController : MonoBehaviour {
    public Toggle[] battleTypeToggles;

    BattleType selectedBattleType;
    RaceType raceType;

    List<GameObject> allDeckObjects = new List<GameObject>();
    public GameObject selectedDeck { get; private set; }

    public GameObject CardListModal, CardInfoModal;

    void Start() {
        battleTypeToggles[0].GetComponent<ToggleHandler>().OnValueChanged();
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            GraphicRaycaster gr = this.GetComponent<GraphicRaycaster>();
            PointerEventData ped = new PointerEventData(null);
            ped.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            gr.Raycast(ped, results);

            if (selectedDeck != null && isNoneDeckClicked(results)) {
                OffClickDeck();
            }
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
        SceneManager.Instance.LoadScene(SceneManager.Scene.MISSION_INGAME);
        //SceneManager.Instance.LoadScene(SceneManager.Scene.MAIN_SCENE);
    }

    public void OnBackButton() {
        SceneManager.Instance.LoadScene(SceneManager.Scene.MAIN_SCENE);
    }

    public void OnDeckListButton() {
        SceneManager.Instance.LoadScene(SceneManager.Scene.DECK_LIST_SCNE);
    }

    public void ChangeBattleType(BattleType type) {
        selectedBattleType = type;
    }

    public void ChangeRaceType(RaceType type) {
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

    public void OnClickDeck(GameObject target) {
        bool isOn = target.GetComponent<dataModules.BooleanIndex>().isOn;
        if (isOn) return;

        foreach (GameObject obj in allDeckObjects) {
            if (target == obj) continue;

            if (obj.GetComponent<BooleanIndex>().isOn) {
                EasyTween itween = obj.transform.Find("Animations/OnClose").GetComponent<EasyTween>();
                itween.OpenCloseObjectAnimation();

                obj.GetComponent<BooleanIndex>().isOn = false;
            }
        }

        selectedDeck = target;
        target.GetComponent<dataModules.BooleanIndex>().isOn = true;

        selectedDeck
            .transform.Find("Animations/OnClick")
            .GetComponent<EasyTween>()
            .OpenCloseObjectAnimation();
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
