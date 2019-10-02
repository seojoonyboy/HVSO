using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckSettingManager : MonoBehaviour
{
    [SerializeField] public GameObject cardDictionaryCanvas;
    [SerializeField] Transform deckList;
    [SerializeField] TMPro.TextMeshProUGUI deckNum;
    [SerializeField] HUDController hudController;
    [SerializeField] HeroSelectController heroSelectController;

    public MenuSceneController menuSceneController;
    public Transform selectedDeck;

    MyDecksLoader decksLoader;


    public void AttachDecksLoader(ref MyDecksLoader decksLoader) {
        this.decksLoader = decksLoader;
        this.decksLoader.OnLoadFinished.AddListener(() => { OnDecksInfoLoaded(); });
    }

    public void RefreshLine() {
        CloseDeckButtonsFast();
        Canvas.ForceUpdateCanvases();
        for (int i = 0; i < 3; i++) {
            LayoutRebuilder.ForceRebuildLayoutImmediate(deckList.GetComponent<RectTransform>());
        }

        Invoke("UpdateContentHeight", 0.25f);
    }

    private void UpdateContentHeight() {
        float height = deckList.GetComponent<RectTransform>().rect.height;
        transform.Find("DeckListParent").GetComponent<RectTransform>().sizeDelta = new Vector2(1080, height);
        transform.Find("DeckListParent").GetComponent<RectTransform>().anchoredPosition = new Vector2(transform.Find("DeckListParent").GetComponent<RectTransform>().anchoredPosition.x, -height / 2);
        GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
        transform.GetComponent<ScrollRect>().enabled = deckList.GetChild(4).gameObject.activeSelf;
    }

    public void SetPlayerNewDecks() {
        InitNewDecks();
        int humanDecks = AccountManager.Instance.humanDecks.Count;
        int orcDecks = AccountManager.Instance.orcDecks.Count;
        int deckCount = humanDecks + orcDecks;
        transform.Find("Header/NumValue").GetComponent<TMPro.TextMeshProUGUI>().text = deckCount.ToString() + "/10";
        if(deckCount > 0) {
            for(int i = 0; i < humanDecks; i++) {
                deckList.GetChild(i).gameObject.SetActive(true);
                deckList.GetChild(i).GetComponent<DeckHandler>().SetNewDeck(AccountManager.Instance.humanDecks[i]);
            }
            for (int i = humanDecks; i < deckCount; i++) {
                deckList.GetChild(i).gameObject.SetActive(true);
                deckList.GetChild(i).GetComponent<DeckHandler>().SetNewDeck(AccountManager.Instance.orcDecks[i - humanDecks]);
            }
        }
        if (deckCount != 10) {
            deckList.GetChild(deckCount).gameObject.SetActive(true);
            deckList.GetChild(deckCount).GetChild(0).Find("NewDeck").gameObject.SetActive(true);
        }
        RefreshLine();
    }

    private void InitNewDecks() {
        for (int i = 0; i < 10; i++) {
            Transform deck = deckList.GetChild(i).GetChild(0);
            deckList.GetChild(i).gameObject.SetActive(false);
            deck.localPosition = new Vector3(-5, 0, 0);
            deck.Find("HeroImg").gameObject.SetActive(false);
            deck.Find("CardNum").gameObject.SetActive(false);
            deck.Find("DeckName").gameObject.SetActive(false);
            deck.Find("NewDeck").gameObject.SetActive(false);
        }
    }

    public void MakeNewDeck() {
        CloseDeckButtonsFast();
        heroSelectController.gameObject.SetActive(true);
        heroSelectController.SetHumanHeroes();
        hudController.SetHeader(HUDController.Type.HIDE);
        hudController.SetBackButton(() => ExitHeroSelect());
    }

    public void ExitHeroSelect() {
        heroSelectController.transform.Find("RaceSelect/HumanSelect").GetChild(0).gameObject.SetActive(false);
        heroSelectController.transform.Find("RaceSelect/OrcSelect").GetChild(0).gameObject.SetActive(false);
        heroSelectController.gameObject.SetActive(false);
        hudController.SetHeader(HUDController.Type.SHOW_USER_INFO);
    }


    public IEnumerator OpenDeckButtons(Transform deck) {
        if (selectedDeck != null)
            yield return CloseDeckButtons();
        selectedDeck = deck;
        int deckIndex = deck.GetSiblingIndex();
        iTween.MoveTo(selectedDeck.GetChild(0).Find("Buttons").gameObject, iTween.Hash("y", -175, "islocal", true, "time", 0.3f));
        for (int i = deckIndex + 1; i < 10; i++) {
            if (deckList.GetChild(i).gameObject.activeSelf) {
                iTween.MoveTo(deckList.GetChild(i).GetChild(0).gameObject, iTween.Hash("y", -175, "islocal", true, "time", 0.3f));
            }
        }
        yield return new WaitForSeconds(0.4f);
    }

    public IEnumerator CloseDeckButtons() {
        int deckIndex = 0;
        if (selectedDeck != null) {
            deckIndex = selectedDeck.GetSiblingIndex();
            iTween.MoveTo(selectedDeck.GetChild(0).Find("Buttons").gameObject, iTween.Hash("y", 0, "islocal", true, "time", 0.1f));
        }
        for (int i = deckIndex + 1; i < 10; i++) {
            if (deckList.GetChild(i).gameObject.activeSelf) {
                iTween.MoveTo(deckList.GetChild(i).GetChild(0).gameObject, iTween.Hash("y", 0, "islocal", true, "time", 0.1f));
            }
        }
        yield return new WaitForSeconds(0.2f);
        selectedDeck = null;
    }

    public void CloseDeckButtonsFast() {
        int deckIndex = 0;
        if (selectedDeck != null) {
            deckIndex = selectedDeck.GetSiblingIndex();
            selectedDeck.GetChild(0).Find("Buttons").localPosition = new Vector3(-5, 0, 0);
        }
        for (int i = deckIndex + 1; i < 10; i++) {
            if (deckList.GetChild(i).gameObject.activeSelf) {
                deckList.GetChild(i).GetChild(0).localPosition = Vector3.zero;
            }
        }
    }

    void OnDecksInfoLoaded() {
        SetPlayerNewDecks();
    }
}
