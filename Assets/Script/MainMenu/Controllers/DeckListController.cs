using dataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckListController : MonoBehaviour {
    public GameObject selectedDeck { get; private set; }

    [SerializeField] GameObject 
        DeckPrefab, 
        DeckGroupPrefab,
        PortraitPrefab;

    [SerializeField] Transform Content;
    [SerializeField] Transform PortraitParent;

    List<GameObject> allDeckObjects = new List<GameObject>();
    // Start is called before the first frame update
    void Start() {
        //CreateDummyDecks();
    }

    // Update is called once per frame
    void Update() {

    }

    /// <summary>
    /// 고정 덱 생성
    /// </summary>
    public void CreateDummyDecks() {
        ClearDecks();
        var decks = AccountManager.Instance.myDecks;
        for(int i=0; i<decks.Count; i++) {
            GameObject newDeckPanel = Instantiate(DeckGroupPrefab, Content);
            newDeckPanel.transform.Find("Header/Text").GetComponent<Text>().text = decks[i].heroName;

            GameObject portrait = Instantiate(PortraitPrefab, PortraitParent);
            portrait.transform.Find("Name").GetComponent<Text>().text = decks[i].heroName;

            Transform slot = newDeckPanel.transform.Find("Decks").GetChild(0);
            GameObject deck = Instantiate(DeckPrefab, slot);

            deck.transform.Find("Text").GetComponent<Text>().text = decks[i].type + " 덱";

            deck.GetComponent<Button>().onClick.AddListener(() => { OnClickDeck(deck); });
            allDeckObjects.Add(deck);

            deck.GetComponent<IntergerIndex>().Id = i;
        }

        for(int i=0; i< 10-decks.Count; i++) {
            GameObject portrait = Instantiate(PortraitPrefab, PortraitParent);
            portrait.transform.Find("Deactive").gameObject.SetActive(true);
            portrait.transform.Find("Name").GetComponent<Text>().text = "없음";
        }
    }

    public void CreateDecks() {
        foreach(Transform tf in Content) {
            Destroy(tf.gameObject);
        }
    }

    public void CreateDecks(string hero) {
        ClearDecks();
        allDeckObjects.Clear();
    }

    private void ClearDecks() {

    }

    public void OnClickDeck(GameObject target) {
        bool isOn = target.GetComponent<dataModules.BooleanIndex>().isOn;
        if (isOn) return;

        foreach(GameObject obj in allDeckObjects) {
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
        if(selectedDeck != null) {
            if (selectedDeck.GetComponent<BooleanIndex>().isOn) {
                EasyTween itween = selectedDeck.transform.Find("Animations/OnClose").GetComponent<EasyTween>();
                itween.OpenCloseObjectAnimation();

                selectedDeck.GetComponent<BooleanIndex>().isOn = false;
            }
        }
    }

    public void OnBackButton() {
        SceneManager.Instance.LoadLastScene();
    }
}
