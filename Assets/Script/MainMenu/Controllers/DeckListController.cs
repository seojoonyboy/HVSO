using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckListController : MonoBehaviour {
    GameObject selectedDeck;

    [SerializeField] GameObject 
        DeckPrefab, 
        DeckGroupPrefab;
    [SerializeField] Transform Content;

    // Start is called before the first frame update
    void Start() {
        CreateDummyDecks();
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
        }
    }

    public void CreateDecks() {
        foreach(Transform tf in Content) {
            Destroy(tf.gameObject);
        }
    }

    public void CreateDecks(string hero) {
        ClearDecks();
    }

    private void ClearDecks() {

    }

    public void OnClickDeck(GameObject target) {
        bool isOn = target.GetComponent<dataModules.BooleanIndex>().isOn;
        if (isOn) return;

        selectedDeck = target;
        target.GetComponent<dataModules.BooleanIndex>().isOn = true;

        selectedDeck
            .transform.Find("Animations/OnClick")
            .GetComponent<EasyTween>()
            .OpenCloseObjectAnimation();
    }

    public void OffClickedDeck() {
        GameObject target = selectedDeck;
        if (target == null) return;
        target.GetComponent<dataModules.BooleanIndex>().isOn = false;
    }

    public void OnBackButton() {
        SceneManager.Instance.LoadLastScene();
    }
}
