using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckSettingManager : MonoBehaviour
{
    [SerializeField] Transform deckList;
    [SerializeField] TMPro.TextMeshProUGUI deckNum;
    [SerializeField] HUDController hudController;
    [SerializeField] HeroSelectController heroSelectController;
    [SerializeField] Transform anchorGuide1;
    [SerializeField] Transform anchorGuide2;

    public MenuSceneController menuSceneController;
    public Transform selectedDeck;    
    bool initialized = false;
    public bool isAni = false;

    public void Awake() {
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_DECKS_UPDATED, SetPlayerNewDecks);
    }

    void OnDestroy() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_DECKS_UPDATED, SetPlayerNewDecks);
    }

    private void SetPlayerNewDecks(Enum Event_Type, Component Sender, object Param) {
        SetPlayerNewDecks();
    }

    public void RefreshLine() {
        CloseDeckButtonsFast();
        Canvas.ForceUpdateCanvases();
        for (int i = 0; i < 3; i++) {
            LayoutRebuilder.ForceRebuildLayoutImmediate(deckList.GetComponent<RectTransform>());
        }
        GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
        transform.GetComponent<ScrollRect>().enabled = deckList.GetChild(4).gameObject.activeSelf;
        Invoke("UpdateContentHeight", 0.25f);
    }

    private void UpdateContentHeight() {
        if (initialized) return;
        initialized = true;
        float height = anchorGuide1.localPosition.y - anchorGuide2.localPosition.y + 200;
        //float height = deckList.GetComponent<RectTransform>().rect.height;
        transform.Find("DeckListParent").GetComponent<RectTransform>().sizeDelta = new Vector2(1080, -height);
        transform.Find("DeckListParent").GetComponent<RectTransform>().anchoredPosition = new Vector2(transform.Find("DeckListParent").GetComponent<RectTransform>().anchoredPosition.x, height / 2);
    }

    public void SetPlayerNewDecks() {
        if (transform == null) return;
        InitNewDecks();
        int humanDecks = AccountManager.Instance.humanDecks.Count;
        int orcDecks = AccountManager.Instance.orcDecks.Count;
        int deckCount = humanDecks + orcDecks;
        transform.Find("Header/NumValue").GetComponent<TMPro.TextMeshProUGUI>().text = deckCount.ToString() + "/10";
        deckList.GetChild(0).gameObject.SetActive(true);
        deckList.GetChild(0).GetChild(0).Find("NewDeck").gameObject.SetActive(true);
        deckList.GetChild(0).GetChild(0).Find("RaceFlag").gameObject.SetActive(false);
        if (deckCount > 0) {
            for(int i = 0; i < humanDecks; i++) {
                deckList.GetChild(i + 1).gameObject.SetActive(true);
                deckList.GetChild(i + 1).GetComponent<DeckHandler>().SetNewDeck(AccountManager.Instance.humanDecks[i]);
            }
            for (int i = humanDecks; i < deckCount; i++) {
                deckList.GetChild(i + 1).gameObject.SetActive(true);
                deckList.GetChild(i + 1).GetComponent<DeckHandler>().SetNewDeck(AccountManager.Instance.orcDecks[i - humanDecks]);
            }
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
        hudController.SetBackButton(() => ExitHeroSelect_Edit());
        EscapeKeyController.escapeKeyCtrl.AddEscape(ExitHeroSelect_Edit);
    }

    public void ExitHeroSelect_Edit() {
        heroSelectController.transform.Find("InnerCanvas/RaceSelect/HumanSelect").GetChild(0).gameObject.SetActive(false);
        heroSelectController.transform.Find("InnerCanvas/RaceSelect/OrcSelect").GetChild(0).gameObject.SetActive(false);
        heroSelectController.gameObject.SetActive(false);
        hudController.SetHeader(HUDController.Type.SHOW_USER_INFO);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(ExitHeroSelect_Edit);
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
        if(EditCardHandler.questInfo != null) {
            Transform hand = deck.Find("DeckObject/tutorialHand");
            if(hand != null) hand.gameObject.SetActive(false);
        }
        yield return new WaitForSeconds(0.25f);
        isAni = false;
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
        if(EditCardHandler.questInfo != null) {
            Transform hand = selectedDeck.Find("DeckObject/tutorialHand");
            if(hand != null) hand.gameObject.SetActive(true);
        }
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
        if(EditCardHandler.questInfo != null && selectedDeck != null) {
            Transform hand = selectedDeck.Find("DeckObject/tutorialHand");
            if(hand != null) hand.gameObject.SetActive(true);
        }
    }
}
